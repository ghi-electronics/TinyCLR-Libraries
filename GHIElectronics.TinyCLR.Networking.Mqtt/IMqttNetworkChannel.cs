namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    /// <summary>
    /// Interface for channel under MQTT library
    /// </summary>
    public interface IMqttNetworkChannel {
        /// <summary>
        /// Data available on channel
        /// </summary>
        bool DataAvailable { get; }

        /// <summary>
        /// Receive data from the network channel
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <returns>Number of bytes received</returns>
        int Receive(byte[] buffer);

        /// <summary>
        /// Receive data from the network channel with a specified timeout
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <param name="timeout">Timeout on receiving (in milliseconds)</param>
        /// <returns>Number of bytes received</returns>
        int Receive(byte[] buffer, int timeout);

        /// <summary>
        /// Send data on the network channel to the broker
        /// </summary>
        /// <param name="buffer">Data buffer to send</param>
        /// <returns>Number of byte sent</returns>
        int Send(byte[] buffer);

        /// <summary>
        /// Close the network channel
        /// </summary>
        void Close();

        /// <summary>
        /// Connect to remote server
        /// </summary>
        void Connect();

        /// <summary>
        /// Accept client connection
        /// </summary>
        void Accept();
    }
}
