using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbHost.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    public enum UsbHostDeviceState {
        Detached = 0,
        Attached = 1,
    };

    //public enum UsbHostMode {
    //    Cdc = 0,
    //    WinUsb = 1
    //}

    public delegate void DataReceivedEventHandler(UsbHostController sender, uint count);
    public delegate void DeviceStateChangedEventHandler(UsbHostController sender, UsbHostDeviceState state);

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
        private void OnDeviceStateChanged(UsbHostController sender, UsbHostDeviceState state) => this.deviceStateChangedCallbacks?.Invoke(this, state);


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
                this.deviceStateChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.deviceStateChangedCallbacks?.Invoke(null, (UsbHostDeviceState)d0); };

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
