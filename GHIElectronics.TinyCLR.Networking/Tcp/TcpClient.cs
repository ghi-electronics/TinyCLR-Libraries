// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
//using System.Runtime.ExceptionServices;
using System.Threading;
//using System.Threading.Tasks;

namespace System.Net.Sockets
{
    /// <summary>Provides client connections for TCP network services.</summary>
    // The System.Net.Sockets.TcpClient class provide TCP services at a higher level
    // of abstraction than the System.Net.Sockets.Socket class. System.Net.Sockets.TcpClient
    // is used to create a Client connection to a remote host.
    public class TcpClient : IDisposable
    {
        private AddressFamily _family;
        private Socket _clientSocket = null; // initialized by helper called from ctor
        private NetworkStream _dataStream;
        private int _disposed;
        // _active means "Connect/Accept has been called" — distinct from
        // Connected which tracks the live link state. Mirrors full .NET.
        private bool _active;

        private bool Disposed => this._disposed != 0;

        /// <summary>Initializes a new instance with the default address family.</summary>
        // Initializes a new instance of the System.Net.Sockets.TcpClient class.
        public TcpClient() : this(AddressFamily.Unknown)
        {
        }

        /// <summary>Initializes a new instance using the specified address family.</summary>
        // Initializes a new instance of the System.Net.Sockets.TcpClient class.
        public TcpClient(AddressFamily family)
        {
            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, family);

            //if (family is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6 or AddressFamily.Unknown))
            //{
            //    throw new ArgumentException(SR.Format(SR.net_protocol_invalid_family, "TCP"), nameof(family));
            //}

            this._family = family;
            this.InitializeClientSocket();
        }

        /// <summary>Initializes a new instance and binds it to the specified local endpoint.</summary>
        // Initializes a new instance of the System.Net.Sockets.TcpClient class with the specified end point.
        public TcpClient(IPEndPoint localEP)
        {
            //ArgumentNullException.ThrowIfNull(localEP);

            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, localEP);
            this._family = localEP.Address.AddressFamily; // set before calling CreateSocket
            this.InitializeClientSocket();
            this._clientSocket.Bind(localEP);
        }

        /// <summary>Initializes a new instance and connects to the specified host and port.</summary>
        // Initializes a new instance of the System.Net.Sockets.TcpClient class and connects to the specified port on
        // the specified host.
        public TcpClient(string hostname, int port) : this(AddressFamily.Unknown)
        {
            //ArgumentNullException.ThrowIfNull(hostname);

            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, hostname);
            //if (!TcpValidationHelpers.ValidatePortNumber(port))
            //{
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}

            try
            {
                this.Connect(hostname, port);
            }
            catch
            {
                this._clientSocket?.Close();
                throw;
            }
        }

        // Used by TcpListener.Accept().
        internal TcpClient(Socket acceptedSocket)
        {
            this._clientSocket = acceptedSocket;
            this._active = true;
        }

        /// <summary>Whether a connection has been established.</summary>
        // Used by the class to indicate that a connection has been made.
        protected bool Active {
            get => this._active;
            set => this._active = value;
        }

        /// <summary>The number of bytes available to be read from the connection.</summary>
        public int Available => this.Client.Available;

        /// <summary>The underlying socket used by the client.</summary>
        // Used by the class to provide the underlying network socket.
        public Socket Client {
            get => this.Disposed ? null : this._clientSocket;
            set {
                this._clientSocket = value;
                this._family = AddressFamily.InterNetwork;// this._clientSocket?.AddressFamily ?? AddressFamily.Unknown;
                if (this._clientSocket == null) {
                    this.InitializeClientSocket();
                }
            }
        }

        /// <summary>Whether the client is connected to a remote host.</summary>
        // Delegates to the Socket so we have a single source of truth.
        // Previously stored in a separate _isConnected bool that the
        // accept-ctor forgot to set, causing GetStream to throw "not
        // connected" on accepted clients. Socket.Connected already covers
        // both Connect()-set and accept-ctor-set cases via m_isConnected.
        public bool Connected => this._clientSocket != null && this._clientSocket.Connected;

        //public bool ExclusiveAddressUse
        //{
        //    get { return this.Client?.ExclusiveAddressUse ?? false; }
        //    set
        //    {
        //        if (this._clientSocket != null)
        //        {
        //            this._clientSocket.ExclusiveAddressUse = value;
        //        }
        //    }
        //}

        /// <summary>Connects the client to the specified host and port.</summary>
        // Connects the Client to the specified port on the specified host.
        public void Connect(string hostname, int port)
        {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(hostname);
            //if (!TcpValidationHelpers.ValidatePortNumber(port))
            //{
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}
            var addresses = Dns.GetHostEntry(hostname).AddressList;

            var remoteEndPoint = new IPEndPoint(addresses[0], port);
            this.Client.Connect(remoteEndPoint);
            this._family = AddressFamily.InterNetwork;
            this._active = true;
        }

        /// <summary>Connects the client to the specified IP address and port.</summary>
        // Connects the Client to the specified port on the specified host.
        public void Connect(IPAddress address, int port)
        {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(address);
            //if (!TcpValidationHelpers.ValidatePortNumber(port))
            //{
            //    throw new ArgumentOutOfRangeException(nameof(port));
            //}

            var remoteEP = new IPEndPoint(address, port);
            this.Connect(remoteEP);
        }

        /// <summary>Connects the client to the specified remote endpoint.</summary>
        // Connect the Client to the specified end point.
        public void Connect(IPEndPoint remoteEP)
        {
            this.ThrowIfDisposed();

            //ArgumentNullException.ThrowIfNull(remoteEP);

            this.Client.Connect(remoteEP);
            this._family = AddressFamily.InterNetwork;
            this._active = true;
        }

        /// <summary>Connects the client to the first of the specified IP addresses on the given port.</summary>
        public void Connect(IPAddress[] ipAddresses, int port)
        {
            var remoteEndPoint = new IPEndPoint(ipAddresses[0], port);
            this.Client.Connect(remoteEndPoint);
            this._family = AddressFamily.InterNetwork;
            this._active = true;
        }

        //public Task ConnectAsync(IPAddress address, int port) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(address, port));

        //public Task ConnectAsync(string host, int port) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(host, port));

        //public Task ConnectAsync(IPAddress[] addresses, int port) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(addresses, port));

        ///// <summary>
        ///// Connects the client to a remote TCP host using the specified endpoint as an asynchronous operation.
        ///// </summary>
        ///// <param name="remoteEP">The <see cref="IPEndPoint"/> to which you intend to connect.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public Task ConnectAsync(IPEndPoint remoteEP) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(remoteEP));

        //private async Task CompleteConnectAsync(Task task)
        //{
        //    await task.ConfigureAwait(false);
        //    this._active = true;
        //}

        //public ValueTask ConnectAsync(IPAddress address, int port, CancellationToken cancellationToken) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(address, port, cancellationToken));

        //public ValueTask ConnectAsync(string host, int port, CancellationToken cancellationToken) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(host, port, cancellationToken));

        //public ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken cancellationToken) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(addresses, port, cancellationToken));

        ///// <summary>
        ///// Connects the client to a remote TCP host using the specified endpoint as an asynchronous operation.
        ///// </summary>
        ///// <param name="remoteEP">The <see cref="IPEndPoint"/> to which you intend to connect.</param>
        ///// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //public ValueTask ConnectAsync(IPEndPoint remoteEP, CancellationToken cancellationToken) =>
        //    CompleteConnectAsync(this.Client.ConnectAsync(remoteEP, cancellationToken));

        //private async ValueTask CompleteConnectAsync(ValueTask task)
        //{
        //    await task.ConfigureAwait(false);
        //    this._active = true;
        //}

        //public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback? requestCallback, object? state) =>
        //    this.Client.BeginConnect(address, port, requestCallback, state);

        //public IAsyncResult BeginConnect(string host, int port, AsyncCallback? requestCallback, object? state) =>
        //    this.Client.BeginConnect(host, port, requestCallback, state);

        //public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback? requestCallback, object? state) =>
        //    this.Client.BeginConnect(addresses, port, requestCallback, state);

        //public void EndConnect(IAsyncResult asyncResult)
        //{
        //    this._clientSocket.EndConnect(asyncResult);
        //    this._active = true;

        //}

        /// <summary>Returns the network stream used to send and receive data.</summary>
        // Returns the stream used to read and write data to the remote host.
        public NetworkStream GetStream()
        {
            this.ThrowIfDisposed();

            if (!this.Connected)
            {
                throw new InvalidOperationException("SR.net_notconnected");
            }

            if (this._dataStream == null)
                this._dataStream = new NetworkStream(this.Client, true);

            return this._dataStream;
        }

        /// <summary>Closes the client and releases its resources.</summary>
        public void Close() => this.Dispose();

        /// <summary>Releases the resources used by the client.</summary>
        // Disposes the Tcp connection.
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 0)
            {
                if (disposing)
                {
                    var dataStream = this._dataStream;
                    if (dataStream != null)
                    {
                        dataStream.Dispose();
                        dataStream = null;
                    }
                    else
                    {
                        // If the NetworkStream wasn't created, the Socket might
                        // still be there and needs to be closed. In the case in which
                        // we are bound to a local IPEndPoint this will remove the
                        // binding and free up the IPEndPoint for later uses.
                        // Match full .NET: graceful half-close in both
                        // directions before close. lwIP delivers a FIN to the
                        // peer so a remote side sees a clean shutdown rather
                        // than RST. Best-effort — Shutdown errors are
                        // ignored; the Close() below cleans up regardless.
                        var chk = this._clientSocket;
                        if (chk != null) {
                            try {
                                chk.Shutdown(SocketShutdown.Both);
                            }
                            catch { /* swallow */ }
                            try {
                                chk.Close();
                            }
                            catch { /* swallow */ }
                        }

                    }

                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>Releases the resources used by the client.</summary>
        public void Dispose() => this.Dispose(true);

        /// <summary>Releases unmanaged resources when the client is finalized.</summary>
        ~TcpClient() => this.Dispose(false);

        /// <summary>The size, in bytes, of the receive buffer.</summary>
        // Gets or sets the size of the receive buffer in bytes.
        public int ReceiveBufferSize {
            get => (int)this.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
            set => this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
        }

        /// <summary>The size, in bytes, of the send buffer.</summary>
        // Gets or sets the size of the send buffer in bytes.
        public int SendBufferSize {
            get => (int)this.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
            set => this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
        }

        /// <summary>The amount of time, in milliseconds, that a receive operation waits before timing out.</summary>
        // Gets or sets the receive time out value of the connection in milliseconds.
        public int ReceiveTimeout {
            get => (int)this.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
            set => this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
        }

        /// <summary>The amount of time, in milliseconds, that a send operation waits before timing out.</summary>
        // Gets or sets the send time out value of the connection in milliseconds.
        public int SendTimeout {
            get => (int)this.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
            set => this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
        }

        // Gets or sets the value of the connection's linger option.
        //[DisallowNull]
        //public LingerOption? LingerState
        //{
        //    get { return this.Client.LingerState; }
        //    set { this.Client.LingerState = value!; }
        //}

        //// Enables or disables delay when send or receive buffers are full.
        //public bool NoDelay
        //{
        //    get { return this.Client.NoDelay; }
        //    set { this.Client.NoDelay = value; }
        //}

        private void InitializeClientSocket()
        {
            Debug.Assert(this._clientSocket == null);
            if (this._family == AddressFamily.Unknown)
            {
                // If AF was not explicitly set try to initialize dual mode socket or fall-back to IPv4.
                this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //if (this._clientSocket.AddressFamily == AddressFamily.InterNetwork)
                //{
                //    this._family = AddressFamily.InterNetwork;
                //}
                this._family = AddressFamily.InterNetwork;
            }
            else
            {
                this._clientSocket = new Socket(this._family, SocketType.Stream, ProtocolType.Tcp);
            }
        }

        private void ThrowIfDisposed() {
            if (this._disposed != 0) {
                throw new ObjectDisposedException();
            }
        }
    }
}
