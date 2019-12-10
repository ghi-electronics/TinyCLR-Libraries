using System;
using System.Collections;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Modbus.Interface;

namespace GHIElectronics.TinyCLR.Devices.Modbus {
    /// <summary>
    /// ModbusDevice is a base class for any modbus device.
    /// </summary>
    /// <remarks>
    /// Override the methods named like On"ModbusFunctionCode"() to implement this FunctionCode or OnCustomTelegram for any not directly supported function code.
    /// </remarks>
    public abstract class ModbusDevice {
        private readonly ArrayList interfaces = new ArrayList();
        private readonly byte deviceAddress;
        private readonly object syncObject;
        private byte[] buffer;
        private Thread thread;

        /// <summary>
        /// Gets the internal buffer
        /// </summary>
        protected byte[] Buffer => this.Buffer1;

        /// <summary>
        /// Creates a new ModebusDevice without assigned interfaces.
        /// </summary>
        /// <param name="deviceAddress">Device address. Must be between 1 and 247 for RTU and should be 248 for TCP.</param>
        /// <param name="syncObject">Optional object for communication interface synchronization.</param>
        /// <remarks>
        /// Interfaces can be add or removed by AddInterface and RemoveInterface methods.
        /// </remarks>
        protected ModbusDevice(byte deviceAddress, object syncObject = null) {
            this.deviceAddress = deviceAddress;
            this.syncObject = syncObject ?? new object();
        }

        /// <summary>
        /// Creates a new ModebusDevice with one initial interface.
        /// </summary>
        /// <param name="intf">Initial interface.</param>
        /// <param name="deviceAddress">Device address. Must be between 1 and 247 for RTU and should be 248 for TCP.</param>
        /// <param name="syncObject">Optional object for communication interface synchronization.</param>
        /// <remarks>
        /// More interfaces can be add or removed by AddInterface and RemoveInterface methods.
        /// </remarks>
        protected ModbusDevice(IModbusInterface intf, byte deviceAddress, object syncObject = null) :
           this(deviceAddress, syncObject) {
            intf.PrepareRead();
            this.interfaces.Add(intf);
            this.Buffer1 = new byte[intf.MaxTelegramLength];
        }

        /// <summary>
        /// Adds an additional interface to be polled for incoming messages.
        /// </summary>
        /// <param name="intf">Interface to add.</param>
        public void AddInterface(IModbusInterface intf) {
            lock (this.syncObject) {
                if (this.Buffer1 == null || intf.MaxTelegramLength > this.Buffer1.Length) {
                    this.Buffer1 = new byte[intf.MaxTelegramLength];
                }
                intf.PrepareRead();
                this.interfaces.Add(intf);
            }
        }

        /// <summary>
        /// Removes an interface.
        /// </summary>
        /// <param name="intf">Interface to remove.</param>
        public void RemoveInterface(IModbusInterface intf) {
            lock (this.syncObject) {
                this.interfaces.Remove(intf);
                if (this.interfaces.Count == 0) {
                    this.Buffer1 = null;
                }
            }
        }

        /// <summary>
        /// Start the interface message polling.
        /// </summary>
        public void Start() {
            if (!this.IsRunning) {
                this.thread = new Thread(this.Run);
                this.IsRunning = true;
                this.thread.Start();
            }
        }

        /// <summary>
        /// Stop the interface message polling.
        /// </summary>
        public void Stop() => this.IsRunning = false;

        /// <summary>
        /// Gets if the interface message polling is running.
        /// </summary>
        public bool IsRunning { get; private set; }
        public byte[] Buffer1 { get => this.Buffer2; set => this.Buffer2 = value; }
        public byte[] Buffer2 { get => this.buffer; set => this.buffer = value; }

        private void Run() {
            // ReSharper disable TooWideLocalVariableScope
            object telegramContext = null;

            while (this.IsRunning) {
                try {
                    lock (this.syncObject) {
                        for (var n = this.interfaces.Count - 1; n >= 0; --n) {
                            var intf = (IModbusInterface)this.interfaces[n];
                            try {
                                if (intf.IsDataAvailable) {
                                    if (intf.ReceiveTelegram(this.Buffer1, -1, 1000, out var telegramLength)) {
                                        try {
                                            intf.ParseTelegram(this.Buffer1, telegramLength, false, ref telegramContext,
                                               out var deviceAddress, out var fc,
                                               out var dataPos, out var dataLength);
                                            intf.PrepareWrite();
                                            this.OnMessageReeived(intf, deviceAddress, (ModbusFunctionCode)fc);
                                            var isBroadcast = deviceAddress == ModbusConst.BroadcastAddress;
                                            if (isBroadcast || this.deviceAddress == 248 || deviceAddress == this.deviceAddress) {
                                                this.OnHandleTelegram(intf, deviceAddress, isBroadcast, telegramLength, telegramContext,
                                       (ModbusFunctionCode)fc, dataPos,
                                       dataLength);
                                            }
                                        }
                                        catch {
                                            intf.ClearInputBuffer();
                                        }
                                        finally {
                                            intf.PrepareRead();
                                        }
                                    }
                                }
                            }
                            catch {
                                // ignored
                            }
                            if (!intf.IsConnectionOk) {
                                this.interfaces.RemoveAt(n);
                                if (intf is IDisposable disp) {
                                    try {
                                        disp.Dispose();
                                    }
                                    catch {
                                        // ignored
                                    }
                                }
                            }

                        }
                    }
                    Thread.Sleep(1);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            }
            // ReSharper restore TooWideLocalVariableScope
            this.thread = null;
        }

        /// <summary>
        /// Is called when ever a modbus message was received, no matter if it was for this device or not.
        /// </summary>
        /// <param name="modbusInterface">Interface by which the message was received</param>
        /// <param name="deviceAddress">Address to which device the message was sent</param>
        /// <param name="functionCode">Function code</param>
        protected virtual void OnMessageReeived(IModbusInterface modbusInterface, byte deviceAddress, ModbusFunctionCode functionCode) { }

        /// <summary>
        /// Handles a received message.
        /// </summary>
        /// <param name="intf">Interface by which the message was received.</param>
        /// <param name="deviceAddress">Address of the target device</param>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no response is sent.</param>
        /// <param name="telegramLength">Length of the message in bytes.</param>
        /// <param name="telegramContext">Interface specific message context.</param>
        /// <param name="fc">Function code.</param>
        /// <param name="dataPos">Index of function code specific data.</param>
        /// <param name="dataLength">Length of the function code specific data in bytes.</param>
        protected virtual void OnHandleTelegram(IModbusInterface intf, byte deviceAddress, bool isBroadcast, short telegramLength, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            try {
                switch (fc) {
                    case ModbusFunctionCode.ReadCoils:
                        this.ReadCoils(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.ReadDiscreteInputs:
                        this.ReadDiscreteInputs(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.ReadHoldingRegisters:
                        this.ReadHoldingRegisters(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.ReadInputRegisters:
                        this.ReadInputRegisters(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.WriteSingleCoil:
                        this.WriteSingleCoil(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.WriteSingleRegister:
                        this.WriteSingleRegister(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.WriteMultipleCoils:
                        this.WriteMultipleCoils(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.WriteMultipleRegisters:
                        this.WriteMultipleRegisters(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.ReadWriteMultipleRegisters:
                        this.ReadWriteMultipleRegisters(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    case ModbusFunctionCode.ReadDeviceIdentification:
                    case ModbusFunctionCode.ReadDeviceIdentification2:
                        this.ReadDeviceIdentification(intf, isBroadcast, telegramContext, fc, dataPos, dataLength);
                        break;

                    default:
                        if (!this.OnCustomTelegram(intf, isBroadcast, this.Buffer1, telegramLength, telegramContext, fc, dataPos, dataLength)) {
                            this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalFunction);
                        }
                        break;
                }
            }
            catch {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.ServerDeviceFailure);
            }
        }

        private void ReadCoils(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var coilCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                byte[] coils;
                if (coilCount % 8 == 0) {
                    coils = new byte[coilCount / 8];
                }
                else {
                    coils = new byte[coilCount / 8 + 1];
                    coils[coils.Length - 1] = 0;
                }
                var err = this.OnReadCoils(isBroadcast, startAddress, coilCount, coils);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(1 + coils.Length), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    this.Buffer1[dataPos] = (byte)(coils.Length);
                    Array.Copy(coils, 0, this.Buffer1, dataPos + 1, coils.Length);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }

        }

        /// <summary>
        /// Is called when a ReadCoils request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="coilCount">Number of coils to read: 1 .. 2000</param>
        /// <param name="coils">Byte array which must receive the coils. The coils are written as single bits into the array starting with coil 1 at the lsb.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnReadCoils(bool isBroadcast, ushort startAddress, ushort coilCount, byte[] coils) => ModbusErrorCode.IllegalFunction;

        private void ReadDiscreteInputs(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var inputCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                byte[] inputs;
                if (inputCount % 8 == 0) {
                    inputs = new byte[inputCount / 8];
                }
                else {
                    inputs = new byte[inputCount / 8 + 1];
                    inputs[inputs.Length - 1] = 0;
                }
                var err = this.OnReadDiscreteInputs(isBroadcast, startAddress, inputCount, inputs);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(1 + inputs.Length), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    this.Buffer1[dataPos] = (byte)(inputs.Length);
                    Array.Copy(inputs, 0, this.Buffer1, dataPos + 1, inputs.Length);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a ReadDiscreteInputs request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="inputCount">Number of inputs to read: 1 .. 2000</param>
        /// <param name="inputs">Byte array which must receive the inputs. The inputs are written as single bits into the array starting with input 1 at the lsb.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnReadDiscreteInputs(bool isBroadcast, ushort startAddress, ushort inputCount, byte[] inputs) => ModbusErrorCode.IllegalFunction;

        private void ReadHoldingRegisters(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var registerCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                var registers = new ushort[registerCount];

                var err = this.OnReadHoldingRegisters(isBroadcast, startAddress, registers);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(1 + 2 * registers.Length), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    this.Buffer1[dataPos] = (byte)(2 * registers.Length);
                    for (var i = 0; i < registerCount; i++) {
                        ModbusUtils.InsertUShort(this.Buffer1, dataPos + 1 + 2 * i, registers[i]);
                    }
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a ReadHoldingRegisters request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="registers">Array in which the read register values must be written.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnReadHoldingRegisters(bool isBroadcast, ushort startAddress, ushort[] registers) => ModbusErrorCode.IllegalFunction;

        private void ReadInputRegisters(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var registerCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                var registers = new ushort[registerCount];

                var err = this.OnReadInputRegisters(isBroadcast, startAddress, registers);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(1 + 2 * registers.Length), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    this.Buffer1[dataPos] = (byte)(2 * registers.Length);
                    for (var i = 0; i < registerCount; i++) {
                        ModbusUtils.InsertUShort(this.Buffer1, dataPos + 1 + 2 * i, registers[i]);
                    }
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a ReadInputRegisters request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="registers">Array in which the read register values must be written.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnReadInputRegisters(bool isBroadcast, ushort startAddress, ushort[] registers) => ModbusErrorCode.IllegalFunction;

        private void WriteSingleCoil(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var address = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var value = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);

                var err = this.OnWriteSingleCoil(isBroadcast, address, value != 0);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, 4, this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos, address);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos + 2, value);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a WriteSingleCoil request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="address">Address of the coil: 0x0000 .. 0xFFFF</param>
        /// <param name="value">Value to write.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnWriteSingleCoil(bool isBroadcast, ushort address, bool value) => ModbusErrorCode.IllegalFunction;

        private void WriteSingleRegister(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 4) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var address = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var value = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);

                var err = this.OnWriteSingleRegister(isBroadcast, address, value);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, 4, this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos, address);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos + 2, value);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a WriteSingleRegister request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="address">Address of the register: 0x0000 .. 0xFFFF</param>
        /// <param name="value">Register value to write</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnWriteSingleRegister(bool isBroadcast, ushort address, ushort value) => ModbusErrorCode.IllegalFunction;

        private void WriteMultipleCoils(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 5) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var outputCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                var byteCount = this.Buffer1[dataPos + 4];
                var values = new byte[byteCount];
                Array.Copy(this.Buffer1, dataPos + 5, values, 0, values.Length);

                var err = this.OnWriteMultipleCoils(isBroadcast, startAddress, outputCount, values);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, 4, this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos, startAddress);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos + 2, outputCount);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a WriteMultipleCoils request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="outputCount">Number of couils (bist) to write.</param>
        /// <param name="values">Values to write. Each bit is a value, starting at the lsb.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnWriteMultipleCoils(bool isBroadcast, ushort startAddress, ushort outputCount, byte[] values) => ModbusErrorCode.IllegalFunction;

        private void WriteMultipleRegisters(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 5) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var startAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var registerCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                //byte byteCount = _Buffer[dataPos + 4];
                var registers = new ushort[registerCount];
                for (var i = 0; i < registerCount; i++) {
                    registers[i] = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 5 + 2 * i);
                }

                var err = this.OnWriteMultipleRegisters(isBroadcast, startAddress, registers);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, 4, this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos, startAddress);
                    ModbusUtils.InsertUShort(this.Buffer1, dataPos + 2, registerCount);
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a WriteMultipleRegisters request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="startAddress">Start address: 0x0000 .. 0xFFFF</param>
        /// <param name="registers">Registers to write</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnWriteMultipleRegisters(bool isBroadcast, ushort startAddress, ushort[] registers) => ModbusErrorCode.IllegalFunction;

        private void ReadWriteMultipleRegisters(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (dataLength < 9) {
                this.SendErrorResult(intf, isBroadcast, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var readStartAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos);
                var readCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 2);
                var writeStartAddress = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 4);
                var writeCount = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 6);
                //byte byteCount = _Buffer[dataPos + 8];
                var writeRegisters = new ushort[writeCount];
                for (var i = 0; i < writeCount; i++) {
                    writeRegisters[i] = ModbusUtils.ExtractUShort(this.Buffer1, dataPos + 5 + 2 * i);
                }
                var readRegisters = new ushort[readCount];

                var err = this.OnReadWriteMultipleRegisters(isBroadcast, writeStartAddress, writeRegisters, readStartAddress, readRegisters);
                if (isBroadcast) {
                    return;
                }
                if (err != ModbusErrorCode.NoError) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, err);
                }
                else {
                    intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(1 + 2 * readRegisters.Length), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                    this.Buffer1[dataPos] = (byte)(2 * readRegisters.Length);
                    for (var i = 0; i < readCount; i++) {
                        ModbusUtils.InsertUShort(this.Buffer1, dataPos + 1 + 2 * i, readRegisters[i]);
                    }
                    intf.SendTelegram(this.Buffer1, telegramLength);
                }
            }
        }

        /// <summary>
        /// Is called when a ReadWriteMultipleRegisters request comes in.
        /// </summary>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="writeStartAddress">Start address of writeRegisters: 0x0000 .. 0xFFFF</param>
        /// <param name="writeRegisters">Registers to write</param>
        /// <param name="readStartAddress">Start address readRegisters: 0x0000 .. 0xFFFF</param>
        /// <param name="readRegisters">Array to write the read registers into.</param>
        /// <returns>Returns <see cref="ModbusErrorCode.NoError"/> on success or any other <see cref="ModbusErrorCode"/> on errors.</returns>
        /// <remarks>
        /// The write operation is performed before the read operation.
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details.
        /// </remarks>
        protected virtual ModbusErrorCode OnReadWriteMultipleRegisters(bool isBroadcast, ushort writeStartAddress, ushort[] writeRegisters, ushort readStartAddress, ushort[] readRegisters) => ModbusErrorCode.IllegalFunction;

        /// <summary>
        /// Is called when the device identification of this devuice is requested.
        /// </summary>
        /// <param name="intf">Interface from wich the requst was received</param>
        /// <param name="isBroadcast">true if request is a broadcast</param>
        /// <param name="telegramContext">Conext of the telegram</param>
        /// <param name="fc">Function code</param>
        /// <param name="dataPos">Posittion (offset) of the data in the buffer</param>
        /// <param name="dataLength">Length of the data in the buffer</param>
        protected virtual void ReadDeviceIdentification(IModbusInterface intf, bool isBroadcast, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) {
            if (isBroadcast) {
                return;
            }
            if (dataLength < 3) {
                this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
            }
            else {
                var deviceIdCode = this.Buffer1[dataPos + 1];
                if (deviceIdCode < 1 || deviceIdCode > 4) {
                    this.SendErrorResult(intf, false, this.deviceAddress, telegramContext, fc, ModbusErrorCode.IllegalDataValue);
                    return;
                }
                var objectId = this.Buffer1[dataPos + 2];
                byte lastObjectId;
                switch (deviceIdCode) {
                    case 0x00:
                        lastObjectId = 0x02; // basic
                        break;

                    case 0x01:
                        lastObjectId = 0x7f; // regular
                        break;

                    case 0x02:
                        lastObjectId = 0xff; // extended
                        break;

                    default:
                        lastObjectId = objectId; // specific
                        break;
                }

                var values = new byte[intf.MaxTelegramLength - 6];
                byte objectCount = 0;
                short valuePos = 0;
                var moreFolows = false;
                byte nextId = 0;
                for (short id = objectId; id <= lastObjectId; ++id) {
                    var value = this.OnGetDeviceIdentification((ModbusObjectId)id);
                    if (value == null) {
                        // no more values
                        break;
                    }
                    if (values.Length - (valuePos + 2) >= value.Length) {
                        ++objectCount;
                        values[valuePos++] = (byte)id;
                        values[valuePos++] = (byte)value.Length;
                        for (var c = 0; c < value.Length; c++) {
                            values[valuePos++] = (byte)value[c];
                        }
                    }
                    else {
                        // more to come
                        moreFolows = true;
                        nextId = (byte)(id + 1);
                        break;
                    }
                }

                intf.CreateTelegram(this.deviceAddress, (byte)fc, (short)(6 + valuePos), this.Buffer1, out var telegramLength, out dataPos, true, ref telegramContext);
                this.Buffer1[dataPos] = 0x0e;
                this.Buffer1[dataPos + 1] = deviceIdCode;
                this.Buffer1[dataPos + 2] = (byte)((byte)this.GetConformityLevel() & 0x80);
                this.Buffer1[dataPos + 3] = (byte)(moreFolows ? 0xff : 0x00);
                this.Buffer1[dataPos + 4] = nextId;
                this.Buffer1[dataPos + 5] = objectCount;
                Array.Copy(values, 0, this.Buffer1, dataPos + 6, valuePos);
                intf.SendTelegram(this.Buffer1, telegramLength);
            }
        }

        /// <summary>
        /// Is called when the value of the object id is needed for a ReadDeviceIdentification request.
        /// </summary>
        /// <param name="objectId">Object id.</param>
        /// <returns>Returns the value of the requested object id.</returns>
        protected abstract string OnGetDeviceIdentification(ModbusObjectId objectId);

        /// <summary>
        /// Is called when the maximum conformity level is needed for a ReadDeviceIdentification request.
        /// </summary>
        /// <returns>Returns the maximum conformity level which is provided by this device.</returns>
        protected abstract ModbusConformityLevel GetConformityLevel();

        /// <summary>
        /// OnCustomTelegram is called for any function code which is not explicitly handeled by a On"FunctionCode" methos.
        /// </summary>
        /// <param name="intf">Interface which sent the request.</param>
        /// <param name="isBroadcast">true if the request is a broadcast</param>
        /// <param name="buffer">Buffer containing the message.</param>
        /// <param name="telegramLength">Total length of message in bytes.</param>
        /// <param name="telegramContext">Interface specific message context.</param>
        /// <param name="fc">Function code.</param>
        /// <param name="dataPos">Index of the function code specific data.</param>
        /// <param name="dataLength">Length of the function code specific data in bytes.</param>
        /// <returns></returns>
        /// <remarks>
        /// Look at http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf for more details about function codes.
        /// </remarks>
        protected virtual bool OnCustomTelegram(IModbusInterface intf, bool isBroadcast, byte[] buffer, short telegramLength, object telegramContext, ModbusFunctionCode fc, short dataPos, short dataLength) => false;

        /// <summary>
        /// Sends a error result message.
        /// </summary>
        /// <param name="intf">Interface to send message to.</param>
        /// <param name="isBroadcast">true if the message is a broadcast. For broadcast messages no reponse is sent.</param>
        /// <param name="deviceAddress">Device address for response</param>
        /// <param name="telegramContext">Interface specific telegram context.</param>
        /// <param name="fc">Function code. The msg is automatically set.</param>
        /// <param name="modbusErrorCode">Modbus error code to send.</param>
        protected virtual void SendErrorResult(IModbusInterface intf, bool isBroadcast, byte deviceAddress, object telegramContext, ModbusFunctionCode fc, ModbusErrorCode modbusErrorCode) {
            if (isBroadcast) {
                return;
            }
            intf.CreateTelegram(deviceAddress, (byte)((byte)fc | 0x80), 1, this.Buffer1, out var telegramLength, out var dataPos, true, ref telegramContext);
            this.Buffer1[dataPos] = (byte)modbusErrorCode;
            intf.SendTelegram(this.Buffer1, telegramLength);
        }
    }
}
