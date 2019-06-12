#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
using System;
#else
using Microsoft.SPOT;
#endif

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Messages {
    /// <summary>
    /// Event Args class for PUBLISH message received from broker
    /// </summary>
    public class MqttMsgPublishEventArgs : EventArgs {
        #region Properties...

        /// <summary>
        /// Message topic
        /// </summary>
        public string Topic {
            get => this.topic;
            internal set => this.topic = value;
        }

        /// <summary>
        /// Message data
        /// </summary>
        public byte[] Message {
            get => this.message;
            internal set => this.message = value;
        }

        /// <summary>
        /// Duplicate message flag
        /// </summary>
        public bool DupFlag {
            get => this.dupFlag;
            set => this.dupFlag = value;
        }

        /// <summary>
        /// Quality of Service level
        /// </summary>
        public byte QosLevel {
            get => this.qosLevel;
            internal set => this.qosLevel = value;
        }

        /// <summary>
        /// Retain message flag
        /// </summary>
        public bool Retain {
            get => this.retain;
            internal set => this.retain = value;
        }

        #endregion

        // message topic
        private string topic;
        // message data
        private byte[] message;
        // duplicate delivery
        private bool dupFlag;
        // quality of service level
        private byte qosLevel;
        // retain flag
        private bool retain;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data</param>
        /// <param name="dupFlag">Duplicate delivery flag</param>
        /// <param name="qosLevel">Quality of Service level</param>
        /// <param name="retain">Retain flag</param>
        public MqttMsgPublishEventArgs(string topic,
            byte[] message,
            bool dupFlag,
            byte qosLevel,
            bool retain) {
            this.topic = topic;
            this.message = message;
            this.dupFlag = dupFlag;
            this.qosLevel = qosLevel;
            this.retain = retain;
        }
    }
}
