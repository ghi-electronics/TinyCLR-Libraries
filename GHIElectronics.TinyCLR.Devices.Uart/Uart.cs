using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Uart.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Uart {
    /// <summary>
    /// Represents a UART (serial) port. Configure via <see cref="SetActiveSettings(UartSetting)"/>,
    /// then <see cref="Enable"/> the port and exchange bytes through <see cref="Read(byte[])"/>
    /// / <see cref="Write(byte[])"/>. Subscribe to <see cref="DataReceived"/> for
    /// event-driven receive instead of polling <see cref="BytesToRead"/>.
    /// </summary>
    public class UartController : IDisposable {
        private ClearToSendChangedEventHandler clearToSendChangedCallbacks;
        private DataReceivedEventHandler dataReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        /// <summary>The low-level provider backing this controller.</summary>
        public IUartControllerProvider Provider { get; }

        private UartController(IUartControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default UART controller for this device.</summary>
        public static UartController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UartController) is UartController c ? c : UartController.FromName(NativeApi.GetDefaultName(NativeApiType.UartController));
        /// <summary>Returns a UART controller identified by its native API name.</summary>
        public static UartController FromName(string name) => UartController.FromProvider(new UartControllerApiWrapper(NativeApi.Find(name, NativeApiType.UartController)));
        /// <summary>Creates a controller from a custom <see cref="IUartControllerProvider"/>.</summary>
        public static UartController FromProvider(IUartControllerProvider provider) => new UartController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Powers on the port. Call after <see cref="SetActiveSettings(UartSetting)"/>.</summary>
        public void Enable() => this.Provider.Enable();
        /// <summary>Powers off the port.</summary>
        public void Disable() => this.Provider.Disable();

        /// <summary>Applies a complete set of serial settings (baud, framing, handshake, polarity).</summary>
        /// <param name="setting">Settings to apply.</param>
        public void SetActiveSettings(UartSetting setting) => this.Provider.SetActiveSettings(setting.BaudRate, setting.DataBits, setting.Parity, setting.StopBits, setting.Handshaking, setting.EnableDePin, setting.InvertTxPolarity, setting.InvertRxPolarity, setting.InvertBinaryData, setting.SwapTxRxPin, setting.InvertDePolarity);
        /// <summary>Blocks until all buffered TX bytes have been shifted out.</summary>
        public void Flush() => this.Provider.Flush();

        /// <summary>Reads up to <paramref name="buffer"/>.Length bytes; returns the count actually read.</summary>
        public int Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        /// <summary>Reads up to <paramref name="length"/> bytes into <paramref name="buffer"/> at <paramref name="offset"/>.</summary>
        /// <returns>Number of bytes actually read (may be less than requested).</returns>
        public int Read(byte[] buffer, int offset, int length) => this.Provider.Read(buffer, offset, length);

        /// <summary>Writes <paramref name="buffer"/>.Length bytes.</summary>
        public int Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        /// <summary>Writes <paramref name="length"/> bytes from <paramref name="buffer"/> at <paramref name="offset"/>.</summary>
        public int Write(byte[] buffer, int offset, int length) => this.Write(buffer, offset, length, TimeSpan.Zero);
        /// <summary>
        /// Writes a slice and optionally follows it with a break condition.
        /// A non-zero <paramref name="breakDuration"/> flushes the TX FIFO first.
        /// </summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="length">Number of bytes to write.</param>
        /// <param name="breakDuration">Length of the break condition to assert after the write; <see cref="TimeSpan.Zero"/> means no break.</param>
        /// <returns>Number of bytes actually queued.</returns>
        public int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration) {
            if (breakDuration != TimeSpan.Zero && this.BytesToWrite > 0) {
                this.Flush();

                System.Threading.Thread.Sleep(1);
            }

            return this.Provider.Write(buffer, offset, length, breakDuration);
        }

        /// <summary>Empties the transmit buffer.</summary>
        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        /// <summary>Empties the receive buffer.</summary>
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        /// <summary>Size in bytes of the transmit buffer.</summary>
        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        /// <summary>Size in bytes of the receive buffer.</summary>
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }
        /// <summary>Bytes currently queued for transmission.</summary>
        public int BytesToWrite => this.Provider.BytesToWrite;
        /// <summary>Bytes currently available to read.</summary>
        public int BytesToRead => this.Provider.BytesToRead;

        /// <summary>Drives the RTS line when hardware handshaking is enabled.</summary>
        public bool IsRequestToSendEnabled { get => this.Provider.IsRequestToSendEnabled; set => this.Provider.IsRequestToSendEnabled = value; }
        /// <summary>Current state of the CTS line.</summary>
        public bool ClearToSendState => this.Provider.ClearToSendState;

        private void OnClearToSendChanged(UartController sender, ClearToSendChangedEventArgs e) => this.clearToSendChangedCallbacks?.Invoke(this, e);
        private void OnDataReceived(UartController sender, DataReceivedEventArgs e) => this.dataReceivedCallbacks?.Invoke(this, e);

        private void OnErrorReceived(UartController sender, ErrorReceivedEventArgs e) => this.errorReceivedCallbacks?.Invoke(this, e);

        /// <summary>Raised when the CTS input changes state.</summary>
        public event ClearToSendChangedEventHandler ClearToSendChanged {
            add {
                if (this.clearToSendChangedCallbacks == null)
                    this.Provider.ClearToSendChanged += this.OnClearToSendChanged;

                this.clearToSendChangedCallbacks += value;
            }
            remove {
                this.clearToSendChangedCallbacks -= value;

                if (this.clearToSendChangedCallbacks == null)
                    this.Provider.ClearToSendChanged -= this.OnClearToSendChanged;
            }
        }

        /// <summary>
        /// Raised when receive data is available. <see cref="DataReceivedEventArgs.Count"/>
        /// gives the number of bytes that have just been buffered; call <see cref="Read(byte[])"/>
        /// from the handler to consume them.
        /// </summary>
        public event DataReceivedEventHandler DataReceived {
            add {
                if (this.dataReceivedCallbacks == null)
                    this.Provider.DataReceived += this.OnDataReceived;

                this.dataReceivedCallbacks += value;
            }
            remove {
                this.dataReceivedCallbacks -= value;

                if (this.dataReceivedCallbacks == null)
                    this.Provider.DataReceived -= this.OnDataReceived;
            }
        }

        /// <summary>Raised when the controller detects a frame, parity, or buffer error.</summary>
        public event ErrorReceivedEventHandler ErrorReceived {
            add {
                if (this.errorReceivedCallbacks == null)
                    this.Provider.ErrorReceived += this.OnErrorReceived;

                this.errorReceivedCallbacks += value;
            }
            remove {
                this.errorReceivedCallbacks -= value;

                if (this.errorReceivedCallbacks == null)
                    this.Provider.ErrorReceived -= this.OnErrorReceived;
            }
        }
    }

    /// <summary>Parity bit policy for a UART frame.</summary>
    public enum UartParity {
        /// <summary>No parity bit transmitted.</summary>
        None = 0,
        /// <summary>Odd parity.</summary>
        Odd = 1,
        /// <summary>Even parity.</summary>
        Even = 2,
        /// <summary>Mark parity (parity bit always 1).</summary>
        Mark = 3,
        /// <summary>Space parity (parity bit always 0).</summary>
        Space = 4,
    }

    /// <summary>Number of stop bits per frame.</summary>
    public enum UartStopBitCount {
        /// <summary>One stop bit.</summary>
        One = 0,
        /// <summary>One and a half stop bits.</summary>
        OnePointFive = 1,
        /// <summary>Two stop bits.</summary>
        Two = 2,
    }

    /// <summary>Flow-control policy.</summary>
    public enum UartHandshake {
        /// <summary>No flow control.</summary>
        None = 0,
        /// <summary>Hardware RTS/CTS flow control.</summary>
        RequestToSend = 1,
    }

    /// <summary>Categories of error reported via <see cref="UartController.ErrorReceived"/>.</summary>
    public enum UartError {
        /// <summary>Stop bit not detected at the expected time.</summary>
        Frame = 0,
        /// <summary>Receive overrun — incoming byte arrived before the previous one was consumed.</summary>
        Overrun = 1,
        /// <summary>Internal buffer full; subsequent bytes will be dropped.</summary>
        BufferFull = 2,
        /// <summary>Parity bit did not match.</summary>
        ReceiveParity = 3,
    }

    /// <summary>Aggregate configuration for a UART port — passed to <see cref="UartController.SetActiveSettings(UartSetting)"/>.</summary>
    public class UartSetting {
        /// <summary>Bits per second.</summary>
        public int BaudRate { get; set; }
        /// <summary>Frame width in data bits (typically 7 or 8).</summary>
        public int DataBits { get; set; }
        /// <summary>Parity policy.</summary>
        public UartParity Parity { get; set; }
        /// <summary>Stop-bit policy.</summary>
        public UartStopBitCount StopBits { get; set; }
        /// <summary>Flow-control mode.</summary>
        public UartHandshake Handshaking { get; set; }

        /// <summary>Drive the DE (driver-enable) line for RS-485 transceivers during TX.</summary>
        public bool EnableDePin { get; set; }

        /// <summary>When true, the TX line is inverted on the wire.</summary>
        public bool InvertTxPolarity { get; set; }
        /// <summary>When true, the RX line is inverted on the wire.</summary>
        public bool InvertRxPolarity { get; set; }
        /// <summary>When true, the bit values themselves are inverted (idle low becomes idle high).</summary>
        public bool InvertBinaryData { get; set; }
        /// <summary>When true, the TX and RX pin assignments are swapped.</summary>
        public bool SwapTxRxPin { get; set; }
        /// <summary>When true, the DE line is asserted low instead of high.</summary>
        public bool InvertDePolarity { get; set; }

    }

    /// <summary>Handler signature for <see cref="UartController.ClearToSendChanged"/>.</summary>
    public delegate void ClearToSendChangedEventHandler(UartController sender, ClearToSendChangedEventArgs e);
    /// <summary>Handler signature for <see cref="UartController.DataReceived"/>.</summary>
    public delegate void DataReceivedEventHandler(UartController sender, DataReceivedEventArgs e);
    /// <summary>Handler signature for <see cref="UartController.ErrorReceived"/>.</summary>
    public delegate void ErrorReceivedEventHandler(UartController sender, ErrorReceivedEventArgs e);

    /// <summary>Arguments for <see cref="UartController.ClearToSendChanged"/>.</summary>
    public sealed class ClearToSendChangedEventArgs {
        /// <summary>New CTS state.</summary>
        public bool State { get; }
        /// <summary>Driver-captured time of the transition.</summary>
        public DateTime Timestamp { get; }

        internal ClearToSendChangedEventArgs(bool state, DateTime timestamp) {
            this.State = state;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Arguments for <see cref="UartController.DataReceived"/>.</summary>
    public class DataReceivedEventArgs {
        /// <summary>Number of bytes that have just become available to read.</summary>
        public int Count { get; }
        /// <summary>Driver-captured time of the receive event.</summary>
        public DateTime Timestamp { get; }

        internal DataReceivedEventArgs(int count, DateTime timestamp) {
            this.Count = count;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Arguments for break-condition events (reserved; not raised by the standard controller).</summary>
    public class BreakDetectedEventArgs {
        /// <summary>Driver-captured time of the break.</summary>
        public DateTime Timestamp { get; }

        internal BreakDetectedEventArgs(DateTime timestamp) => this.Timestamp = timestamp;
    }
    /// <summary>Arguments for <see cref="UartController.ErrorReceived"/>.</summary>
    public class ErrorReceivedEventArgs {
        /// <summary>The kind of error detected.</summary>
        public UartError Error { get; }
        /// <summary>Driver-captured time of the error.</summary>
        public DateTime Timestamp { get; }

        internal ErrorReceivedEventArgs(UartError error, DateTime timestamp) {
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    namespace Provider {
        /// <summary>Provider contract for a UART controller.</summary>
        public interface IUartControllerProvider : IDisposable {
            /// <summary>Powers on the port.</summary>
            void Enable();
            /// <summary>Powers off the port.</summary>
            void Disable();

            /// <summary>Applies a complete set of serial settings.</summary>
            void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking, bool enableDePin, bool invertTxPolarity, bool invertRxPolarity, bool invertBinaryData, bool swapTxRxPin, bool invertDePolarity);
            /// <summary>Blocks until all buffered TX bytes have been shifted out.</summary>
            void Flush();
            /// <summary>Reads up to <paramref name="length"/> bytes; returns the count actually read.</summary>
            int Read(byte[] buffer, int offset, int length);
            /// <summary>Writes <paramref name="length"/> bytes; optional trailing break of <paramref name="breakDuration"/>.</summary>
            int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration);

            /// <summary>Empties the transmit buffer.</summary>
            void ClearWriteBuffer();
            /// <summary>Empties the receive buffer.</summary>
            void ClearReadBuffer();

            /// <summary>Size in bytes of the transmit buffer.</summary>
            int WriteBufferSize { get; set; }
            /// <summary>Size in bytes of the receive buffer.</summary>
            int ReadBufferSize { get; set; }
            /// <summary>Bytes currently queued for transmission.</summary>
            int BytesToWrite { get; }
            /// <summary>Bytes currently available to read.</summary>
            int BytesToRead { get; }

            /// <summary>Drives the RTS line when hardware handshaking is enabled.</summary>
            bool IsRequestToSendEnabled { get; set; }
            /// <summary>Current state of the CTS line.</summary>
            bool ClearToSendState { get; }

            /// <summary>Raised when CTS changes state.</summary>
            event ClearToSendChangedEventHandler ClearToSendChanged;
            /// <summary>Raised when receive data becomes available.</summary>
            event DataReceivedEventHandler DataReceived;
            /// <summary>Raised when a frame/parity/overrun error is detected.</summary>
            event ErrorReceivedEventHandler ErrorReceived;
        }

        /// <summary>Concrete <see cref="IUartControllerProvider"/> backed by the native TinyCLR UART HAL.</summary>
        public sealed class UartControllerApiWrapper : IUartControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher clearToSendChangedDispatcher;
            private readonly NativeEventDispatcher dataReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;
            private ClearToSendChangedEventHandler clearToSendChangedCallbacks;
            private DataReceivedEventHandler dataReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public UartControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.clearToSendChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.ClearToSendChanged");
                this.dataReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.DataReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.ErrorReceived");

                this.clearToSendChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.clearToSendChangedCallbacks?.Invoke(null, new ClearToSendChangedEventArgs(d0 != 0, ts)); };
                this.dataReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.dataReceivedCallbacks?.Invoke(null, new DataReceivedEventArgs((int)d0, ts)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.errorReceivedCallbacks?.Invoke(null, new ErrorReceivedEventArgs((UartError)d0, ts)); };
            }

            /// <inheritdoc/>
            public event ClearToSendChangedEventHandler ClearToSendChanged {
                add {
                    if (this.clearToSendChangedCallbacks == null)
                        this.SetClearToSendChangedEventEnabled(true);

                    this.clearToSendChangedCallbacks += value;
                }
                remove {
                    this.clearToSendChangedCallbacks -= value;

                    if (this.clearToSendChangedCallbacks == null)
                        this.SetClearToSendChangedEventEnabled(false);
                }
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public event ErrorReceivedEventHandler ErrorReceived {
                add {
                    if (this.errorReceivedCallbacks == null)
                        this.SetErrorReceivedEventEnabled(true);

                    this.errorReceivedCallbacks += value;
                }
                remove {
                    this.errorReceivedCallbacks -= value;

                    if (this.errorReceivedCallbacks == null)
                        this.SetErrorReceivedEventEnabled(false);
                }
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetClearToSendChangedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetErrorReceivedEventEnabled(bool enabled);

            /// <inheritdoc/>
            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            /// <inheritdoc/>
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            /// <inheritdoc/>
            public extern int BytesToWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int BytesToRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            public extern bool IsRequestToSendEnabled { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            /// <inheritdoc/>
            public extern bool ClearToSendState { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking, bool enableDePin, bool invertTxPolarity, bool invertRxPolarity, bool invertBinaryData, bool swapTxRxPin, bool invertDePolarity);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] buffer, int offset, int length);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();
        }
    }
}
