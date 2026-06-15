using System;
using System.Text;
using System.Threading;

namespace System.IO.Ports {
    /// <summary>Type of data received on the serial port.</summary>
    [Flags]
    public enum SerialData {
        /// <summary>Data other than the end-of-file character was received.</summary>
        Chars = 1,
        /// <summary>The end-of-file character was received.</summary>
        Eof = 2,
    }

    /// <summary>Errors that can occur on the serial port.</summary>
    [Flags]
    public enum SerialError {
        /// <summary>The transmit buffer is full.</summary>
        TXFull = 0x100,
        /// <summary>The receive buffer overflowed.</summary>
        RXOver = 1,
        /// <summary>A character was received before the previous one was read (hardware overrun).</summary>
        Overrun = 2,
        /// <summary>A parity error was detected.</summary>
        RXParity = 4,
        /// <summary>A framing error was detected.</summary>
        Frame = 8,
    }

    /// <summary>Serial control-pin changes that can raise the pin-changed event.</summary>
    [Flags]
    public enum SerialPin {
        /// <summary>Clear-to-Send changed.</summary>
        CtsChanged = 0x08,
        /// <summary>Data-Set-Ready changed.</summary>
        DsrChanged = 0x10,
        /// <summary>Carrier-Detect changed.</summary>
        CDChanged = 0x20,
        /// <summary>A break was detected.</summary>
        Break = 0x40,
        /// <summary>A ring indicator was detected.</summary>
        Ring = 0x100,
    }

    /// <summary>Number of stop bits per frame.</summary>
    public enum StopBits {
        /// <summary>No stop bits (not supported by most hardware).</summary>
        None = 0,
        /// <summary>One stop bit.</summary>
        One = 1,
        /// <summary>Two stop bits.</summary>
        Two = 2,
        /// <summary>One and a half stop bits.</summary>
        OnePointFive = 3,
    }

    /// <summary>Flow-control method.</summary>
    public enum Handshake {
        /// <summary>No flow control.</summary>
        None = 0,
        /// <summary>Software (XON/XOFF) flow control.</summary>
        XOnXOff = 1,
        /// <summary>Hardware (RTS/CTS) flow control.</summary>
        RequestToSend = 2,
        /// <summary>Both hardware and software flow control.</summary>
        RequestToSendXOnXOff = 3,
    }

    /// <summary>Parity-checking scheme.</summary>
    public enum Parity {
        /// <summary>No parity bit.</summary>
        None = 0,
        /// <summary>Odd parity.</summary>
        Odd = 1,
        /// <summary>Even parity.</summary>
        Even = 2,
        /// <summary>Parity bit always 1.</summary>
        Mark = 3,
        /// <summary>Parity bit always 0.</summary>
        Space = 4,
    }

    /// <summary>Handler for the <see cref="SerialPort.DataReceived"/> event.</summary>
    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
    /// <summary>Handler for the <see cref="SerialPort.ErrorReceived"/> event.</summary>
    public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e);
    /// <summary>Handler for the <see cref="SerialPort.PinChanged"/> event.</summary>
    public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e);

    /// <summary>Arguments for the data-received event.</summary>
    public class SerialDataReceivedEventArgs : EventArgs {
        /// <summary>The kind of data that was received.</summary>
        public SerialData EventType { get; }
        /// <summary>Creates the event arguments.</summary>
        public SerialDataReceivedEventArgs(SerialData eventType) => this.EventType = eventType;
    }

    /// <summary>Arguments for the error-received event.</summary>
    public class SerialErrorReceivedEventArgs : EventArgs {
        /// <summary>The error that occurred.</summary>
        public SerialError EventType { get; }
        /// <summary>Creates the event arguments.</summary>
        public SerialErrorReceivedEventArgs(SerialError eventType) => this.EventType = eventType;
    }

    /// <summary>Arguments for the pin-changed event.</summary>
    public class SerialPinChangedEventArgs : EventArgs {
        /// <summary>The control pin that changed.</summary>
        public SerialPin EventType { get; }
        /// <summary>Creates the event arguments.</summary>
        public SerialPinChangedEventArgs(SerialPin eventType) => this.EventType = eventType;
    }

    /// <summary>
    /// .NET-style serial port. Same surface as <c>System.IO.Ports.SerialPort</c>;
    /// internally routes through TinyCLR's <see cref="GHIElectronics.TinyCLR.Devices.Uart.UartController"/>.
    /// </summary>
    public class SerialPort : IDisposable {
        private GHIElectronics.TinyCLR.Devices.Uart.UartController controller;
        private readonly object sync;
        private bool disposed;
        private bool isOpen;
        private int bytesToReadCache;

        /// <summary>The port name, e.g. "COM1" or a TinyCLR UART API name.</summary>
        public string PortName { get; set; }
        /// <summary>The baud rate. Defaults to 9600.</summary>
        public int BaudRate { get; set; } = 9600;
        /// <summary>The parity scheme. Defaults to none.</summary>
        public Parity Parity { get; set; } = Parity.None;
        /// <summary>Bits per byte. Defaults to 8.</summary>
        public int DataBits { get; set; } = 8;
        /// <summary>Number of stop bits. Defaults to one.</summary>
        public StopBits StopBits { get; set; } = StopBits.One;
        /// <summary>The flow-control method. Defaults to none.</summary>
        public Handshake Handshake { get; set; } = Handshake.None;
        /// <summary>Read timeout in milliseconds, or <see cref="InfiniteTimeout"/>.</summary>
        public int ReadTimeout { get; set; } = InfiniteTimeout;
        /// <summary>Write timeout in milliseconds, or <see cref="InfiniteTimeout"/>.</summary>
        public int WriteTimeout { get; set; } = InfiniteTimeout;
        /// <summary>Receive buffer size in bytes. Defaults to 256.</summary>
        public int ReadBufferSize { get; set; } = 256;
        /// <summary>Transmit buffer size in bytes. Defaults to 256.</summary>
        public int WriteBufferSize { get; set; } = 256;
        /// <summary>Bytes that must be buffered before the data-received event fires. Defaults to 1.</summary>
        public int ReceivedBytesThreshold { get; set; } = 1;
        /// <summary>The line terminator used by <see cref="ReadLine"/> and <see cref="WriteLine"/>. Defaults to "\n".</summary>
        public string NewLine { get; set; } = "\n";
        /// <summary>The text encoding used by the string read/write methods. Defaults to UTF-8.</summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>True if the port is open.</summary>
        public bool IsOpen => this.isOpen;
        /// <summary>Number of bytes available to read.</summary>
        public int BytesToRead => this.controller != null ? this.controller.BytesToRead : this.bytesToReadCache;
        /// <summary>Number of bytes waiting to be sent.</summary>
        public int BytesToWrite => this.controller != null ? this.controller.BytesToWrite : 0;

        /// <summary>State of the Clear-to-Send line.</summary>
        public bool CtsHolding => this.controller != null && this.controller.ClearToSendState;
        /// <summary>State of the Carrier-Detect line. Not supported.</summary>
        public bool CDHolding {
            get => throw CreateTodoNotSupportedException("CDHolding (Carrier Detect state)");
        }

        /// <summary>State of the Data-Set-Ready line. Not supported.</summary>
        public bool DsrHolding {
            get => throw CreateTodoNotSupportedException("DsrHolding (DSR state)");
        }

        /// <summary>Whether the port is in a break state. Not supported.</summary>
        public bool BreakState {
            get => throw CreateTodoNotSupportedException("BreakState getter");
            set => throw CreateTodoNotSupportedException("BreakState setter");
        }

        /// <summary>Whether the Data-Terminal-Ready line is enabled. Not supported.</summary>
        public bool DtrEnable {
            get => throw CreateTodoNotSupportedException("DtrEnable getter");
            set => throw CreateTodoNotSupportedException("DtrEnable setter");
        }

        /// <summary>Whether the Request-to-Send line is enabled.</summary>
        public bool RtsEnable {
            get => this.controller != null && this.controller.IsRequestToSendEnabled;
            set {
                this.ThrowIfDisposed();
                if (!this.isOpen)
                    throw new InvalidOperationException("Port is closed.");

                this.controller.IsRequestToSendEnabled = value;
            }
        }

        /// <summary>Value meaning "no timeout" for the timeout properties.</summary>
        public const int InfiniteTimeout = -1;

        /// <summary>Raised when data is received.</summary>
        public event SerialDataReceivedEventHandler DataReceived;
        /// <summary>Raised when a receive error occurs.</summary>
        public event SerialErrorReceivedEventHandler ErrorReceived;
        /// <summary>Raised when a control pin changes.</summary>
        public event SerialPinChangedEventHandler PinChanged;

        /// <summary>Creates a port using "COM1".</summary>
        public SerialPort() : this("COM1") {
        }

        /// <summary>Creates a port for the given port name.</summary>
        public SerialPort(string portName) {
            this.sync = new object();
            this.PortName = portName;
        }

        /// <summary>Creates a port with the given name and baud rate.</summary>
        public SerialPort(string portName, int baudRate) : this(portName) => this.BaudRate = baudRate;
        /// <summary>Creates a port with the given name, baud rate, and parity.</summary>
        public SerialPort(string portName, int baudRate, Parity parity) : this(portName, baudRate) => this.Parity = parity;
        /// <summary>Creates a port with the given name, baud rate, parity, and data bits.</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : this(portName, baudRate, parity) => this.DataBits = dataBits;
        /// <summary>Creates a port with the given name, baud rate, parity, data bits, and stop bits.</summary>
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : this(portName, baudRate, parity, dataBits) => this.StopBits = stopBits;

        /// <summary>Opens the port using the current settings.</summary>
        public void Open() {
            this.ThrowIfDisposed();
            if (this.isOpen)
                throw new InvalidOperationException("Port is already open.");

            if (string.IsNullOrEmpty(this.PortName))
                throw new ArgumentException("PortName cannot be null or empty.");

            this.controller = this.CreateController(this.PortName);

            var settings = new GHIElectronics.TinyCLR.Devices.Uart.UartSetting {
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

        /// <summary>Closes the port.</summary>
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

        /// <summary>Closes the port and releases its resources.</summary>
        public void Dispose() {
            if (this.disposed)
                return;

            this.Close();
            this.disposed = true;
        }

        /// <summary>Reads up to <paramref name="count"/> bytes into the buffer. Returns the number of bytes read.</summary>
        public int Read(byte[] buffer, int offset, int count) {
            this.ThrowIfDisposedAndClosed();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            return this.ReadCore(buffer, offset, count, this.ReadTimeout);
        }

        /// <summary>Reads a single byte. Throws on timeout.</summary>
        public int ReadByte() {
            var b = new byte[1];
            var read = this.Read(b, 0, 1);
            if (read == 0)
                throw CreateTimeoutException();
            return b[0];
        }

        /// <summary>Reads a single character. Throws on timeout.</summary>
        public int ReadChar() {
            var ch = new char[1];
            var read = this.Read(ch, 0, 1);
            if (read == 0)
                throw CreateTimeoutException();
            return ch[0];
        }

        /// <summary>Reads characters into the buffer. Not supported.</summary>
        public int Read(char[] buffer, int offset, int count) {
            this.ThrowIfDisposedAndClosed();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            throw CreateTodoNotSupportedException("Read(char[], int, int) with full .NET decoder parity");
        }

        /// <summary>Reads all bytes currently available and returns them as a string.</summary>
        public string ReadExisting() {
            this.ThrowIfDisposedAndClosed();
            var available = this.BytesToRead;
            if (available <= 0)
                return string.Empty;

            var data = new byte[available];
            var read = this.controller.Read(data, 0, data.Length);
            return this.Encoding.GetString(data, 0, read);
        }

        /// <summary>Reads up to the <see cref="NewLine"/> terminator and returns the line.</summary>
        public string ReadLine() => this.ReadTo(this.NewLine);

        /// <summary>Reads up to the given delimiter and returns the text before it.</summary>
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

        /// <summary>Writes a string using the current encoding.</summary>
        public void Write(string text) {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var data = this.Encoding.GetBytes(text);
            this.Write(data, 0, data.Length);
        }

        /// <summary>Writes a string followed by the <see cref="NewLine"/> terminator.</summary>
        public void WriteLine(string text) => this.Write((text ?? string.Empty) + this.NewLine);

        /// <summary>Writes <paramref name="count"/> bytes from the buffer.</summary>
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

        /// <summary>Writes <paramref name="count"/> characters from the buffer using the current encoding.</summary>
        public void Write(char[] buffer, int offset, int count) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException();

            var text = string.Empty;
            for (var i = 0; i < count; i++)
                text += buffer[offset + i];

            var data = this.Encoding.GetBytes(text);
            this.Write(data, 0, data.Length);
        }

        /// <summary>Discards the contents of the receive buffer.</summary>
        public void DiscardInBuffer() {
            this.ThrowIfDisposedAndClosed();
            this.controller.ClearReadBuffer();
        }

        /// <summary>Discards the contents of the transmit buffer.</summary>
        public void DiscardOutBuffer() {
            this.ThrowIfDisposedAndClosed();
            this.controller.ClearWriteBuffer();
        }

        /// <summary>Returns the available port names. Not supported.</summary>
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

        private GHIElectronics.TinyCLR.Devices.Uart.UartController CreateController(string portName) {
            if (StartsWithCom(portName))
                return GHIElectronics.TinyCLR.Devices.Uart.UartController.GetDefault();

            return GHIElectronics.TinyCLR.Devices.Uart.UartController.FromName(portName);
        }

        private GHIElectronics.TinyCLR.Devices.Uart.UartParity MapParity(Parity parity) {
            switch (parity) {
                case Parity.None: return GHIElectronics.TinyCLR.Devices.Uart.UartParity.None;
                case Parity.Odd: return GHIElectronics.TinyCLR.Devices.Uart.UartParity.Odd;
                case Parity.Even: return GHIElectronics.TinyCLR.Devices.Uart.UartParity.Even;
                case Parity.Mark: return GHIElectronics.TinyCLR.Devices.Uart.UartParity.Mark;
                case Parity.Space: return GHIElectronics.TinyCLR.Devices.Uart.UartParity.Space;
                default: throw new ArgumentOutOfRangeException(nameof(parity));
            }
        }

        private GHIElectronics.TinyCLR.Devices.Uart.UartStopBitCount MapStopBits(StopBits stopBits) {
            switch (stopBits) {
                case StopBits.One: return GHIElectronics.TinyCLR.Devices.Uart.UartStopBitCount.One;
                case StopBits.OnePointFive: return GHIElectronics.TinyCLR.Devices.Uart.UartStopBitCount.OnePointFive;
                case StopBits.Two: return GHIElectronics.TinyCLR.Devices.Uart.UartStopBitCount.Two;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stopBits));
            }
        }

        private GHIElectronics.TinyCLR.Devices.Uart.UartHandshake MapHandshake(Handshake handshake) {
            switch (handshake) {
                case Handshake.None: return GHIElectronics.TinyCLR.Devices.Uart.UartHandshake.None;
                case Handshake.RequestToSend: return GHIElectronics.TinyCLR.Devices.Uart.UartHandshake.RequestToSend;
                case Handshake.XOnXOff:
                case Handshake.RequestToSendXOnXOff:
                    throw CreateTodoNotSupportedException("XOnXOff handshaking");
                default:
                    throw new ArgumentOutOfRangeException(nameof(handshake));
            }
        }

        private SerialError MapError(GHIElectronics.TinyCLR.Devices.Uart.UartError error) {
            switch (error) {
                case GHIElectronics.TinyCLR.Devices.Uart.UartError.Frame: return SerialError.Frame;
                case GHIElectronics.TinyCLR.Devices.Uart.UartError.Overrun: return SerialError.Overrun;
                case GHIElectronics.TinyCLR.Devices.Uart.UartError.BufferFull: return SerialError.RXOver;
                case GHIElectronics.TinyCLR.Devices.Uart.UartError.ReceiveParity: return SerialError.RXParity;
                default: return SerialError.RXOver;
            }
        }

        private void OnTinyClrDataReceived(GHIElectronics.TinyCLR.Devices.Uart.UartController sender, GHIElectronics.TinyCLR.Devices.Uart.DataReceivedEventArgs e) {
            this.bytesToReadCache = sender.BytesToRead;

            if (this.DataReceived != null && sender.BytesToRead >= this.ReceivedBytesThreshold)
                this.DataReceived(this, new SerialDataReceivedEventArgs(SerialData.Chars));
        }

        private void OnTinyClrErrorReceived(GHIElectronics.TinyCLR.Devices.Uart.UartController sender, GHIElectronics.TinyCLR.Devices.Uart.ErrorReceivedEventArgs e) =>
            this.ErrorReceived?.Invoke(this, new SerialErrorReceivedEventArgs(this.MapError(e.Error)));

        private void OnTinyClrClearToSendChanged(GHIElectronics.TinyCLR.Devices.Uart.UartController sender, GHIElectronics.TinyCLR.Devices.Uart.ClearToSendChangedEventArgs e) =>
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
