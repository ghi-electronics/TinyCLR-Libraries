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
        Joystick = 5
    }

    public delegate void DataReceivedEventHandler(RawDevice sender, uint count);
    public delegate void DeviceStateChangedEventHandler(RawDevice sender, DeviceState state);

    public sealed class UsbClientSetting {
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

    public sealed class UsbClientController : IDisposable {        
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
            void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor,  uint index);
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

        public sealed class UsbClientControllerApiWrapper : IUsbClientControllerProvider {
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
                this.dataReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.dataReceivedCallbacks?.Invoke(null, (uint)d0); };

                this.deviceStateChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbClient.DeviceStateChanged");
                this.deviceStateChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.deviceStateChangedCallbacks?.Invoke(null, (DeviceState)d0); };

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

            public void Dispose() => this.Release();

            public int BytesToRead(int streamIndex) => this.GetByteToRead(streamIndex);
            public int BytesToWrite(int streamIndex) => this.GetByteToWrite(streamIndex);

            public DeviceState DeviceState => this.GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSetting(UsbClientSetting setting);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(int streamIndex, byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(int streamIndex, byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(int streamIndex);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer(int streamIndex);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer(int streamIndex);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern DeviceState GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int GetByteToRead(int streamIndex);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int GetByteToWrite(int streamIndex);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataStateChangedEventEnabled(bool enabled);

            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int GetControlPacketSize();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern ushort GetEndpointMap();

            [MethodImpl(MethodImplOptions.InternalCall)]
            internal static extern void InitializeStream(byte[] streamMap, uint[] interfaceMap);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDeviceDescriptor(Configuration.DeviceDescriptor[] deviceDescriptor);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetConfigurationDescriptor(Configuration.ConfigurationDescriptor[] configurationDescriptor);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor, uint index);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetGenericDescriptor(Configuration.GenericDescriptor[] genericDescriptor);

        }
    }
}
