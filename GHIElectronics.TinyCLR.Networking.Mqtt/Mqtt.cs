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
        public int KeepAliveTimeout { get; set; } = 60;
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
        const int RETRY_DEFAULT = 3;

        public delegate void PublishReceivedEventHandler(object sender, string topic, byte[] data, bool duplicate, QoSLevel qosLevel, bool retain);
        public delegate void PublishedEventHandler(object sender, uint packetId, bool published);
        public delegate void SubscribedEventHandler(object sender, uint packetId, QoSLevel[] grantedQoSLevels);
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
        private readonly Queue internalPacketQueue;
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
            this.internalPacketQueue = new Queue();

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
            this.internalPacketQueue.Clear();
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
                    QosLevels = qosLevels,
                    LastWillQosLevel = QoSLevel.LeastOnce // Subcribe is always Qos1
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

        private void OnTopicPublishReceived(MqttPacket packet) => this.PublishReceivedChanged?.Invoke(this, packet.LastWillTopic, packet.Data, packet.IsDuplicated, packet.LastWillQosLevel, packet.LastWillRetain);

        private void OnTopicPublished(uint packetId, bool isPublished) => this.PublishedChanged?.Invoke(this, packetId, isPublished);

        private void OnTopicSubscribed(MqttPacket packet) => this.SubscribedChanged?.Invoke(this, packet.PacketId, packet.QosLevels);

        private void OnTopicUnsubscribed(uint packetId) => this.UnsubscribedChanged?.Invoke(this, packetId);
        private void OnConnectedChanged() => this.ConnectedChanged?.Invoke(this);

        private void Send(byte[] packetBytes) {
            try {
                var sent = this.stream.Send(packetBytes);

                if (sent == packetBytes.Length)
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

        private bool PushPacketToInternalQueue(MqttPacket packet) {
            var enqueue = packet != null;

            if (packet.Type == PacketType.Suback ||
                packet.Type == PacketType.PubAck ||
                packet.Type == PacketType.Pubrec ||
                packet.Type == PacketType.Pubrel ||
                packet.Type == PacketType.PubComp ||
                packet.Type == PacketType.Unsuback
                ) {

                if (packet.Type == PacketType.Pubrel) {
                    lock (this.packetQueue) {

                        foreach (var item in this.packetQueue) {
                            var q = (MqttPacket)item;
                            if (q.PacketId == packet.PacketId && q.Direction == PacketDirection.ToClient) {

                                var pubcomp = q;
                                pubcomp.Type = PacketType.PubComp;

                                this.Send(pubcomp);

                                enqueue = false;
                                break;
                            }
                        }

                    }
                }

                else if (packet.Type == PacketType.PubComp) {
                    lock (this.packetQueue) {

                        var found = false;
                        foreach (var item in this.packetQueue) {
                            var q = (MqttPacket)item;
                            if (q.PacketId == packet.PacketId && q.Direction == PacketDirection.ToServer) {

                                found = true;

                                break;
                            }
                        }

                        enqueue = found;

                    }
                }
                else if (packet.Type == PacketType.Pubrec) {
                    lock (this.packetQueue) {

                        var found = false;
                        foreach (var item in this.packetQueue) {
                            var q = (MqttPacket)item;
                            if (q.PacketId == packet.PacketId && q.Direction == PacketDirection.ToServer) {
                                found = true;

                                break;
                            }
                        }
                        enqueue = found;
                    }
                }

                if (enqueue) {
                    lock (this.internalPacketQueue) {
                        this.internalPacketQueue.Enqueue(packet);

                        this.waitForPushPacketEvent.Set();
                    }
                }

            }

            return enqueue;
        }

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

                            q.State = PacketState.QueuedQos2;
                            q.Direction = PacketDirection.ToClient;

                            enqueue = false;

                            break;
                        }
                    }
                }
            }

            if (enqueue) {
                packet.State = PacketState.QueuedQos0;
                packet.Direction = dir;
                packet.RetryCount = 0;

                switch (packet.LastWillQosLevel) {
                    case QoSLevel.MostOnce:
                        packet.State = PacketState.QueuedQos0;
                        break;

                    case QoSLevel.LeastOnce:
                        packet.State = PacketState.QueuedQos1;
                        break;

                    case QoSLevel.ExactlyOnce:
                        packet.State = PacketState.QueuedQos2;
                        break;
                }

                if (packet.Type == PacketType.Subscribe) {
                    packet.State = PacketState.SendSubscribe;
                }
                else if (packet.Type == PacketType.Unsubscribe) {
                    packet.State = PacketState.SendSubscribe;
                }

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

                                this.PushPacketToInternalQueue(suback);

                                break;

                            case PacketType.Publish:
                                var publish = this.DecodePacketTypePublish(controlHeaderByte[0]);

                                this.PushPacketToQueue(publish, PacketDirection.ToClient);

                                break;

                            case PacketType.PubAck:
                                var puback = this.DecodePacketTypePublishAck(controlHeaderByte[0]);

                                this.PushPacketToInternalQueue(puback);

                                break;

                            case PacketType.Pubrec:
                                var pubrec = this.DecodePacketTypePublishRec(controlHeaderByte[0]);

                                this.PushPacketToInternalQueue(pubrec);
                                break;

                            case PacketType.Pubrel:
                                var pubrel = this.DecodePacketTypePublishRel(controlHeaderByte[0]);

                                this.PushPacketToInternalQueue(pubrel);
                                break;

                            case PacketType.PubComp:
                                var pubcom = this.DecodePacketTypePublishComp(controlHeaderByte[0]);

                                this.PushPacketToInternalQueue(pubcom);
                                break;

                            case PacketType.Unsuback:
                                var pubUnsubAck = this.DecodePacketTypeUnsubAck(controlHeaderByte[0]);

                                this.PushPacketToInternalQueue(pubUnsubAck);
                                break;



                            default:

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
                    this.CloseConnection();

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
            var timeout = Timeout.Infinite;
            try {
                while (this.isRunning) {
                    this.waitForPushPacketEvent.WaitOne(timeout, false);
                    if (this.isRunning) {
                        lock (this.packetQueue) {

                            var packetReceivedProcessed = false;
                            var acknowledge = false;
                            MqttPacket packetReceived = null;

                            timeout = int.MaxValue;

                            var count = this.packetQueue.Count;

                            while (count > 0) {
                                count--;

                                acknowledge = false;
                                packetReceived = null;

                                if (!this.isRunning)
                                    break;

                                packetFromQueue = (MqttPacket)this.packetQueue.Dequeue();

                                switch (packetFromQueue.State) {
                                    case PacketState.QueuedQos0:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            this.Send(packetFromQueue);
                                        }

                                        else if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                            packetFromQueue.IsPublished = false;
                                            this.PushEventToQueue(packetFromQueue);
                                        }

                                        break;
                                    case PacketState.QueuedQos1:
                                    case PacketState.SendSubscribe:
                                    case PacketState.SendUnsubscribe:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            packetFromQueue.Timestamp = DateTime.Now.Ticks;
                                            packetFromQueue.RetryCount++;

                                            if (packetFromQueue.Type == PacketType.Publish) {
                                                packetFromQueue.State = PacketState.WaitForPubAck;
                                                if (packetFromQueue.RetryCount > 1)
                                                    packetFromQueue.IsDuplicated = true;
                                            }
                                            else if (packetFromQueue.Type == PacketType.Subscribe)
                                                packetFromQueue.State = PacketState.WaitForSubAck;
                                            else if (packetFromQueue.Type == PacketType.Unsubscribe)
                                                packetFromQueue.State = PacketState.WaitForUnsubAck;

                                            this.Send(packetFromQueue);

                                            timeout = (CONNECTION_TIMEOUT_DEFAULT < timeout) ? CONNECTION_TIMEOUT_DEFAULT : timeout;
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

                                    case PacketState.QueuedQos2:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            packetFromQueue.Timestamp = DateTime.Now.Ticks;
                                            packetFromQueue.RetryCount++;
                                            packetFromQueue.State = PacketState.WaitForPubRec;

                                            if (packetFromQueue.RetryCount > 1)
                                                packetFromQueue.IsDuplicated = true;

                                            this.Send(packetFromQueue);

                                            timeout = (CONNECTION_TIMEOUT_DEFAULT < timeout) ? CONNECTION_TIMEOUT_DEFAULT : timeout;

                                            this.packetQueue.Enqueue(packetFromQueue);
                                        }
                                        else if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                            var pubrec = new MqttPacket(PacketType.Pubrec) {
                                                PacketId = packetFromQueue.PacketId
                                            };

                                            packetFromQueue.State = PacketState.WaitForPubRel;

                                            this.Send(pubrec);

                                            this.packetQueue.Enqueue(packetFromQueue);
                                        }
                                        break;

                                    case PacketState.WaitForPubAck:
                                    case PacketState.WaitForSubAck:
                                    case PacketState.WaitForUnsubAck:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            acknowledge = false;
                                            lock (this.internalPacketQueue) {
                                                if (this.internalPacketQueue.Count > 0)
                                                    packetReceived = (MqttPacket)this.internalPacketQueue.Peek();
                                            }

                                            if (packetReceived != null) {
                                                if (((packetReceived.Type == PacketType.PubAck) && (packetFromQueue.Type == PacketType.Publish) && (packetReceived.PacketId == packetFromQueue.PacketId)) ||
                                                    ((packetReceived.Type == PacketType.Suback) && (packetFromQueue.Type == PacketType.Subscribe) && (packetReceived.PacketId == packetFromQueue.PacketId)) ||
                                                    ((packetReceived.Type == PacketType.Unsuback) && (packetFromQueue.Type == PacketType.Unsubscribe) && (packetReceived.PacketId == packetFromQueue.PacketId))) {
                                                    lock (this.internalPacketQueue) {
                                                        this.internalPacketQueue.Dequeue();
                                                        acknowledge = true;
                                                        packetReceivedProcessed = true;
                                                    }

                                                    if (packetReceived.Type == PacketType.PubAck) {
                                                        packetReceived.IsPublished = true;
                                                    }
                                                    else {
                                                        packetReceived.IsPublished = false;
                                                    }

                                                    this.PushEventToQueue(packetReceived);
                                                }
                                            }

                                            if (!acknowledge) {
                                                var delta = DateTime.Now.Ticks - packetFromQueue.Timestamp;

                                                if (delta >= CONNECTION_TIMEOUT_DEFAULT) {

                                                    if (packetFromQueue.RetryCount < RETRY_DEFAULT) {
                                                        packetFromQueue.State = PacketState.QueuedQos1;

                                                        this.packetQueue.Enqueue(packetFromQueue);

                                                        timeout = 0;
                                                    }
                                                    else {
                                                        if (packetFromQueue.Type == PacketType.Publish) {
                                                            packetFromQueue.IsPublished = false;

                                                            this.PushEventToQueue(packetFromQueue);
                                                        }
                                                    }
                                                }
                                                else {
                                                    this.packetQueue.Enqueue(packetFromQueue);

                                                    var msgTimeout = (CONNECTION_TIMEOUT_DEFAULT - delta);
                                                    timeout = (msgTimeout < timeout) ? (int)msgTimeout : timeout;
                                                }
                                            }
                                        }
                                        break;

                                    case PacketState.WaitForPubRec:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            acknowledge = false;
                                            lock (this.internalPacketQueue) {
                                                if (this.internalPacketQueue.Count > 0)
                                                    packetReceived = (MqttPacket)this.internalPacketQueue.Peek();
                                            }

                                            if ((packetReceived != null) && (packetReceived.Type == PacketType.Pubrec)) {
                                                if (packetReceived.PacketId == packetFromQueue.PacketId) {
                                                    lock (this.internalPacketQueue) {
                                                        this.internalPacketQueue.Dequeue();
                                                        acknowledge = true;
                                                        packetReceivedProcessed = true;
                                                    }

                                                    var pubrel = new MqttPacket(PacketType.Pubrel) {
                                                        PacketId = packetFromQueue.PacketId
                                                    };

                                                    packetFromQueue.State = PacketState.WaitForPubComp;
                                                    packetFromQueue.Timestamp = DateTime.Now.Ticks;
                                                    packetFromQueue.RetryCount = 1;

                                                    this.Send(pubrel);

                                                    timeout = (CONNECTION_TIMEOUT_DEFAULT < timeout) ? CONNECTION_TIMEOUT_DEFAULT : timeout;

                                                    this.packetQueue.Enqueue(packetFromQueue);
                                                }
                                            }

                                            if (!acknowledge) {
                                                var delta = DateTime.Now.Ticks - packetFromQueue.Timestamp;

                                                if (delta >= CONNECTION_TIMEOUT_DEFAULT) {
                                                    if (packetFromQueue.RetryCount < RETRY_DEFAULT) {
                                                        packetFromQueue.State = PacketState.QueuedQos2;

                                                        this.packetQueue.Enqueue(packetFromQueue);

                                                        timeout = 0;
                                                    }
                                                    else {
                                                        packetFromQueue.IsPublished = false;
                                                        this.PushEventToQueue(packetFromQueue);
                                                    }
                                                }
                                                else {
                                                    this.packetQueue.Enqueue(packetFromQueue);
                                                    var msgTimeout = (CONNECTION_TIMEOUT_DEFAULT - delta);
                                                    timeout = (msgTimeout < timeout) ? (int)msgTimeout : timeout;
                                                }
                                            }
                                        }
                                        break;

                                    case PacketState.WaitForPubRel:
                                        if (packetFromQueue.Direction == PacketDirection.ToClient) {
                                            lock (this.internalPacketQueue) {
                                                if (this.internalPacketQueue.Count > 0)
                                                    packetReceived = (MqttPacket)this.internalPacketQueue.Peek();
                                            }
                                            if ((packetReceived != null) && (packetReceived.Type == PacketType.Pubrel)) {
                                                if (packetReceived.PacketId == packetFromQueue.PacketId) {
                                                    lock (this.internalPacketQueue) {
                                                        this.internalPacketQueue.Dequeue();
                                                        packetReceivedProcessed = true;
                                                    }

                                                    var pubcomp = new MqttPacket(PacketType.PubComp) {
                                                        PacketId = packetFromQueue.PacketId
                                                    };

                                                    this.Send(pubcomp);

                                                    this.PushEventToQueue(packetFromQueue);

                                                }
                                                else {
                                                    this.packetQueue.Enqueue(packetFromQueue);
                                                }
                                            }
                                            else {
                                                this.packetQueue.Enqueue(packetFromQueue);
                                            }
                                        }
                                        break;

                                    case PacketState.WaitForPubComp:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            acknowledge = false;
                                            lock (this.internalPacketQueue) {
                                                if (this.internalPacketQueue.Count > 0)
                                                    packetReceived = (MqttPacket)this.internalPacketQueue.Peek();
                                            }

                                            if ((packetReceived != null) && (packetReceived.Type == PacketType.PubComp)) {
                                                if (packetReceived.PacketId == packetFromQueue.PacketId) {
                                                    lock (this.internalPacketQueue) {
                                                        this.internalPacketQueue.Dequeue();
                                                        acknowledge = true;
                                                        packetReceivedProcessed = true;

                                                    }

                                                    packetReceived.IsPublished = true;
                                                    this.PushEventToQueue(packetReceived);

                                                }
                                            }
                                            else if ((packetReceived != null) && (packetReceived.Type == PacketType.Pubrec)) {
                                                if (packetReceived.PacketId == packetFromQueue.PacketId) {
                                                    lock (this.internalPacketQueue) {
                                                        this.internalPacketQueue.Dequeue();
                                                        acknowledge = true;
                                                        packetReceivedProcessed = true;
                                                        this.packetQueue.Enqueue(packetFromQueue);
                                                    }
                                                }
                                            }

                                            if (!acknowledge) {
                                                var delta = DateTime.Now.Ticks - packetFromQueue.Timestamp;
                                                if (delta >= CONNECTION_TIMEOUT_DEFAULT) {
                                                    if (packetFromQueue.RetryCount < RETRY_DEFAULT) {
                                                        packetFromQueue.State = PacketState.SendPubRel;
                                                        this.packetQueue.Enqueue(packetFromQueue);

                                                        timeout = 0;
                                                    }
                                                    else {
                                                        packetFromQueue.IsPublished = false;
                                                        this.PushEventToQueue(packetFromQueue);
                                                    }
                                                }
                                                else {
                                                    this.packetQueue.Enqueue(packetFromQueue);

                                                    var msgTimeout = (CONNECTION_TIMEOUT_DEFAULT - delta);
                                                    timeout = (msgTimeout < timeout) ? (int)msgTimeout : timeout;
                                                }
                                            }
                                        }
                                        break;
                                    case PacketState.SendPubRel:
                                        if (packetFromQueue.Direction == PacketDirection.ToServer) {
                                            var pubrel = new MqttPacket(PacketType.Pubrel) {
                                                PacketId = packetFromQueue.PacketId
                                            };

                                            packetFromQueue.State = PacketState.WaitForPubComp;
                                            packetFromQueue.Timestamp = DateTime.Now.Ticks;
                                            packetFromQueue.RetryCount++;

                                            this.Send(pubrel);

                                            timeout = (CONNECTION_TIMEOUT_DEFAULT < timeout) ? CONNECTION_TIMEOUT_DEFAULT : timeout;

                                            this.packetQueue.Enqueue(packetFromQueue);
                                        }
                                        break;

                                    default:
                                        break;
                                }

                                if (timeout == int.MaxValue)
                                    timeout = Timeout.Infinite;

                                if ((packetReceived != null) && !packetReceivedProcessed) {
                                    if (this.internalPacketQueue.Count > 0)
                                        this.internalPacketQueue.Dequeue();
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

            packet.QosLevels = new QoSLevel[remainSize - 2];

            var qosIdx = 0;
            do {
                packet.QosLevels[qosIdx++] = (QoSLevel)buffer[index++];
            } while (index < remainSize);

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

        private MqttPacket DecodePacketTypePublishRec(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.Pubrec);

            EnsureValidFlag(controlHeaderByte, PACKET_PUBREC_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
            packet.PacketId |= (buffer[index++]);

            return packet;
        }

        private MqttPacket DecodePacketTypePublishRel(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.Pubrel);

            EnsureValidFlag(controlHeaderByte, PACKET_PUBREL_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
            packet.PacketId |= (buffer[index++]);

            return packet;
        }

        private MqttPacket DecodePacketTypePublishComp(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.PubComp);

            EnsureValidFlag(controlHeaderByte, PACKET_PUBCOMP_FLAG_BITS);

            var remainSize = RemainSizeFromStream(this.stream);
            buffer = new byte[remainSize];

            this.stream.Receive(buffer);

            packet.PacketId = (ushort)((buffer[index++] << 8) & 0xFF00);
            packet.PacketId |= (buffer[index++]);

            return packet;
        }

        private MqttPacket DecodePacketTypeUnsubAck(byte controlHeaderByte) {
            byte[] buffer;
            var index = 0;
            var packet = new MqttPacket(PacketType.Unsuback);

            EnsureValidFlag(controlHeaderByte, PACKET_UNSUBACK_FLAG_BITS);

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
