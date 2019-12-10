using System;
using System.Collections;
using GHIElectronics.TinyCLR.Devices.Modbus.Interface;

namespace GHIElectronics.TinyCLR.Devices.Modbus {
    /// <summary>
    /// Modbus TCP/RTU Gateway
    /// </summary>
    /// <remarks>
    /// The <see cref="ModbusGateway"/> class provides a gateway from an Modbus/TCP device to one or more Modbus/RTU (or even TCP) master busses.
    /// </remarks>
    /// <example>
    /// The following code creates a 1:1 gateway to as single RTU master.
    /// <code>
    /// var modbusRtuInterface = new ModbusRtuInterface(...);
    /// var gateway = new ModbusGateway(ModbusConst.TcpDeviceAddress, modbusRtuInterface, 1000);
    /// var tcpInterfaceListener = ModbusTcpInterface.StartDeviceListener(gateway);
    /// gateway.Start();
    /// </code>
    /// </example>
    public class ModbusGateway : ModbusDevice {
        private readonly Hashtable masterMap = new Hashtable(247);

        /// <summary>
        /// Creates a new Modbus Gateway without any master interfaces.
        /// </summary>
        /// <param name="deviceAddress">Address of this device</param>
        /// <remarks>
        /// Use <see cref="AddMaster(IModbusInterface, int, Hashtable)"/> to add various Modbus masters.
        /// </remarks>
        public ModbusGateway(byte deviceAddress) :
           base(deviceAddress) { }

        /// <summary>
        /// Creates a new Modbus Gateway with a single master interface and an 1:1 device id mapping
        /// </summary>
        /// <param name="deviceAddress">Address of this device</param>
        /// <param name="masterInterface">Master interface to add</param>
        /// <param name="timeout">Timeout for master interface</param>
        public ModbusGateway(byte deviceAddress, IModbusInterface masterInterface, int timeout) :
           this(deviceAddress) => this.AddMaster(masterInterface, timeout);

        /// <summary>
        /// Adds an additional master interface with a 1:1 device id mapping
        /// </summary>
        /// <param name="masterInterface">Master interface to add</param>
        /// <param name="timeout">Timeout for master interface</param>
        public void AddMaster(IModbusInterface masterInterface, int timeout) {
            var addressMap = new Hashtable();
            for (byte n = 1; n < 248; ++n) {
                addressMap.Add(n, n);
            }
            this.AddMaster(masterInterface, timeout, addressMap);
        }

        /// <summary>
        /// Adds an additional master interface with an custom device id mapping
        /// </summary>
        /// <param name="masterInterface">master interface to add</param>
        /// <param name="timeout">Timeout for master interface</param>
        /// <param name="addressMap">A hashtable where the key is the incoming device id that is mapped to it's value as device id on the master interface.
        /// Both, key and value of the hashtable must be of type byte.</param>
        public void AddMaster(IModbusInterface masterInterface, int timeout, Hashtable addressMap) {
            masterInterface.PrepareWrite();
            foreach (byte sourceAddress in addressMap.Keys) {
                this.masterMap[sourceAddress] = new GatewayMaster(masterInterface, (byte)addressMap[sourceAddress], timeout);
            }
        }

        private class GatewayMaster {
            public readonly IModbusInterface MasterInterface;
            public readonly byte TargetAddress;
            public readonly int Timeout;
            public readonly byte[] Buffer;

            public GatewayMaster(IModbusInterface masterInterface, byte targetAddress, int timeout) {
                this.MasterInterface = masterInterface;
                this.TargetAddress = targetAddress;
                this.Timeout = timeout;
                this.Buffer = new byte[masterInterface.MaxTelegramLength];
            }
        }

        /// <summary>
        /// Is called when the value of the object id is needed for a ReadDeviceIdentification request.
        /// </summary>
        /// <param name="objectId">Object id.</param>
        /// <returns>Returns the value of the requested object id.</returns>
        protected override string OnGetDeviceIdentification(ModbusObjectId objectId) => "";

        /// <summary>
        /// Is called when the maximum conformity level is needed for a ReadDeviceIdentification request.
        /// </summary>
        /// <returns>Returns the maximum conformity level which is provided by this device.</returns>
        protected override ModbusConformityLevel GetConformityLevel() => ModbusConformityLevel.Basic;

        /// <summary>
        /// Handles a received message.
        /// </summary>
        /// <param name="intf">Interface by which te message was received.</param>
        /// <param name="deviceAddress">Address of the target device</param>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="telegramLength">Length of the message in bytes.</param>
        /// <param name="telegramContext">Interface specific message context.</param>
        /// <param name="fc">Function code.</param>
        /// <param name="dataPos">Index of function code specific data.</param>
        /// <param name="dataLength">Length of the function code specific data in bytes.</param>
        protected override void OnHandleTelegram(IModbusInterface intf, byte deviceAddress, bool isBroadcast, short telegramLength,
         object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            var master = (GatewayMaster)this.masterMap[deviceAddress];
            if (master != null) {
                object masterTelegramContext = null;

                master.MasterInterface.CreateTelegram(master.TargetAddress, (byte)fc, dataLength, master.Buffer, out var masterTelegramLength, out var masterDataPos, false, ref masterTelegramContext);
                Array.Copy(this.Buffer, dataPos, master.Buffer, masterDataPos, dataLength);

                try {
                    var masterDataLength = this.SendReceive(master.MasterInterface, master.Buffer, master.TargetAddress, fc, master.Timeout,
                       masterTelegramLength, -1, masterTelegramContext, ref masterDataPos);

                    intf.CreateTelegram(deviceAddress, (byte)fc, masterDataLength, this.Buffer, out telegramLength, out dataPos,
                       true, ref telegramContext);
                    Array.Copy(master.Buffer, masterDataPos, this.Buffer, dataPos, masterDataLength);
                    intf.SendTelegram(this.Buffer, telegramLength);
                }
                catch (ModbusException ex) {
                    try {
                        if (ex.ErrorCode == ModbusErrorCode.Timeout) {
                            this.SendErrorResult(intf, false, deviceAddress, telegramContext, fc, ModbusErrorCode.GatewayTargetDeviceFailedToRespond);
                        }
                        else if (((ushort)ex.ErrorCode & 0xff00) != 0) {
                            this.SendErrorResult(intf, false, deviceAddress, telegramContext, fc, ModbusErrorCode.GatewayTargetDeviceFailedToRespond);
                        }
                        else {
                            this.SendErrorResult(intf, false, deviceAddress, telegramContext, fc, ex.ErrorCode);
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                }
                catch {
                    try {
                        this.SendErrorResult(intf, false, deviceAddress, telegramContext, fc, ModbusErrorCode.GatewayPathUnavailable);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                }
            }
            else {
                base.OnHandleTelegram(intf, deviceAddress, isBroadcast, telegramLength, telegramContext, fc, dataPos, dataLength);
            }
        }

        private short SendReceive(IModbusInterface masterInterface, byte[] buffer, byte deviceAddress, ModbusFunctionCode fc, int timeout, short telegramLength, short desiredDataLength,
                                            object telegramContext, ref short dataPos) {
            lock (masterInterface) {
                masterInterface.SendTelegram(buffer, telegramLength);

                if (deviceAddress == ModbusConst.BroadcastAddress) {
                    return 0;
                }

                try {
                    masterInterface.PrepareRead();

                    byte responseFc = 0;
                    short dataLength = 0;

                    while (timeout > 0) {
                        var ts = DateTime.Now.Ticks;
                        if (!masterInterface.ReceiveTelegram(buffer, desiredDataLength, timeout, out telegramLength)) {
                            throw new ModbusException(ModbusErrorCode.Timeout);
                        }
                        timeout -= (int)((DateTime.Now.Ticks - ts) / 10000);

                        // if this is not the response we are waiting for wait again until time runs out
                        if (masterInterface.ParseTelegram(buffer, telegramLength, true, ref telegramContext,
                           out var responseDeviceAddress,
                           out responseFc,
                           out dataPos, out dataLength)
                            && responseDeviceAddress == deviceAddress
                            && (responseFc & 0x7f) == (byte)fc) {
                            break;
                        }
                        if (timeout <= 0) {
                            throw new ModbusException(ModbusErrorCode.Timeout);
                        }
                    }
                    if ((responseFc & 0x80) != 0) {
                        // error response
                        throw new ModbusException((ModbusErrorCode)buffer[dataPos]);
                    }
                    return dataLength;
                }
                finally {
                    masterInterface.PrepareWrite();
                }
            }
        }
    }
}
