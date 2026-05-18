using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Can.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Can {
    /// <summary>
    /// Represents a CAN bus controller. Configure bit timing and acceptance
    /// filters, <see cref="Enable"/> the controller, then exchange
    /// <see cref="CanMessage"/>s. Subscribe to <see cref="MessageReceived"/>
    /// for event-driven receive instead of polling <see cref="MessagesToRead"/>.
    /// </summary>
    public class CanController : IDisposable {
        private MessageReceivedEventHandler messageReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        /// <summary>The low-level provider backing this controller.</summary>
        public ICanControllerProvider Provider { get; }

        private CanController(ICanControllerProvider provider) {
            this.Provider = provider;

            this.Filter = new Filter(this.Provider);
        }

        /// <summary>Returns the default CAN controller for this device.</summary>
        public static CanController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.CanController) is CanController c ? c : CanController.FromName(NativeApi.GetDefaultName(NativeApiType.CanController));
        /// <summary>Returns a CAN controller identified by its native API name.</summary>
        public static CanController FromName(string name) => CanController.FromProvider(new CanControllerApiWrapper(NativeApi.Find(name, NativeApiType.CanController)));
        /// <summary>Creates a controller from a custom <see cref="ICanControllerProvider"/>.</summary>
        public static CanController FromProvider(ICanControllerProvider provider) => new CanController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Powers on the controller. Call after configuring timing and filters.</summary>
        public void Enable() => this.Provider.Enable();
        /// <summary>Powers off the controller.</summary>
        public void Disable() => this.Provider.Disable();

        /// <summary>Queues a single message for transmission. Returns true on success.</summary>
        public bool WriteMessage(CanMessage message) => this.WriteMessages(new[] { message }, 0, 1) == 1;

        /// <summary>Queues a slice of <paramref name="messages"/> for transmission.</summary>
        /// <returns>Number of messages successfully queued.</returns>
        public int WriteMessages(CanMessage[] messages, int offset, int count) {
            if (offset + count > messages.Length) throw new ArgumentOutOfRangeException(nameof(count), "offset + count is beyond the end of the array");

            return this.Provider.WriteMessages(messages, offset, count);
        }

        /// <summary>Dequeues a single received message. Returns true if one was available.</summary>
        public bool ReadMessage(out CanMessage message) => this.ReadMessages(new[] { message = new CanMessage() }, 0, 1) == 1;
        /// <summary>Dequeues up to <paramref name="count"/> received messages into <paramref name="messages"/>.</summary>
        /// <returns>Number of messages actually read.</returns>
        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.Provider.ReadMessages(messages, offset, count);

        /// <summary>Configures the arbitration-phase bit timing (used for the whole frame in classic CAN).</summary>
        public void SetNominalBitTiming(CanBitTiming bitTiming) => this.Provider.SetNominalBitTiming(bitTiming);
        /// <summary>Configures the data-phase bit timing for CAN-FD frames with bit-rate switching.</summary>
        public void SetDataBitTiming(CanBitTiming bitTiming) => this.Provider.SetDataBitTiming(bitTiming);
        /// <summary>Empties the transmit queue.</summary>
        public void ClearWriteBuffer() => this.Provider.ClearReadBuffer();
        /// <summary>Empties the receive queue.</summary>
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        /// <summary>Size of the transmit message queue.</summary>
        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        /// <summary>Size of the receive message queue.</summary>
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }

        /// <summary>Messages currently queued for transmission.</summary>
        public int MessagesToWrite => this.Provider.MessagesToWrite;
        /// <summary>Messages currently available to read.</summary>
        public int MessagesToRead => this.Provider.MessagesToRead;
        /// <summary>True when the transmit queue has room.</summary>
        public bool CanWriteMessage => this.Provider.CanWriteMessage;
        /// <summary>True when at least one received message is available.</summary>
        public bool CanReadMessage => this.Provider.CanReadMessage;
        /// <summary>Current transmit error counter (TEC).</summary>
        public int WriteErrorCount => this.Provider.WriteErrorCount;
        /// <summary>Current receive error counter (REC).</summary>
        public int ReadErrorCount => this.Provider.ReadErrorCount;
        /// <summary>Source clock feeding the CAN prescaler, in Hz. Used to compute bit timing.</summary>
        public int SourceClock => this.Provider.SourceClock;

        private void OnMessageReceived(CanController sender, MessageReceivedEventArgs e) => this.messageReceivedCallbacks?.Invoke(this, e);
        private void OnErrorReceived(CanController sender, ErrorReceivedEventArgs e) => this.errorReceivedCallbacks?.Invoke(this, e);

        /// <summary>Raised when one or more messages have arrived. Call <see cref="ReadMessage"/> from the handler.</summary>
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

        /// <summary>Raised when the controller detects a bus error or enters bus-off.</summary>
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

        /// <summary>Acceptance filter configuration for this controller.</summary>
        public Filter Filter { get; }
    }

    /// <summary>Acceptance-filter configuration for a CAN controller.</summary>
    public class Filter {
        /// <summary>CAN identifier width.</summary>
        public enum IdType {
            /// <summary>11-bit standard identifier.</summary>
            Standard = 0,
            /// <summary>29-bit extended identifier.</summary>
            Extended = 1,
        }

        /// <summary>How a filter matches arbitration IDs.</summary>
        public enum FilterType {
            /// <summary>Accept IDs in the inclusive range [id1, id2].</summary>
            Range = 0,
            /// <summary>Accept IDs where (id &amp; mask) == (compare &amp; mask).</summary>
            Mask = 1,
        }

        private readonly ICanControllerProvider provider;

        internal Filter(ICanControllerProvider provider) => this.provider = provider;

        /// <summary>Accepts IDs in the inclusive range [<paramref name="startId"/>, <paramref name="endId"/>].</summary>
        public void AddRangeFilter(IdType idType, uint startId, uint endId) => this.provider.AddFilter(idType, FilterType.Range, startId, endId);
        /// <summary>Accepts IDs where the bits selected by <paramref name="mask"/> equal those in <paramref name="compare"/>.</summary>
        public void AddMaskFilter(IdType idType, uint compare, uint mask) => this.provider.AddFilter(idType, FilterType.Mask, compare, mask);
        /// <summary>Filters out remote-transmission-request (RTR) frames of the given ID width.</summary>
        public void RejectRemoteFrame(IdType idType) => this.provider.RejectRemoteFrame(idType);
        /// <summary>Removes every previously installed filter (controller will pass all frames).</summary>
        public void Clear() => this.provider.ClearFilter();
    }

    /// <summary>Categories of error reported via <see cref="CanController.ErrorReceived"/>.</summary>
    public enum CanError {
        /// <summary>Receive overrun.</summary>
        ReadBufferOverrun = 0,
        /// <summary>Receive buffer full; subsequent messages will be dropped.</summary>
        ReadBufferFull = 1,
        /// <summary>Controller has entered bus-off state and stopped transmitting.</summary>
        BusOff = 2,
        /// <summary>Controller has entered error-passive state.</summary>
        Passive = 3,
    }

    /// <summary>CAN-FD error state indicator carried in a received message.</summary>
    public enum ErrorStateIndicator {
        /// <summary>Sender was error-active.</summary>
        Active = 0,
        /// <summary>Sender was error-passive.</summary>
        Passive = 1,
    }

    /// <summary>Handler signature for <see cref="CanController.MessageReceived"/>.</summary>
    public delegate void MessageReceivedEventHandler(CanController sender, MessageReceivedEventArgs e);
    /// <summary>Handler signature for <see cref="CanController.ErrorReceived"/>.</summary>
    public delegate void ErrorReceivedEventHandler(CanController sender, ErrorReceivedEventArgs e);

    /// <summary>Arguments for <see cref="CanController.MessageReceived"/>.</summary>
    public class MessageReceivedEventArgs {
        /// <summary>Number of messages that have just become available.</summary>
        public int Count { get; }
        /// <summary>Driver-captured time of the receive event.</summary>
        public DateTime Timestamp { get; }

        internal MessageReceivedEventArgs(int count, DateTime timestamp) {
            this.Count = count;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Arguments for <see cref="CanController.ErrorReceived"/>.</summary>
    public class ErrorReceivedEventArgs {
        /// <summary>The kind of error reported.</summary>
        public CanError Error { get; }
        /// <summary>Driver-captured time of the error.</summary>
        public DateTime Timestamp { get; }

        internal ErrorReceivedEventArgs(CanError error, DateTime timestamp) {
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Bit-timing parameters for one phase (arbitration or data) of a CAN frame.</summary>
    public class CanBitTiming {
        /// <summary>Time segment 1 (propagation + phase 1) in time quanta.</summary>
        public int Phase1 { get; set; }
        /// <summary>Time segment 2 (phase 2) in time quanta.</summary>
        public int Phase2 { get; set; }
        /// <summary>Clock divider applied to the controller's source clock to produce the time-quantum frequency.</summary>
        public int BaudratePrescaler { get; set; }
        /// <summary>Synchronization Jump Width (SJW), in time quanta.</summary>
        public int SynchronizationJumpWidth { get; set; }
        /// <summary>If true, the controller samples each bit three times and takes the majority value.</summary>
        public bool UseMultiBitSampling { get; set; }

        /// <summary>Constructs an uninitialized bit-timing object.</summary>
        public CanBitTiming()
            : this(0, 0, 0, 0, false) {
        }

        /// <summary>Constructs a bit-timing object without multi-bit sampling.</summary>
        public CanBitTiming(int propagationPhase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth)
            : this(propagationPhase1, phase2, baudratePrescaler, synchronizationJumpWidth, false) {
        }

        /// <summary>Constructs a fully specified bit-timing object.</summary>
        public CanBitTiming(int propagationPhase1, int phase2, int baudratePrescaler, int synchronizationJumpWidth, bool useMultiBitSampling) {
            this.Phase1 = propagationPhase1;
            this.Phase2 = phase2;
            this.BaudratePrescaler = baudratePrescaler;
            this.SynchronizationJumpWidth = synchronizationJumpWidth;
            this.UseMultiBitSampling = useMultiBitSampling;
        }
    }

    /// <summary>
    /// One CAN (or CAN-FD) frame. Set <see cref="ArbitrationId"/>, optionally
    /// <see cref="ExtendedId"/> for 29-bit IDs, and the payload via <see cref="Data"/>
    /// / <see cref="Length"/>. For CAN-FD, set <see cref="FdCan"/> and optionally
    /// <see cref="BitRateSwitch"/>.
    /// </summary>
    public class CanMessage {
        private byte[] data;
        private bool remoteTransmissionRequest;
        private bool fdCan;
        private int length;

        /// <summary>The CAN arbitration ID. Limited to 11 bits when <see cref="ExtendedId"/> is false, 29 bits otherwise.</summary>
        public int ArbitrationId { get; set; }
        /// <summary>True when <see cref="ArbitrationId"/> is a 29-bit extended identifier.</summary>
        public bool ExtendedId { get; set; }
        /// <summary>For received messages: driver-captured arrival time. For TX: not used.</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>CAN-FD only: switch to data-phase bit timing for the payload.</summary>
        public bool BitRateSwitch { get; set; }
        /// <summary>Reports whether a received message was sent by an error-active or error-passive node.</summary>
        public ErrorStateIndicator ErrorStateIndicator { get; }

        /// <summary>True when this is a remote-transmission-request (RTR) frame. Not allowed in CAN-FD.</summary>
        public bool RemoteTransmissionRequest {
            get => this.remoteTransmissionRequest;
            set {
                if (this.FdCan && value) throw new ArgumentException("No remote request in flexible data mode.");

                this.remoteTransmissionRequest = value;
            }
        }

        /// <summary>
        /// Payload length in bytes. 0..8 for classic CAN; for CAN-FD also accepts
        /// 12, 16, 20, 24, 32, 48, or 64. Larger classic values are clamped to 8.
        /// </summary>
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

        /// <summary>True if this is a CAN-FD (flexible-data) frame.</summary>
        public bool FdCan {
            get => this.fdCan;
            set {
                if (this.RemoteTransmissionRequest && value) throw new ArgumentException("No remote request in flexible data mode.");

                this.fdCan = value;
            }
        }

        /// <summary>The payload buffer. Up to 64 bytes for CAN-FD, 8 for classic CAN.</summary>
        public byte[] Data {
            get => this.data;

            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length > 64) throw new ArgumentException("value must be between 0 and 64 bytes in length.", nameof(value));

                this.data = value;
            }
        }

        /// <summary>Constructs an empty 8-byte classic CAN message with ID 0.</summary>
        public CanMessage()
            : this(0, new byte[8], 0, 0, false, false) {
        }

        /// <summary>Constructs an empty message with the given arbitration ID.</summary>
        public CanMessage(int arbitrationId)
            : this(arbitrationId, null, 0, 0) {
        }

        /// <summary>Constructs a message carrying <paramref name="data"/> in full.</summary>
        public CanMessage(int arbitrationId, byte[] data)
            : this(arbitrationId, data, 0, data != null ? data.Length : 0) {
        }

        /// <summary>Constructs a message from a slice of <paramref name="data"/>.</summary>
        public CanMessage(int arbitrationId, byte[] data, int offset, int count)
            : this(arbitrationId, data, offset, count, false, false) {
        }

        /// <summary>Constructs a message with explicit RTR and extended-ID flags.</summary>
        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId)
           : this(arbitrationId, data, offset, count, isRemoteTransmissionRequesti, isExtendedId, false, false) {
        }

        /// <summary>Constructs a possibly-CAN-FD message.</summary>
        public CanMessage(int arbitrationId, byte[] data, int offset, int count, bool isRemoteTransmissionRequesti, bool isExtendedId, bool isFdCan)
           : this(arbitrationId, data, offset, count, isRemoteTransmissionRequesti, isExtendedId, isFdCan, false) {
        }

        /// <summary>Constructs a fully specified message including bit-rate switch.</summary>
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
        /// <summary>Provider contract for a CAN controller.</summary>
        public interface ICanControllerProvider : IDisposable {
            /// <summary>Powers on the controller.</summary>
            void Enable();
            /// <summary>Powers off the controller.</summary>
            void Disable();

            /// <summary>Queues a slice of messages for transmission. Returns count actually queued.</summary>
            int WriteMessages(CanMessage[] messages, int offset, int count);
            /// <summary>Dequeues up to <paramref name="count"/> received messages. Returns count actually read.</summary>
            int ReadMessages(CanMessage[] messages, int offset, int count);

            /// <summary>Configures arbitration-phase bit timing.</summary>
            void SetNominalBitTiming(CanBitTiming bitTiming);
            /// <summary>Configures CAN-FD data-phase bit timing.</summary>
            void SetDataBitTiming(CanBitTiming bitTiming);

            /// <summary>Installs an acceptance filter.</summary>
            void AddFilter(Filter.IdType idType, Filter.FilterType filterType, uint id1, uint id2);
            /// <summary>Filters out RTR frames of the given ID width.</summary>
            void RejectRemoteFrame(Filter.IdType idType);
            /// <summary>Removes every installed filter.</summary>
            void ClearFilter();

            /// <summary>Empties the transmit queue.</summary>
            void ClearWriteBuffer();
            /// <summary>Empties the receive queue.</summary>
            void ClearReadBuffer();

            /// <summary>Size of the transmit message queue.</summary>
            int WriteBufferSize { get; set; }
            /// <summary>Size of the receive message queue.</summary>
            int ReadBufferSize { get; set; }

            /// <summary>Messages currently queued for transmission.</summary>
            int MessagesToWrite { get; }
            /// <summary>Messages currently available to read.</summary>
            int MessagesToRead { get; }
            /// <summary>True when the transmit queue has room.</summary>
            bool CanWriteMessage { get; }
            /// <summary>True when at least one received message is available.</summary>
            bool CanReadMessage { get; }
            /// <summary>Current transmit error counter (TEC).</summary>
            int WriteErrorCount { get; }
            /// <summary>Current receive error counter (REC).</summary>
            int ReadErrorCount { get; }
            /// <summary>Source clock feeding the CAN prescaler, in Hz.</summary>
            int SourceClock { get; }

            /// <summary>Raised when messages arrive.</summary>
            event MessageReceivedEventHandler MessageReceived;
            /// <summary>Raised when a bus error or state change is observed.</summary>
            event ErrorReceivedEventHandler ErrorReceived;
        }

        /// <summary>Concrete <see cref="ICanControllerProvider"/> backed by the native TinyCLR CAN HAL.</summary>
        public sealed class CanControllerApiWrapper : ICanControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher messageReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;
            private MessageReceivedEventHandler messageReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public CanControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.messageReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

                this.messageReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.messageReceivedCallbacks?.Invoke(null, new MessageReceivedEventArgs((int)d0, ts)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.errorReceivedCallbacks?.Invoke(null, new ErrorReceivedEventArgs((CanError)d0, ts)); };
            }

            /// <inheritdoc/>
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
            private extern void SetMessageaReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetErrorReceivedEventEnabled(bool enabled);

            /// <inheritdoc/>
            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            /// <inheritdoc/>
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            /// <inheritdoc/>
            public extern int MessagesToWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int MessagesToRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern bool CanWriteMessage { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern bool CanReadMessage { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int WriteErrorCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int ReadErrorCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int SourceClock { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int WriteMessages(CanMessage[] messages, int offset, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReadMessages(CanMessage[] messages, int offset, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetNominalBitTiming(CanBitTiming bitTiming);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDataBitTiming(CanBitTiming bitTiming);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void AddFilter(Filter.IdType idType, Filter.FilterType filterType, uint id1, uint id2);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void RejectRemoteFrame(Filter.IdType idType);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearFilter();
        }
    }
}
