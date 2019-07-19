using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Uart.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Uart {
    public sealed class UartController : IDisposable {
        private ClearToSendChangedEventHandler clearToSendChangedCallbacks;
        private DataReceivedEventHandler dataReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        public IUartControllerProvider Provider { get; }

        private UartController(IUartControllerProvider provider) => this.Provider = provider;

        public static UartController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UartController) is UartController c ? c : UartController.FromName(NativeApi.GetDefaultName(NativeApiType.UartController));
        public static UartController FromName(string name) => UartController.FromProvider(new UartControllerApiWrapper(NativeApi.Find(name, NativeApiType.UartController)));
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
        XOnXOff = 2,
        RequestToSendXOnXOff = 3,
    }

    public enum UartError {
        Frame = 0,
        Overrun = 1,
        BufferFull = 2,
        ReceiveParity = 3,
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

    public sealed class DataReceivedEventArgs {
        public int Count { get; }
        public DateTime Timestamp { get; }

        internal DataReceivedEventArgs(int count, DateTime timestamp) {
            this.Count = count;
            this.Timestamp = timestamp;
        }
    }

    public sealed class ErrorReceivedEventArgs {
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
            private ClearToSendChangedEventHandler clearToSendChangedCallbacks;
            private DataReceivedEventHandler dataReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            public NativeApi Api { get; }

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
