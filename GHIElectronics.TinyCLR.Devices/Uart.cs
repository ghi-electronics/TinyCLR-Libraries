using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Uart.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Uart {
    public sealed class UartController : IDisposable {
        public IUartControllerProvider Provider { get; }

        private UartController(IUartControllerProvider provider) {
            this.Provider = provider;

            this.Provider.ClearToSendChanged += (_, e) => this.ClearToSendChanged?.Invoke(this, e);
            this.Provider.DataReceived += (_, e) => this.DataReceived?.Invoke(this, e);
            this.Provider.ErrorReceived += (_, e) => this.ErrorReceived?.Invoke(this, e);
        }

        public static UartController GetDefault() => Api.GetDefaultFromCreator(ApiType.UartController) is UartController c ? c : UartController.FromName(Api.GetDefaultName(ApiType.UartController));
        public static UartController FromName(string name) => UartController.FromProvider(new UartControllerApiWrapper(Api.Find(name, ApiType.UartController)));
        public static UartController FromProvider(IUartControllerProvider provider) => new UartController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking) => this.Provider.SetActiveSettings(baudRate, dataBits, parity, stopBits, handshaking);
        public void Flush() => this.Provider.Flush();

        public int Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        public int Read(byte[] buffer, int offset, int length) => this.Provider.Read(buffer, offset, length);

        public int Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        public int Write(byte[] buffer, int offset, int length) => this.Provider.Write(buffer, offset, length);

        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }
        public int BytesToWrite => this.Provider.BytesToWrite;
        public int BytesToRead => this.Provider.BytesToRead;

        public bool IsRequestToSendEnabled { get => this.Provider.IsRequestToSendEnabled; set => this.Provider.IsRequestToSendEnabled = value; }
        public bool ClearToSendState => this.Provider.ClearToSendState;

        public event ClearToSendChangedEventHandler ClearToSendChanged;
        public event DataReceivedEventHandler DataReceived;
        public event ErrorReceivedEventHandler ErrorReceived;
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
        XOnXOff = 2,
        RequestToSendXOnXOff = 3,
    }

    public enum UartError {
        Frame = 0,
        Overrun = 1,
        BufferFull = 2,
        ReceiveParity = 3,
    }

    public delegate void ClearToSendChangedEventHandler(UartController sender, EventArgs e);
    public delegate void DataReceivedEventHandler(UartController sender, DataReceivedEventArgs e);
    public delegate void ErrorReceivedEventHandler(UartController sender, ErrorReceivedEventArgs e);

    public sealed class DataReceivedEventArgs {
        public int Count { get; }

        internal DataReceivedEventArgs(int count) => this.Count = count;
    }

    public sealed class ErrorReceivedEventArgs {
        public UartError Error { get; }

        internal ErrorReceivedEventArgs(UartError error) => this.Error = error;
    }

    namespace Provider {
        public interface IUartControllerProvider : IDisposable {
            void Enable();
            void Disable();

            void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking);
            void Flush();
            int Read(byte[] buffer, int offset, int length);
            int Write(byte[] buffer, int offset, int length);

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

        public sealed class UartControllerApiWrapper : IUartControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher clearToSendChangedDispatcher;
            private readonly NativeEventDispatcher dataReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;

            public Api Api { get; }

            public UartControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.clearToSendChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.ClearToSendChanged");
                this.dataReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.DataReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Uart.ErrorReceived");

                this.clearToSendChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.ClearToSendChanged?.Invoke(null, new EventArgs()); };
                this.dataReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.DataReceived?.Invoke(null, new DataReceivedEventArgs((int)d0)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.ErrorReceived?.Invoke(null, new ErrorReceivedEventArgs((UartError)d0)); };
            }

            public event ClearToSendChangedEventHandler ClearToSendChanged;
            public event DataReceivedEventHandler DataReceived;
            public event ErrorReceivedEventHandler ErrorReceived;

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern int BytesToWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int BytesToRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool IsRequestToSendEnabled { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern bool ClearToSendState { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(int baudRate, int dataBits, UartParity parity, UartStopBitCount stopBits, UartHandshake handshaking);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] buffer, int offset, int length);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] buffer, int offset, int length);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();
        }
    }
}
