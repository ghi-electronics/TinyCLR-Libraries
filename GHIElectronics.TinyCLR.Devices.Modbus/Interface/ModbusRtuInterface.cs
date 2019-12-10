using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;

namespace GHIElectronics.TinyCLR.Devices.Modbus.Interface {
    /// <summary>
    /// ModbusRtuInterface is a Modbus RTU implemention of the <see cref="IModbusInterface"/> interface 
    /// to be used with <see cref="ModbusMaster"/> or <see cref="ModbusDevice"/>.
    /// </summary>
    public class ModbusRtuInterface : IModbusInterface {
        private readonly UartController serial;
        private readonly long halfCharLength;
        private long nextSend;

        private int baudRate;
        private int dataBits;
        private UartStopBitCount stopBits;
        private UartParity parity;
        public bool isOpen = true;
        /// <summary>
        /// Gets the serial port which is used.
        /// </summary>
        public UartController UartController => this.serial;

        /// <summary>
        /// Creates a new Modbus RTU interface using an existing and fully initialized <see cref="UartController"/>.
        /// This interface can be used with <see cref="ModbusMaster"/> or <see cref="ModbusDevice"/>.
        /// </summary>
        /// <param name="serial">Fully initialized <see cref="UartController"/>.</param>
        /// <param name="maxDataLength">Maximum number of data bytes</param>
        public ModbusRtuInterface(UartController serial, int baudRate, int dataBits, UartStopBitCount stopBits, UartParity parity, short maxDataLength = 252) {
            this.dataBits = dataBits;
            this.baudRate = baudRate;
            this.stopBits = stopBits;
            this.parity = parity;

            if (this.dataBits < 8) {
                throw new ArgumentException("serial.DataBits must be >= 8");
            }

            this.serial = serial;
            this.MaxDataLength = maxDataLength;
            this.MaxTelegramLength = (short)(maxDataLength + 4);

            // calc char length in µs
            if (this.baudRate > 19200) {
                // use a fixed value for high baudrates (recommended by Modbus spec.)
                this.halfCharLength = 500;
            }
            else {
                var bitCnt = (short)this.dataBits;
                switch (this.stopBits) {
                    case UartStopBitCount.One:
                        ++bitCnt;
                        break;

                    case UartStopBitCount.OnePointFive:
                    case UartStopBitCount.Two:
                        bitCnt += 2;
                        break;
                }
                if (this.parity != UartParity.None) {
                    ++bitCnt;
                }
                this.halfCharLength = (short)((bitCnt * 1000 * 10000) / this.baudRate) >> 1;
            }
        }

        /// <summary>
        /// Gets the maximum data length (not including address, function code, ...) of e telegram.
        /// </summary>
        public short MaxDataLength { get; private set; }

        /// <summary>
        /// Gets the maximum length of a Modbus telegram.
        /// </summary>
        public short MaxTelegramLength { get; private set; }

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
        public void CreateTelegram(byte addr, byte fkt, short dataLength, byte[] buffer, out short telegramLength, out short dataPos, bool isResponse, ref object telegramContext) {
            telegramLength = (short)(4 + dataLength);
            dataPos = 2;
            buffer[0] = addr;
            buffer[1] = fkt;
        }

        public void PrepareWrite() {
        }

        public void PrepareRead() {
        }

        /// <summary>
        /// Sends the given telegram.
        /// If necessary additional information like a checksum can be inserted here.
        /// </summary>
        /// <param name="buffer">Buffer containing the data.</param>
        /// <param name="telegramLength">Length of the telegram in bytes.</param>
        public void SendTelegram(byte[] buffer, short telegramLength) {
            var crc = ModbusUtils.CalcCrc(buffer, telegramLength - 2);
            buffer[telegramLength - 2] = (byte)(crc & 0x00ff);
            buffer[telegramLength - 1] = (byte)((crc & 0xff00) >> 8);

            // ticks and _NextSend are multiples of 100 ns
            var dt = this.nextSend - DateTime.Now.Ticks;
            if (dt > 0) {
                Thread.Sleep(Math.Max(1, (int)dt / 10000));
            }

            // clear buffers
            this.serial.ClearReadBuffer();
            this.serial.ClearWriteBuffer();

            // next send is 3.5 chars after the end of this telegram
            this.nextSend = DateTime.Now.Ticks + (telegramLength * 2 + 7) * this.halfCharLength;

            this.serial.Write(buffer, 0, telegramLength);
        }

        /// <summary>
        /// Waits and receives a telegram.
        /// </summary>
        /// <param name="buffer">Buffer to write data into.</param>
        /// <param name="desiredDataLength">Desired length of the function code specific data in bytes. -1 if length is unknown.</param>
        /// <param name="timeout">Timeout in milliseconds to wait for the telegram.</param>
        /// <param name="telegramLength">Returns the total length of the telegram in bytes.</param>
        /// <returns>Returns true if the telegram was received successfully; false on timeout.</returns>
        public bool ReceiveTelegram(byte[] buffer, short desiredDataLength, int timeout, out short telegramLength) {
            short desiredLength;
            if (desiredDataLength >= 0) {
                desiredLength = (short)(desiredDataLength + 4);
                if (desiredLength > buffer.Length) {
                    throw new ArgumentException(string.Concat("buffer size (", buffer.Length, ") must be at least 4 byte larger than desiredDataLength (", desiredDataLength, ")"));
                }
            }
            else {
                desiredLength = -1;
            }

            var n = 0;
            var tOut = DateTime.Now.AddMilliseconds(timeout);
            long nextRead = 0;
            var errorChecked = false;
            while (true) {
                //if ((desiredLength > 0 || n == 0) && DateTime.Now > tOut)
                if (DateTime.Now > tOut) {
                    break;
                }
                if (this.serial.BytesToRead > 0) {
                    if (desiredLength > 0) {
                        n += this.serial.Read(buffer, n, desiredLength - n);
                    }
                    else {
                        n += this.serial.Read(buffer, n, buffer.Length - n);
                    }
                    // a delay of more than 1.5 chars means end of telegram /////, but since this is a extreme short time, we extend it by factor 2
                    nextRead = DateTime.Now.Ticks + 6 * this.halfCharLength;
                }
                if (!errorChecked && n >= 2) {
                    errorChecked = true;
                    if ((buffer[1] & 0x80) != 0) {
                        // modbus error, so desired length is 5
                        desiredLength = 5;
                    }
                }
                if (desiredLength > 0 && n == desiredLength) {
                    telegramLength = (short)n;
                    return true;
                }
                if (desiredLength <= 0 && n >= 2 && DateTime.Now.Ticks > nextRead && this.serial.BytesToRead == 0) {
                    var crc = ModbusUtils.CalcCrc(buffer, n - 2);
                    if (buffer[n - 2] != (byte)(crc & 0x00ff) ||
                        buffer[n - 1] != (byte)((crc & 0xff00) >> 8)) {
                        // read a little bit longer
                        Thread.Sleep(1);
                        nextRead = DateTime.Now.Ticks + 6 * this.halfCharLength;
                    }
                    else {
                        telegramLength = (short)n;
                        return true;
                    }
                }
            }
            telegramLength = 0;
            return false;
        }

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
        public bool ParseTelegram(byte[] buffer, short telegramLength, bool isResponse, ref object telegramContext, out byte address, out byte fkt,
                                  out short dataPos, out short dataLength) {
            if (telegramLength < 4) {
                throw new ModbusException(ModbusErrorCode.ResponseTooShort);
            }
            var crc = ModbusUtils.CalcCrc(buffer, telegramLength - 2);
            if (buffer[telegramLength - 2] != (byte)(crc & 0x00ff) ||
                buffer[telegramLength - 1] != (byte)((crc & 0xff00) >> 8)) {
                throw new ModbusException(ModbusErrorCode.CrcError);
            }
            address = buffer[0];
            fkt = buffer[1];
            dataPos = 2;
            dataLength = (short)(telegramLength - 4);
            return true;
        }

        /// <summary>
        /// Gets if there is currently dataavailable on the interface.
        /// </summary>
        public bool IsDataAvailable => this.serial.BytesToRead > 0;

        /// <summary>
        /// Removes all data from the input interface.
        /// </summary>
        public void ClearInputBuffer() => this.serial.ClearReadBuffer();

        /// <summary>
        /// Gets if the connection is ok
        /// </summary>
        public bool IsConnectionOk => this.serial != null && this.isOpen;
    }
}
