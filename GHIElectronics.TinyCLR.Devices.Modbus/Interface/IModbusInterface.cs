using System;

namespace GHIElectronics.TinyCLR.Devices.Modbus.Interface {
    /// <summary>
    /// Interface for physical modbus implementations
    /// </summary>
    public interface IModbusInterface {
        /// <summary>
        /// Gets the maximum data length (not including address, function code, ...) of e telegram.
        /// </summary>
        short MaxDataLength { get; }

        /// <summary>
        /// Gets the maximum length of a Modbus telegram.
        /// </summary>
        short MaxTelegramLength { get; }

        /// <summary>
        /// Creates a new telegram for a modbus request or response.
        /// All data except the function code specific user data is written into the given buffer.
        /// </summary>
        /// <param name="addr">Device address. 0 = Breadcast, 1..247 are valid device addresses.</param>
        /// <param name="fkt">Function code. <see cref="ModbusFunctionCode"/></param>
        /// <param name="dataLength">Number of bytes for function code sspecific user data.</param>
        /// <param name="buffer">Buffer to write data into. The buffer must be at least MaxTelegramLength - MaxDataLength + dataLength bytes long.</param>
        /// <param name="telegramLength">Returns the total length of the telegram in bytes.</param>
        /// <param name="dataPos">Returns the offset of the function code specific user data in buffer.</param>
        /// <param name="isResponse">true if this is a response telegram; false if this is a request telegram.</param>
        /// <param name="telegramContext">
        /// If isResponse == false, this parameter returns the interface implementation specific data which must be passed to the ParseTelegram method of the received response.
        /// If isResponse == true, this parameter must be called with the telegramContext parameter returned by ParseTelegram of the request telegram.</param>
        void CreateTelegram(byte addr, byte fkt, short dataLength, byte[] buffer, out short telegramLength, out short dataPos, bool isResponse, ref object telegramContext);

        void PrepareWrite();

        void PrepareRead();


        /// <summary>
        /// Sends the given telegram.
        /// If necessary additional information like a checksum can be inserted here.
        /// </summary>
        /// <param name="buffer">Buffer containing the data.</param>
        /// <param name="telegramLength">Length of the telegram in bytes.</param>
        void SendTelegram(byte[] buffer, short telegramLength);

        /// <summary>
        /// Waits and receives a telegram.
        /// </summary>
        /// <param name="buffer">Buffer to write data into.</param>
        /// <param name="desiredDataLength">Desired length of the function code specific data in bytes. -1 if length is unknown.</param>
        /// <param name="timeout">Timeout in milliseconds to wait for the telegram.</param>
        /// <param name="telegramLength">Returns the total length of the telegram in bytes.</param>
        /// <returns>Returns true if the telegram was received successfully; false on timeout.</returns>
        bool ReceiveTelegram(byte[] buffer, short desiredDataLength, int timeout, out short telegramLength);

        /// <summary>
        /// Parses a telegram received by ReceiveTelegram.
        /// </summary>
        /// <param name="buffer">Buffer containing the data.</param>
        /// <param name="telegramLength">Total length of the telegram in bytes.</param>
        /// <param name="isResponse">true if the telegram is a response telegram; false if the telegram is a request telegram.</param>
        /// <param name="telegramContext">
        /// If isResponse == true: pass the telegramContext returned by CreateTelegram from the request.
        /// If isResponse == false: returns the telegramContext from the received request. It must pe passed to the CreateTelegram method for the response.
        /// </param>
        /// <param name="address">Returns the device address.</param>
        /// <param name="fkt">Returns the function code.</param>
        /// <param name="dataPos">Returns the offset in buffer of the function code specific data.</param>
        /// <param name="dataLength">Returns the length of the function code specific data.</param>
        /// <returns>Returns true if this is the matching response according to the telegramContext; else false. If isResponse == false this method should return always true.</returns>
        bool ParseTelegram(byte[] buffer, short telegramLength, bool isResponse, ref object telegramContext, out byte address, out byte fkt,
                           out short dataPos, out short dataLength);

        /// <summary>
        /// Gets if there is currently data available on the interface.
        /// </summary>
        bool IsDataAvailable { get; }

        /// <summary>
        /// Removes all data from the input interface.
        /// </summary>
        void ClearInputBuffer();

        /// <summary>
        /// Gets if the connection is ok
        /// </summary>
        bool IsConnectionOk { get; }
    }
}
