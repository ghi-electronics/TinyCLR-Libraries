using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbHost.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    public enum UsbHostDeviceState {
        Disconnected = 0,
        Connected = 1,
        Bad = 2,
    };


    public delegate void DeviceStateChangedEventHandler(UsbHostController sender, DeviceConnectedEventArgs e);

    public class DeviceConnectedEventArgs : EventArgs {
        private uint id;
        private byte interfaceIndex;
        private BaseDevice.DeviceType type;
        private ushort vendorId;
        private ushort productId;
        private byte portNumber;

        /// <summary>The device id.</summary>
        public uint Id => this.id;

        /// <summary>The logical device interface index.</summary>
        public byte InterfaceIndex => this.interfaceIndex;

        /// <summary>The device's type.</summary>
        public BaseDevice.DeviceType Type => this.type;

        /// <summary>The devic's vendor id.</summary>
        public ushort VendorId => this.vendorId;

        /// <summary>The device's product id.</summary>
        public ushort ProductId => this.productId;

        /// <summary>The device's USB port number.</summary>
        public byte PortNumber => this.portNumber;

        internal DeviceConnectedEventArgs(uint id, byte interfaceIndex, BaseDevice.DeviceType type, ushort vendorId, ushort productId, byte portNumber) {
            this.id = id;
            this.interfaceIndex = interfaceIndex;
            this.type = type;
            this.vendorId = vendorId;
            this.productId = productId;
            this.portNumber = portNumber;
        }
    }

    public sealed class UsbHostController : IDisposable {
        private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

        public IUsbHostControllerProvider Provider { get; }

        private UsbHostController(IUsbHostControllerProvider provider) => this.Provider = provider;

        public static UsbHostController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbHostController) is UsbHostController c ? c : UsbHostController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbHostController));
        public static UsbHostController FromName(string name) => UsbHostController.FromProvider(new UsbHostControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbHostController)));
        public static UsbHostController FromProvider(IUsbHostControllerProvider provider) => new UsbHostController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public void SendSetupPacket(byte requestType, byte request, ushort value, ushort index, byte[] data, int dataOffset, int dataCount) => this.Provider.SendSetupPacket(requestType, request, value, index, data, dataOffset, dataCount);

        public void GetParameters(uint id, out ushort vendorId, out ushort productId, out byte port) => this.Provider.GetParameters(id, out vendorId, out productId, out port);

        public int Transfer(byte[] buffer, int offset, int count, int transferTimeout) => this.Provider.Transfer(buffer, offset, count, transferTimeout);

        public UsbHostDeviceState DeviceState => this.Provider.DeviceState;
        private void OnDeviceStateChanged(UsbHostController sender, DeviceConnectedEventArgs e) => this.deviceStateChangedCallbacks?.Invoke(this, e);


        public event DeviceStateChangedEventHandler DeviceStateChanged {
            add {
                if (this.deviceStateChangedCallbacks == null)
                    this.Provider.DeviceStateChanged += this.OnDeviceStateChanged;

                this.deviceStateChangedCallbacks += value;
            }
            remove {
                this.deviceStateChangedCallbacks -= value;

                if (this.deviceStateChangedCallbacks == null)
                    this.Provider.DeviceStateChanged -= this.OnDeviceStateChanged;
            }
        }

    }

    namespace Provider {
        public interface IUsbHostControllerProvider : IDisposable {

            UsbHostDeviceState DeviceState { get; }

            void Enable();
            void Disable();

            void SendSetupPacket(byte requestType, byte request, ushort value, ushort index, byte[] data, int dataOffset, int dataCount);

            void GetParameters(uint id, out ushort vendorId, out ushort productId, out byte port);

            int Transfer(byte[] buffer, int offset, int count, int transferTimeout);

            event DeviceStateChangedEventHandler DeviceStateChanged;
        }

        public sealed class UsbHostControllerApiWrapper : IUsbHostControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher deviceStateChangedDispatcher;

            private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

            public NativeApi Api { get; }

            public UsbHostControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.deviceStateChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbHost.DeviceStateChanged");
                this.deviceStateChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) {

                        var connection = (UsbHostDeviceState)d2;

                        if (connection == UsbHostDeviceState.Connected) {
                            var interfaceIndex = (uint)d0;
                            var deviceType = (BaseDevice.DeviceType)(d0 >> 8);

                            this.GetParameters(interfaceIndex, out var vendor, out var product, out var port);

                            //var deviceType = BaseDevice.DeviceType.Unknown;

                            //if (d1 == (uint)(UsbHostDeviceClass.Hid))
                            //    deviceType = (BaseDevice.DeviceType.HID);

                            //if (d1 == (uint)(UsbHostDeviceClass.MassStorage))
                            //    deviceType = (BaseDevice.DeviceType.MassStorage);

                            var deviceConnectedEventArgs = new DeviceConnectedEventArgs(interfaceIndex, (byte)interfaceIndex, deviceType, vendor, product, port);

                            this.deviceStateChangedCallbacks?.Invoke(null, deviceConnectedEventArgs);
                        }
                        else {
                            var deviceConnectedEventArgs = new DeviceConnectedEventArgs(0, 0, 0, 0, 0, 0);

                            this.deviceStateChangedCallbacks?.Invoke(null, deviceConnectedEventArgs);
                        }
                    }
                };


            }

            public event DeviceStateChangedEventHandler DeviceStateChanged {
                add {
                    if (this.deviceStateChangedCallbacks == null)
                        this.SetDataStateChangedEventEnabled(true);

                    this.deviceStateChangedCallbacks += value;
                }
                remove {
                    this.deviceStateChangedCallbacks -= value;

                    if (this.deviceStateChangedCallbacks == null)
                        this.SetDataStateChangedEventEnabled(false);
                }
            }

            public void Dispose() => this.Release();

            public UsbHostDeviceState DeviceState => this.GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SendSetupPacket(byte requestType, byte request, ushort value, ushort index, byte[] data, int dataOffset, int dataCount);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetParameters(uint id, out ushort vendorId, out ushort productId, out byte port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Transfer(byte[] buffer, int offset, int count, int transferTimeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern UsbHostDeviceState GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataStateChangedEventEnabled(bool enabled);

        }
    }
}
