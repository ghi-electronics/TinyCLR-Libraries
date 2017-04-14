using GHIElectronics.TinyCLR.Devices.Internal;
using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public delegate void ErrorReceivedDelegate(SerialDevice sender, ErrorReceivedEventArgs e);
    public delegate void PinChangedDelegate(SerialDevice sender, PinChangedEventArgs e);

    public class SerialDevice : IDisposable {
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

            public event ErrorReceivedDelegate ErrorReceived;

            public Stream(SerialDevice parent) {
                this.parent = parent;
                this.opened = false;
                this.port = uint.Parse(this.parent.PortName.Substring(3)) - 1;
            }

            public void Dispose() => this.parent.Dispose();

            internal void ParentDispose() {
                if (this.opened) {
                    this.errorReceivedEvent.Dispose();

                    Stream.NativeClose(this.port, (uint)this.parent.Handshake);
                }
            }

            public bool Flush() {
                this.Open();

                Stream.NativeFlush(this.port);

                return true;
            }

            public IBuffer Read(IBuffer buffer, uint count, InputStreamOptions options) {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (count > buffer.Capacity)
                    throw new InvalidOperationException($"{nameof(count)} is more than the capacity of {nameof(buffer)}.");
                if (this.parent.disposed)
                    throw new ObjectDisposedException();
                if (options == InputStreamOptions.ReadAhead)
                    throw new NotSupportedException($"{nameof(options)} is not supported.");

                this.Open();

                var read = 0U;
                var total = 0U;
                var end = DateTime.UtcNow.Add(this.parent.ReadTimeout);

                //TODO UWP on RPI and desktop appear to block indefinitely until exactly count are received regardless of InputStreamOptions or timeout
                while (total < count) {
                    read = (uint)Stream.NativeRead(this.port, ((Buffer)buffer).data, (int)(((Buffer)buffer).offset + total), (int)(count - total), (int)((end - DateTime.UtcNow).TotalMilliseconds));
                    total += read;

                    if (read > 0 && options == InputStreamOptions.Partial || DateTime.UtcNow > end)
                        break;
                }

                buffer.Length = this.parent.BytesReceived = read;

                return buffer;
            }

            public uint Write(IBuffer buffer) {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (this.parent.disposed)
                    throw new ObjectDisposedException();

                this.Open();

                return (uint)Stream.NativeWrite(this.port, ((Buffer)buffer).data, ((Buffer)buffer).offset, (int)buffer.Length, (int)this.parent.WriteTimeout.TotalMilliseconds);
            }

            private void Open() {
                if (this.opened)
                    return;

                Stream.NativeOpen(this.port, this.parent.BaudRate, (uint)this.parent.Parity, this.parent.DataBits, (uint)this.parent.StopBits, (uint)this.parent.Handshake);

                this.errorReceivedEvent = new NativeEventDispatcher("SerialPortErrorEvent", this.port);
                this.errorReceivedEvent.OnInterrupt += (s, evt, ts) => this.ErrorReceived?.Invoke(this.parent, new ErrorReceivedEventArgs((SerialError)evt));

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