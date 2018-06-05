using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Storage.Streams;

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
        public event PinChangedDelegate PinChanged { add => this.stream.PinChanged += value; remove => this.stream.PinChanged -= value; }

        public static string GetDeviceSelector() => throw new NotSupportedException();
        public static string GetDeviceSelector(string friendlyName) => throw new NotSupportedException();
        public static string GetDeviceSelectorFromUsbVidPid(ushort vendorId, ushort productId) => throw new NotSupportedException();

        public static SerialDevice FromId(string deviceId) => Api.ParseSelector(deviceId, out var providerId, out var index) ?
            new SerialDevice(deviceId, providerId, index) {
                BaudRate = 9600,
                DataBits = 8,
                Parity = SerialParity.None,
                StopBits = SerialStopBitCount.One,
            } : null;

        private SerialDevice(string deviceId, string providerId, uint idx) {
            this.PortName = deviceId;
            this.stream = new Stream(this, providerId, idx);
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

        internal class Stream : IInputStream, IOutputStream, IDisposable {
#pragma warning disable CS0169
            private IntPtr nativeProvider;
#pragma warning restore CS0169

            private readonly SerialDevice parent;
            private string providerId;
            private uint idx;
            private bool opened;
            private NativeEventDispatcher errorReceivedEvent;
            private NativeEventDispatcher pinChangedEvent;

            public event ErrorReceivedDelegate ErrorReceived;
            public event PinChangedDelegate PinChanged;

            public Stream(SerialDevice parent, string providerId, uint idx) {
                this.parent = parent;
                this.opened = false;
                this.providerId = providerId;
                this.idx = idx;


                var api = Api.Find(this.providerId, ApiType.UartProvider);

                if (api == null) throw new ArgumentException("Invalid id.", nameof(providerId));

                this.nativeProvider = api.Implementation;
            }

            public void Dispose() => this.parent.Dispose();

            internal void ParentDispose() {
                if (this.opened) {
                    this.NativeClose((uint)this.parent.Handshake);
                }
            }

            public bool Flush() {
                this.Open();

                this.NativeFlush();

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
                    read = (uint)this.NativeRead(((Buffer)buffer).data, (int)(((Buffer)buffer).offset + total), (int)(count - total), (int)((end - DateTime.UtcNow).TotalMilliseconds));
                    total += read;

                    if ((read > 0 && options == InputStreamOptions.Partial) || DateTime.UtcNow > end)
                        break;
                }

                buffer.Length = this.parent.BytesReceived = total;

                return buffer;
            }

            public uint Write(IBuffer buffer) {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));
                if (this.parent.disposed)
                    throw new ObjectDisposedException();

                this.Open();

                return (uint)this.NativeWrite(((Buffer)buffer).data, ((Buffer)buffer).offset, (int)buffer.Length, (int)this.parent.WriteTimeout.TotalMilliseconds);
            }

            public extern uint ReadBufferSize {
                [MethodImpl(MethodImplOptions.InternalCall)]
                get;
                [MethodImpl(MethodImplOptions.InternalCall)]
                set;
            }

            public extern uint WriteBufferSize {
                [MethodImpl(MethodImplOptions.InternalCall)]
                get;
                [MethodImpl(MethodImplOptions.InternalCall)]
                set;
            }

            public extern uint UnreadCount {
                [MethodImpl(MethodImplOptions.InternalCall)]
                get;
            }

            public extern uint UnwrittenCount {
                [MethodImpl(MethodImplOptions.InternalCall)]
                get;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();


            private void Open() {
                if (this.opened)
                    return;

                this.NativeOpen(this.parent.BaudRate, (uint)this.parent.Parity, this.parent.DataBits, (uint)this.parent.StopBits, (uint)this.parent.Handshake);

                this.errorReceivedEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.ErrorReceived");
                this.errorReceivedEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.providerId == pn && this.idx == ci) this.ErrorReceived?.Invoke(this.parent, new ErrorReceivedEventArgs((SerialError)d0)); };

                this.pinChangedEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.PinChanged");
                this.pinChangedEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.providerId == pn && this.idx == ci) this.PinChanged?.Invoke(this.parent, new PinChangedEventArgs((SerialPinChange)d0)); };

                this.opened = true;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeOpen(uint baudRate, uint parity, uint dataBits, uint stopBits, uint handshaking);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeClose(uint handshaking);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeFlush();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int NativeRead(byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int NativeWrite(byte[] buffer, int offset, int count, int timeout);
        }
    }
}