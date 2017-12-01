using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.ControllerAreaNetwork {

    public interface ICanProvider {
        ICanControllerProvider[] GetControllers();
    }

    public interface ICanControllerProvider {

        void Enable();

        void SetSpeed(int propagation, int phase1, int phase2, int brp, int synchronizationJumpWidth, int useMultiBitSampling);

        bool Reset();

        int ReadMessages(Message[] messages, int offset, int count);

        int SendMessages(Message[] messages, int offset, int count);

        int ReceivedMessageCount();        
    }

    public class CanProvider : ICanProvider {
        private ICanControllerProvider[] controllers;
        private static Hashtable providers = new Hashtable();
       
        public string Name { get; }

        public ICanControllerProvider[] GetControllers() => this.controllers;

        private CanProvider(string name) {
            var api = Api.Find(name, ApiType.CanProvider);

            this.Name = name;
            this.controllers = new ICanControllerProvider[api.Count];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultCanControllerProvider(api.Implementation[i]);
        }

        public static ICanProvider FromId(string id) {
            if (CanProvider.providers.Contains(id))
                return (ICanProvider)CanProvider.providers[id];

            var res = new CanProvider(id);

            CanProvider.providers[id] = res;

            return res;
        }
    }
    internal class DefaultCanControllerProvider : ICanControllerProvider {
#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
#pragma warning restore CS0169

        internal DefaultCanControllerProvider(IntPtr nativeProvider) => this.nativeProvider = nativeProvider;

        public void Enable() => NativeEnable();

        public void SetSpeed(int propagation, int phase1, int phase2, int brp, int synchronizationJumpWidth, int useMultiBitSampling) => NativeSetSpeed(propagation, phase1, phase2, brp, synchronizationJumpWidth, useMultiBitSampling);

        public bool Reset() => NativeReset();

        public int ReadMessages(Message[] messages, int offset, int count) => NativeReadMessages(messages, offset, count);

        public int SendMessages(Message[] messages, int offset, int count) => NativeSendMessages(messages, offset, count);

        public int ReceivedMessageCount() => NativeReceivedMessageCount();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeEnable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSetSpeed(int propagation, int phase1, int phase2, int brp, int synchronizationJumpWidth, int useMultiBitSampling);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeReadMessages(Message[] messages, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeSendMessages(Message[] messages, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeReceivedMessageCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeReset();

    }

    /// <summary>A CAN message.</summary>
    public class Message {
        private byte[] data;
        private uint arbitrationId;
        private int length;
        private bool isRTR;
        private bool isEID;
        private DateTime timeStamp;

        /// <summary>The message arbitration id.</summary>
        public uint ArbitrationId { get => this.arbitrationId; set => this.arbitrationId = value; }

        /// <summary>The number of bytes in the message.</summary>
        public int Length { get => this.length; set => this.length = value; }

        /// <summary>Whether or not the message is a remote transmission request.</summary>
        public bool IsRemoteTransmissionRequest { get => this.isRTR; set => this.isRTR = value; }

        /// <summary>Whether or not the message uses an extended id.</summary>
        public bool IsExtendedId { get => this.isEID; set => this.isEID = value; }

        /// <summary>When the message was received.</summary>
        public DateTime TimeStamp { get => this.timeStamp; set => this.timeStamp = value; }

        /// <summary>The message data. It must be eight bytes.</summary>
        public byte[] Data {
            get => this.data;

            set {
                if (value == null) throw new ArgumentNullException("value");
                if (value.Length != 8) throw new ArgumentException("value must be eight bytes in length.", "value");

                this.data = value;
            }
        }

        /// <summary>Constructs a new message.</summary>
        public Message() {
            this.data = new byte[8];
            this.arbitrationId = 0;
            this.length = 0;
            this.isRTR = false;
            this.isEID = false;
            this.timeStamp = DateTime.Now;
        }

        /// <summary>Constructs a new message with no data.</summary>
        /// <param name="arbitrationId">The arbitration id.</param>
        public Message(uint arbitrationId)
            : this(arbitrationId, null, 0, 0) {
        }

        /// <summary>Constructs a new message.</summary>
        /// <param name="arbitrationId">The arbitration id.</param>
        /// <param name="data">The message data.</param>
        public Message(uint arbitrationId, byte[] data)
            : this(arbitrationId, data, 0, data != null ? data.Length : 0) {
        }

        /// <summary>Constructs a new message.</summary>
        /// <param name="arbitrationId">The arbitration id.</param>
        /// <param name="data">The message data.</param>
        /// <param name="offset">The offset into the buffer from which to create the message.</param>
        /// <param name="count">The number of bytes in the message.</param>
        public Message(uint arbitrationId, byte[] data, int offset, int count)
            : this(arbitrationId, data, offset, count, false, false) {
        }

        /// <summary>Constructs a new message.</summary>
        /// <param name="arbitrationId">The arbitration id.</param>
        /// <param name="data">The message data.</param>
        /// <param name="offset">The offset into the buffer from which to create the message.</param>
        /// <param name="count">The number of bytes in the message.</param>
        /// <param name="isRTR">If the message is a remote transmission request.</param>
        /// <param name="isEID">If the id is an extended id.</param>
        public Message(uint arbitrationId, byte[] data, int offset, int count, bool isRTR, bool isEID) {
            if (count < 0 || count > 8) throw new ArgumentOutOfRangeException("count", "count must be between zero and eight.");
            if (data == null && count != 0) throw new ArgumentOutOfRangeException("count", "count must be zero when data is null.");
            if (count != 0 && offset + count > data.Length) throw new ArgumentOutOfRangeException("data", "data.Length must be at least offset + count.");
            if (isEID && arbitrationId > 0x1FFFFFFF) throw new ArgumentOutOfRangeException("arbitrationId", "arbitrationId must not exceed 29 bits when using an Extended ID.");
            if (!isEID && arbitrationId > 0x7FF) throw new ArgumentOutOfRangeException("arbitrationId", "arbitrationId must not exceed 11 bits when not using an Extended ID.");

            this.arbitrationId = arbitrationId;
            this.isRTR = isRTR;
            this.isEID = isEID;
            this.timeStamp = DateTime.Now;
            this.length = count;
            this.data = new byte[8];

            if (count != 0)
                Array.Copy(data, offset, this.data, 0, count);
        }
    }
    /// <summary>Possible CAN errors.</summary>
    public enum Error : byte {
        /// <summary>A CAN message was lost because the hardware was not able to receive the message in time.</summary>
        Overrun,
        /// <summary>
        /// A CAN message was lost because the receive buffer is full. Try increasing the receive message buffer in the CAN constructor and use CAN filters.
        /// </summary>
        RXOver,
        /// <summary>A CAN bus off condition was encountered. The controller is disabled in this case. Use Reset() to re-enable it.</summary>
        BusOff,
        /// <summary>An error passive condition was encountered.</summary>
        ErrorPassive,
    }

}
