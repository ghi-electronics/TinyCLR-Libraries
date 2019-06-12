
namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    /// <summary>
    /// Context for MQTT message
    /// </summary>
    public class MqttMsgContext {
        /// <summary>
        /// MQTT message
        /// </summary>
        public MqttMsgBase Message { get; set; }

        /// <summary>
        /// MQTT message state
        /// </summary>
        public MqttMsgState State { get; set; }

        /// <summary>
        /// Flow of the message
        /// </summary>
        public MqttMsgFlow Flow { get; set; }

        /// <summary>
        /// Timestamp in ticks (for retry)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Attempt (for retry)
        /// </summary>
        public int Attempt { get; set; }

        /// <summary>
        /// Unique key
        /// </summary>
        public string Key => this.Flow + "_" + this.Message.MessageId;
    }

    /// <summary>
    /// Flow of the message
    /// </summary>
    public enum MqttMsgFlow {
        /// <summary>
        /// To publish to subscribers
        /// </summary>
        ToPublish,

        /// <summary>
        /// To acknowledge to publisher
        /// </summary>
        ToAcknowledge
    }

    /// <summary>
    /// MQTT message state
    /// </summary>
    public enum MqttMsgState {
        /// <summary>
        /// QOS = 0, Message queued
        /// </summary>
        QueuedQos0,

        /// <summary>
        /// QOS = 1, Message queued
        /// </summary>
        QueuedQos1,

        /// <summary>
        /// QOS = 2, Message queued
        /// </summary>
        QueuedQos2,

        /// <summary>
        /// QOS = 1, PUBLISH sent, wait for PUBACK
        /// </summary>
        WaitForPuback,

        /// <summary>
        /// QOS = 2, PUBLISH sent, wait for PUBREC
        /// </summary>
        WaitForPubrec,

        /// <summary>
        /// QOS = 2, PUBREC sent, wait for PUBREL
        /// </summary>
        WaitForPubrel,

        /// <summary>
        /// QOS = 2, PUBREL sent, wait for PUBCOMP
        /// </summary>
        WaitForPubcomp,

        /// <summary>
        /// QOS = 2, start first phase handshake send PUBREC
        /// </summary>
        SendPubrec,

        /// <summary>
        /// QOS = 2, start second phase handshake send PUBREL
        /// </summary>
        SendPubrel,

        /// <summary>
        /// QOS = 2, end second phase handshake send PUBCOMP
        /// </summary>
        SendPubcomp,

        /// <summary>
        /// QOS = 1, PUBLISH received, send PUBACK
        /// </summary>
        SendPuback,

        // [v3.1.1] SUBSCRIBE isn't "officially" QOS = 1
        /// <summary>
        /// Send SUBSCRIBE message
        /// </summary>
        SendSubscribe,

        // [v3.1.1] UNSUBSCRIBE isn't "officially" QOS = 1
        /// <summary>
        /// Send UNSUBSCRIBE message
        /// </summary>
        SendUnsubscribe,

        /// <summary>
        /// (QOS = 1), SUBSCRIBE sent, wait for SUBACK
        /// </summary>
        WaitForSuback,

        /// <summary>
        /// (QOS = 1), UNSUBSCRIBE sent, wait for UNSUBACK
        /// </summary>
        WaitForUnsuback
    }
}
