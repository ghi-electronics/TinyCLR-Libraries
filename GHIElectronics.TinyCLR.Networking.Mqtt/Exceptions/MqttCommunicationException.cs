
using System;

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Exceptions
{
    /// <summary>
    /// Exception due to error communication with broker on socket
    /// </summary>
    public class MqttCommunicationException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MqttCommunicationException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Inner Exception</param>
        public MqttCommunicationException(Exception e)
            : base(string.Empty, e)
        {
        }
    }
}
