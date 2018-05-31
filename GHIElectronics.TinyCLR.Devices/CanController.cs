using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Can.Provider;

namespace GHIElectronics.TinyCLR.Devices.Can {
    public delegate void MessageReceivedEventHandler(CanController sender, MessageReceivedEventArgs e);
    public delegate void ErrorReceivedEventHandler(CanController sender, ErrorReceivedEventArgs e);

    public class MessageReceivedEventArgs {
        public int Count { get; }

        internal MessageReceivedEventArgs(int count) => this.Count = count;
    }

    public class ErrorReceivedEventArgs {
        public CanError Error { get; }

        internal ErrorReceivedEventArgs(CanError error) => this.Error = error;
    }

    public sealed class CanController {
        private readonly ICanControllerProvider provider;
        private readonly int idx;
        private readonly NativeEventDispatcher nativeMessageAvailableEvent;
        private readonly NativeEventDispatcher nativeErrorEvent;

        internal CanController(ICanControllerProvider provider, int idx) {
            this.provider = provider;
            this.idx = idx;

            this.nativeMessageAvailableEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
            this.nativeErrorEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

            this.nativeMessageAvailableEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.idx == ci) this.MessageReceived?.Invoke(this, new MessageReceivedEventArgs((int)d0)); };
            this.nativeErrorEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.idx == ci) this.ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs((CanError)d0)); };
        }

        public static CanController GetDefault() {
            var idx = 0U;

            return new CanController(LowLevelDevicesController.DefaultProvider?.CanControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.CanProvider), out var providerId, out idx) ? CanProvider.FromId(providerId).GetControllers((int)idx) : null), (int)idx);
        }

        public static CanController FromId(string controllerId) => Api.ParseSelector(controllerId, out var providerId, out var idx) ? new CanController(CanProvider.FromId(providerId).GetControllers((int)idx), (int)idx) : null;

        public void Reset() => this.provider.Reset(this.idx);
        public void SetBitTiming(CanBitTiming bitTiming) => this.provider.SetBitTiming(this.idx, bitTiming);

        public bool ReadMessage(out CanMessage message) => this.ReadMessages(new[] { message = new CanMessage() }, 0, 1) == 1;
        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.provider.ReadMessages(this.idx, messages, offset, count);

        public bool WriteMessage(CanMessage message) => this.WriteMessages(new[] { message }, 0, 1) == 1;
        public int WriteMessages(CanMessage[] messages, int offset, int count) => this.provider.WriteMessages(this.idx, messages, offset, count);

        public void SetExplicitFilters(uint[] filters) => this.provider.SetExplicitFilters(this.idx, filters);
        public void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds) => this.provider.SetGroupFilters(this.idx, lowerBounds, upperBounds);
        public void ClearReadBuffer() => this.provider.ClearReadBuffer(this.idx);
        public void ClearWriteBuffer() => this.provider.ClearReadBuffer(this.idx);

        public int UnreadMessageCount => this.provider.UnreadMessageCount(this.idx);
        public int UnwrittenMessageCount => this.provider.UnwrittenMessageCount(this.idx);
        public bool IsWritingAllowed => this.provider.IsWritingAllowed(this.idx);
        public int ReadErrorCount => this.provider.ReadErrorCount(this.idx);
        public int WriteErrorCount => this.provider.WriteErrorCount(this.idx);
        public uint SourceClock => this.provider.SourceClock(this.idx);

        public event MessageReceivedEventHandler MessageReceived;
        public event ErrorReceivedEventHandler ErrorReceived;
    }

    public class CanMessage {
        private byte[] data;

        public uint ArbitrationId { get; set; }
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

        public CanMessage() {
            this.data = new byte[8];
            this.ArbitrationId = 0;
            this.Length = 0;
            this.IsRemoteTransmissionRequest = false;
            this.IsExtendedId = false;
            this.TimeStamp = DateTime.Now;
        }

        public CanMessage(uint arbitrationId)
            : this(arbitrationId, null, 0, 0) {
        }

        public CanMessage(uint arbitrationId, byte[] data)
            : this(arbitrationId, data, 0, data != null ? data.Length : 0) {
        }

        public CanMessage(uint arbitrationId, byte[] data, int offset, int count)
            : this(arbitrationId, data, offset, count, false, false) {
        }

        public CanMessage(uint arbitrationId, byte[] data, int offset, int count, bool IsRemoteTransmissionRequesti, bool isExtendedId) {
            if (count < 0 || count > 8) throw new ArgumentOutOfRangeException(nameof(count), "count must be between zero and eight.");
            if (data == null && count != 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be zero when data is null.");
            if (count != 0 && offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data), "data.Length must be at least offset + count.");
            if (isExtendedId && arbitrationId > 0x1FFFFFFF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 29 bits when using an Extended ID.");
            if (!isExtendedId && arbitrationId > 0x7FF) throw new ArgumentOutOfRangeException(nameof(arbitrationId), "arbitrationId must not exceed 11 bits when not using an Extended ID.");

            this.ArbitrationId = arbitrationId;
            this.IsRemoteTransmissionRequest = IsRemoteTransmissionRequesti;
            this.IsExtendedId = isExtendedId;
            this.TimeStamp = DateTime.Now;
            this.Length = count;
            this.data = new byte[8];

            if (count != 0)
                Array.Copy(data, offset, this.data, 0, count);
        }
    }

    public enum CanError : byte {
        ReadBufferOverrun = 0,
        ReadBufferFull = 1,
        BusOff = 2,
        Passive = 3,
    }

    public class CanBitTiming {
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
}
