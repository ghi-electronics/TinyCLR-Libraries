using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.UsbClient.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>The possible states of a USB device.</summary>
    public enum DeviceState {
        /// <summary>The device is detached from the host.</summary>
        Detached = 0,
        /// <summary>The device is attached to the host.</summary>
        Attached = 1,
        /// <summary>The device is powered.</summary>
        Powered = 2,
        /// <summary>The device is in the default state.</summary>
        Default = 3,
        /// <summary>The device has been assigned an address.</summary>
        Address = 4,
        /// <summary>The device is configured and ready for use.</summary>
        Configured = 5,
        /// <summary>The device is suspended.</summary>
        Suspended = 6,
    };

    /// <summary>The type of device to emulate.</summary>
    public enum UsbClientMode {
        /// <summary>A raw device.</summary>
        RawDevice = 0,
        /// <summary>A CDC virtual COM port.</summary>
        Cdc = 1,
        /// <summary>A WinUsb device.</summary>
        WinUsb = 2,
        /// <summary>A keyboard.</summary>
        Keyboard = 3,
        /// <summary>A mouse.</summary>
        Mouse = 4,
        /// <summary>A joystick.</summary>
        Joystick = 5,
        /// <summary>A mass storage device.</summary>
        MassStorage = 6
    }

    /// <summary>Represents the method that handles a data received event.</summary>
    public delegate void DataReceivedEventHandler(RawDevice sender, uint count);
    /// <summary>Represents the method that handles a device state changed event.</summary>
    public delegate void DeviceStateChangedEventHandler(RawDevice sender, DeviceState state);

    /// <summary>The settings used to configure a USB client device.</summary>
    public class UsbClientSetting {
        /// <summary>The type of device to emulate.</summary>
        public UsbClientMode Mode { get; set; }

        /// <summary>The manufacturer name.</summary>
        public string ManufactureName { get; set; }
        /// <summary>The product name.</summary>
        public string ProductName { get; set; }
        /// <summary>The serial number.</summary>
        public string SerialNumber { get; set; }
        /// <summary>The interface GUID, used by WinUsb devices.</summary>
        public string Guid { get; set; }

        /// <summary>The product id.</summary>
        public ushort ProductId { get; set; }
        /// <summary>The vendor id.</summary>
        public ushort VendorId { get; set; } = RawDevice.GHI_VID;

        /// <summary>The USB specification release number.</summary>
        public ushort BcdUsb { get; set; } = 0x210;
        /// <summary>The device release number.</summary>
        public ushort BcdDevice { get; set; }
        /// <summary>The maximum power the device uses, in 2 mA units.</summary>
        public ushort MaxPower { get; set; }
        /// <summary>The interface name.</summary>
        public string InterfaceName { get; set; }
    }

    /// <summary>Provides access to a USB client controller.</summary>
    public class UsbClientController : IDisposable {
        /// <summary>The underlying provider for the controller.</summary>
        public IUsbClientControllerProvider Provider { get; }

        private UsbClientController(IUsbClientControllerProvider provider) => this.Provider = provider;

        /// <summary>Gets the default USB client controller.</summary>
        public static UsbClientController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbClientController) is UsbClientController c ? c : UsbClientController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbClientController));
        /// <summary>Gets the USB client controller with the given name.</summary>
        public static UsbClientController FromName(string name) => UsbClientController.FromProvider(new UsbClientControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbClientController)));
        /// <summary>Gets a USB client controller from the given provider.</summary>
        public static UsbClientController FromProvider(IUsbClientControllerProvider provider) => new UsbClientController(provider);

        /// <summary>Disposes the controller.</summary>
        public void Dispose() => this.Provider.Dispose();
    }

    namespace Provider {
        /// <summary>Provides the low-level interface for a USB client controller.</summary>
        public interface IUsbClientControllerProvider : IDisposable {
            /// <summary>The number of bytes available to read on the given stream.</summary>
            int BytesToRead(int streamIndex);
            /// <summary>The number of bytes that are in the process of being written on the given stream.</summary>
            int BytesToWrite(int streamIndex);

            /// <summary>The size of the write buffer.</summary>
            int WriteBufferSize { get; set; }
            /// <summary>The size of the read buffer.</summary>
            int ReadBufferSize { get; set; }

            /// <summary>The current state of the device.</summary>
            DeviceState DeviceState { get; }

            /// <summary>Enables the device.</summary>
            void Enable();
            /// <summary>Disables the device.</summary>
            void Disable();

            /// <summary>Sets the active device setting.</summary>
            void SetActiveSetting(UsbClientSetting setting);
            /// <summary>Sets the device descriptor.</summary>
            void SetDeviceDescriptor(Configuration.DeviceDescriptor[] deviceDescriptor);
            /// <summary>Sets the configuration descriptor.</summary>
            void SetConfigurationDescriptor(Configuration.ConfigurationDescriptor[] configurationDescriptor);
            /// <summary>Sets the string descriptor at the given index.</summary>
            void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor, uint index);
            /// <summary>Sets the generic descriptor.</summary>
            void SetGenericDescriptor(Configuration.GenericDescriptor[] genericDescriptor);

            /// <summary>Reads data from the given stream.</summary>
            int Read(int streamIndex, byte[] data, int offset, int count);
            /// <summary>Writes data to the given stream.</summary>
            int Write(int streamIndex, byte[] data, int offset, int count);

            /// <summary>Gets the maximum control packet size.</summary>
            int GetControlPacketSize();
            /// <summary>Gets a bitmap of the reserved endpoints.</summary>
            ushort GetEndpointMap();

            /// <summary>Clears the read buffer of the given stream.</summary>
            void ClearReadBuffer(int streamIndex);
            /// <summary>Clears the write buffer of the given stream.</summary>
            void ClearWriteBuffer(int streamIndex);

            /// <summary>Flushes the write buffer of the given stream.</summary>
            void Flush(int streamIndex);


            /// <summary>Raised when data is received from the host.</summary>
            event DataReceivedEventHandler DataReceived;
            /// <summary>Raised when the device state changes.</summary>
            event DeviceStateChangedEventHandler DeviceStateChanged;
        }

        /// <summary>The native implementation of a USB client controller provider.</summary>
        public class UsbClientControllerApiWrapper : IUsbClientControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher dataReceivedDispatcher;
            private readonly NativeEventDispatcher deviceStateChangedDispatcher;

            private DataReceivedEventHandler dataReceivedCallbacks;
            private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

            /// <summary>The native API backing this provider.</summary>
            public NativeApi Api { get; }

            /// <summary>Creates a new provider for the given native API.</summary>
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

            /// <summary>Raised when data is received from the host.</summary>
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

            /// <summary>Raised when the device state changes.</summary>
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

            /// <summary>Disposes the provider.</summary>
            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>Disposes the provider.</summary>
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

            /// <inheritdoc/>
            public int BytesToRead(int streamIndex) => this.GetByteToRead(streamIndex);
            /// <inheritdoc/>
            public int BytesToWrite(int streamIndex) => this.GetByteToWrite(streamIndex);

            /// <inheritdoc/>
            public DeviceState DeviceState => this.GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSetting(UsbClientSetting setting);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(int streamIndex, byte[] data, int offset, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(int streamIndex, byte[] data, int offset, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(int streamIndex);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer(int streamIndex);

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            /// <inheritdoc/>
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int GetControlPacketSize();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern ushort GetEndpointMap();

            [MethodImpl(MethodImplOptions.InternalCall)]
            internal static extern void InitializeStream(byte[] streamMap, uint[] interfaceMap);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDeviceDescriptor(Configuration.DeviceDescriptor[] deviceDescriptor);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetConfigurationDescriptor(Configuration.ConfigurationDescriptor[] configurationDescriptor);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetStringDescriptor(Configuration.StringDescriptor[] stringDescriptor, uint index);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetGenericDescriptor(Configuration.GenericDescriptor[] genericDescriptor);

        }
    }
}
