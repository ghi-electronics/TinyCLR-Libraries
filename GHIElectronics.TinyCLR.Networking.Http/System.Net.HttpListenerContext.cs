////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Collections;

    /// <summary>
    /// Provides access to the request and response objects used by the
    /// <itemref>HttpListener</itemref> class.  This class cannot be inherited.
    /// </summary>
    public class HttpListenerContext
    {
        /// <summary>
        /// A flag that indicates whether an HTTP request was parsed.
        /// </summary>
        /// <remarks>
        /// The HTTP request is parsed upon the first access to the Request to
        /// Response property.  Access to that property might be done from a
        /// different thread than the thread that is used for construction of
        /// the HttpListenerContext.
        /// </remarks>
        bool m_IsHTTPRequestParsed;

        /// <summary>
        /// Member with network stream connected to client.
        /// This stream is used for writing data.
        /// This stream owns the socket.
        /// </summary>
        private OutputNetworkStreamWrapper m_clientOutputStream;

        /// <summary>
        /// Member with network stream connected to client.
        /// This stream is used for Reading data.
        /// This stream does not own the socket.
        /// </summary>
        private InputNetworkStreamWrapper m_clientInputStream;

        /// <summary>
        /// Instance of the request from client.
        /// it is a server side representation of HttpWebRequest.
        /// It is the same data, but instead of composing request we parse it.
        /// </summary>
        private HttpListenerRequest m_ClientRequest;

        /// <summary>
        /// Instance of the response to client.
        ///
        /// </summary>
        private HttpListenerResponse m_ResponseToClient;

        /// <summary>
        /// Internal constructor, used each time client connects.
        /// </summary>
        /// <param name="clientStream">The stream that is connected to the client. A stream is needed, to
        /// provide information about the connected client.
        /// See also the <see cref="System.Net.HttpListenerRequest"/> class.
        /// </param>
        /// <param name="httpListener">TBD</param>
        internal HttpListenerContext(OutputNetworkStreamWrapper clientStream, HttpListener httpListener)
        {
            // Saves the stream.
            this.m_clientOutputStream = clientStream;

            // Input stream does not own socket.
            this.m_clientInputStream = new InputNetworkStreamWrapper(clientStream.m_Stream, clientStream.m_Socket, false, null);

            // Constructs request and response classes.
            this.m_ClientRequest = new HttpListenerRequest(this.m_clientInputStream, httpListener.m_maxResponseHeadersLen);

            // Closing reponse to client causes removal from clientSocketsList.
            // Thus we need to pass clientSocketsList to client response.
            this.m_ResponseToClient = new HttpListenerResponse(this.m_clientOutputStream, httpListener);

            // There is incoming connection HTTP connection. Add new Socket to the list of connected sockets
            // The socket is removed from this array after correponding HttpListenerResponse is closed.
            httpListener.AddClientStream(this.m_clientOutputStream);

            // Set flag that HTTP request was not parsed yet.
            // It will be parsed on first access to m_ClientRequest or m_ResponseToClient
            this.m_IsHTTPRequestParsed = false;
        }

        public void Reset()
        {
            this.m_IsHTTPRequestParsed = false;
            this.m_ClientRequest.Reset();
        }

        /// <summary>
        /// Gets the <itemref>HttpListenerRequest</itemref> that represents a
        /// client's request for a resource.
        /// </summary>
        /// <value>An <itemref>HttpListenerRequest</itemref> object that
        /// represents the client request.</value>
        public HttpListenerRequest Request
        {
            get
            {
                if (!this.m_IsHTTPRequestParsed)
                {
                    this.m_ClientRequest.ParseHTTPRequest();
                    // After request parsed check for "transfer-ecoding" header. If it is chunked, change stream property.
                    // If m_EnableChunkedDecoding is set to true, then readig from stream automatically processing chunks.
                    var chunkedVal = this.m_ClientRequest.Headers[HttpKnownHeaderNames.TransferEncoding];
                    if (chunkedVal != null && chunkedVal.ToLower() == "chunked")
                    {
                        this.m_clientInputStream.m_EnableChunkedDecoding = true;
                    }

                    this.m_IsHTTPRequestParsed = true;
                }

                return this.m_ClientRequest;
            }
        }

        /// <summary>
        /// Gets the <itemref>HttpListenerResponse</itemref> object that will be
        /// sent to the client in response to the client's request.
        /// </summary>
        /// <value>An <itemref>HttpListenerResponse</itemref> object used to
        /// send a response back to the client.</value>
        public HttpListenerResponse Response
        {
            get
            {
                if (!this.m_IsHTTPRequestParsed)
                {
                    this.m_ClientRequest.ParseHTTPRequest();
                    this.m_IsHTTPRequestParsed = true;
                }

                return this.m_ResponseToClient;
            }
        }

        public void Close() => Close(-2);

        /// <summary>
        /// Closes the stream attached to this listener context. 
        /// </summary>
        public void Close(int lingerValue)
        {
            try
            {  
                if (this.m_clientOutputStream != null)
                {
                    try
                    {
                        if(this.m_clientOutputStream.m_Socket != null)
                        {
                            this.m_clientOutputStream.m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerValue);
                        }
                    }
                    catch{}
                }
                
                if (this.m_ResponseToClient != null)
                {
                    this.m_ResponseToClient.Close();
                    this.m_ResponseToClient = null;
                }
                
                // Close the underlying stream
                if (this.m_clientOutputStream != null)
                {
                    this.m_clientOutputStream.Dispose();
                    this.m_clientOutputStream = null;
                }
                
                if (this.m_clientInputStream != null)
                {
                    this.m_clientInputStream.Dispose();
                    this.m_clientInputStream = null;
                }
            }
            catch
            {
            }
        }
    }
}


