////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Net.Sockets;
    using System.Collections;

    /// <summary>
    /// Represents a response to a request being handled by an
    /// <see cref="System.Net.HttpListener"/> object.
    /// </summary>
    public sealed class HttpListenerResponse : IDisposable
    {
        /// <summary>
        /// A flag that indicates whether the response was already sent.
        /// </summary>
        private bool m_WasResponseSent = false;

        /// <summary>
        /// A flag that indicates that the response was closed.
        /// Writing to client is not allowed after response is closed.
        /// </summary>
        private bool m_IsResponseClosed = false;

        /// <summary>
        /// The length of the content of the response.
        /// </summary>
        long m_ContentLength = -1;

        /// <summary>
        /// The response headers from the HTTP client.
        /// </summary>
        private WebHeaderCollection m_httpResponseHeaders = new WebHeaderCollection(true);

        /// <summary>
        /// The HTTP version for the response.
        /// </summary>
        private Version m_version = new Version(1, 1);

        /// <summary>
        /// Indicates whether the server requests a persistent connection.
        /// Persistent connection is used if KeepAlive is <itemref>true</itemref>
        /// in both the request and the response.
        /// </summary>
        private bool m_KeepAlive = false;

        /// <summary>
        /// Encoding for this response's OutputStream.
        /// </summary>
        private Encoding m_Encoding = Encoding.UTF8;

        /// <summary>
        /// Keeps content type for the response, set by user application.
        /// </summary>
        private string m_contentType;

        /// <summary>
        /// Response status code.
        /// </summary>
        private int m_ResponseStatusCode = (int)HttpStatusCode.OK;

        /// <summary>
        /// Array of connected client streams
        /// </summary>
        private HttpListener m_Listener;

        /// <summary>
        /// Member with network stream connected to client.
        /// After call to Close() the stream is closed, no further writing allowed.
        /// </summary>
        private OutputNetworkStreamWrapper m_clientStream;

        /// <summary>
        /// The value of the HTTP Location header in this response.
        /// </summary>
        private string m_redirectLocation;

        /// <summary>
        /// Response uses chunked transfer encoding.
        /// </summary>
        private bool m_sendChunked = false;

        /// <summary>
        /// text description of the HTTP status code returned to the client.
        /// </summary>
        private string m_statusDescription;

        /// <summary>
        /// Throws InvalidOperationException is HTTP response was sent.
        /// Called before setting of properties.
        /// </summary>
        private void ThrowIfResponseSent()
        {
            if (this.m_WasResponseSent)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// HttpListenerResponse is created by HttpListenerContext
        /// </summary>
        /// <param name="clientStream">Network stream to the client</param>
        /// <param name="httpListener">TBD</param>
        internal HttpListenerResponse(OutputNetworkStreamWrapper clientStream, HttpListener httpListener)
        {
            // Sets the delegate, so SendHeaders will be called on first write.
            clientStream.HeadersDelegate = new OutputNetworkStreamWrapper.SendHeadersDelegate(this.SendHeaders);
            // Saves network stream as member.
            this.m_clientStream = clientStream;
            // Saves list of client streams. m_clientStream is removed from clientStreamsList during Close().
            this.m_Listener = httpListener;
        }

        /// <summary>
        /// Updates the HTTP WEB header collection to prepare it for request.
        /// For each property set it adds member to m_httpResponseHeaders.
        /// m_httpResponseHeaders is serializes to string and sent to client.
        /// </summary>
        private void PrepareHeaders()
        {
            // Adds content length if it was present.
            if (this.m_ContentLength != -1)
            {
                this.m_httpResponseHeaders.ChangeInternal(HttpKnownHeaderNames.ContentLength, this.m_ContentLength.ToString());
            }

            // Since we do not support persistent connection, send close always.
            var connection = this.m_KeepAlive ? "Keep-Alive" : "Close";
            this.m_httpResponseHeaders.ChangeInternal(HttpKnownHeaderNames.Connection, connection);

            // Adds content type if user set it:
            if (this.m_contentType != null)
            {
                this.m_httpResponseHeaders.AddWithoutValidate(HttpKnownHeaderNames.ContentType, this.m_contentType);
            }

            if (this.m_redirectLocation != null)
            {
                this.m_httpResponseHeaders.AddWithoutValidate(HttpKnownHeaderNames.Location, this.m_redirectLocation);
                this.m_ResponseStatusCode = (int)HttpStatusCode.Redirect;
            }
        }

        /// <summary>
        /// Composes HTTP response line based on
        /// </summary>
        /// <returns></returns>
        private string ComposeHTTPResponse()
        {
            // Starts with HTTP
            var resp = "HTTP/";
            // Adds version of HTTP
            resp += this.m_version.ToString();
            // Add status code.
            resp += " " + this.m_ResponseStatusCode;
            // Adds description
            if (this.m_statusDescription == null)
            {
                resp += " " + GetStatusDescription(this.m_ResponseStatusCode);
            }
            else // User provided description is present.
            {
                resp += " " + this.m_statusDescription;
            }

            // Add line termindation.
            resp += "\r\n";
            return resp;
        }

        /// <summary>
        /// Sends HTTP status and headers to client.
        /// </summary>
        private void SendHeaders()
        {
            // As first step we disable the callback to SendHeaders, so m_clientStream.Write would not call
            // SendHeaders() again.
            this.m_clientStream.HeadersDelegate = null;

            // Creates encoder, generates headers and sends the data.
            var encoder = Encoding.UTF8;

            var statusLine = encoder.GetBytes(ComposeHTTPResponse());
            this.m_clientStream.Write(statusLine, 0, statusLine.Length);

            // Prepares/Updates WEB header collection.
            PrepareHeaders();

            // Serialise WEB header collection to byte array.
            var pHeaders = this.m_httpResponseHeaders.ToByteArray();

            // Sends the headers
            this.m_clientStream.Write(pHeaders, 0, pHeaders.Length);

            this.m_WasResponseSent = true;
        }

        /// <summary>
        /// Gets or sets the HTTP status code to be returned to the client.
        /// </summary>
        /// <value>An <itemref>Int32</itemref> value that specifies the
        /// <see cref="System.Net.HttpStatusCode"/> for the requested resource.
        /// The default is <itemref>OK</itemref>, indicating that the server
        /// successfully processed the client's request and included the
        /// requested resource in the response body.</value>
        public int StatusCode {
            get => this.m_ResponseStatusCode;
            set {
                ThrowIfResponseSent();
                this.m_ResponseStatusCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes in the body data included in the
        /// response.
        /// </summary>
        /// <value>The value of the response's <itemref>Content-Length</itemref>
        /// header.</value>
        public long ContentLength64 {
            get => this.m_ContentLength;

            set {
                ThrowIfResponseSent();
                this.m_ContentLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of header name/value pairs that is
        /// returned by the server.
        /// </summary>
        /// <value>A <itemref>WebHeaderCollection</itemref> instance that
        /// contains all the explicitly set HTTP headers to be included in the
        /// response.</value>
        public WebHeaderCollection Headers {
            get => this.m_httpResponseHeaders;
            set {
                ThrowIfResponseSent();
                this.m_httpResponseHeaders = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the server requests a persistent connection.
        /// </summary>
        /// <value><itemref>true</itemref> if the server requests a persistent
        /// connection; otherwise, <itemref>false</itemref>.  The default is
        /// <itemref>true</itemref>.</value>
        public bool KeepAlive {
            get => this.m_KeepAlive;

            set => this.m_KeepAlive = value;
        }

        /// <summary>
        /// Gets a <itemref>Stream</itemref> object to which a response can be
        /// written.
        /// </summary>
        /// <value>A <itemref>Stream</itemref> object to which a response can be
        /// written.</value>
        /// <remarks>
        /// The first write to the output stream sends a response to the client.
        /// </remarks>
        public Stream OutputStream
        {
            get
            {
                if (this.m_IsResponseClosed)
                {
                    throw new ObjectDisposedException("Response has been sent");
                }

                return this.m_clientStream;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP version that is used for the response.
        /// </summary>
        /// <value>A <itemref>Version</itemref> object indicating the version of
        /// HTTP used when responding to the client.  This property is obsolete.
        /// </value>
        public Version ProtocolVersion {
            get => this.m_version;
            set {
                ThrowIfResponseSent();
                this.m_version = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the HTTP <itemref>Location</itemref>
        /// header in this response.
        /// </summary>
        /// <value>A <itemref>String</itemref> that contains the absolute URL to
        /// be sent to the client in the <itemref>Location</itemref> header.
        /// </value>
        public string RedirectLocation {
            get => this.m_redirectLocation;
            set {
                ThrowIfResponseSent();
                this.m_redirectLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the response uses chunked transfer encoding.
        /// </summary>
        /// <value><itemref>true</itemref> if the response is set to use chunked
        /// transfer encoding; otherwise, <itemref>false</itemref>.  The default
        /// is <itemref>false</itemref>.</value>
        public bool SendChunked {
            get => this.m_sendChunked;
            set {
                ThrowIfResponseSent();
                this.m_sendChunked = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoding for this response's
        /// <itemref>OutputStream</itemref>.
        /// </summary>
        /// <value>An <itemref>Encoding</itemref> object suitable for use with
        /// the data in the
        /// <see cref="System.Net.HttpListenerResponse.OutputStream"/> property,
        /// or <itemref>null</itemref> reference if no encoding is specified.
        /// </value>
        /// <remarks>
        /// Only UTF8 encoding is supported.
        /// </remarks>
        public Encoding ContentEncoding {
            get => this.m_Encoding;
            set {
                ThrowIfResponseSent();
                this.m_Encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets the MIME type of the returned content.
        /// </summary>
        /// <value>A <itemref>String</itemref> instance that contains the text
        /// of the response's <itemref>Content-Type</itemref> header.</value>
        public string ContentType {
            get => this.m_contentType;
            set {
                ThrowIfResponseSent();
                this.m_contentType = value;
            }
        }

        /// <summary>
        /// Gets or sets a text description of the HTTP status code that is
        /// returned to the client.
        /// </summary>
        /// <value>The text description of the HTTP status code returned to the
        /// client.</value>
        public string StatusDescription {
            get => this.m_statusDescription;
            set {
                ThrowIfResponseSent();
                this.m_statusDescription = value;
            }
        }

        public void Detach()
        {
            if (!this.m_IsResponseClosed)
            {
                if (!this.m_WasResponseSent)
                {
                    SendHeaders();
                }

                this.m_IsResponseClosed = true;
            }
        }

        /// <summary>
        /// Sends the response to the client and releases the resources held by
        /// this HttpListenerResponse instance.
        /// </summary>
        /// <remarks>
        /// This method flushes data to the client and closes the network
        /// connection.
        /// </remarks>
        public void Close()
        {
            if (!this.m_IsResponseClosed)
            {
                try
                {
                    if (!this.m_WasResponseSent)
                    {
                        SendHeaders();
                    }
                }
                finally
                {
                    // Removes from the list of streams and closes the socket.
                    ((IDisposable)this).Dispose();
                }
            }
        }

        /// <summary>
        /// Closes the socket and sends the response if it was not done earlier
        /// and the socket is present.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (!this.m_IsResponseClosed)
            {
                try
                {
                    // Iterates over list of client connections and remove its stream from it.
                    this.m_Listener.RemoveClientStream(this.m_clientStream);

                    this.m_clientStream.Flush();

                    // If KeepAlive is true,
                    if (this.m_KeepAlive)
                    {   // Then socket is tramsferred to the list of waiting for new data.
                        this.m_Listener.AddToWaitingConnections(this.m_clientStream);
                    }
                    else  // If not KeepAlive then close
                    {
                        this.m_clientStream.Dispose();
                    }
                }
                catch{}

                this.m_IsResponseClosed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called to close the socket if necessary.
        /// </summary>
        ~HttpListenerResponse()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Return default Description based in response status code.
        /// </summary>
        /// <param name="code">HTTP status code</param>
        /// <returns>
        /// Default string with description.
        /// </returns>
        internal static string GetStatusDescription(int code)
        {
            switch (code)
            {
                case 100: return "Continue";
                case 101: return "Switching Protocols";
                case 102: return "Processing";
                case 200: return "OK";
                case 201: return "Created";
                case 202: return "Accepted";
                case 203: return "Non-Authoritative Information";
                case 204: return "No Content";
                case 205: return "Reset Content";
                case 206: return "Partial Content";
                case 207: return "Multi-Status";
                case 300: return "Multiple Choices";
                case 301: return "Moved Permanently";
                case 302: return "Found";
                case 303: return "See Other";
                case 304: return "Not Modified";
                case 305: return "Use Proxy";
                case 307: return "Temporary Redirect";
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 402: return "Payment Required";
                case 403: return "Forbidden";
                case 404: return "Not Found";
                case 405: return "Method Not Allowed";
                case 406: return "Not Acceptable";
                case 407: return "Proxy Authentication Required";
                case 408: return "Request Timeout";
                case 409: return "Conflict";
                case 410: return "Gone";
                case 411: return "Length Required";
                case 412: return "Precondition Failed";
                case 413: return "Request Entity Too Large";
                case 414: return "Request-Uri Too Long";
                case 415: return "Unsupported Media Type";
                case 416: return "Requested Range Not Satisfiable";
                case 417: return "Expectation Failed";
                case 422: return "Unprocessable Entity";
                case 423: return "Locked";
                case 424: return "Failed Dependency";
                case 500: return "Internal Server Error";
                case 501: return "Not Implemented";
                case 502: return "Bad Gateway";
                case 503: return "Service Unavailable";
                case 504: return "Gateway Timeout";
                case 505: return "Http Version Not Supported";
                case 507: return "Insufficient Storage";
            }

            return "";
        }

    }
}


