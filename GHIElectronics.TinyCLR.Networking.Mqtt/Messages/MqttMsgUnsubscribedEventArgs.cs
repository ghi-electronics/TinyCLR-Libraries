#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
using System;
#else
using Microsoft.SPOT;
#endif

namespace GHIElectronics.TinyCLR.Networking.Mqtt
{
    /// <summary>
    /// Event Args class for unsubscribed topic
    /// </summary>
    public class MqttMsgUnsubscribedEventArgs : EventArgs
    {
        #region Properties...

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId {
            get => this.messageId;
            internal set => this.messageId = value;
        }

        #endregion

        // message identifier
        private ushort messageId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for unsubscribed topic</param>
        public MqttMsgUnsubscribedEventArgs(ushort messageId) => this.messageId = messageId;
    }
}
