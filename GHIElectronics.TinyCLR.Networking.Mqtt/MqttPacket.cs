using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    public enum PacketType {
        Connect = 1,
        ConnAck = 2,
        Publish = 3,
        PubAck = 4,
        Pubrec = 5,
        Pubrel = 6,
        PubComp = 7,
        Subscribe = 8,
        Suback = 9,
        Unsubscribe = 10,
        Unsuback = 11,
        PingReq = 12,
        PingResp = 13,
        Disconnect = 14,
    }

    public sealed class MqttPacket {
        internal const byte PACKET_TYPE_MASK = 0xF0;
        internal const byte PACKET_TYPE_OFFSET = 0x04;
        internal const byte PACKET_TYPE_SIZE = 0x04;
        internal const byte PACKET_FLAG_BITS_MASK = 0x0F;
        internal const byte PACKET_FLAG_BITS_OFFSET = 0x00;
        internal const byte PACKET_FLAG_BITS_SIZE = 0x04;
        internal const byte DUPLICATE_FLAG_MASK = 0x08;
        internal const byte DUPLICATE_FLAG_OFFSET = 0x03;
        internal const byte DUPLICATE_FLAG_SIZE = 0x01;
        internal const byte QOS_LEVEL_MASK = 0x06;
        internal const byte QOS_LEVEL_OFFSET = 0x01;
        internal const byte QOS_LEVEL_SIZE = 0x02;
        internal const byte RETAIN_FLAG_MASK = 0x01;
        internal const byte RETAIN_FLAG_OFFSET = 0x00;
        internal const byte RETAIN_FLAG_SIZE = 0x01;

        internal const byte PACKET_CONNECT_FLAG_BITS = 0x00;
        internal const byte PACKET_CONNACK_FLAG_BITS = 0x00;
        internal const byte PACKET_PUBLISH_FLAG_BITS = 0x00;
        internal const byte PACKET_PUBACK_FLAG_BITS = 0x00;
        internal const byte PACKET_PUBREC_FLAG_BITS = 0x00;
        internal const byte PACKET_PUBREL_FLAG_BITS = 0x02;
        internal const byte PACKET_PUBCOMP_FLAG_BITS = 0x00;
        internal const byte PACKET_SUBSCRIBE_FLAG_BITS = 0x02;
        internal const byte PACKET_SUBACK_FLAG_BITS = 0x00;
        internal const byte PACKET_UNSUBSCRIBE_FLAG_BITS = 0x02;
        internal const byte PACKET_UNSUBACK_FLAG_BITS = 0x00;
        internal const byte PACKET_PINGREQ_FLAG_BITS = 0x00;
        internal const byte PACKET_PINGRESP_FLAG_BITS = 0x00;
        internal const byte PACKET_DISCONNECT_FLAG_BITS = 0x00;



        internal enum PacketDirection {
            ToServer,
            ToClient
        }

        internal enum PacketState {
            WaitToPublish,
            WaitForPublishAck,
            SendSubscribe,
            SendUnsubscribe,
            WaitForSubscribeAck,
            WaitForUnsubscribeAck
        }

        internal ConnectReturnCode ReturnCode { get; set; }

        internal const string PROTOCOL_NAME_V3_1_1 = "MQTT";
        internal const byte PROTOCOL_NAME_V3_1_1_SIZE = 4;

        internal const byte USERNAME_FLAG_MASK = 0x80;
        internal const byte USERNAME_FLAG_OFFSET = 0x07;

        internal const byte PASSWORD_FLAG_MASK = 0x40;
        internal const byte PASSWORD_FLAG_OFFSET = 0x06;

        internal const byte LASTWILLRETAIN_FLAG_MASK = 0x20;
        internal const byte LASTWILLRETAIN_FLAG_OFFSET = 0x05;

        internal const byte LASTWILLQOS_FLAG_MASK = 0x18;
        internal const byte LASTWILLQOS_FLAG_OFFSET = 0x03;

        internal const byte LASTWILL_FLAG_MASK = 0x04;
        internal const byte LASTWILL_FLAG_OFFSET = 0x02;

        internal const byte CLEAN_SESSION_FLAG_MASK = 0x02;
        internal const byte CLEAN_SESSION_FLAG_OFFSET = 0x01;

        // public
        public byte[] Data { get; internal set; }
        public uint PacketId { get; internal set; }
        public bool IsPublished { get; internal set; }

        internal string[] Topics { get; set; }
        internal QoSLevel[] QosLevels { get; set; }

        internal bool SessionPresent { get; set; }
        internal PacketType Type { get; set; }
        internal bool IsDuplicated { get; set; }
        internal PacketState State { get; set; }
        internal PacketDirection Direction { get; set; }
        internal int RetryCount { get; set; }
        internal string ClientId { get; set; }
        internal bool LastWillRetain { get; set; }
        internal bool CleanSession { get; set; }
        internal QoSLevel LastWillQosLevel { get; set; }
        internal string LastWillTopic { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal int KeepAliveTimeout { get; set; } = 60;

        internal MqttPacket(PacketType type) => this.Type = type;

        internal byte[] CreatePacket() {
            byte[] buffer = null;
            var vheaderSize = 0;
            var payloadSize = 0;
            var remainSize = 0;
            var index = 0;

            int fheaderSize;
            if (this.Type == PacketType.Connect) {

                var clientIdToByte = Encoding.UTF8.GetBytes(this.ClientId);
                var usernameToBytes = ((this.Username != null) && (this.Username.Length > 0)) ? Encoding.UTF8.GetBytes(this.Username) : null;
                var passwordToBytes = ((this.Password != null) && (this.Password.Length > 0)) ? Encoding.UTF8.GetBytes(this.Password) : null;

                vheaderSize += 2;

                vheaderSize += PROTOCOL_NAME_V3_1_1_SIZE;

                vheaderSize++;// protocal 1 bytes

                vheaderSize++; // connect flag 1 bytes

                vheaderSize += 2; // keep alive 2 bytes

                payloadSize += clientIdToByte.Length + 2;

                payloadSize += (usernameToBytes != null) ? (usernameToBytes.Length + 2) : 0;

                payloadSize += (passwordToBytes != null) ? (passwordToBytes.Length + 2) : 0;

                remainSize += (vheaderSize + payloadSize);

                fheaderSize = 1;

                var tmp = remainSize;
                do {
                    fheaderSize++;
                    tmp /= 128;
                } while (tmp > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index++] = ((byte)PacketType.Connect << PACKET_TYPE_OFFSET) | PACKET_CONNECT_FLAG_BITS;

                index = RemainingSizeToPacket(remainSize, buffer, index);

                buffer[index++] = 0;

                // protocal 3.1.1
                buffer[index++] = PROTOCOL_NAME_V3_1_1_SIZE;
                Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1_1), 0, buffer, index, PROTOCOL_NAME_V3_1_1_SIZE);
                index += PROTOCOL_NAME_V3_1_1_SIZE;
                buffer[index++] = 0x4; // protocal version

                byte connectFlags = 0x00;

                connectFlags |= (usernameToBytes != null) ? (byte)(1 << USERNAME_FLAG_OFFSET) : (byte)0x00;
                connectFlags |= (passwordToBytes != null) ? (byte)(1 << PASSWORD_FLAG_OFFSET) : (byte)0x00;
                connectFlags |= (this.LastWillRetain) ? (byte)(1 << LASTWILLRETAIN_FLAG_OFFSET) : (byte)0x00;

                connectFlags |= (this.CleanSession) ? (byte)(1 << CLEAN_SESSION_FLAG_OFFSET) : (byte)0x00;
                buffer[index++] = connectFlags;

                buffer[index++] = (byte)((this.KeepAliveTimeout >> 8) & 0x00FF);
                buffer[index++] = (byte)(this.KeepAliveTimeout & 0x00FF);

                buffer[index++] = (byte)((clientIdToByte.Length >> 8) & 0x00FF);
                buffer[index++] = (byte)(clientIdToByte.Length & 0x00FF);

                Array.Copy(clientIdToByte, 0, buffer, index, clientIdToByte.Length);
                index += clientIdToByte.Length;

                if (usernameToBytes != null) {
                    buffer[index++] = (byte)((usernameToBytes.Length >> 8) & 0x00FF);
                    buffer[index++] = (byte)(usernameToBytes.Length & 0x00FF);
                    Array.Copy(usernameToBytes, 0, buffer, index, usernameToBytes.Length);
                    index += usernameToBytes.Length;
                }

                if (passwordToBytes != null) {
                    buffer[index++] = (byte)((passwordToBytes.Length >> 8) & 0x00FF);
                    buffer[index++] = (byte)(passwordToBytes.Length & 0x00FF);
                    Array.Copy(passwordToBytes, 0, buffer, index, passwordToBytes.Length);
                }
            }
            else if (this.Type == PacketType.PingReq) {
                buffer = new byte[2];

                buffer[index++] = ((byte)PacketType.PingReq << PACKET_TYPE_OFFSET) | PACKET_PINGREQ_FLAG_BITS;// 3.1.1

                buffer[index++] = 0x00;
            }
            else if (this.Type == PacketType.PubAck) {

                vheaderSize += 2; //packet id 2 bytes

                remainSize += (vheaderSize + payloadSize);

                fheaderSize = 1;

                var temp = remainSize;
                do {
                    fheaderSize++;
                    temp /= 128;
                } while (temp > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index++] = ((byte)PacketType.PubAck << PACKET_TYPE_OFFSET) | PACKET_PUBACK_FLAG_BITS; // 3.1.1

                index = RemainingSizeToPacket(remainSize, buffer, index);

                buffer[index++] = (byte)((this.PacketId >> 8) & 0x00FF);
                buffer[index++] = (byte)(this.PacketId & 0x00FF);
            }
            else if (this.Type == PacketType.PubComp) {

                vheaderSize += 2; //packet id 2 bytes;

                remainSize += (vheaderSize + payloadSize);

                fheaderSize = 1;

                var tmpSize = remainSize;

                do {
                    fheaderSize++;
                    tmpSize = tmpSize / 128;
                } while (tmpSize > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index++] = ((byte)PacketType.PubComp << PACKET_TYPE_OFFSET) | PACKET_PUBCOMP_FLAG_BITS; // 3.1.1

                index = RemainingSizeToPacket(remainSize, buffer, index);

                buffer[index++] = (byte)((this.PacketId >> 8) & 0x00FF);
                buffer[index++] = (byte)(this.PacketId & 0x00FF);
            }
            else if (this.Type == PacketType.Publish) {
                var topicToBytes = Encoding.UTF8.GetBytes(this.LastWillTopic);

                vheaderSize += topicToBytes.Length + 2;

                if ((this.LastWillQosLevel == QoSLevel.LeastOnce) ||
                    (this.LastWillQosLevel == QoSLevel.ExactlyOnce)) {
                    vheaderSize += 2; //packet id 2 bytes;
                }

                if (this.Data != null)
                    payloadSize += this.Data.Length;

                remainSize += (vheaderSize + payloadSize);
                fheaderSize = 1;

                var tmpSize = remainSize;

                do {
                    fheaderSize++;
                    tmpSize /= 128;
                } while (tmpSize > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index] = (byte)(((byte)PacketType.Publish << PACKET_TYPE_OFFSET) |
                                       ((byte)this.LastWillQosLevel << QOS_LEVEL_OFFSET));
                buffer[index] |= this.IsDuplicated ? (byte)(1 << DUPLICATE_FLAG_OFFSET) : (byte)0x00;
                buffer[index] |= this.LastWillRetain ? (byte)(1 << RETAIN_FLAG_OFFSET) : (byte)0x00;
                index++;

                index = RemainingSizeToPacket(remainSize, buffer, index);

                buffer[index++] = (byte)((topicToBytes.Length >> 8) & 0x00FF);
                buffer[index++] = (byte)(topicToBytes.Length & 0x00FF);
                Array.Copy(topicToBytes, 0, buffer, index, topicToBytes.Length);
                index += topicToBytes.Length;

                if ((this.LastWillQosLevel == QoSLevel.LeastOnce) ||
                    (this.LastWillQosLevel == QoSLevel.ExactlyOnce)) {
                    if (this.PacketId == 0)
                        throw new Exception("PacketId cannot be 0.");

                    buffer[index++] = (byte)((this.PacketId >> 8) & 0x00FF);
                    buffer[index++] = (byte)(this.PacketId & 0x00FF);
                }

                if (this.Data != null) {
                    Array.Copy(this.Data, 0, buffer, index, this.Data.Length);
                }
            }

            else if (this.Type == PacketType.Subscribe) {
                vheaderSize += 2; //packet id 2 bytes;

                var topicsToBytes = new byte[this.Topics.Length][];

                int topicIndex;
                for (topicIndex = 0; topicIndex < this.Topics.Length; topicIndex++) {

                    topicsToBytes[topicIndex] = Encoding.UTF8.GetBytes(this.Topics[topicIndex]);
                    payloadSize += 2;
                    payloadSize += topicsToBytes[topicIndex].Length;
                    payloadSize++;
                }

                remainSize += (vheaderSize + payloadSize);
                fheaderSize = 1;

                var tmpSize = remainSize;
                do {
                    fheaderSize++;
                    tmpSize /= 128;
                } while (tmpSize > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index++] = ((byte)PacketType.Subscribe << PACKET_TYPE_OFFSET) | PACKET_SUBSCRIBE_FLAG_BITS; // 3.1.1

                index = RemainingSizeToPacket(remainSize, buffer, index);

                if (this.PacketId == 0)
                    throw new Exception("PacketId cannot be 0.");

                buffer[index++] = (byte)((this.PacketId >> 8) & 0x00FF);
                buffer[index++] = (byte)(this.PacketId & 0x00FF);
                for (topicIndex = 0; topicIndex < this.Topics.Length; topicIndex++) {
                    buffer[index++] = (byte)((topicsToBytes[topicIndex].Length >> 8) & 0x00FF);
                    buffer[index++] = (byte)(topicsToBytes[topicIndex].Length & 0x00FF);
                    Array.Copy(topicsToBytes[topicIndex], 0, buffer, index, topicsToBytes[topicIndex].Length);
                    index += topicsToBytes[topicIndex].Length;

                    buffer[index++] = (byte)this.QosLevels[topicIndex];
                }

            }
            else if (this.Type == PacketType.Unsubscribe) {
                vheaderSize += 2; //packet id 2 bytes;

                var topicsToBytes = new byte[this.Topics.Length][];

                int toppicIndex;
                for (toppicIndex = 0; toppicIndex < this.Topics.Length; toppicIndex++) {
                    topicsToBytes[toppicIndex] = Encoding.UTF8.GetBytes(this.Topics[toppicIndex]);
                    payloadSize += 2;
                    payloadSize += topicsToBytes[toppicIndex].Length;
                }

                remainSize += (vheaderSize + payloadSize);
                fheaderSize = 1;

                var tmpSize = remainSize;

                do {
                    fheaderSize++;
                    tmpSize /= 128;
                } while (tmpSize > 0);

                buffer = new byte[fheaderSize + vheaderSize + payloadSize];

                buffer[index++] = ((byte)PacketType.Unsubscribe << PACKET_TYPE_OFFSET) | PACKET_UNSUBSCRIBE_FLAG_BITS; // 3.1.1

                index = RemainingSizeToPacket(remainSize, buffer, index);

                if (this.PacketId == 0)
                    throw new Exception("PacketId cannot be 0.");

                buffer[index++] = (byte)((this.PacketId >> 8) & 0x00FF);
                buffer[index++] = (byte)(this.PacketId & 0x00FF);
                for (toppicIndex = 0; toppicIndex < this.Topics.Length; toppicIndex++) {
                    buffer[index++] = (byte)((topicsToBytes[toppicIndex].Length >> 8) & 0x00FF);
                    buffer[index++] = (byte)(topicsToBytes[toppicIndex].Length & 0x00FF);
                    Array.Copy(topicsToBytes[toppicIndex], 0, buffer, index, topicsToBytes[toppicIndex].Length);
                    index += topicsToBytes[toppicIndex].Length;
                }

            }
            else if (this.Type == PacketType.Disconnect) {
                buffer = new byte[2];

                buffer[index++] = ((byte)PacketType.Disconnect << PACKET_TYPE_OFFSET) | PACKET_DISCONNECT_FLAG_BITS; // 3.1.1

                buffer[index++] = 0x00;
            }

            return buffer;
        }

        internal static int RemainingSizeToPacket(int remainSize, byte[] buffer, int index) {
            do {
                var d = remainSize & 0x7F;
                remainSize >>= 7;
                if (remainSize > 0)
                    d = d | 0x80;
                buffer[index++] = (byte)d;
            } while (remainSize > 0);

            return index;
        }

        internal static int RemainSizeFromStream(MqttStream stream) {
            var mul = 1;
            var v = 0;
            int d;
            do {
                var data = new byte[1];

                stream.Receive(data);

                d = data[0];
                v += ((d & 0x7F) * mul);
                mul <<= 7;
            } while ((d & 0x80) != 0);

            return v;
        }
    }
}
