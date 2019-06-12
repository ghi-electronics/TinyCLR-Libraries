
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
using System;
#else
using Microsoft.SPOT;
#endif

namespace GHIElectronics.TinyCLR.Networking.Mqtt
{
    /// <summary>
    /// Event Args class for subscribe request on topics
    /// </summary>
    public class MqttMsgSubscribeEventArgs : EventArgs
    {
        #region Properties...

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId {
            get => this.messageId;
            internal set => this.messageId = value;
        }

        /// <summary>
        /// Topics requested to subscribe
        /// </summary>
        public string[] Topics {
            get => this.topics;
            internal set => this.topics = value;
        }

        /// <summary>
        /// List of QOS Levels requested
        /// </summary>
        public byte[] QoSLevels {
            get => this.qosLevels;
            internal set => this.qosLevels = value;
        }

        #endregion

        // message identifier
        private ushort messageId;

        // topics requested to subscribe
        private string[] topics;

        // QoS levels requested
        private byte[] qosLevels;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribe topics request</param>
        /// <param name="topics">Topics requested to subscribe</param>
        /// <param name="qosLevels">List of QOS Levels requested</param>
        public MqttMsgSubscribeEventArgs(ushort messageId, string[] topics, byte[] qosLevels)
        {
            this.messageId = messageId;
            this.topics = topics;
            this.qosLevels = qosLevels;
        }
    }
}
