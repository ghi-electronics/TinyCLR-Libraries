using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public delegate void ErrorReceivedDelegate(SerialDevice sender, ErrorReceivedEventArgs e);
    public delegate void PinChangedDelegate(SerialDevice sender, PinChangedEventArgs e);

    public class SerialDevice : IDisposable {
        private bool disposed;

        public bool BreakSignalState { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public uint BytesReceived => throw new NotSupportedException();
        public bool CarrierDetectState => throw new NotSupportedException();
        public bool ClearToSendState => throw new NotSupportedException();
        public bool DataSetReadyState => throw new NotSupportedException();
        public bool IsDataTerminalReadyEnabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool IsRequestToSendEnabled { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public ushort UsbProductId => throw new NotSupportedException();
        public ushort UsbVendorId => throw new NotSupportedException();
        public string PortName { get; private set; }
        public uint BaudRate { get; set; }
        public ushort DataBits { get; set; }
        public SerialParity Parity { get; set; }
        public SerialHandshake Handshake { get; set; }
        public SerialStopBitCount StopBits { get; set; }
        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }

        public IInputStream InputStream { get; private set; }
        public IOutputStream OutputStream { get; private set; }

        public event ErrorReceivedDelegate ErrorReceived { add { throw new NotSupportedException(); } remove { throw new NotSupportedException(); } }
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

            var device = new SerialDevice();
            var stream = new Stream(device);

            device.PortName = deviceId;
            device.BaudRate = 9600;
            device.DataBits = 8;
            device.Parity = SerialParity.None;
            device.StopBits = SerialStopBitCount.One;
            device.InputStream = stream;
            device.OutputStream = stream;

            device.disposed = false;

            return device;
        }

        public void Dispose() => this.Dispose(true);

        protected void Dispose(bool disposing) {
            if (this.disposed)
                return;

            if (disposing) {
                (this.InputStream as Stream)?.ParentDispose();
                this.InputStream = null;
                this.OutputStream = null;
            }

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

            public Stream(SerialDevice parent) {
                this.parent = parent;
                this.opened = false;
            }

            public void Dispose() => this.parent.Dispose();

            internal void ParentDispose() => Stream.NativeClose(this.port, (uint)this.parent.Handshake);

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

                return (uint)Stream.NativeRead(this.port, (buffer as Buffer).Data, 0, (int)buffer.Length, (int)this.parent.ReadTimeout.TotalMilliseconds);
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

                this.port = uint.Parse(this.parent.PortName.Substring(3)) - 1;

                Stream.NativeOpen(this.port, (uint)this.parent.BaudRate, (uint)this.parent.Parity, this.parent.DataBits, (uint)this.parent.StopBits, (uint)this.parent.Handshake);

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
        }
    }
}