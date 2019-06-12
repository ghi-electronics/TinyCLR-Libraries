using GHIElectronics.TinyCLR.Networking.Mqtt.Messages;

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Internal
{
    /// <summary>
    /// Internal event for a published message
    /// </summary>
    public class MsgPublishedInternalEvent : MsgInternalEvent
    {
        #region Properties...

        /// <summary>
        /// Message published (or failed due to retries)
        /// </summary>
        public bool IsPublished
        {
            get { return this.isPublished; }
            internal set { this.isPublished = value; }
        }

        #endregion

        // published flag
        bool isPublished;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message published</param>
        /// <param name="isPublished">Publish flag</param>
        public MsgPublishedInternalEvent(MqttMsgBase msg, bool isPublished) 
            : base(msg)
        {
            this.isPublished = isPublished;
        }
    }
}
