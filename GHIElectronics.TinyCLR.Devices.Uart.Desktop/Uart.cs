using System;
using GHIElectronics.TinyCLR.Devices.Uart.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Uart\Uart.cs.
// Bodies on Desktop are safe no-ops:
//   * Read returns 0 bytes.
//   * Write returns count (claims success).
//   * Buffer-size getters return current stored value; setters round-trip.
//   * Events stored but never raised.
namespace GHIElectronics.TinyCLR.Devices.Uart {
    public class UartController : IDisposable {
        private ClearToSendChangedEventHandler clearToSendChangedCallbacks;
        private DataReceivedEventHandler dataReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        public IUartControllerProvider Provider { get; }

        private UartController(IUartControllerProvider provider) => this.Provider = provider;

        public static UartController GetDefault() => UartController.FromName("Simulator");
        public static UartController FromName(string name) => UartController.FromProvider(new UartControllerApiWrapper(NativeApi.Find(name, NativeApiType.UartController)));
        public static UartController FromProvider(IUartControllerProvider provider) => new UartController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public void SetActiveSettings(UartSetting setting) => this.Provider.SetActiveSettings(setting.BaudRate, setting.DataBits, setting.Parity, setting.StopBits, setting.Handshaking, setting.EnableDePin, setting.InvertTxPolarity, setting.InvertRxPolarity, setting.InvertBinaryData, setting.SwapTxRxPin, setting.InvertDePolarity);
        public void Flush() => this.Provider.Flush();

        public int Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        public int Read(byte[] buffer, int offset, int length) => this.Provider.Read(buffer, offset, length);

        public int Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        public int Write(byte[] buffer, int offset, int length) => this.Write(buffer, offset, length, TimeSpan.Zero);
        public int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration) => this.Provider.Write(buffer, offset, length, breakDuration);

        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }
        public int BytesToWrite => this.Provider.BytesToWrite;
        public int BytesToRead => this.Provider.BytesToRead;

        public bool IsRequestToSendEnabled { get => this.Provider.IsRequestToSendEnabled; set => this.Provider.IsRequestToSendEnabled = value; }
        public bool ClearToSendState => this.Provider.ClearToSendState;

        private void OnClearToSendChanged(UartController sender, ClearToSendChangedEventArgs e) => this.clearToSendChangedCallbacks?.Invoke(this, e);
        private void OnDataReceived(UartController sender, DataReceivedEventArgs e) => this.dataReceivedCallbacks?.Invoke(this, e);
        private void OnErrorReceived(UartController sender, ErrorReceivedEventArgs e) => this.errorReceivedCallbacks?.Invoke(this, e);

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

    public enum UartParity {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4,
    }

    public enum UartStopBitCount {
        One = 0,
        OnePointFive = 1,
        Two = 2,
    }

    public enum UartHandshake {
        None = 0,
        RequestToSend = 1,
    }

    public enum UartError {
        Frame = 0,
        Overrun = 1,
        BufferFull = 2,
        ReceiveParity = 3,
    }

    public class UartSetting {
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public UartParity Parity { get; set; }
        public UartStopBitCount StopBits { get; set; }
        public UartHandshake Handshaking { get; set; }

        public bool EnableDePin { get; set; }

        public bool InvertTxPolarity { get; set; }
        public bool InvertRxPolarity { get; set; }
        public bool InvertBinaryData { get; set; }
        public bool SwapTxRxPin { get; set; }
        public bool InvertDePolarity { get; set; }
    }

    public delegate void ClearToSendChangedEventHandler(UartController sender, ClearToSendChangedEventArgs e);
    public delegate void DataReceivedEventHandler(UartController sender, DataReceivedEventArgs e);
    public delegate void ErrorReceivedEventHandler(UartController sender, ErrorReceivedEventArgs e);

    public sealed class ClearToSendChangedEventArgs {
        public bool State { get; }
        public DateTime Timestamp { get; }

        internal ClearToSendChangedEventArgs(bool state, DateTime timestamp) {
            this.State = state;
            this.Timestamp = timestamp;
        }
    }

    public class DataReceivedEventArgs {
        public int Count { get; }
        public DateTime Timestamp { get; }

        internal DataReceivedEventArgs(int count, DateTime timestamp) {
            this.Count = count;
            this.Timestamp = timestamp;
        }
    }

    public class BreakDetectedEventArgs {
        public DateTime Timestamp { get; }

        internal BreakDetectedEventArgs(DateTime timestamp) => this.Timestamp = timestamp;
    }

    public class ErrorReceivedEventArgs {
        public UartError Error { get; }
        public DateTime Timestamp { get; }

        internal ErrorReceivedEventArgs(UartError error, DateTime timestamp) {
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    namespace Provider {
        public interface IUartControllerProvider : IDisposable {
            void Enable();
            void Disable();

            void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking, bool enableDePin, bool invertTxPolarity, bool invertRxPolarity, bool invertBinaryData, bool swapTxRxPin, bool invertDePolarity);
            void Flush();
            int Read(byte[] buffer, int offset, int length);
            int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration);

            void ClearWriteBuffer();
            void ClearReadBuffer();

            int WriteBufferSize { get; set; }
            int ReadBufferSize { get; set; }
            int BytesToWrite { get; }
            int BytesToRead { get; }

            bool IsRequestToSendEnabled { get; set; }
            bool ClearToSendState { get; }

            event ClearToSendChangedEventHandler ClearToSendChanged;
            event DataReceivedEventHandler DataReceived;
            event ErrorReceivedEventHandler ErrorReceived;
        }

        // No-op provider. Read returns 0 bytes; Write claims full count
        // (so callers don't think it timed out). Buffer sizes round-trip.
        public sealed class UartControllerApiWrapper : IUartControllerProvider {
            private int writeBufferSize = 256;
            private int readBufferSize = 256;
            private bool requestToSendEnabled;

            public NativeApi Api { get; }

            public UartControllerApiWrapper(NativeApi api) => this.Api = api;

            public event ClearToSendChangedEventHandler ClearToSendChanged;
            public event DataReceivedEventHandler DataReceived;
            public event ErrorReceivedEventHandler ErrorReceived;

            public void Dispose() { }
            public void Enable() { }
            public void Disable() { }

            public void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking, bool enableDePin, bool invertTxPolarity, bool invertRxPolarity, bool invertBinaryData, bool swapTxRxPin, bool invertDePolarity) { }
            public void Flush() { }

            public int Read(byte[] buffer, int offset, int length) => 0;
            public int Write(byte[] buffer, int offset, int length, TimeSpan breakDuration) => length;

            public void ClearWriteBuffer() { }
            public void ClearReadBuffer() { }

            public int WriteBufferSize { get => this.writeBufferSize; set => this.writeBufferSize = value; }
            public int ReadBufferSize { get => this.readBufferSize; set => this.readBufferSize = value; }
            public int BytesToWrite => 0;
            public int BytesToRead => 0;

            public bool IsRequestToSendEnabled { get => this.requestToSendEnabled; set => this.requestToSendEnabled = value; }
            public bool ClearToSendState => false;
        }
    }
}
