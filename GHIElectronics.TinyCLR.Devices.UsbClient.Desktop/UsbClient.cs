using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.UsbClient.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    public enum DeviceState {
        Detached = 0,
        Attached = 1,
        Powered = 2,
        Default = 3,
        Address = 4,
        Configured = 5,
        Suspended = 6,
    };

    public enum UsbClientMode {
        RawDevice = 0,
        Cdc = 1,
        WinUsb = 2,
        Keyboard = 3,
        Mouse = 4,
        Joystick = 5,
        MassStorage = 6
    }

    public delegate void DataReceivedEventHandler(RawDevice sender, uint count);
    public delegate void DeviceStateChangedEventHandler(RawDevice sender, DeviceState state);

    public class UsbClientSetting {
        public UsbClientMode Mode { get; set; }

        public string ManufactureName { get; set; }
        public string ProductName { get; set; }
        public string SerialNumber { get; set; }
        public string Guid { get; set; }

        public ushort ProductId { get; set; }
        public ushort VendorId { get; set; } = RawDevice.GHI_VID;

        public ushort BcdUsb { get; set; } = 0x210;
        public ushort BcdDevice { get; set; }
        public ushort MaxPower { get; set; }
        public string InterfaceName { get; set; }
    }

    public class UsbClientController : IDisposable {
        public IUsbClientControllerProvider Provider { get; }

        private UsbClientController(IUsbClientControllerProvider provider) => this.Provider = provider;

        public static UsbClientController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbClientController) is UsbClientController c ? c : UsbClientController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbClientController));
        public static UsbClientController FromName(string name) => UsbClientController.FromProvider(new UsbClientControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbClientController)));
        public static UsbClientController FromProvider(IUsbClientControllerProvider provider) => new UsbClientController(provider);

        public void Dispose() => this.Provider.Dispose();
    }

    namespace Provider {
        public interface IUsbClientControllerProvider : IDisposable {
            int BytesToRead(int streamIndex);
            int BytesToWrite(int streamIndex);

            int WriteBufferSize { get; set; }
            int ReadBufferSize { get; set; }

            DeviceState DeviceState { get; }

            void Enable();
            void Disable();

            void SetActiveSetting(UsbClientSetting setting);
            void SetDeviceDescriptor(Configuration.DeviceDescriptor[] deviceDescriptor);
            void SetConfigurationDescriptor(Configuration.ConfigurationDescriptor[] configurationDescriptor);
            void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor, uint index);
            void SetGenericDescriptor(Configuration.GenericDescriptor[] genericDescriptor);

            int Read(int streamIndex, byte[] data, int offset, int count);
            int Write(int streamIndex, byte[] data, int offset, int count);

            int GetControlPacketSize();
            ushort GetEndpointMap();

            void ClearReadBuffer(int streamIndex);
            void ClearWriteBuffer(int streamIndex);

            void Flush(int streamIndex);


            event DataReceivedEventHandler DataReceived;
            event DeviceStateChangedEventHandler DeviceStateChanged;
        }

        public class UsbClientControllerApiWrapper : IUsbClientControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher dataReceivedDispatcher;
            private readonly NativeEventDispatcher deviceStateChangedDispatcher;

            private DataReceivedEventHandler dataReceivedCallbacks;
            private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

            public NativeApi Api { get; }

            public UsbClientControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.dataReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbClient.DataReceived");
                this.dataReceivedDispatcher.OnInterrupt += this.OnDataReceivedEventHandler;

                this.deviceStateChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbClient.DeviceStateChanged");
                this.deviceStateChangedDispatcher.OnInterrupt += this.OnDeviceStateChangedEventHandler;

            }

            void OnDataReceivedEventHandler(string apiName, long d0, long d1, long d2, IntPtr d3, DateTime ts) {
                if (this.Api.Name == apiName)
                    this.dataReceivedCallbacks?.Invoke(null, (uint)d0);
            }

            void OnDeviceStateChangedEventHandler(string apiName, long d0, long d1, long d2, IntPtr d3, DateTime ts) {
                if (this.Api.Name == apiName)
                    this.deviceStateChangedCallbacks?.Invoke(null, (DeviceState)d0);
            }

            public event DataReceivedEventHandler DataReceived {
                add {
                    if (this.dataReceivedCallbacks == null)
                        this.SetDataReceivedEventEnabled(true);

                    this.dataReceivedCallbacks += value;
                }
                remove {
                    this.dataReceivedCallbacks -= value;

                    if (this.dataReceivedCallbacks == null)
                        this.SetDataReceivedEventEnabled(false);
                }
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

            private bool disposed = false;

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) {
                if (!this.disposed) {

                    this.dataReceivedDispatcher.OnInterrupt -= this.OnDataReceivedEventHandler; ;
                    this.deviceStateChangedDispatcher.OnInterrupt -= this.OnDeviceStateChangedEventHandler;
                    this.Release();

                    this.disposed = true;
                }
            }

            ~UsbClientControllerApiWrapper() {
                this.Dispose(false);
            }

            public int BytesToRead(int streamIndex) => this.GetByteToRead(streamIndex);
            public int BytesToWrite(int streamIndex) => this.GetByteToWrite(streamIndex);

            public DeviceState DeviceState => this.GetDeviceState();

            // Desktop: USB hardware doesn't exist. All methods are safe no-ops so
            // app code can boot and exercise non-USB paths without crashing.
            // Reads return 0 (no bytes available), writes accept all data silently,
            // state queries return Detached, events never fire.
            // Apps that need real USB testing should run on the device.
            private void Acquire() { }
            private void Release() { }
            public void Enable() { }
            public void Disable() { }
            public void SetActiveSetting(UsbClientSetting setting) { }
            public int Read(int streamIndex, byte[] data, int offset, int count) => 0;
            public int Write(int streamIndex, byte[] data, int offset, int count) => count;
            public void Flush(int streamIndex) { }
            public void ClearReadBuffer(int streamIndex) { }
            public void ClearWriteBuffer(int streamIndex) { }
            private DeviceState GetDeviceState() => DeviceState.Detached;
            private int GetByteToRead(int streamIndex) => 0;
            private int GetByteToWrite(int streamIndex) => 0;
            private void SetDataReceivedEventEnabled(bool enabled) { }
            private void SetDataStateChangedEventEnabled(bool enabled) { }
            public int WriteBufferSize { get; set; }
            public int ReadBufferSize { get; set; }
            public int GetControlPacketSize() => 64;
            public ushort GetEndpointMap() => 0;
            internal static void InitializeStream(byte[] streamMap, uint[] interfaceMap) { }
            public void SetDeviceDescriptor(Configuration.DeviceDescriptor[] deviceDescriptor) { }
            public void SetConfigurationDescriptor(Configuration.ConfigurationDescriptor[] configurationDescriptor) { }
            public void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor, uint index) { }
            public void SetGenericDescriptor(Configuration.GenericDescriptor[] genericDescriptor) { }
        }
    }
}
