using GHIElectronics.TinyCLR.Devices.Internal;
using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public delegate void ErrorReceivedDelegate(SerialDevice sender, ErrorReceivedEventArgs e);
    public delegate void PinChangedDelegate(SerialDevice sender, PinChangedEventArgs e);

    public class SerialDevice : IDisposable {
        private delegate void DataReceivedDelegate(SerialDevice sender, EventArgs e);

        private readonly Stream stream;
        private bool disposed;

        public bool BreakSignalState { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool CarrierDetectState => throw new NotSupportedException();
        public bool ClearToSendState => throw new NotSupportedException();
        public bool DataSetReadyState => throw new NotSupportedException();
        public bool IsDataTerminalReadyEnabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool IsRequestToSendEnabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public ushort UsbProductId => throw new NotSupportedException();
        public ushort UsbVendorId => throw new NotSupportedException();
        public uint BytesReceived { get; private set; }
        public string PortName { get; }
        public uint BaudRate { get; set; }
        public ushort DataBits { get; set; }
        public SerialParity Parity { get; set; }
        public SerialHandshake Handshake { get; set; }
        public SerialStopBitCount StopBits { get; set; }
        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }

        public IInputStream InputStream => this.stream;
        public IOutputStream OutputStream => this.stream;

        public event ErrorReceivedDelegate ErrorReceived { add => this.stream.ErrorReceived += value; remove => this.stream.ErrorReceived -= value; }
        public event PinChangedDelegate PinChanged { add { throw new NotSupportedException(); } remove { throw new NotSupportedException(); } }

        public static string GetDeviceSelector() => string.Empty;
        public static string GetDeviceSelector(string portName) => string.Empty + portName;
        public static string GetDeviceSelectorFromUsbVidPid(ushort vendorId, ushort productId) => throw new NotSupportedException();

        public static SerialDevice FromId(string deviceId) {
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));
            if (deviceId.Length < 4 || deviceId.ToUpper().IndexOf("COM") != 0 || deviceId[3] == '0')
                throw new ArgumentException("Invalid COM port.", nameof(deviceId));

            try {
                uint.Parse(deviceId.Substring(3));
            }
            catch {
                throw new ArgumentException("Invalid COM port.", nameof(deviceId));
            }

            return new SerialDevice(deviceId) {
                BaudRate = 9600,
                DataBits = 8,
                Parity = SerialParity.None,
                StopBits = SerialStopBitCount.One,
            };
        }

        private SerialDevice(string portName) {
            this.PortName = portName;
            this.stream = new Stream(this);
        }

        public void Dispose() => this.Dispose(true);

        protected void Dispose(bool disposing) {
            if (this.disposed)
                return;

            if (disposing)
                this.stream.ParentDispose();

            this.disposed = true;
        }

        ~SerialDevice() {
            GC.SuppressFinalize(this);
            this.Dispose(false);
        }

        private class Stream : IInputStream, IOutputStream, IDisposable {
            private readonly SerialDevice parent;
            private uint port;
            private bool opened;
            private NativeEventDispatcher errorReceivedEvent;
            private NativeEventDispatcher dataReceivedEvent;
            private ErrorReceivedDelegate errorReceivedCallbacks;

            private void ErrorReceivedEventHandler(uint evt, uint data2, DateTime timestamp) => this.errorReceivedCallbacks?.Invoke(this.parent, new ErrorReceivedEventArgs((SerialError)evt));
            private void DataReceivedEventHandler(uint evt, uint data2, DateTime timestamp) => this.DataReceived?.Invoke(this.parent, EventArgs.Empty);

            public event ErrorReceivedDelegate ErrorReceived {
                add {
                    var wasEmpty = this.errorReceivedCallbacks == null;

                    this.errorReceivedCallbacks += value;

                    if (wasEmpty && this.errorReceivedCallbacks != null)
                        this.errorReceivedEvent.OnInterrupt += this.ErrorReceivedEventHandler;
                }
                remove {
                    this.errorReceivedCallbacks -= value;

                    if (this.errorReceivedCallbacks == null)
                        this.errorReceivedEvent.OnInterrupt -= this.ErrorReceivedEventHandler;
                }
            }

            public event DataReceivedDelegate DataReceived;

            public Stream(SerialDevice parent) {
                this.parent = parent;
                this.opened = false;
                this.port = uint.Parse(this.parent.PortName.Substring(3)) - 1;
                this.errorReceivedEvent = new NativeEventDispatcher("SerialPortErrorEvent", this.port);
                this.dataReceivedEvent = new NativeEventDispatcher("SerialPortDataEvent", this.port);

                this.dataReceivedEvent.OnInterrupt += this.DataReceivedEventHandler;
            }

            public void Dispose() => this.parent.Dispose();

            internal void ParentDispose() {
                Stream.NativeClose(this.port, (uint)this.parent.Handshake);

                if (this.errorReceivedCallbacks != null) {
                    this.errorReceivedEvent.OnInterrupt -= this.ErrorReceivedEventHandler;
                    this.errorReceivedCallbacks = null;
                    this.errorReceivedEvent.Dispose();
                }

                this.dataReceivedEvent.OnInterrupt -= this.DataReceivedEventHandler;
                this.dataReceivedEvent.Dispose();
            }

            public bool Flush() {
                this.Open();

                Stream.NativeFlush(this.port);

                return true;
            }

            public uint Read(IBuffer buffer, uint count, InputStreamOptions options) {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (count > buffer.Capacity)
                    throw new InvalidOperationException($"{nameof(count)} is more than the capacity of {nameof(buffer)}.");
                if (this.parent.disposed)
                    throw new ObjectDisposedException();
                if (options != InputStreamOptions.None)
                    throw new NotSupportedException($"{nameof(options)} is not supported.");

                this.Open();

                return this.parent.BytesReceived = (uint)Stream.NativeRead(this.port, (buffer as Buffer).Data, 0, (int)buffer.Length, (int)this.parent.ReadTimeout.TotalMilliseconds);
            }

            public uint Write(IBuffer buffer) {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (this.parent.disposed)
                    throw new ObjectDisposedException();

                this.Open();

                return (uint)Stream.NativeWrite(this.port, (buffer as Buffer).Data, 0, (int)buffer.Length, (int)this.parent.WriteTimeout.TotalMilliseconds);
            }

            private void Open() {
                if (this.opened)
                    return;

                Stream.NativeOpen(this.port, this.parent.BaudRate, (uint)this.parent.Parity, this.parent.DataBits, (uint)this.parent.StopBits, (uint)this.parent.Handshake);

                this.opened = true;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static void NativeOpen(uint port, uint baudRate, uint parity, uint dataBits, uint stopBits, uint handshaking);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static void NativeClose(uint port, uint handshaking);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static void NativeFlush(uint port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static int NativeRead(uint port, byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static int NativeWrite(uint port, byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static int NativeBytesToRead(uint port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static int NativeBytesToWrite(uint port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static void NativeDiscardRead(uint port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern static void NativeDiscardWrite(uint port);
        }
    }
}