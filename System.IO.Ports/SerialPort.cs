using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Text;
using System.Threading;

namespace System.IO.Ports {
    [Flags]
    public enum SerialData {
        Chars = 1,
        Eof = 2,
    }

    [Flags]
    public enum SerialError {
        TXFull = 0x100,
        RXOver = 1,
        Overrun = 2,
        RXParity = 4,
        Frame = 8,
    }

    [Flags]
    public enum SerialPin {
        CtsChanged = 0x08,
        DsrChanged = 0x10,
        CDChanged = 0x20,
        Break = 0x40,
        Ring = 0x100,
    }

    public enum StopBits {
        None = 0,
        One = 1,
        Two = 2,
        OnePointFive = 3,
    }

    public enum Handshake {
        None = 0,
        XOnXOff = 1,
        RequestToSend = 2,
        RequestToSendXOnXOff = 3,
    }

    public enum Parity {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4,
    }

    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
    public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e);
    public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e);

    public class SerialDataReceivedEventArgs : EventArgs {
        public SerialData EventType { get; }
        public SerialDataReceivedEventArgs(SerialData eventType) => this.EventType = eventType;
    }

    public class SerialErrorReceivedEventArgs : EventArgs {
        public SerialError EventType { get; }
        public SerialErrorReceivedEventArgs(SerialError eventType) => this.EventType = eventType;
    }

    public class SerialPinChangedEventArgs : EventArgs {
        public SerialPin EventType { get; }
        public SerialPinChangedEventArgs(SerialPin eventType) => this.EventType = eventType;
    }

    public class SerialPort : IDisposable {
        private UartController controller;
        private readonly object sync;
        private bool disposed;
        private bool isOpen;
        private int bytesToReadCache;

        public string PortName { get; set; }
        public int BaudRate { get; set; } = 9600;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Handshake Handshake { get; set; } = Handshake.None;
        public int ReadTimeout { get; set; } = InfiniteTimeout;
        public int WriteTimeout { get; set; } = InfiniteTimeout;
        public int ReadBufferSize { get; set; } = 256;
        public int WriteBufferSize { get; set; } = 256;
        public int ReceivedBytesThreshold { get; set; } = 1;
        public string NewLine { get; set; } = "\n";
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public bool IsOpen => this.isOpen;
        public int BytesToRead => this.controller != null ? this.controller.BytesToRead : this.bytesToReadCache;
        public int BytesToWrite => this.controller != null ? this.controller.BytesToWrite : 0;

        public bool CtsHolding => this.controller != null && this.controller.ClearToSendState;
        public bool CDHolding {
            get => throw CreateTodoNotSupportedException("CDHolding (Carrier Detect state)");
        }

        public bool DsrHolding {
            get => throw CreateTodoNotSupportedException("DsrHolding (DSR state)");
        }

        public bool BreakState {
            get => throw CreateTodoNotSupportedException("BreakState getter");
            set => throw CreateTodoNotSupportedException("BreakState setter");
        }

        public bool DtrEnable {
            get => throw CreateTodoNotSupportedException("DtrEnable getter");
            set => throw CreateTodoNotSupportedException("DtrEnable setter");
        }

        public bool RtsEnable {
            get => this.controller != null && this.controller.IsRequestToSendEnabled;
            set {
                this.ThrowIfDisposed();
                if (!this.isOpen)
                    throw new InvalidOperationException("Port is closed.");

                this.controller.IsRequestToSendEnabled = value;
            }
        }

        public const int InfiniteTimeout = -1;

        public event SerialDataReceivedEventHandler DataReceived;
        public event SerialErrorReceivedEventHandler ErrorReceived;
        public event SerialPinChangedEventHandler PinChanged;

        public SerialPort() : this("COM1") {
        }

        public SerialPort(string portName) {
            this.sync = new object();
            this.PortName = portName;
        }

        public SerialPort(string portName, int baudRate) : this(portName) => this.BaudRate = baudRate;
        public SerialPort(string portName, int baudRate, Parity parity) : this(portName, baudRate) => this.Parity = parity;
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : this(portName, baudRate, parity) => this.DataBits = dataBits;
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : this(portName, baudRate, parity, dataBits) => this.StopBits = stopBits;

        public void Open() {
            this.ThrowIfDisposed();
            if (this.isOpen)
                throw new InvalidOperationException("Port is already open.");

            if (string.IsNullOrEmpty(this.PortName))
                throw new ArgumentException("PortName cannot be null or empty.");

            this.controller = this.CreateController(this.PortName);

            var settings = new UartSetting {
                BaudRate = this.BaudRate,
                DataBits = this.DataBits,
                Parity = this.MapParity(this.Parity),
                StopBits = this.MapStopBits(this.StopBits),
                Handshaking = this.MapHandshake(this.Handshake),
            };

            this.controller.SetActiveSettings(settings);
            this.controller.ReadBufferSize = this.ReadBufferSize;
            this.controller.WriteBufferSize = this.WriteBufferSize;
            this.controller.DataReceived += this.OnTinyClrDataReceived;
            this.controller.ErrorReceived += this.OnTinyClrErrorReceived;
            this.controller.ClearToSendChanged += this.OnTinyClrClearToSendChanged;
            this.controller.Enable();
            this.isOpen = true;
        }

        public void Close() {
            if (!this.isOpen)
                return;

            if (this.controller != null) {
                this.controller.DataReceived -= this.OnTinyClrDataReceived;
                this.controller.ErrorReceived -= this.OnTinyClrErrorReceived;
                this.controller.ClearToSendChanged -= this.OnTinyClrClearToSendChanged;
                this.controller.Disable();
                this.controller.Dispose();
                this.controller = null;
            }

            this.isOpen = false;
        }

        public void Dispose() {
            if (this.disposed)
                return;

            this.Close();
            this.disposed = true;
        }

        public int Read(byte[] buffer, int offset, int count) {
            this.ThrowIfDisposedAndClosed();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            return this.ReadCore(buffer, offset, count, this.ReadTimeout);
        }

        public int ReadByte() {
            var b = new byte[1];
            var read = this.Read(b, 0, 1);
            if (read == 0)
                throw CreateTimeoutException();
            return b[0];
        }

        public int ReadChar() {
            var ch = new char[1];
            var read = this.Read(ch, 0, 1);
            if (read == 0)
                throw CreateTimeoutException();
            return ch[0];
        }

        public int Read(char[] buffer, int offset, int count) {
            this.ThrowIfDisposedAndClosed();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            throw CreateTodoNotSupportedException("Read(char[], int, int) with full .NET decoder parity");
        }

        public string ReadExisting() {
            this.ThrowIfDisposedAndClosed();
            var available = this.BytesToRead;
            if (available <= 0)
                return string.Empty;

            var data = new byte[available];
            var read = this.controller.Read(data, 0, data.Length);
            return this.Encoding.GetString(data, 0, read);
        }

        public string ReadLine() => this.ReadTo(this.NewLine);

        public string ReadTo(string value) {
            this.ThrowIfDisposedAndClosed();

            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length == 0)
                throw new ArgumentException("Delimiter cannot be empty.", nameof(value));

            var start = DateTime.Now.Ticks;
            var timeoutTicks = this.ReadTimeout == InfiniteTimeout ? long.MaxValue : this.ReadTimeout * TimeSpan.TicksPerMillisecond;
            var sb = string.Empty;
            var one = new char[1];

            while (true) {
                var charsRead = this.Read(one, 0, 1);
                if (charsRead == 0)
                    throw CreateTimeoutException();

                sb += one[0];
                if (EndsWith(sb, value))
                    return sb.Substring(0, sb.Length - value.Length);

                if (this.ReadTimeout != InfiniteTimeout && DateTime.Now.Ticks - start > timeoutTicks)
                    throw CreateTimeoutException();
            }
        }

        public void Write(string text) {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var data = this.Encoding.GetBytes(text);
            this.Write(data, 0, data.Length);
        }

        public void WriteLine(string text) => this.Write((text ?? string.Empty) + this.NewLine);

        public void Write(byte[] buffer, int offset, int count) {
            this.ThrowIfDisposedAndClosed();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            lock (this.sync) {
                var written = this.controller.Write(buffer, offset, count);
                if (written < count) {
                    throw CreateTimeoutException();
                }
            }
        }

        public void Write(char[] buffer, int offset, int count) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            var text = string.Empty;
            for (var i = 0; i < count; i++)
                text += buffer[offset + i];

            var data = this.Encoding.GetBytes(text);
            this.Write(data, 0, data.Length);
        }

        public void DiscardInBuffer() {
            this.ThrowIfDisposedAndClosed();
            this.controller.ClearReadBuffer();
        }

        public void DiscardOutBuffer() {
            this.ThrowIfDisposedAndClosed();
            this.controller.ClearWriteBuffer();
        }

        public static string[] GetPortNames() {
            throw CreateTodoNotSupportedException("GetPortNames UART enumeration");
        }

        private int ReadCore(byte[] buffer, int offset, int count, int timeoutMs) {
            var total = 0;
            var start = DateTime.Now.Ticks;
            var timeoutTicks = timeoutMs == InfiniteTimeout ? long.MaxValue : timeoutMs * TimeSpan.TicksPerMillisecond;

            while (total < count) {
                var read = this.controller.Read(buffer, offset + total, count - total);
                if (read > 0) {
                    total += read;
                    if (timeoutMs == 0)
                        break;
                    continue;
                }

                if (timeoutMs == 0)
                    break;

                if (timeoutMs != InfiniteTimeout && DateTime.Now.Ticks - start > timeoutTicks) {
                if (total == 0)
                    throw CreateTimeoutException();
                    break;
                }

                Thread.Sleep(1);
            }

            return total;
        }

        private UartController CreateController(string portName) {
            // Full .NET uses COMx names; TinyCLR uses native API names.
            // If caller passes "COMx", try default controller as a fallback.
            if (StartsWithCom(portName))
                return UartController.GetDefault();

            return UartController.FromName(portName);
        }

        private UartParity MapParity(Parity parity) {
            switch (parity) {
                case Parity.None: return UartParity.None;
                case Parity.Odd: return UartParity.Odd;
                case Parity.Even: return UartParity.Even;
                case Parity.Mark: return UartParity.Mark;
                case Parity.Space: return UartParity.Space;
                default: throw new ArgumentOutOfRangeException(nameof(parity));
            }
        }

        private UartStopBitCount MapStopBits(StopBits stopBits) {
            switch (stopBits) {
                case StopBits.One: return UartStopBitCount.One;
                case StopBits.OnePointFive: return UartStopBitCount.OnePointFive;
                case StopBits.Two: return UartStopBitCount.Two;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stopBits));
            }
        }

        private UartHandshake MapHandshake(Handshake handshake) {
            switch (handshake) {
                case Handshake.None: return UartHandshake.None;
                case Handshake.RequestToSend: return UartHandshake.RequestToSend;
                case Handshake.XOnXOff:
                case Handshake.RequestToSendXOnXOff:
                    throw CreateTodoNotSupportedException("XOnXOff handshaking");
                default:
                    throw new ArgumentOutOfRangeException(nameof(handshake));
            }
        }

        private SerialError MapError(UartError error) {
            switch (error) {
                case UartError.Frame: return SerialError.Frame;
                case UartError.Overrun: return SerialError.Overrun;
                case UartError.BufferFull: return SerialError.RXOver;
                case UartError.ReceiveParity: return SerialError.RXParity;
                default: return SerialError.RXOver;
            }
        }

        private void OnTinyClrDataReceived(UartController sender, DataReceivedEventArgs e) {
            this.bytesToReadCache = sender.BytesToRead;

            if (this.DataReceived != null && sender.BytesToRead >= this.ReceivedBytesThreshold)
                this.DataReceived(this, new SerialDataReceivedEventArgs(SerialData.Chars));
        }

        private void OnTinyClrErrorReceived(UartController sender, ErrorReceivedEventArgs e) =>
            this.ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(this.MapError(e.Error)));

        private void OnTinyClrClearToSendChanged(UartController sender, ClearToSendChangedEventArgs e) =>
            this.PinChanged?.Invoke(this, new SerialPinChangedEventArgs(SerialPin.CtsChanged));

        private void ThrowIfDisposed() {
            if (this.disposed)
                throw new ObjectDisposedException(nameof(SerialPort));
        }

        private void ThrowIfDisposedAndClosed() {
            this.ThrowIfDisposed();
            if (!this.isOpen || this.controller == null)
                throw new InvalidOperationException("Port is closed.");
        }

        private static Exception CreateTimeoutException() => new Exception("The operation has timed out.");

        private static NotSupportedException CreateTodoNotSupportedException(string feature) =>
            new NotSupportedException("TODO-Not supported: " + feature);

        private static bool StartsWithCom(string value) {
            if (value == null || value.Length < 3)
                return false;

            var c0 = value[0];
            var c1 = value[1];
            var c2 = value[2];

            if (c0 >= 'a' && c0 <= 'z') c0 = (char)(c0 - 32);
            if (c1 >= 'a' && c1 <= 'z') c1 = (char)(c1 - 32);
            if (c2 >= 'a' && c2 <= 'z') c2 = (char)(c2 - 32);

            return c0 == 'C' && c1 == 'O' && c2 == 'M';
        }

        private static bool EndsWith(string value, string suffix) {
            if (value == null || suffix == null || value.Length < suffix.Length)
                return false;

            var start = value.Length - suffix.Length;
            for (var i = 0; i < suffix.Length; i++)
                if (value[start + i] != suffix[i])
                    return false;

            return true;
        }
    }
}
