using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbClient.Provider;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>Represent a USB device.</summary>
	public class RawDevice {
        public const ushort MAX_POWER = 250;
        public const ushort GHI_VID = 0x1B9F;
        private const int MAX_INTERFACE_COUNT = 10;
        private const byte MANUFACTURER_STRING_INDEX = 1;
        private const byte PRODUCT_STRING_INDEX = 2;
        private const byte SERIAL_NUMBER_STRING_INDEX = 3;
        private const byte START_STRING_INDEX = 4;
        private byte nextStringIndex;
        private byte currentInterfaceIndex;

        private bool[] streamReservations;
        private bool[] endpointReservations;

        private byte[] streamMap;
        private uint[] interfaceMap;

        private ArrayList descriptors;
        private ArrayList interfaceDescriptors;
        private ArrayList stringDescriptors;
        private Configuration configuration;

        private ushort vendorId;
        private ushort productId;
        private ushort maximumPower;
        private ushort bcdUsb;
        private ushort bcdDevice;
        private object manufacturer;
        private object product;
        private object serialNumber;

        /// <summary>The maximum number of endpoints.</summary>
        public static int MaxEndpoint => 16;

        /// <summary>The maximum number of streams.</summary>
        public static int MaxStreams => 16;

        /// <summary>The vendor id of the device.</summary>
        public ushort VendorId => this.vendorId;

        /// <summary>The product id of the device.</summary>
        public ushort ProductId => this.productId;

        /// <summary>The version of the device.</summary>
        public ushort BcdUsb => this.bcdUsb;

        /// <summary>The version of the device.</summary>
        public ushort BcdDevice => this.bcdDevice;

        /// <summary>The maximum power the device might need.</summary>
        public ushort MaximumPower => this.maximumPower;

        /// <summary>The manufacturer of the device.</summary>
        public string Manufacturer => (string)this.manufacturer;

        /// <summary>The product name.</summary>
        public string Product => (string)this.product;

        /// <summary>The serial number of the device.</summary>
        public string SerialNumber => (string)this.serialNumber;

        public enum PID : ushort {
            Mouse = 0xF000,
            CDC = 0xF001,
            MassStorage = 0xF002,
            Keyboard = 0xF004,
            Joystick = 0xF005,
            WinUsb = 0xF006,
            RawDevice = 0xF007,
        }

        internal UsbClientController usbClientController;
        internal UsbClientSetting usbClientSetting;

        /// <summary>Creates a new raw device.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>        
        public RawDevice(UsbClientController usbClientController, UsbClientSetting usbClientSetting) {
            // Check setting
            this.vendorId = usbClientSetting.VendorId == 0 ? GHI_VID : usbClientSetting.VendorId;

            if (this.vendorId == GHI_VID) {
                switch (usbClientSetting.Mode) {
                    case UsbClientMode.Cdc:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.CDC;
                        break;

                    case UsbClientMode.Joystick:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.Joystick;
                        break;

                    case UsbClientMode.Keyboard:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.Keyboard;
                        break;

                    case UsbClientMode.Mouse:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.Mouse;
                        break;

                    case UsbClientMode.WinUsb:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.WinUsb;
                        break;

                    default:
                        usbClientSetting.ProductId = (ushort)RawDevice.PID.RawDevice;
                        break;                    
                }
            }

            this.usbClientController = usbClientController;
            this.usbClientSetting = usbClientSetting;
           
            this.descriptors = new ArrayList();
            this.interfaceDescriptors = new ArrayList();
            this.stringDescriptors = new ArrayList();
            this.configuration = new Configuration();

            this.vendorId = usbClientSetting.VendorId == 0 ? GHI_VID : usbClientSetting.VendorId;
            this.productId = usbClientSetting.ProductId;
            this.bcdUsb = usbClientSetting.BcdUsb;
            this.bcdDevice = usbClientSetting.BcdDevice;
            this.maximumPower = usbClientSetting.MaxPower;
            this.manufacturer = usbClientSetting.ManufactureName;
            this.product = usbClientSetting.ProductName;
            this.serialNumber = usbClientSetting.SerialNumber;
            

            if (this.manufacturer != null)
                this.stringDescriptors.Add(new Configuration.StringDescriptor(RawDevice.MANUFACTURER_STRING_INDEX, usbClientSetting.ManufactureName));

            if (this.product != null)
                this.stringDescriptors.Add(new Configuration.StringDescriptor(RawDevice.PRODUCT_STRING_INDEX, usbClientSetting.ProductName));

            this.stringDescriptors.Add(new Configuration.StringDescriptor(RawDevice.SERIAL_NUMBER_STRING_INDEX, (this.serialNumber == null) ? "0" : usbClientSetting.SerialNumber));

            this.nextStringIndex = RawDevice.START_STRING_INDEX;
            this.currentInterfaceIndex = 0;

            var startingReservations = this.usbClientController.Provider.GetEndpointMap();
            this.endpointReservations = new bool[RawDevice.MaxEndpoint];
            for (var i = 0; i < RawDevice.MaxEndpoint; i++)
                this.endpointReservations[i] = (startingReservations & (1 << i)) != 0;

            this.streamMap = new byte[RawDevice.MaxStreams];
            this.streamReservations = new bool[RawDevice.MaxStreams];
            for (var i = 0; i < RawDevice.MaxStreams; i++) {
                this.streamMap[i] = 0;
                this.streamReservations[i] = false;
            }

            this.interfaceMap = new uint[RawDevice.MAX_INTERFACE_COUNT];
            for (var i = 0; i < RawDevice.MAX_INTERFACE_COUNT; i++)
                this.interfaceMap[i] = 0;
        }
        
        private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

        private void OnDeviceStateChanged(RawDevice sender, DeviceState state) => this.deviceStateChangedCallbacks?.Invoke(this, state);

        public event DeviceStateChangedEventHandler DeviceStateChanged {
            add {
                if (this.deviceStateChangedCallbacks == null)
                    this.usbClientController.Provider.DeviceStateChanged += this.OnDeviceStateChanged;

                this.deviceStateChangedCallbacks += value;
            }

            remove {
                this.deviceStateChangedCallbacks -= value;

                if (this.deviceStateChangedCallbacks == null)
                    this.usbClientController.Provider.DeviceStateChanged -= this.OnDeviceStateChanged;
            }
        }
        /// <summary>Gets the next available endpoint.</summary>
        /// <returns>The endpoint number.</returns>
        /// <remarks>The number of available endpoints depends on your platform.</remarks>
        public int ReserveNewEndpoint() {
            for (var i = 0; i < RawDevice.MaxEndpoint; i++) {
                if (!this.endpointReservations[i]) {
                    this.endpointReservations[i] = true;

                    return i;
                }
            }

            throw new InvalidOperationException("There are no available endpoints.");
        }

        /// <summary>Reserves the given endpoint.</summary>
        /// <param name="endpoint">The specific endpoint number to reserve.</param>
        public void ReserveNewEndpoint(int endpoint) {
            if (endpoint >= RawDevice.MaxEndpoint || endpoint < 0) throw new ArgumentOutOfRangeException("endpoint", "endpoint must be between 0 and MaxEndpoint.");

            if (this.endpointReservations[endpoint])
                throw new InvalidOperationException("There are no available endpoints.");

            this.endpointReservations[endpoint] = true;
        }

        /// <summary>Adds a descriptor to the device.</summary>
        /// <param name="descriptor">The descriptor to add.</param>
        /// <remarks>This is for descriptors that are not part of the configuration descriptors that need a special request.</remarks>
        public void AddDescriptor(Configuration.Descriptor descriptor) {
            if (descriptor == null) throw new ArgumentNullException("descriptor");

            this.descriptors.Add(descriptor);
        }

        /// <summary>Adds an interface to the device.</summary>
        /// <param name="usbInterface">The interface.</param>
        /// <param name="interfaceString">The interface name.</param>
        /// <returns>The assigned interface index.</returns>
        public byte AddInterface(Configuration.UsbInterface usbInterface, string interfaceString) {
            if (usbInterface == null) throw new ArgumentNullException("descriptor");

            usbInterface.bInterfaceNumber = this.currentInterfaceIndex++;

            this.interfaceDescriptors.Add(usbInterface);

            if (interfaceString != null && interfaceString != string.Empty) {
                usbInterface.iInterface = this.nextStringIndex;
                this.stringDescriptors.Add(new Configuration.StringDescriptor(this.nextStringIndex++, interfaceString));
            }

            return usbInterface.bInterfaceNumber;
        }

        /// <summary>Sets this device as the active device on the USB client controller. The existing active device will be deactivated if present.</summary>
        private void Initialize() {
            var deviceDescriptor = new Configuration.DeviceDescriptor(this.vendorId, this.productId, this.bcdUsb, this.bcdDevice) {
                bMaxPacketSize0 = (byte)this.usbClientController.Provider.GetControlPacketSize(),
                bcdUSB = this.bcdUsb,
                bcdDevice = this.bcdDevice,
                iManufacturer = RawDevice.MANUFACTURER_STRING_INDEX,
                iProduct = RawDevice.PRODUCT_STRING_INDEX,
                iSerialNumber = RawDevice.SERIAL_NUMBER_STRING_INDEX, 
                bDeviceClass = 0,
                bDeviceSubClass = 0,
                bDeviceProtocol = 0

            };

            var interfaceDescriptors = (Configuration.UsbInterface[])this.interfaceDescriptors.ToArray(typeof(Configuration.UsbInterface));

            var index = 0;

            this.configuration.descriptors = new Configuration.Descriptor[this.descriptors.Count + this.stringDescriptors.Count + 2]; //device and configuration descriptor
            this.configuration.descriptors[index++] = deviceDescriptor;
            this.configuration.descriptors[index++] = new Configuration.ConfigurationDescriptor(this.maximumPower, interfaceDescriptors);

            for (var i = 0; i < this.descriptors.Count; i++)
                this.configuration.descriptors[index++] = (Configuration.Descriptor)this.descriptors[i];

            for (var i = 0; i < this.stringDescriptors.Count; i++)
                this.configuration.descriptors[index++] = (Configuration.Descriptor)this.stringDescriptors[i];

                        
            for (var i = 0; i < this.configuration.descriptors.Length; i++) {
                var type = this.configuration.descriptors[i].ToString();

                if (type.IndexOf("DeviceDescriptor") > 0) {
                    var descriptor = new Configuration.DeviceDescriptor[] { (Configuration.DeviceDescriptor)this.configuration.descriptors[i] };
                    this.usbClientController.Provider.SetDeviceDescriptor(descriptor);
                }
                else if (type.IndexOf("ConfigurationDescriptor") > 0) {
                    var descriptor = new Configuration.ConfigurationDescriptor[] { (Configuration.ConfigurationDescriptor)this.configuration.descriptors[i] };
                    this.usbClientController.Provider.SetConfigurationDescriptor(descriptor);
                }
                else if (type.IndexOf("GenericDescriptor") > 0) {
                    var descriptor = new Configuration.GenericDescriptor[] { (Configuration.GenericDescriptor)this.configuration.descriptors[i] };
                    this.usbClientController.Provider.SetGenericDescriptor(descriptor);
                }
                else if (type.IndexOf("StringDescriptor") > 0) {
                    var descriptor = new Configuration.StringDescriptor[] { (Configuration.StringDescriptor)this.configuration.descriptors[i] };
                   this.usbClientController.Provider.SetStringDescriptor(descriptor, descriptor[0].bIndex);
                }
                else {
                    throw new Exception("Unknow descriptor.");
                }
            }

            UsbClientControllerApiWrapper.InitializeStream(this.streamMap, this.interfaceMap);
            
        }

        private bool initialized = false;

        internal bool Initialized { get => this.initialized; set => this.initialized = value; }

        public void Enable() {
            if (!this.initialized) {
                this.Initialize();

                this.initialized = true;
            }

            this.usbClientController.Provider.SetActiveSetting(this.usbClientSetting);
            this.usbClientController.Provider.Enable();
        }
        public void Disable() => this.usbClientController.Provider.Disable();        
        public void Dispose() => this.usbClientController.Dispose();
        public DeviceState DeviceState => this.usbClientController.Provider.DeviceState;

        /// <summary>Creates a steam for reading and writing to the device.</summary>
        /// <param name="writeEndpoint">The write endpoint. Use RawStream.NullEndpoint if not available.</param>
        /// <param name="readEndpoint">The read endpoint. Use RawStream.NullEndpoint if not available.</param>
        /// <returns>The new stream.</returns>
        public RawStream CreateStream(int writeEndpoint, int readEndpoint) {
            if (writeEndpoint < 0) throw new ArgumentOutOfRangeException("writeEndpoint", "writeEndpoint must not be negative.");
            if (readEndpoint < 0) throw new ArgumentOutOfRangeException("writeEndpoint", "writeEndpoint must not be negative.");

            for (var i = 0; i < RawDevice.MaxStreams; i++) {
                if (!this.streamReservations[i]) {
                    this.streamReservations[i] = true;
                    this.streamMap[i] = (byte)(writeEndpoint | (readEndpoint << 4));

                    return this.CreateStream(i, this);
                }
            }

            throw new InvalidOperationException("There are no available streams.");
        }

        internal void FreeUsbStream(int index) {
            this.streamReservations[index] = false;
            this.streamMap[index] = 0;
        }

        public void SetInterfaceMap(byte interfaceIndex, byte data1, byte data2, byte data3) => this.interfaceMap[interfaceIndex] = (uint)((data1 << 8) | (data2 << 16) | (data3 << 24));

        /// <summary>Creates a new instance of the stream type for this device type.</summary>
        /// <param name="index">The index of the stream</param>
        /// <param name="parent">The owning raw device.</param>
        /// <returns>The new stream.</returns>
        protected virtual RawStream CreateStream(int index, RawDevice parent) => new RawStream(index, parent);      

        /// <summary>USB stream for reading and writing data through two endpoints.</summary>
        public class RawStream : Stream {
            private int lastWritten;
            private int readTimeout;
            private int writeTimeout;
            private bool disposed;
            private int streamIndex;
            private RawDevice parent;

            /// <summary>Represents no endpoint.</summary>
            public static int NullEndpoint => 0;

            /// <summary>The index of this stream.</summary>
            public int StreamIndex => this.streamIndex;

            /// <summary>How long to wait before timing out a read operation.</summary>
            public override int ReadTimeout {
                get => this.readTimeout;

                set {
                    if (value < 0 && value != Timeout.Infinite) throw new ArgumentOutOfRangeException("value", "value must not be negative.");

                    this.readTimeout = value;
                }
            }

            /// <summary>How long to wait before timing out a write operation.</summary>
            public override int WriteTimeout {
                get => this.writeTimeout;

                set {
                    if (value < 0 && value != Timeout.Infinite) throw new ArgumentOutOfRangeException("value", "value must not be negative.");

                    this.writeTimeout = value;
                }
            }

            /// <summary>Whether or not read is allowed from the stream.</summary>
            public override bool CanRead => true;

            /// <summary>Whether or not seeking is allowed in the stream.</summary>
            public override bool CanSeek => false;

            /// <summary>Whether or not writting is allowed to the stream.</summary>
            public override bool CanWrite => true;

            /// <summary>Not supported.</summary>
            public override long Length => throw new NotSupportedException();

            /// <summary>Not supported.</summary>
            public override long Position {
                get => throw new NotSupportedException();

                set => throw new NotSupportedException();
            }

            /// <summary>The number of bytes successfully written in the last call to any Write function.</summary>
            /// <remarks>
            /// Since System.IO.Stream.Write returns void, we cannot return this information from our overload. We also are not updating Position or
            /// Length because they are not well defined for a stream like this and they would require tracking the difference yourself.
            /// </remarks>
            public int LastWritten {
                get => this.lastWritten;

                private set => this.lastWritten = value;
            }

            /// <summary>The number of bytes available to read.</summary>
            public int BytesToRead => this.parent.usbClientController.Provider.BytesToRead(this.streamIndex);

            /// <summary>The number of bytes that are in the process of being written.</summary>
            public int BytesToWrite => this.parent.usbClientController.Provider.BytesToWrite(this.streamIndex);

            public RawStream(int streamIndex, RawDevice parent) {
                this.disposed = false;
                this.readTimeout = 0;
                this.writeTimeout = 0;
                this.streamIndex = streamIndex;
                this.parent = parent;
            }

            /// <summary>The finalizer.</summary>
            ~RawStream() {
                this.Dispose(false);
            }

            /// <summary>Not supported.</summary>
            /// <param name="offset">Not supported.</param>
            /// <param name="origin">Not supported.</param>
            /// <returns>Not supported.</returns>
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            /// <summary>Not supported.</summary>
            /// <param name="value">Not supported.</param>
            public override void SetLength(long value) => throw new NotSupportedException();

            /// <summary>Flushs the USB write buffers.</summary>
            public override void Flush() => this.parent.usbClientController.Provider.Flush(this.streamIndex);

            /// <summary>Reads data from the stream.</summary>
            /// <param name="buffer">The buffer to read into.</param>
            /// <param name="offset">The offset into the buffer at which to write the data.</param>
            /// <param name="count">The number of bytes to read.</param>
            /// <returns>The number of bytes read.</returns>
            public override int Read(byte[] buffer, int offset, int count) {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (buffer.Length == 0) throw new ArgumentOutOfRangeException("buffer", "buffer must have a positive length");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
                if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
                if (buffer.Length < offset + count) throw new ArgumentOutOfRangeException("buffer", "buffer.Length must be at least offset + count.");

                int read;
                var endTime = DateTime.Now.AddMilliseconds(this.ReadTimeout);

                while (true) {
                    read = this.parent.usbClientController.Provider.Read(this.streamIndex, buffer, offset, count);
                    
                    if (read > 0)
                        return read;

                    if (endTime < DateTime.Now && this.ReadTimeout != Timeout.Infinite)
                        break;

                    Thread.Sleep(5);

                }

                return read;
            }

            /// <summary>Writes data to the stream.</summary>
            /// <param name="buffer">The buffer from which to write.</param>
            /// <param name="offset">The offset into the buffer at which to begin writing.</param>
            /// <param name="count">The number of bytes to write.</param>
            public override void Write(byte[] buffer, int offset, int count) {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
                if (count < 0) throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
                if (buffer.Length < offset + count) throw new ArgumentOutOfRangeException("buffer", "buffer.Length must be at least offset + count.");

                this.LastWritten = 0;

                var endTime = DateTime.Now.AddMilliseconds(this.WriteTimeout);

                while (true) {
                    //this.LastWritten += RawDevice.NativeWrite(this.streamIndex, buffer, offset + this.LastWritten, count - this.LastWritten);
                    this.LastWritten += this.parent.usbClientController.Provider.Write(this.streamIndex, buffer, offset + this.LastWritten, count - this.LastWritten);

                    if (this.LastWritten >= count)
                        break;

                    if (endTime < DateTime.Now && this.WriteTimeout != Timeout.Infinite)
                        throw new Exception("Timeout.");

                    Thread.Sleep(5);
                }
            }

            /// <summary>Reads data from the stream.</summary>
            /// <param name="buffer">The buffer into which to read.</param>
            /// <returns>The number of bytes read.</returns>
            public int Read(byte[] buffer) {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (buffer.Length == 0) throw new ArgumentOutOfRangeException("buffer", "buffer must have a positive length");

                return this.Read(buffer, 0, buffer.Length);
            }

            /// <summary>Writes data to the stream.</summary>
            /// <param name="buffer">The buffer from which to write.</param>
            public void Write(byte[] buffer) {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (buffer.Length == 0) throw new ArgumentOutOfRangeException("buffer", "buffer must have a positive length");

                this.Write(buffer, 0, buffer.Length);
            }

            /// <summary>Disposes the object.</summary>
            /// <param name="disposing">Whether or not this is called from Dispose.</param>
            protected override void Dispose(bool disposing) {
                if (this.disposed)
                    return;

                this.parent.FreeUsbStream(this.streamIndex);

                base.Dispose(disposing);
            }
        }
    }
}
