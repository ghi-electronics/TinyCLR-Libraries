#if BROKER
using System.Collections;
using System.Collections.Generic;
using GHIElectronics.TinyCLR.Networking.Mqtt.Managers;
using GHIElectronics.TinyCLR.Networking.Mqtt.Messages;

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Session
{
    /// <summary>
    /// MQTT Broker Session
    /// </summary>
    public class MqttBrokerSession : MqttSession
    {
        /// <summary>
        /// Client related to the subscription
        /// </summary>
        public MqttClient Client { get; set; }

        /// <summary>
        /// Subscriptions for the client session
        /// </summary>
        public List<MqttSubscription> Subscriptions;

        /// <summary>
        /// Outgoing messages to publish
        /// </summary>
        public Queue<MqttMsgPublish> OutgoingMessages;

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttBrokerSession()
            : base()
        {
            this.Client = null;
            this.Subscriptions = new List<MqttSubscription>();
            this.OutgoingMessages = new Queue<MqttMsgPublish>();
        }

        public override void Clear()
        {
            base.Clear();
            this.Client = null;
            this.Subscriptions.Clear();
            this.OutgoingMessages.Clear();
        }
    }
}
#endif