// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// TinyCLR-side TcpListener — managed wrapper around a Socket configured for
// listen+accept. Mirrors full .NET surface so server code (HTTP, custom
// protocols) ports to TinyCLR without conditionals. IPv4-only.

namespace System.Net.Sockets {
    public class TcpListener {
        private readonly IPEndPoint _serverSocketEP;
        private Socket _serverSocket;
        private bool _active;

        // Initializes a new instance of the TcpListener class with the
        // specified local endpoint.
        public TcpListener(IPEndPoint localEP) {
            if (localEP == null) throw new ArgumentNullException(nameof(localEP));
            this._serverSocketEP = localEP;
            this._serverSocket = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        // Initializes a new instance of the TcpListener class that listens for
        // incoming connection attempts on the specified local IP address and
        // port number.
        public TcpListener(IPAddress localaddr, int port) : this(new IPEndPoint(localaddr, port)) { }

        // The underlying socket. Useful for setting socket options before
        // calling Start().
        public Socket Server => this._serverSocket;

        // Indicates whether the listener has been started.
        protected bool Active => this._active;

        public EndPoint LocalEndpoint => this._active ? this._serverSocket.LocalEndPoint : (EndPoint)this._serverSocketEP;

        public void Start() => this.Start(int.MaxValue);

        public void Start(int backlog) {
            if (backlog < 0) throw new ArgumentOutOfRangeException(nameof(backlog));
            if (this._active) return;

            this._serverSocket.Bind(this._serverSocketEP);
            try {
                this._serverSocket.Listen(backlog);
            }
            catch (SocketException) {
                // Bind succeeded but Listen failed — give up the bind by
                // recreating the socket so a Stop() doesn't leave a half-open
                // listening state.
                this.Stop();
                throw;
            }
            this._active = true;
        }

        public void Stop() {
            try {
                this._serverSocket.Close();
            }
            catch { /* swallow — best-effort close */ }

            this._active = false;
            // Recreate so Start() can be called again. Matches full .NET.
            this._serverSocket = new Socket(this._serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        // Determines if a pending connection is waiting. Non-blocking.
        public bool Pending() {
            if (!this._active) throw new InvalidOperationException("Listener has not been started.");
            return this._serverSocket.Poll(0, SelectMode.SelectRead);
        }

        // Accepts a pending connection request as a Socket. Blocks.
        public Socket AcceptSocket() {
            if (!this._active) throw new InvalidOperationException("Listener has not been started.");
            return this._serverSocket.Accept();
        }

        // Accepts a pending connection request as a TcpClient. Blocks.
        public TcpClient AcceptTcpClient() {
            if (!this._active) throw new InvalidOperationException("Listener has not been started.");
            return new TcpClient(this._serverSocket.Accept());
        }

        // Convenience factory matching full .NET — listens on any interface
        // and lets the OS pick a port.
        public static TcpListener Create(int port) {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(port));
            return new TcpListener(IPAddress.Any, port);
        }
    }
}
