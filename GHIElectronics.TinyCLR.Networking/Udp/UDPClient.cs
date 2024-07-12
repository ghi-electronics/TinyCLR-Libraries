// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
//using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Threading;

namespace System.Net.Sockets {
    // The System.Net.Sockets.UdpClient class provides access to UDP services at a higher abstraction
    // level than the System.Net.Sockets.Socket class. System.Net.Sockets.UdpClient is used to
    // connect to a remote host and to receive connections from a remote client.
    public partial class UdpClient : IDisposable {
        private const int MaxUDPSize = 0x10000;

        private Socket _clientSocket = null; // initialized by helper called from ctor
        private bool _active;
        private readonly byte[] _buffer = new byte[MaxUDPSize];
        private AddressFamily _family = AddressFamily.InterNetwork;

        // Initializes a new instance of the System.Net.Sockets.UdpClientclass.
        public UdpClient() : this(AddressFamily.InterNetwork) {
        }

        // Initializes a new instance of the System.Net.Sockets.UdpClientclass.
        public UdpClient(AddressFamily family) {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6) {
                //throw new ArgumentException(SR.Format(SR.net_protocol_invalid_family, "UDP"), nameof(family));
                throw new ArgumentException("net_protocol_invalid_family");
            }

            this._family = family;

            this.CreateClientSocket();
        }

        // Creates a new instance of the UdpClient class that communicates on the
        // specified port number.
        //
        // NOTE: We should obsolete this. This also breaks IPv6-only scenarios.
        // But fixing it has many complications that we have decided not
        // to fix it and instead obsolete it later.
        public UdpClient(int port) : this(port, AddressFamily.InterNetwork) {
        }

        // Creates a new instance of the UdpClient class that communicates on the
        // specified port number.
        public UdpClient(int port, AddressFamily family) {
            //if (!TcpValidationHelpers.ValidatePortNumber(port)) {
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6) {
                //throw new ArgumentException(SR.Format(SR.net_protocol_invalid_family, "UDP"), nameof(family));
                throw new ArgumentException("net_protocol_invalid_family");
            }

            IPEndPoint localEP = null;
            this._family = family;

            if (this._family == AddressFamily.InterNetwork) {
                localEP = new IPEndPoint(IPAddress.Any, port);
            }
            else {
                //localEP = new IPEndPoint(IPAddress.IPv6Any, port);

                throw new NotSupportedException();
            }

            this.CreateClientSocket();

            this._clientSocket.Bind(localEP);
        }

        // Creates a new instance of the UdpClient class that communicates on the
        // specified end point.
        public UdpClient(IPEndPoint localEP) {
            //ArgumentNullException.ThrowIfNull(localEP);

            // IPv6 Changes: Set the AddressFamily of this object before
            //               creating the client socket.
            this._family = AddressFamily.InterNetwork;

            this.CreateClientSocket();

            this._clientSocket.Bind(localEP);
        }

        // Used by the class to indicate that a connection to a remote host has been made.
        protected bool Active {
            get => this._active;
            set => this._active = value;
        }

        public int Available => this._clientSocket.Available;

       

        public Socket Client {
            get => this._clientSocket;
            set => this._clientSocket = value;
        }

        //public short Ttl {
        //    get => this._clientSocket.Ttl;
        //    set => this._clientSocket.Ttl = value;
        //}

        //public bool DontFragment {
        //    get => this._clientSocket.DontFragment;
        //    set => this._clientSocket.DontFragment = value;
        //}

        //public bool MulticastLoopback {
        //    get => this._clientSocket.MulticastLoopback;
        //    set => this._clientSocket.MulticastLoopback = value;
        //}

        //public bool EnableBroadcast {
        //    get => this._clientSocket.EnableBroadcast;
        //    set => this._clientSocket.EnableBroadcast = value;
        //}

        //public bool ExclusiveAddressUse {
        //    get => this._clientSocket.ExclusiveAddressUse;
        //    set => this._clientSocket.ExclusiveAddressUse = value;
        //}

        //[SupportedOSPlatform("windows")]
        //public void AllowNatTraversal(bool allowed) {
        //    this._clientSocket.SetIPProtectionLevel(allowed ? IPProtectionLevel.Unrestricted : IPProtectionLevel.EdgeRestricted);
        //}

        private bool _disposed;

        private bool IsAddressFamilyCompatible(AddressFamily family) {
            // Check if the provided address family is compatible with the socket address family
            if (family == this._family) {
                return true;
            }

            //if (family == AddressFamily.InterNetwork) {
            //    return this._family == AddressFamily.InterNetworkV6 && this._clientSocket.DualMode;
            //}

            return false;
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this);

                // The only resource we need to free is the network stream, since this
                // is based on the client socket, closing the stream will cause us
                // to flush the data to the network, close the stream and (in the
                // NetoworkStream code) close the socket as well.
                if (this._disposed) {
                    return;
                }

                var chkClientSocket = this._clientSocket;
                if (chkClientSocket != null) {
                    // If the NetworkStream wasn't retrieved, the Socket might
                    // still be there and needs to be closed to release the effect
                    // of the Bind() call and free the bound IPEndPoint.
                    //chkClientSocket.InternalShutdown(SocketShutdown.Both);
                    //chkClientSocket.Dispose();

                    chkClientSocket.Close();
                    this._clientSocket = null;
                }

                this._disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private bool _isBroadcast;
        private void CheckForBroadcast(IPAddress ipAddress) {
            // Here we check to see if the user is trying to use a Broadcast IP address
            // we only detect IPAddress.Broadcast (which is not the only Broadcast address)
            // and in that case we set SocketOptionName.Broadcast on the socket to allow its use.
            // if the user really wants complete control over Broadcast addresses they need to
            // inherit from UdpClient and gain control over the Socket and do whatever is appropriate.
            if (this._clientSocket != null && !this._isBroadcast && IsBroadcast(ipAddress)) {
                // We need to set the Broadcast socket option.
                // Note that once we set the option on the Socket we never reset it.
                this._isBroadcast = true;
                this._clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
        }

        private static bool IsBroadcast(IPAddress address) {
            if (address.AddressFamily == AddressFamily.InterNetworkV6) {
                // No such thing as a broadcast address for IPv6.
                return false;
            }
            else {
                return address.Equals(IPAddress.Broadcast);
            }
        }

        public int BeginSend(byte[] datagram, int bytes) =>
            this.BeginSend(datagram, bytes, null);

        public int BeginSend(byte[] datagram, int bytes, string hostname, int port) =>
            this.BeginSend(datagram, bytes, this.GetEndpoint(hostname, port));

        public int BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint) {
            this.ValidateDatagram(datagram, bytes, endPoint);

            if (endPoint is null) {
                //return this._clientSocket.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
                return this._clientSocket.Send(datagram, 0, bytes, SocketFlags.None);
            }
            else {
                this.CheckForBroadcast(endPoint.Address);
                return this._clientSocket.SendTo(datagram, 0, bytes, SocketFlags.None, endPoint);
            }
        }

        public int EndSend(IAsyncResult asyncResult) {
            //this.ThrowIfDisposed();

            //return this._active ?
            //    this._clientSocket.EndSend(asyncResult) :
            //    this._clientSocket.EndSendTo(asyncResult);

            throw new NotImplementedException(); ;
        }

        private void ValidateDatagram(byte[] datagram, int bytes, IPEndPoint endPoint) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(datagram);

            //ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            //ArgumentOutOfRangeException.ThrowIfGreaterThan(bytes, datagram.Length);

            //if (this._active && endPoint != null) {
            //    // Do not allow sending packets to arbitrary host when connected.
            //    throw new InvalidOperationException(SR.net_udpconnected);
            //}

            if (datagram == null) throw new ArgumentNullException("datagram");
            if (bytes < 0 || bytes > datagram.Length) throw new ArgumentOutOfRangeException("bytes");

            if (this._active && endPoint != null) {
                // Do not allow sending packets to arbitrary host when connected.
                throw new InvalidOperationException("net_udpconnected");
            }
        }

        private IPEndPoint GetEndpoint(string hostname, int port) {
            if (this._active && ((hostname != null) || (port != 0))) {
                // Do not allow sending packets to arbitrary host when connected.
                throw new InvalidOperationException("net_udpconnected");
            }

            IPEndPoint ipEndPoint = null;
            if (hostname != null && port != 0) {
                var iPHostEntry = Dns.GetHostEntry(hostname);

                var addresses = iPHostEntry.addressList;

                var i = 0;
                for (; i < addresses.Length && !this.IsAddressFamilyCompatible(addresses[i].AddressFamily); i++) {
                }

                if (addresses.Length == 0 || i == addresses.Length) {
                    throw new ArgumentException("net_invalidAddressList " +  nameof(hostname));
                }

                this.CheckForBroadcast(addresses[i]);
                ipEndPoint = new IPEndPoint(addresses[i], port);
            }

            return ipEndPoint;
        }

        public int BeginReceive(int port) {
            this.ThrowIfDisposed();

            // Due to the nature of the ReceiveFrom() call and the ref parameter convention,
            // we need to cast an IPEndPoint to its base class EndPoint and cast it back down
            // to IPEndPoint.
            //EndPoint tempRemoteEP = this._family == AddressFamily.InterNetwork ?
            //    IPEndPointStatics.Any :
            //    IPEndPointStatics.IPv6Any;

            //EndPoint tempRemoteEP = IPAddress.Any;

            EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, port);
            return this._clientSocket.ReceiveFrom(this._buffer, 0, MaxUDPSize, SocketFlags.None, ref tempRemoteEP);
        }

        public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP) {
            this.ThrowIfDisposed();

            //EndPoint tempRemoteEP = this._family == AddressFamily.InterNetwork ?
            //    IPEndPointStatics.Any :
            //    IPEndPointStatics.IPv6Any;

            //int received = this._clientSocket.EndReceiveFrom(asyncResult, ref tempRemoteEP);
            //remoteEP = (IPEndPoint)tempRemoteEP;

            //// Because we don't return the actual length, we need to ensure the returned buffer
            //// has the appropriate length.
            //if (received < MaxUDPSize) {
            //    var newBuffer = new byte[received];
            //    Buffer.BlockCopy(this._buffer, 0, newBuffer, 0, received);
            //    return newBuffer;
            //}

            return this._buffer;
        }

        // Joins a multicast address group.
        public void JoinMulticastGroup(IPAddress multicastAddr) {
            this.ThrowIfDisposed();
            //ArgumentNullException.ThrowIfNull(multicastAddr);
            if (multicastAddr.AddressFamily != this._family) {
                // For IPv6, we need to create the correct MulticastOption and must also check for address family compatibility.
                // Note: we cannot reliably use IPv4 multicast over IPv6 in DualMode, as such we keep the compatibility explicit between IP stack versions
                throw new ArgumentException("SR.Format(SR.net_protocol_invalid_multicast_family", nameof(multicastAddr));
            }

            if (this._family == AddressFamily.InterNetwork) {
                //var multicastAddr_group = multicastAddr.GetAddressBytes();
                //var localAddress = IPAddress.Any.GetAddressBytes();

                //var multicastOptionAddress = new byte[multicastAddr_group.Length + localAddress.Length];

                //Array.Copy(multicastAddr_group, 0, multicastOptionAddress, 0, multicastAddr_group.Length);

                //Array.Copy(localAddress, 0, multicastOptionAddress, multicastAddr_group.Length, localAddress.Length);


                this._clientSocket.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.AddMembership,
                    new MulticastOption(multicastAddr).ToBytes());
            }
            else {
                //this._clientSocket.SetSocketOption(
                //    SocketOptionLevel.IPv6,
                //    SocketOptionName.AddMembership,
                //    new IPv6MulticastOption(multicastAddr));
            }
        }

        //public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress) {
        //    this.ThrowIfDisposed();

        //    if (this._family != AddressFamily.InterNetwork) {
        //        throw new SocketException((int)SocketError.OperationNotSupported);
        //    }

        //    this._clientSocket.SetSocketOption(
        //       SocketOptionLevel.IP,
        //       SocketOptionName.AddMembership,
        //       new MulticastOption(multicastAddr, localAddress));
        //}

        //// Joins an IPv6 multicast address group.
        //public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr) {
        //    this.ThrowIfDisposed();

        //    ArgumentNullException.ThrowIfNull(multicastAddr);
        //    ArgumentOutOfRangeException.ThrowIfNegative(ifindex);
        //    if (this._family != AddressFamily.InterNetworkV6) {
        //        // Ensure that this is an IPv6 client, otherwise throw WinSock
        //        // Operation not supported socked exception.
        //        throw new SocketException((int)SocketError.OperationNotSupported);
        //    }

        //    this._clientSocket.SetSocketOption(
        //        SocketOptionLevel.IPv6,
        //        SocketOptionName.AddMembership,
        //        new IPv6MulticastOption(multicastAddr, ifindex));
        //}

        //// Joins a multicast address group with the specified time to live (TTL).
        //public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive) {
        //    this.ThrowIfDisposed();

        //    ArgumentNullException.ThrowIfNull(multicastAddr);
        //    if (!RangeValidationHelpers.ValidateRange(timeToLive, 0, 255)) {
        //        throw new ArgumentOutOfRangeException(nameof(timeToLive));
        //    }

        //    // Join the Multicast Group.
        //    this.JoinMulticastGroup(multicastAddr);

        //    // Set Time To Live (TTL).
        //    this._clientSocket.SetSocketOption(
        //        (this._family == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6,
        //        SocketOptionName.MulticastTimeToLive,
        //        timeToLive);
        //}

        //// Leaves a multicast address group.
        //public void DropMulticastGroup(IPAddress multicastAddr) {
        //    this.ThrowIfDisposed();

        //    ArgumentNullException.ThrowIfNull(multicastAddr);
        //    if (multicastAddr.AddressFamily != this._family) {
        //        // For IPv6, we need to create the correct MulticastOption and must also check for address family compatibility.
        //        throw new ArgumentException(SR.Format(SR.net_protocol_invalid_multicast_family, "UDP"), nameof(multicastAddr));
        //    }

        //    if (this._family == AddressFamily.InterNetwork) {
        //        this._clientSocket.SetSocketOption(
        //            SocketOptionLevel.IP,
        //            SocketOptionName.DropMembership,
        //            new MulticastOption(multicastAddr));
        //    }
        //    else {
        //        this._clientSocket.SetSocketOption(
        //            SocketOptionLevel.IPv6,
        //            SocketOptionName.DropMembership,
        //            new IPv6MulticastOption(multicastAddr));
        //    }
        //}

        //// Leaves an IPv6 multicast address group.
        //public void DropMulticastGroup(IPAddress multicastAddr, int ifindex) {
        //    this.ThrowIfDisposed();

        //    ArgumentNullException.ThrowIfNull(multicastAddr);
        //    ArgumentOutOfRangeException.ThrowIfNegative(ifindex);
        //    if (this._family != AddressFamily.InterNetworkV6) {
        //        // Ensure that this is an IPv6 client.
        //        throw new SocketException((int)SocketError.OperationNotSupported);
        //    }

        //    this._clientSocket.SetSocketOption(
        //        SocketOptionLevel.IPv6,
        //        SocketOptionName.DropMembership,
        //        new IPv6MulticastOption(multicastAddr, ifindex));
        //}

        //public Task<int> SendAsync(byte[] datagram, int bytes) =>
        //    this.SendAsync(datagram, bytes, null);

        ///// <summary>
        ///// Sends a UDP datagram asynchronously to a remote host.
        ///// </summary>
        ///// <param name="datagram">
        ///// An <see cref="ReadOnlyMemory{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        ///// </param>
        ///// <param name="cancellationToken">
        ///// The token to monitor for cancellation requests. The default value is None.
        ///// </param>
        ///// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous send operation. The value of its Result property contains the number of bytes sent.</returns>
        ///// <exception cref="ObjectDisposedException">The <see cref="UdpClient"/> is closed.</exception>
        ///// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        //public ValueTask<int> SendAsync(ReadOnlyMemory<byte> datagram, CancellationToken cancellationToken = default) =>
        //    this.SendAsync(datagram, null, cancellationToken);

        //public Task<int> SendAsync(byte[] datagram, int bytes, string? hostname, int port) =>
        //    this.SendAsync(datagram, bytes, this.GetEndpoint(hostname, port));

        ///// <summary>
        ///// Sends a UDP datagram asynchronously to a remote host.
        ///// </summary>
        ///// <param name="datagram">
        ///// An <see cref="ReadOnlyMemory{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        ///// </param>
        ///// <param name="hostname">
        ///// The name of the remote host to which you intend to send the datagram.
        ///// </param>
        ///// <param name="port">
        ///// The remote port number with which you intend to communicate.
        ///// </param>
        ///// <param name="cancellationToken">
        ///// The token to monitor for cancellation requests. The default value is None.
        ///// </param>
        ///// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous send operation. The value of its Result property contains the number of bytes sent.</returns>
        ///// <exception cref="InvalidOperationException">The <see cref="UdpClient"/> has already established a default remote host.</exception>
        ///// <exception cref="ObjectDisposedException">The <see cref="UdpClient"/> is closed.</exception>
        ///// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        //public ValueTask<int> SendAsync(ReadOnlyMemory<byte> datagram, string? hostname, int port, CancellationToken cancellationToken = default) =>
        //    this.SendAsync(datagram, this.GetEndpoint(hostname, port), cancellationToken);

        //public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint? endPoint) {
        //    this.ValidateDatagram(datagram, bytes, endPoint);

        //    if (endPoint is null) {
        //        return this._clientSocket.SendAsync(new ArraySegment<byte>(datagram, 0, bytes), SocketFlags.None);
        //    }
        //    else {
        //        this.CheckForBroadcast(endPoint.Address);
        //        return this._clientSocket.SendToAsync(new ArraySegment<byte>(datagram, 0, bytes), SocketFlags.None, endPoint);
        //    }
        //}

        ///// <summary>
        ///// Sends a UDP datagram asynchronously to a remote host.
        ///// </summary>
        ///// <param name="datagram">
        ///// An <see cref="ReadOnlyMemory{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        ///// </param>
        ///// <param name="endPoint">
        ///// An <see cref="IPEndPoint"/> that represents the host and port to which to send the datagram.
        ///// </param>
        ///// <param name="cancellationToken">
        ///// The token to monitor for cancellation requests. The default value is None.
        ///// </param>
        ///// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous send operation. The value of its Result property contains the number of bytes sent.</returns>
        ///// <exception cref="InvalidOperationException"><see cref="UdpClient"/> has already established a default remote host and <paramref name="endPoint"/> is not <see langword="null"/>.</exception>
        ///// <exception cref="ObjectDisposedException">The <see cref="UdpClient"/> is closed.</exception>
        ///// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        //public ValueTask<int> SendAsync(ReadOnlyMemory<byte> datagram, IPEndPoint? endPoint, CancellationToken cancellationToken = default) {
        //    this.ThrowIfDisposed();

        //    if (endPoint is null) {
        //        return this._clientSocket.SendAsync(datagram, SocketFlags.None, cancellationToken);
        //    }
        //    if (this._active) {
        //        // Do not allow sending packets to arbitrary host when connected.
        //        throw new InvalidOperationException(SR.net_udpconnected);
        //    }
        //    this.CheckForBroadcast(endPoint.Address);
        //    return this._clientSocket.SendToAsync(datagram, SocketFlags.None, endPoint, cancellationToken);
        //}

        //public Task<UdpReceiveResult> ReceiveAsync() {
        //    this.ThrowIfDisposed();

        //    return WaitAndWrap(this._clientSocket.ReceiveFromAsync(
        //        new ArraySegment<byte>(this._buffer, 0, MaxUDPSize),
        //        SocketFlags.None,
        //        this._family == AddressFamily.InterNetwork ? IPEndPointStatics.Any : IPEndPointStatics.IPv6Any));

        //    async Task<UdpReceiveResult> WaitAndWrap(Task<SocketReceiveFromResult> task) {
        //        SocketReceiveFromResult result = await task.ConfigureAwait(false);

        //        byte[] buffer = result.ReceivedBytes < MaxUDPSize ?
        //            this._buffer.AsSpan(0, result.ReceivedBytes).ToArray() :
        //            this._buffer;

        //        return new UdpReceiveResult(buffer, (IPEndPoint)result.RemoteEndPoint);
        //    }
        //}

        ///// <summary>
        ///// Returns a UDP datagram asynchronously that was sent by a remote host.
        ///// </summary>
        ///// <param name="cancellationToken">
        ///// The token to monitor for cancellation requests.
        ///// </param>
        ///// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
        ///// <exception cref="ObjectDisposedException">The underlying <see cref="Socket"/> has been closed.</exception>
        ///// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        //public ValueTask<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken) {
        //    this.ThrowIfDisposed();

        //    return WaitAndWrap(this._clientSocket.ReceiveFromAsync(
        //        this._buffer,
        //        SocketFlags.None,
        //        this._family == AddressFamily.InterNetwork ? IPEndPointStatics.Any : IPEndPointStatics.IPv6Any, cancellationToken));

        //    async ValueTask<UdpReceiveResult> WaitAndWrap(ValueTask<SocketReceiveFromResult> task) {
        //        SocketReceiveFromResult result = await task.ConfigureAwait(false);

        //        byte[] buffer = result.ReceivedBytes < MaxUDPSize ?
        //            this._buffer.AsSpan(0, result.ReceivedBytes).ToArray() :
        //            this._buffer;

        //        return new UdpReceiveResult(buffer, (IPEndPoint)result.RemoteEndPoint);
        //    }
        //}

        private void CreateClientSocket() =>
            // Common initialization code.
            //
            // IPv6 Changes: Use the AddressFamily of this class rather than hardcode.
            this._clientSocket = new Socket(this._family, SocketType.Dgram, ProtocolType.Udp);

        public UdpClient(string hostname, int port) {
            //ArgumentNullException.ThrowIfNull(hostname);

            //if (!TcpValidationHelpers.ValidatePortNumber(port)) {
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}

            // NOTE: Need to create different kinds of sockets based on the addresses
            //       returned from DNS. As a result, we defer the creation of the
            //       socket until the Connect method.

            this.Connect(hostname, port); ;
        }

        public void Close() => this.Dispose(true);

        public void Connect(string hostname, int port) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(hostname);
            //if (!TcpValidationHelpers.ValidatePortNumber(port)) {
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}

            // We must now look for addresses that use a compatible address family to the client socket. However, in the
            // case of the <hostname,port> constructor we will have deferred creating the socket and will do that here
            // instead.

            var addresses = Dns.GetHostEntry(hostname).AddressList;

            Exception lastex = null;
            //Socket ipv6Socket = null;
            Socket ipv4Socket = null;

            try {
                if (this._clientSocket == null) {
                    //if (Socket.OSSupportsIPv4) {
                        ipv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    //}
                    //if (Socket.OSSupportsIPv6) {
                    //    ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    //}
                }


                foreach (var address in addresses) {
                    try {
                        if (this._clientSocket == null) {
                            // We came via the <hostname,port> constructor. Set the
                            // address family appropriately, create the socket and
                            // try to connect.
                            if (address.AddressFamily == AddressFamily.InterNetwork && ipv4Socket != null) {
                                ipv4Socket.Connect(new IPEndPoint(address, port));
                                this._clientSocket = ipv4Socket;
                                //ipv6Socket?.Close();
                            }
                            //else if (ipv6Socket != null) {
                            //    ipv6Socket.Connect(address, port);
                            //    this._clientSocket = ipv6Socket;
                            //    ipv4Socket?.Close();
                            //}


                            this._family = address.AddressFamily;
                            this._active = true;
                            break;
                        }
                        else if (this.IsAddressFamilyCompatible(address.AddressFamily)) {
                            // Only use addresses with a matching family
                            this.Connect(new IPEndPoint(address, port));
                            this._active = true;
                            break;
                        }
                    }
                    catch (Exception ex) {
                        //if (ExceptionCheck.IsFatal(ex)) {
                        //    throw;
                        //}
                        lastex = ex;
                    }
                }
            }

            catch (Exception ex) {
                //if (ExceptionCheck.IsFatal(ex)) {
                //    throw;
                //}
                lastex = ex;
            }
            finally {
                //cleanup temp sockets if failed
                //main socket gets closed when tcpclient gets closed

                //did we connect?
                if (!this._active) {
                    //ipv6Socket?.Close();
                    ipv4Socket?.Close();

                    // The connect failed - rethrow the last error we had
                    if (lastex != null) {
                        throw lastex;
                    }
                    else {
                        throw new SocketException(SocketError.NotConnected);
                    }
                }
            }
        }

        public void Connect(IPAddress addr, int port) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(addr);
            //if (!TcpValidationHelpers.ValidatePortNumber(port)) {
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}

            var endPoint = new IPEndPoint(addr, port);

            this.Connect(endPoint);
        }

        public void Connect(IPEndPoint endPoint) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(endPoint);

            this.CheckForBroadcast(endPoint.Address);
            this.Client.Connect(endPoint);
            this._active = true;
        }

        public byte[] Receive(ref IPEndPoint remoteEP) {
            this.ThrowIfDisposed();

            // this is a fix due to the nature of the ReceiveFrom() call and the
            // ref parameter convention, we need to cast an IPEndPoint to it's base
            // class EndPoint and cast it back down to IPEndPoint. ugly but it works.
            //EndPoint tempRemoteEP = this._family == AddressFamily.InterNetwork ?
            //    IPEndPointStatics.Any :
            //    IPEndPointStatics.IPv6Any;

            EndPoint tempRemoteEP = new IPEndPoint(IPAddress.Any, 0);

            var received = this.Client.ReceiveFrom(this._buffer, MaxUDPSize, 0, ref tempRemoteEP);
            remoteEP = (IPEndPoint)tempRemoteEP;

            // because we don't return the actual length, we need to ensure the returned buffer
            // has the appropriate length.

            if (received < MaxUDPSize) {
                var newBuffer = new byte[received];
                //Buffer.BlockCopy(this._buffer, 0, newBuffer, 0, received);
                Array.Copy(this._buffer, newBuffer, received);
                return newBuffer;
            }
            return this._buffer;
        }


        // Sends a UDP datagram to the host at the remote end point.
        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(dgram);
            if (this._active && endPoint != null) {
                // Do not allow sending packets to arbitrary host when connected
                throw new InvalidOperationException("SR.net_udpconnected");
            }

            if (endPoint == null) {
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
            }

            this.CheckForBroadcast(endPoint.Address);

            return this.Client.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
        }

        /// <summary>
        /// Sends a UDP datagram to the host at the specified remote endpoint.
        /// </summary>
        /// <param name="datagram">
        /// An <see cref="ReadOnlySpan{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        /// </param>
        /// <param name="endPoint">
        /// An <see cref="IPEndPoint"/> that represents the host and port to which to send the datagram.
        /// </param>
        /// <returns>The number of bytes sent.</returns>
        /// <exception cref="InvalidOperationException"><see cref="UdpClient"/> has already established a default remote host and <paramref name="endPoint"/> is not <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="UdpClient"/> is closed.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        public int Send(byte[] datagram, IPEndPoint endPoint) {
            this.ThrowIfDisposed();

            if (this._active && endPoint != null) {
                // Do not allow sending packets to arbitrary host when connected
                throw new InvalidOperationException("SR.net_udpconnected");
            }

            if (endPoint == null) {
                return this.Client.Send(datagram, SocketFlags.None);
            }

            this.CheckForBroadcast(endPoint.Address);

            return this.Client.SendTo(datagram, SocketFlags.None, endPoint);
        }

        // Sends a UDP datagram to the specified port on the specified remote host.
        public int Send(byte[] dgram, int bytes, string hostname, int port) => this.Send(dgram, bytes, this.GetEndpoint(hostname, port));

        /// <summary>
        /// Sends a UDP datagram to a specified port on a specified remote host.
        /// </summary>
        /// <param name="datagram">
        /// An <see cref="ReadOnlySpan{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        /// </param>
        /// <param name="hostname">
        /// The name of the remote host to which you intend to send the datagram.
        /// </param>
        /// <param name="port">
        /// The remote port number with which you intend to communicate.
        /// </param>
        /// <returns>The number of bytes sent.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="UdpClient"/> has already established a default remote host.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="UdpClient"/> is closed.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        public int Send(byte[] datagram, string hostname, int port) => this.Send(datagram, this.GetEndpoint(hostname, port));

        // Sends a UDP datagram to a remote host.
        public int Send(byte[] dgram, int bytes) {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(dgram);
            if (!this._active) {
                // only allowed on connected socket
                throw new InvalidOperationException("SR.net_notconnected");
            }

            return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
        }

        /// <summary>
        /// Sends a UDP datagram to a remote host.
        /// </summary>
        /// <param name="datagram">
        /// An <see cref="ReadOnlySpan{T}"/> of Type <see cref="byte"/> that specifies the UDP datagram that you intend to send.
        /// </param>
        /// <returns>The number of bytes sent.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="UdpClient"/> has not established a default remote host.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="UdpClient"/> is closed.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        public int Send(byte[] datagram) {
            this.ThrowIfDisposed();

            if (!this._active) {
                // only allowed on connected socket
                throw new InvalidOperationException("SR.net_notconnected");
            }

            return this.Client.Send(datagram, SocketFlags.None);
        }

        private void ThrowIfDisposed() {
            if (this._disposed) {
                throw new ObjectDisposedException();  
            }
        }
    }
}
