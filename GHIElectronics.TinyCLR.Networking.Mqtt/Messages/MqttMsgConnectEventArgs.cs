using System;

namespace GHIElectronics.TinyCLR.Networking.Mqtt
{
    /// <summary>
    /// Event Args class for CONNECT message received from client
    /// </summary>
    public class MqttMsgConnectEventArgs : EventArgs
    {
        /// <summary>
        /// Message received from client
        /// </summary>
        public MqttMsgConnect Message { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">CONNECT message received from client</param>
        public MqttMsgConnectEventArgs(MqttMsgConnect connect) => this.Message = connect;
    }
} 
