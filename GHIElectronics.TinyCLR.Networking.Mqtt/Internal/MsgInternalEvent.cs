using GHIElectronics.TinyCLR.Networking.Mqtt.Messages;

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Internal
{
    /// <summary>
    /// Internal event with a message
    /// </summary>
    public class MsgInternalEvent : InternalEvent
    {
        #region Properties ...

        /// <summary>
        /// Related message
        /// </summary>
        public MqttMsgBase Message
        {
            get { return this.msg; }
            set { this.msg = value; }
        }

        #endregion

        // related message
        protected MqttMsgBase msg;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Related message</param>
        public MsgInternalEvent(MqttMsgBase msg)
        {
            this.msg = msg;
        }
    }
}
