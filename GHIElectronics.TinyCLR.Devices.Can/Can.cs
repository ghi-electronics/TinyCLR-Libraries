using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Can.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Can {
    public sealed class CanController : IDisposable {
        private MessageReceivedEventHandler messageReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        public ICanControllerProvider Provider { get; }

        private CanController(ICanControllerProvider provider) {
            this.Provider = provider;

            this.Filter = new Filter(this.Provider);
        }

        public static CanController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.CanController) is CanController c ? c : CanController.FromName(NativeApi.GetDefaultName(NativeApiType.CanController));
        public static CanController FromName(string name) => CanController.FromProvider(new CanControllerApiWrapper(NativeApi.Find(name, NativeApiType.CanController)));
        public static CanController FromProvider(ICanControllerProvider provider) => new CanController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public bool WriteMessage(CanMessage message) => this.WriteMessages(new[] { message }, 0, 1) == 1;

        public int WriteMessages(CanMessage[] messages, int offset, int count) {
            if (offset + count > messages.Length) throw new ArgumentOutOfRangeException(nameof(count), "offset + count is beyond the end of the array");

            return this.Provider.WriteMessages(messages, offset, count);
        }

        public bool ReadMessage(out CanMessage message) => this.ReadMessages(new[] { message = new CanMessage() }, 0, 1) == 1;
        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.Provider.ReadMessages(messages, offset, count);

        public void SetNominalBitTiming(CanBitTiming bitTiming) => this.Provider.SetNominalBitTiming(bitTiming);
        public void SetDataBitTiming(CanBitTiming bitTiming) => this.Provider.SetDataBitTiming(bitTiming);
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

        private void OnMessageReceived(CanController sender, MessageReceivedEventArgs e) => this.messageReceivedCallbacks?.Invoke(this, e);
        private void OnErrorReceived(CanController sender, ErrorReceivedEventArgs e) => this.errorReceivedCallbacks?.Invoke(this, e);

        public event MessageReceivedEventHandler MessageReceived {
            add {
                if (this.messageReceivedCallbacks == null)
                    this.Provider.MessageReceived += this.OnMessageReceived;

                this.messageReceivedCallbacks += value;
            }
            remove {
                this.messageReceivedCallbacks -= value;

                if (this.messageReceivedCallbacks == null)
                    this.Provider.MessageReceived -= this.OnMessageReceived;
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

        public Filter Filter { get; }
    }

    public sealed class Filter {
        public enum IdType {
            Standard = 0,
            Extended = 1,
        }

        public enum FilterType {
            Range = 0,
            Mask = 1,
        }
       
        private readonly ICanControllerProvider provider;

        internal Filter(ICanControllerProvider provider) => this.provider = provider;

        public void AddRangeFilter(IdType idType, uint startId, uint endId) => this.provider.AddFilter(idType, FilterType.Range, startId, endId);
        public void AddMaskFilter(IdType idType, uint compare, uint mask) => this.provider.AddFilter(idType, FilterType.Mask, compare, mask);       
        public void RejectRemoteFrame(IdType idType) => this.provider.RejectRemoteFrame(idType);
        public void Clear() => this.provider.ClearFilter();
    }

    public enum CanError {
        ReadBufferOverrun = 0,
        ReadBufferFull = 1,
        BusOff = 2,
        Passive = 3,
    }

    public enum ErrorStateIndicator {
        Active = 0,
        Passive = 1,
    }

    public delegate void MessageReceivedEventHandler(CanController sender, MessageReceivedEventArgs e);
    public delegate void ErrorReceivedEventHandler(CanController sender, ErrorReceivedEventArgs e);

    public sealed class MessageReceivedEventArgs {
        public int Count { get; }
        public DateTime Timestamp { get; }

        internal MessageReceivedEventArgs(int count, DateTime timestamp) {
            this.Count = count;
            this.Timestamp = timestamp;
        }
    }

    public sealed class ErrorReceivedEventArgs {
        public CanError Error { get; }
        public DateTime Timestamp { get; }

        internal ErrorReceivedEventArgs(CanError error, DateTime timestamp) {
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    public sealed class CanBitTiming {
        public int Phase1 { get; set; }
        public int Phase2 { get; set; }
        public int BaudratePrescaler { get; set; }
        public int SynchronizationJumpWidth { get; set; }
        public bool UseMultiBitSampling { get; set; }

        public CanBitTiming()
            : this(0, 0, 0, 0, false) {
        }

        public CanBitTiming(int propagationPhase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth)
            : this(propagationPhase1, phase2, baudratePrescaler, synchronizationJumpWidth, false) {
        }

        public CanBitTiming(int propagationPhase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth, bool useMultiBitSampling) {
            this.Phase1 = propagationPhase1;
            this.Phase2 = phase2;
            this.BaudratePrescaler = baudratePrescaler;
            this.SynchronizationJumpWidth = synchronizationJumpWidth;
            this.UseMultiBitSampling = useMultiBitSampling;
        }
    }

    public sealed class CanMessage {
        private byte[] data;
        private bool remoteTransmissionRequest;
        private bool fdCan;
        private int length;

        public int ArbitrationId { get; set; }
        public bool ExtendedId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool BitRateSwitch { get; set; }
        public ErrorStateIndicator ErrorStateIndicator { get; }

        public bool RemoteTransmissionRequest {
            get => this.remoteTransmissionRequest;
            set {
                if (this.FdCan && value) throw new ArgumentException("No remote request in flexible data mode.");

                this.remoteTransmissionRequest = value;
            }
        }
        public int Length {
            get => this.length;
            set {

                if (value > 8 && !this.FdCan)
                    this.length = 8;
                if (value > 8) {
                    if (value != 12 && value != 16 && value != 20 && value != 24 && value != 32 && value != 48 && value != 64) {
                        throw new ArgumentException("Length is invalid.");
                    }
                }

                this.length = value;
            }
        }

        public bool FdCan {
            get => this.fdCan;
            set {
                if (this.RemoteTransmissionRequest && value) throw new ArgumentException("No remote request in flexible data mode.");

                this.fdCan = value;
            }
        }

        public byte[] Data {
            get => this.data;

            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length > 64) throw new ArgumentException("value must be between 0 and 64 bytes in length.", nameof(value));

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

        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId)
           : this(arbitrationId, data, offset, count, isRemoteTransmissionRequesti, isExtendedId, false, false) {
        }

        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId, bool isFdCan)
           : this(arbitrationId, data, offset, count, isRemoteTransmissionRequesti, isExtendedId, isFdCan, false) {
        }

        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId, bool isFdCan, bool isBitRateSwitch) {
            if (count < 0 || count > 64) throw new ArgumentOutOfRangeException(nameof(count), "count must be between 0 and 64.");

            if (data == null && count != 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be zero when data is null.");
            if (count != 0 && offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data), "data.Length must be at least offset + count.");
            if (isExtendedId && arbitrationId > 0x1FFFFFFF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 29 bits when using an Extended ID.");
            if (!isExtendedId && arbitrationId > 0x7FF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 11 bits when not using an Extended ID.");

            this.ArbitrationId = arbitrationId;
            this.RemoteTransmissionRequest = isRemoteTransmissionRequesti;
            this.ExtendedId = isExtendedId;
            this.Timestamp = DateTime.Now;
            this.Length = count;
            this.data = new byte[64];
            this.FdCan = isFdCan;
            this.BitRateSwitch = isBitRateSwitch;

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

            void SetNominalBitTiming(CanBitTiming bitTiming);
            void SetDataBitTiming(CanBitTiming bitTiming);

            void AddFilter(Filter.IdType idType, Filter.FilterType filterType, uint id1, uint id2);
            void RejectRemoteFrame(Filter.IdType idType);
            void ClearFilter();

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
            private MessageReceivedEventHandler messageReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            public NativeApi Api { get; }

            public CanControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.messageReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

                this.messageReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.messageReceivedCallbacks?.Invoke(null, new MessageReceivedEventArgs((int)d0, ts)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.errorReceivedCallbacks?.Invoke(null, new ErrorReceivedEventArgs((CanError)d0, ts)); };
            }

            public event MessageReceivedEventHandler MessageReceived {
                add {
                    if (this.messageReceivedCallbacks == null)
                        this.SetMessageaReceivedEventEnabled(true);

                    this.messageReceivedCallbacks += value;
                }
                remove {
                    this.messageReceivedCallbacks -= value;

                    if (this.messageReceivedCallbacks == null)
                        this.SetMessageaReceivedEventEnabled(false);
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
            private extern void SetMessageaReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetErrorReceivedEventEnabled(bool enabled);

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
            public extern void SetNominalBitTiming(CanBitTiming bitTiming);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDataBitTiming(CanBitTiming bitTiming);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void AddFilter(Filter.IdType idType, Filter.FilterType filterType, uint id1, uint id2);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void RejectRemoteFrame(Filter.IdType idType);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearFilter();
        }
    }
}
