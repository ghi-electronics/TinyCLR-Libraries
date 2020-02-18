using System;
using System.Collections;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.Networking.Mqtt.MqttPacket;

namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    public enum QoSLevel {
        MostOnce = 0,
        LeastOnce = 1,
        ExactlyOnce = 2,
    }

    public enum ConnectReturnCode {
        ConnectionAccepted = 0,
        UnacceptableProtocol = 1,
        IdentifierRejected = 2,
        ServerUnavailable = 3,
        BadUserNameOrPassword = 4,
        NotAuthorized = 5,
        Unknown = -1
    }

    public class MqttConnectionSetting {
        public string ClientId { get; set; }
        public bool CleanSession { get; set; } = true;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LastWillTopic { get; set; }
        public QoSLevel LastWillQos { get; set; } = QoSLevel.LeastOnce;
        public string LastWillMessage { get; set; }
        public bool LastWillRetain { get; set; }
        public int KeepAliveTimeout { get; set; }
    }

    public class MqttClientSetting {
        public string BrokerName { get; set; }
        public int BrokerPort { get; set; }
        public X509Certificate CaCertificate { get; set; }
        public X509Certificate ClientCertificate { get; set; }
        public SslProtocols SslProtocol { get; set; }
    }

    public class Mqtt {
        const int CONNECTION_TIMEOUT_DEFAULT = 60000;
        const int PING_TIMEOUT_DEFAULT = 5000;

        public delegate void PublishReceivedEventHandler(object sender, MqttPacket packet);
        public delegate void PublishedEventHandler(object sender, uint packetId, bool published);
        public delegate void SubscribedEventHandler(object sender, MqttPacket packet);
        public delegate void UnsubscribedEventHandler(object sender, uint packetId);
        public delegate void ConnectedEventHandler(object sender);

        private bool isRunning;

        private AutoResetEvent waitForPushEventEvent;
        private AutoResetEvent waitForPushPacketEvent;
        private AutoResetEvent waitSendReceiveEvent;

        private MqttPacket connectAckReceived;
        private bool isConnectAckReceivedSuccess;

        private int keepAliveTimeoutInMilisecond;
        private AutoResetEvent autoPingReqEvent;
        private long lastCommunationInMilisecond;

        private Thread threadReceiveThread;
        private Thread threadStartThread;
        private Thread threadProssessEventThread;
        private Thread threadProssessPacketsThread;

        public event PublishReceivedEventHandler PublishReceivedChanged;
        public event PublishedEventHandler PublishedChanged;
        public event SubscribedEventHandler SubscribedChanged;
        public event UnsubscribedEventHandler UnsubscribedChanged;
        public event ConnectedEventHandler ConnectedChanged;

        private readonly MqttStream stream;

        private readonly Queue packetQueue;
        private readonly Queue eventQueue;

        private bool isConnectionClosed;

        public bool IsConnected { get; private set; }

        public string ClientId => this.ConnectionSetting.ClientId;

        public MqttClientSetting ClientSetting { get; }
        public MqttConnectionSetting ConnectionSetting { get; private set; }

        public Mqtt(MqttClientSetting setting) {
            this.ClientSetting = setting;

            this.waitSendReceiveEvent = new AutoResetEvent(false);
            this.autoPingReqEvent = new AutoResetEvent(false);

            this.waitForPushPacketEvent = new AutoResetEvent(false);
            this.packetQueue = new Queue();

            this.waitForPushEventEvent = new AutoResetEvent(false);
            this.eventQueue = new Queue();

            this.stream = new MqttStream(this.ClientSetting.BrokerName, this.ClientSetting.BrokerPort, this.ClientSetting.CaCertificate, this.ClientSetting.ClientCertificate, this.ClientSetting.SslProtocol);
        }

        public ConnectReturnCode Connect(MqttConnectionSetting setting) {
            this.ConnectionSetting = setting;

            var connect = new MqttPacket(PacketType.Connect) {
                ClientId = this.ConnectionSetting.ClientId,
                LastWillRetain = this.ConnectionSetting.LastWillRetain,
                CleanSession = this.ConnectionSetting.CleanSession,
                LastWillQosLevel = this.ConnectionSetting.LastWillQos,
                LastWillTopic = this.ConnectionSetting.LastWillTopic,
                Data = this.ConnectionSetting.LastWillMessage != null ? System.Text.UTF8Encoding.UTF8.GetBytes(this.ConnectionSetting.LastWillMessage) : null,
                Username = this.ConnectionSetting.UserName,
                Password = this.ConnectionSetting.Password,
                KeepAliveTimeout = this.ConnectionSetting.KeepAliveTimeout
            };

            try {
                this.stream.Connect();
            }
            catch {
                throw new Exception("Connecting failed");
            }

            this.lastCommunationInMilisecond = 0;
            this.isRunning = true;
            this.isConnectionClosed = false;

            this.threadReceiveThread = new Thread(this.ReceiveThread);
            this.threadReceiveThread.Start();

            var connack = this.SendReceive(connect, CONNECTION_TIMEOUT_DEFAULT);

            if (connack != null && connack.ReturnCode == ConnectReturnCode.ConnectionAccepted) {
                this.keepAliveTimeoutInMilisecond = this.ConnectionSetting.KeepAliveTimeout * 1000;

                if (this.keepAliveTimeoutInMilisecond != 0) {

                    this.threadStartThread = new Thread(this.StartThread);
                    this.threadStartThread.Start();
                }

                this.threadProssessEventThread = new Thread(this.ProcessEventsThread);
                this.threadProssessEventThread.Start();

                this.threadProssessPacketsThread = new Thread(this.ProcessPacketsThread);
                this.threadProssessPacketsThread.Start();

                this.IsConnected = true;

                this.OnConnectedChanged(); // Raise event connected

            }

            if (connack != null)
                return connack.ReturnCode;
            else
                return ConnectReturnCode.Unknown;
        }

        public void Disconnect() {
            var disconnect = new MqttPacket(PacketType.Disconnect);
            this.Send(disconnect);

            this.CloseConnection();
        }


        private void Close() {
            this.isRunning = false;

            if (this.waitForPushEventEvent != null)
                this.waitForPushEventEvent.Set();

            if (this.waitForPushPacketEvent != null)
                this.waitForPushPacketEvent.Set();


            this.autoPingReqEvent.Set();



            this.packetQueue.Clear();
            this.eventQueue.Clear();

            this.stream.Close();

            this.IsConnected = false;
        }


        private MqttPacket PingReq(int timeout) {
            var pingreq = new MqttPacket(PacketType.PingReq);

            MqttPacket pingresp = null;
            try {
                pingresp = this.SendReceive(pingreq, timeout);
            }
            catch {

            }


            if (pingresp == null) {
                this.CloseConnection();

            }
            return pingresp;
        }


        public void Subscribe(string[] topics, QoSLevel[] qosLevels, ushort packetId) {
            if (packetId == 0) {
                throw new ArgumentException(nameof(packetId));
            }

            var subscribe =

                new MqttPacket(PacketType.Subscribe) {
                    PacketId = packetId,
                    Topics = topics,
                    QosLevels = qosLevels
                };

            this.PushPacketToQueue(subscribe, PacketDirection.ToServer);
        }

        public void Unsubscribe(string[] topics, ushort packetId) {
            if (packetId == 0) {
                throw new ArgumentException(nameof(packetId));
            }

            var unsubscribe =
                new MqttPacket(PacketType.Unsubscribe) {
                    Topics = topics,
                    PacketId = packetId
                };

            this.PushPacketToQueue(unsubscribe, PacketDirection.ToServer);
        }


        public void Publish(string topic, byte[] data, QoSLevel qosLevel, bool retain, ushort packetId) {
            if (packetId == 0) {
                throw new ArgumentException(nameof(packetId));
            }

            var publish =
                    new MqttPacket(PacketType.Publish) {
                        LastWillTopic = topic,
                        Data = data,
                        IsDuplicated = false,
                        LastWillQosLevel = qosLevel,
                        LastWillRetain = retain,
                        PacketId = packetId
                    };


            var enqueue = this.PushPacketToQueue(publish, PacketDirection.ToServer);

            if (!enqueue)
                throw new Exception("Packet full.");
        }


        private void PushEventToQueue(MqttPacket packet) {
            lock (this.eventQueue) {
                this.eventQueue.Enqueue(packet);
            }

            this.waitForPushEventEvent.Set();
        }


        private void CloseConnection() {
            if (!this.isConnectionClosed) {
                this.isConnectionClosed = true;
                this.waitForPushEventEvent.Set();

            }
        }

        private void OnTopicPublishReceived(MqttPacket packet) => this.PublishReceivedChanged?.Invoke(this, packet);

        private void OnTopicPublished(uint packetId, bool isPublished) => this.PublishedChanged?.Invoke(this, packetId, isPublished);

        private void OnTopicSubscribed(MqttPacket packet) => this.SubscribedChanged?.Invoke(this, packet);

        private void OnTopicUnsubscribed(uint packetId) => this.UnsubscribedChanged?.Invoke(this, packetId);
        private void OnConnectedChanged() => this.ConnectedChanged?.Invoke(this);

        private void Send(byte[] packetBytes) {
            try {
                this.stream.Send(packetBytes);

                this.lastCommunationInMilisecond = ToMillisecond(DateTime.Now.Ticks);

            }
            catch {

                throw new Exception("Sending failed.");
            }
        }

        private void Send(MqttPacket packet) => this.Send(packet.CreatePacket());

        private MqttPacket SendReceive(byte[] packetBytes, int timeout) {

            this.waitSendReceiveEvent.Reset();

            try {

                this.stream.Send(packetBytes);

                this.lastCommunationInMilisecond = ToMillisecond(DateTime.Now.Ticks);
            }
            catch {

                throw new Exception();
            }

            if (this.waitSendReceiveEvent.WaitOne(timeout, false)) {
                if (this.isConnectAckReceivedSuccess)
                    return this.connectAckReceived;
                else
                    return null;
            }
            else {

                return null;
            }
        }

        private MqttPacket SendReceive(MqttPacket packet, int timeout) => this.SendReceive(packet.CreatePacket(), timeout);
        private bool PushPacketToQueue(MqttPacket packet, PacketDirection dir) {
            var enqueue = packet != null;

            if ((packet.Type == PacketType.Publish) &&
                (packet.LastWillQosLevel == QoSLevel.ExactlyOnce)) {
                lock (this.packetQueue) {
                    foreach (var item in this.packetQueue) {
                        var q = (MqttPacket)item;
                        if ((q.Type == PacketType.Publish) &&
                            (q.PacketId == packet.PacketId) &&
                            q.Direction == PacketDirection.ToClient) {

                            q.State = PacketState.WaitToPublish;
                            q.Direction = PacketDirection.ToClient;

                            enqueue = false;

                            break;
                        }
                    }
                }
            }

            if (enqueue) {
                packet.State = PacketState.WaitToPublish;
                packet.Direction = dir;
                packet.RetryCount = 0;


                lock (this.packetQueue) {
                    enqueue = (this.packetQueue.Count < int.MaxValue);

                    if (enqueue) {
                        this.packetQueue.Enqueue(packet);
                    }
                }
            }

            this.waitForPushPacketEvent.Set();

            return enqueue;
        }

        private void ReceiveThread() {
            var controlHeaderByte = new byte[1];
            PacketType packetType;

            while (this.isRunning) {
                try {
                    var readBytes = this.stream.Receive(controlHeaderByte);

                    if (readBytes > 0) {
                        packetType = (PacketType)((controlHeaderByte[0] & PACKET_TYPE_MASK) >> PACKET_TYPE_OFFSET);

                        switch (packetType) {
                            case PacketType.ConnAck:
                                this.connectAckReceived = this.DecodePacketTypeConnectAck(controlHeaderByte[0]);

                                this.waitSendReceiveEvent.Set();
                                break;

                            case PacketType.PingResp:
                                this.connectAckReceived = this.DecodePacketTypePingResponse(controlHeaderByte[0]);

                                this.waitSendReceiveEvent.Set();
                                break;

                            case PacketType.Suback:
                                var suback = this.DecodePacketTypeSubscribeAck(controlHeaderByte[0]);

                                this.PushPacketToQueue(suback, PacketDirection.ToClient);

                                break;

                            case PacketType.Publish:
                                var publish = this.DecodePacketTypePublish(controlHeaderByte[0]);

                                this.PushPacketToQueue(publish, PacketDirection.ToClient);

                                break;

                            case PacketType.PubAck:
                                var puback = this.DecodePacketTypePublishAck(controlHeaderByte[0]);

                                this.PushPacketToQueue(puback, PacketDirection.ToClient);

                                break;



                            default:
                                // TODO
                                throw new Exception("Unknown packet type.");
                        }

                        this.isConnectAckReceivedSuccess = true;
                    }
                    else {
                        this.CloseConnection();
                    }
                }
                catch {

                    this.isConnectAckReceivedSuccess = false;

                    var close = false;

                    if (close) {
                        this.CloseConnection();
                    }
                }
            }
        }

        private void StartThread() {
            var timeoutInMillisecond = this.keepAliveTimeoutInMilisecond;

            while (this.isRunning) {

                this.autoPingReqEvent.WaitOne(timeoutInMillisecond, false);

                if (this.isRunning) {
                    var d = ToMillisecond(DateTime.Now.Ticks) - this.lastCommunationInMilisecond;

                    if (d > this.keepAliveTimeoutInMilisecond) {

                        Thread.Sleep(5000); // make a delay 1 seconds
                        this.PingReq(PING_TIMEOUT_DEFAULT);
                        timeoutInMillisecond = this.keepAliveTimeoutInMilisecond;

                    }
                    else {
                        timeoutInMillisecond = (int)(this.keepAliveTimeoutInMilisecond - d);
                    }
                }
            }
        }

        private void ProcessEventsThread() {
            while (this.isRunning) {

                if ((this.eventQueue.Count == 0) && !this.isConnectionClosed)
                    this.waitForPushEventEvent.WaitOne();


                if (this.isRunning) {
                    MqttPacket packetFromQueue = null;

                    lock (this.eventQueue) {
                        if (this.eventQueue.Count > 0)
                            packetFromQueue = (MqttPacket)this.eventQueue.Dequeue();
                    }

                    if (packetFromQueue != null) {
                        switch (packetFromQueue.Type) {
                            case PacketType.Suback:
                                this.OnTopicSubscribed((MqttPacket)packetFromQueue);
                                break;

                            case PacketType.Publish:
                                if (packetFromQueue.IsPublished == true) {
                                    this.OnTopicPublished(packetFromQueue.PacketId, false);
                                }
                                else {
                                    this.OnTopicPublishReceived((MqttPacket)packetFromQueue);
                                }

                                break;

                            case PacketType.PubAck:
                                this.OnTopicPublished(packetFromQueue.PacketId, true);
                                break;

                            case PacketType.Pubrel:
                                this.OnTopicPublishReceived((MqttPacket)packetFromQueue);
                                break;


                            case PacketType.PubComp:
                                this.OnTopicPublished(packetFromQueue.PacketId, true);
                                break;

                            case PacketType.Unsuback:

                                this.OnTopicUnsubscribed(packetFromQueue.PacketId);
                                break;

                            default:
                                throw new Exception("Unknown packet type.");
                        }

                    }

                    if ((this.eventQueue.Count == 0) && this.isConnectionClosed) {
                        this.Close();
                        this.OnConnectedChanged();
                    }
                }
            }
        }

        private void ProcessPacketsThread() {
            MqttPacket packetFromQueue = null;

            try {
                while (this.isRunning) {
                    this.waitForPushPacketEvent.WaitOne(CONNECTION_TIMEOUT_DEFAULT, false);
                    if (this.isRunning) {
                        lock (this.packetQueue) {

                            var count = this.packetQueue.Count;
                            while (count > 0) {
                                count--;
                                if (!this.isRunning)
                                    break;
                                packetFromQueue = (MqttPacket)this.packetQueue.Dequeue();

                                switch (packetFromQueue.State) {
                                    case PacketState.WaitToPublish:

                                        if (packetFromQueue.LastWillQosLevel == QoSLevel.MostOnce) {
                                            if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                                this.Send(packetFromQueue);
                                            }
                                            else if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                                packetFromQueue.IsPublished = false;
                                                this.PushEventToQueue(packetFromQueue);
                                            }
                                        }

                                        if (packetFromQueue.LastWillQosLevel == QoSLevel.LeastOnce) {
                                            if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                                packetFromQueue.RetryCount++;
                                                if (packetFromQueue.Type == PacketType.Publish) {

                                                    packetFromQueue.State = PacketState.WaitForPublishAck;

                                                    if (packetFromQueue.RetryCount > 1)
                                                        packetFromQueue.IsDuplicated = true;
                                                }
                                                else if (packetFromQueue.Type == PacketType.Subscribe)
                                                    packetFromQueue.State = PacketState.WaitForSubscribeAck;
                                                else if (packetFromQueue.Type == PacketType.Unsubscribe)
                                                    packetFromQueue.State = PacketState.WaitForUnsubscribeAck;

                                                this.Send(packetFromQueue);

                                                this.packetQueue.Enqueue(packetFromQueue);
                                            }
                                            else if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                                var puback = new MqttPacket(PacketType.PubAck) {
                                                    PacketId = packetFromQueue.PacketId
                                                };

                                                this.Send(puback);

                                                packetFromQueue.IsPublished = false;
                                                this.PushEventToQueue(packetFromQueue);

                                            }
                                        }

                                        if (packetFromQueue.LastWillQosLevel == QoSLevel.LeastOnce) {
                                            // TODO
                                        }

                                        break;

                                    case PacketState.SendSubscribe:
                                    case PacketState.SendUnsubscribe:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            packetFromQueue.RetryCount++;
                                            if (packetFromQueue.Type == PacketType.Publish) {

                                                packetFromQueue.State = PacketState.WaitForPublishAck;

                                                if (packetFromQueue.RetryCount > 1)
                                                    packetFromQueue.IsDuplicated = true;
                                            }
                                            else if (packetFromQueue.Type == PacketType.Subscribe)
                                                packetFromQueue.State = PacketState.WaitForSubscribeAck;
                                            else if (packetFromQueue.Type == PacketType.Unsubscribe)
                                                packetFromQueue.State = PacketState.WaitForUnsubscribeAck;

                                            this.Send(packetFromQueue);

                                            this.packetQueue.Enqueue(packetFromQueue);
                                        }
                                        else if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                            var puback = new MqttPacket(PacketType.PubAck) {
                                                PacketId = packetFromQueue.PacketId
                                            };

                                            this.Send(puback);

                                            packetFromQueue.IsPublished = false;
                                            this.PushEventToQueue(packetFromQueue);

                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch {
                if (packetFromQueue != null)
                    this.packetQueue.Enqueue(packetFromQueue);
                this.CloseConnection();
            }
        }

        private MqttPacket DecodePacketTypeSubscribeAck(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.Suback);

            EnsureValidFlag(controlHeaderByte, PACKET_SUBACK_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
            packet.PacketId |= (buffer[index++]);

            return packet;
        }

        private MqttPacket DecodePacketTypePublish(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            byte[] topicToBytes;
            int topicToBytesLength;
            var packet = new MqttPacket(PacketType.Publish);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            var received = this.stream.Receive(buffer);

            topicToBytesLength = ((buffer[index++] << 8) & 0xFF00);
            topicToBytesLength |= buffer[index++];
            topicToBytes = new byte[topicToBytesLength];
            Array.Copy(buffer, index, topicToBytes, 0, topicToBytesLength);
            index += topicToBytesLength;

            packet.LastWillTopic = new string(Encoding.UTF8.GetChars(topicToBytes));

            packet.LastWillQosLevel = (QoSLevel)((controlHeaderByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);

            packet.IsDuplicated = (((controlHeaderByte & DUPLICATE_FLAG_MASK) >> DUPLICATE_FLAG_OFFSET) == 0x01);

            packet.LastWillRetain = (((controlHeaderByte & RETAIN_FLAG_MASK) >> RETAIN_FLAG_OFFSET) == 0x01);

            if ((packet.LastWillQosLevel == QoSLevel.LeastOnce) ||
                (packet.LastWillQosLevel == QoSLevel.ExactlyOnce)) {

                packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
                packet.PacketId |= (buffer[index++]);
            }

            var packetSize = remainSize - index;
            var remaining = packetSize;
            var packetOffset = 0;
            packet.Data = new byte[packetSize];

            Array.Copy(buffer, index, packet.Data, packetOffset, received - index);
            remaining -= (received - index);
            packetOffset += (received - index);

            while (remaining > 0) {
                received = this.stream.Receive(buffer);
                Array.Copy(buffer, 0, packet.Data, packetOffset, received);
                remaining -= received;
                packetOffset += received;
            }

            return packet;
        }

        private MqttPacket DecodePacketTypeConnectAck(byte controlHeaderByte) {
            byte[] buffer;
            var packet = new MqttPacket(PacketType.ConnAck);

            var connectionReturnCodeOffset = 1;

            EnsureValidFlag(controlHeaderByte, PACKET_CONNACK_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.SessionPresent = (buffer[0] & 1) != 0x00; //3.1.connection Present Flag 

            packet.ReturnCode = (ConnectReturnCode)buffer[connectionReturnCodeOffset];

            return packet;
        }

        private MqttPacket DecodePacketTypePingResponse(byte controlHeaderByte) {
            var packet = new MqttPacket(PacketType.PingResp);

            EnsureValidFlag(controlHeaderByte, PACKET_PINGRESP_FLAG_BITS);

            // Clear stream
            RemainSizeFromStream(this.stream);

            return packet;
        }

        private MqttPacket DecodePacketTypePublishAck(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.PubAck);

            EnsureValidFlag(controlHeaderByte, PACKET_PUBACK_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
            packet.PacketId |= (buffer[index++]);

            return packet;
        }

        static long ToMillisecond(long time) => ((time / 10000));

        static void EnsureValidFlag(byte controlHeaderByte, byte flag) {
            if ((controlHeaderByte & PACKET_FLAG_BITS_MASK) != flag) // 3.1.1
                throw new Exception("Invalid packet type.");
        }

    }
}
