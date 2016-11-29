using GHIElectronics.TinyCLR.Storage.Streams;
using System;

namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public delegate void ErrorReceivedDelegate(SerialDevice sender, ErrorReceivedEventArgs e);
    public delegate void PinChangedDelegate(SerialDevice sender, PinChangedEventArgs e);

    public class SerialDevice : IDisposable {
        private bool disposed;

        public bool BreakSignalState { get { throw new NotSupportedException(); } }
        public uint BytesReceived { get { throw new NotSupportedException(); } }
        public bool CarrierDetectState { get { throw new NotSupportedException(); } }
        public bool ClearToSendState { get { throw new NotSupportedException(); } }
        public bool DataSetReadyState { get { throw new NotSupportedException(); } }
        public bool IsDataTerminalReadyEnabled { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public bool IsRequestToSendEnabled { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public TimeSpan ReadTimeout { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public ushort UsbProductId { get { throw new NotSupportedException(); } }
        public ushort UsbVendorId { get { throw new NotSupportedException(); } }
        public TimeSpan WriteTimeout { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        public string PortName { get; private set; }
        public uint BaudRate { get; set; }
        public ushort DataBits { get; set; }
        public SerialParity Parity { get; set; }
        public SerialHandshake Handshake { get; set; }
        public SerialStopBitCount StopBits { get; set; }

        public IInputStream InputStream { get; private set; }
        public IOutputStream OutputStream { get; private set; }

        public event ErrorReceivedDelegate ErrorReceived { add { throw new NotSupportedException(); } remove { throw new NotSupportedException(); } }
        public event PinChangedDelegate PinChanged { add { throw new NotSupportedException(); } remove { throw new NotSupportedException(); } }

        public static string GetDeviceSelector() => string.Empty;
        public static string GetDeviceSelector(string portName) => string.Empty + portName;
        public static string GetDeviceSelectorFromUsbVidPid(ushort vendorId, ushort productId) { throw new NotSupportedException(); }

        public static SerialDevice FromIdAsync(string deviceId) {
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
            private bool opened;

            public Stream(SerialDevice parent) {
                this.parent = parent;
                this.opened = false;
            }

            public void Dispose() => this.parent.Dispose();

            internal void ParentDispose() {

            }

            public bool FlushAsync() {
                return false;
            }

            public uint ReadAsync(IBuffer buffer, uint count, InputStreamOptions options) {
                if (count > buffer.Capacity) throw new InvalidOperationException($"{nameof(count)} is more than the capacity of {nameof(buffer)}.");
                if (this.parent.disposed) throw new ObjectDisposedException();

                if (!this.opened)
                    this.Open();

                return 0;
            }

            public uint WriteAsync(IBuffer buffer) {
                if (this.parent.disposed) throw new ObjectDisposedException();

                if (!this.opened)
                    this.Open();

                return 0;
            }

            private void Open() {

            }
        }
    }
}