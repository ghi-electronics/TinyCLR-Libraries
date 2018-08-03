using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Can.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Can {
    public sealed class CanController : IDisposable {
        public ICanControllerProvider Provider { get; }

        private CanController(ICanControllerProvider provider) {
            this.Provider = provider;

            this.Provider.MessageReceived += (_, e) => this.MessageReceived?.Invoke(this, e);
            this.Provider.ErrorReceived += (_, e) => this.ErrorReceived?.Invoke(this, e);
        }

        public static CanController GetDefault() => Api.GetDefaultFromCreator(ApiType.CanController) is CanController c ? c : CanController.FromName(Api.GetDefaultName(ApiType.CanController));
        public static CanController FromName(string name) => CanController.FromProvider(new CanControllerApiWrapper(Api.Find(name, ApiType.CanController)));
        public static CanController FromProvider(ICanControllerProvider provider) => new CanController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public bool WriteMessage(CanMessage message) => this.WriteMessages(new[] { message }, 0, 1) == 1;
        public int WriteMessages(CanMessage[] messages, int offset, int count) => this.Provider.WriteMessages(messages, offset, count);

        public bool ReadMessage(out CanMessage message) => this.ReadMessages(new[] { message = new CanMessage() }, 0, 1) == 1;
        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.Provider.ReadMessages(messages, offset, count);

        public void SetBitTiming(CanBitTiming bitTiming) => this.Provider.SetBitTiming(bitTiming);
        public void SetExplicitFilters(int[] filters) => this.Provider.SetExplicitFilters(filters);
        public void SetGroupFilters(int[] lowerBounds, int[] upperBounds) => this.Provider.SetGroupFilters(lowerBounds, upperBounds);
        public void ClearWriteBuffer() => this.Provider.ClearReadBuffer();
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }

        public int MessagesToWrite => this.Provider.MessagesToWrite;
        public int MessagesToRead => this.Provider.MessagesToRead;
        public bool CanWriteMessage => this.Provider.CanWriteMessage;
        public bool CanReadMessage => this.Provider.CanReadMessage;
        public int WriteErrorCount => this.Provider.WriteErrorCount;
        public int ReadErrorCount => this.Provider.ReadErrorCount;
        public int SourceClock => this.Provider.SourceClock;

        public event MessageReceivedEventHandler MessageReceived;
        public event ErrorReceivedEventHandler ErrorReceived;
    }

    public enum CanError {
        ReadBufferOverrun = 0,
        ReadBufferFull = 1,
        BusOff = 2,
        Passive = 3,
    }

    public delegate void MessageReceivedEventHandler(CanController sender, MessageReceivedEventArgs e);
    public delegate void ErrorReceivedEventHandler(CanController sender, ErrorReceivedEventArgs e);

    public sealed class MessageReceivedEventArgs {
        public int Count { get; }

        internal MessageReceivedEventArgs(int count) => this.Count = count;
    }

    public sealed class ErrorReceivedEventArgs {
        public CanError Error { get; }

        internal ErrorReceivedEventArgs(CanError error) => this.Error = error;
    }

    public sealed class CanBitTiming {
        public int Propagation { get; set; }
        public int Phase1 { get; set; }
        public int Phase2 { get; set; }
        public int BaudratePrescaler { get; set; }
        public int SynchronizationJumpWidth { get; set; }
        public bool UseMultiBitSampling { get; set; }

        public CanBitTiming()
            : this(0, 0, 0, 0, 0, false) {
        }

        public CanBitTiming(int propagation, int phase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth)
            : this(propagation, phase1, phase2, baudratePrescaler, synchronizationJumpWidth, false) {
        }

        public CanBitTiming(int propagation, int phase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth, bool useMultiBitSampling) {
            this.Propagation = propagation;
            this.Phase1 = phase1;
            this.Phase2 = phase2;
            this.BaudratePrescaler = baudratePrescaler;
            this.SynchronizationJumpWidth = synchronizationJumpWidth;
            this.UseMultiBitSampling = useMultiBitSampling;
        }
    }

    public sealed class CanMessage {
        private byte[] data;

        public int ArbitrationId { get; set; }
        public bool IsExtendedId { get; set; }
        public bool IsRemoteTransmissionRequest { get; set; }
        public int Length { get; set; }
        public DateTime TimeStamp { get; set; }

        public byte[] Data {
            get => this.data;

            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length != 8) throw new ArgumentException("value must be eight bytes in length.", nameof(value));

                this.data = value;
            }
        }

        public CanMessage()
            : this(0, new byte[8], 0, 0, false, false) {
        }

        public CanMessage(int arbitrationId)
            : this(arbitrationId, null, 0, 0) {
        }

        public CanMessage(int arbitrationId, byte[] data)
            : this(arbitrationId, data, 0, data != null ? data.Length : 0) {
        }

        public CanMessage(int arbitrationId, byte[] data, int offset, int count)
            : this(arbitrationId, data, offset, count, false, false) {
        }

        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId) {
            if (count < 0 || count > 8) throw new ArgumentOutOfRangeException(nameof(count), "count must be between zero and eight.");
            if (data == null && count != 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be zero when data is null.");
            if (count != 0 && offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data), "data.Length must be at least offset + count.");
            if (isExtendedId && arbitrationId > 0x1FFFFFFF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 29 bits when using an Extended ID.");
            if (!isExtendedId && arbitrationId > 0x7FF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 11 bits when not using an Extended ID.");

            this.ArbitrationId = arbitrationId;
            this.IsRemoteTransmissionRequest = isRemoteTransmissionRequesti;
            this.IsExtendedId = isExtendedId;
            this.TimeStamp = DateTime.Now;
            this.Length = count;
            this.data = new byte[8];

            if (count != 0)
                Array.Copy(data, offset, this.data, 0, count);
        }
    }

    namespace Provider {
        public interface ICanControllerProvider : IDisposable {
            void Enable();
            void Disable();

            int WriteMessages(CanMessage[] messages, int offset, int count);
            int ReadMessages(CanMessage[] messages, int offset, int count);

            void SetBitTiming(CanBitTiming bitTiming);
            void SetExplicitFilters(int[] filters);
            void SetGroupFilters(int[] lowerBounds, int[] upperBounds);
            void ClearWriteBuffer();
            void ClearReadBuffer();

            int WriteBufferSize { get; set; }
            int ReadBufferSize { get; set; }

            int MessagesToWrite { get; }
            int MessagesToRead { get; }
            bool CanWriteMessage { get; }
            bool CanReadMessage { get; }
            int WriteErrorCount { get; }
            int ReadErrorCount { get; }
            int SourceClock { get; }

            event MessageReceivedEventHandler MessageReceived;
            event ErrorReceivedEventHandler ErrorReceived;
        }

        public sealed class CanControllerApiWrapper : ICanControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher messageReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;

            public Api Api { get; }

            public CanControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.messageReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

                this.messageReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.MessageReceived?.Invoke(null, new MessageReceivedEventArgs((int)d0)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.ErrorReceived?.Invoke(null, new ErrorReceivedEventArgs((CanError)d0)); };
            }

            public event MessageReceivedEventHandler MessageReceived;
            public event ErrorReceivedEventHandler ErrorReceived;

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            public extern int MessagesToWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MessagesToRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern bool CanWriteMessage { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern bool CanReadMessage { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int WriteErrorCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int ReadErrorCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int SourceClock { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int WriteMessages(CanMessage[] messages, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReadMessages(CanMessage[] messages, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetBitTiming(CanBitTiming bitTiming);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetExplicitFilters(int[] filters);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetGroupFilters(int[] lowerBounds, int[] upperBounds);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();
        }
    }
}
