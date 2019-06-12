#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
using System;
#else
using Microsoft.SPOT;
#endif

namespace GHIElectronics.TinyCLR.Networking.Mqtt
{
    /// <summary>
    /// Event Args class for unsubscribe request on topics
    /// </summary>
    public class MqttMsgUnsubscribeEventArgs : EventArgs
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

        #endregion

        // message identifier
        private ushort messageId;

        // topics requested to unsubscribe
        private string[] topics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="topics">Topics requested to subscribe</param>
        public MqttMsgUnsubscribeEventArgs(ushort messageId, string[] topics)
        {
            this.messageId = messageId;
            this.topics = topics;
        }
    }
}
