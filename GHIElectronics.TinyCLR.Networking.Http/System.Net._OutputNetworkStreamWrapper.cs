////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;
    using System.IO;

    /// <summary>
    /// The OutputNetworkStreamWrapper is used to re-implement calls to  NetworkStream.Write
    /// On first write HttpListenerResponse needs to send HTTP headers to client.
    /// </summary>
    internal class OutputNetworkStreamWrapper : Stream
    {

        /// <summary>
        /// This is a socket connected to client.
        /// OutputNetworkStreamWrapper owns the socket, not NetworkStream.
        /// If connection is persistent, then the m_Socket is transferred to the list of
        /// </summary>
        internal Socket m_Socket;

        /// <summary>
        /// Actual network or SSL stream connected to the client.
        /// It could be SSL stream, so NetworkStream is not exact type, m_Stream would be derived from NetworkStream
        /// </summary>
        internal NetworkStream m_Stream;

        /// <summary>
        /// Type definition of delegate for sending of HTTP headers.
        /// </summary>
        internal delegate void SendHeadersDelegate();

        /// <summary>
        /// If not null - indicates whether we have sent headers or not.
        /// Calling of delegete sends HTTP headers to client - HttpListenerResponse.SendHeaders()
        /// </summary>
        private SendHeadersDelegate m_headersSend;

        /// <summary>
        /// Just passes parameters to the base.
        /// Socket is not owned by base NetworkStream
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="stream"></param>
        public OutputNetworkStreamWrapper(Socket socket, NetworkStream stream)
        {
            this.m_Socket = socket;
            this.m_Stream = stream;
        }

        /// <summary>
        /// Sets the delegate for sending of headers.
        /// </summary>
        internal SendHeadersDelegate HeadersDelegate { set => this.m_headersSend = value; }

        /// <summary>
        /// Return true if stream support reading.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Return true if stream supports seeking
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Return true if timeout is applicable to the stream
        /// </summary>
        public override bool CanTimeout => this.m_Stream.CanTimeout;

        /// <summary>
        /// Return true if stream support writing. It should be true, as this is output stream.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the length of the data available on the stream.
        /// Since this is output stream reading is not allowed and length does not have meaning.
        /// </summary>
        /// <returns>The length of the data available on the stream.</returns>
        public override long Length => throw new NotSupportedException();

        /// <summary>
        /// Position is not supported for NetworkStream
        /// </summary>
        public override long Position {
            get => throw new NotSupportedException();

            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Timeout for read operations.
        /// </summary>
        public override int ReadTimeout {
            get => this.m_Stream.ReadTimeout;
            set => this.m_Stream.ReadTimeout = value;
        }

        /// <summary>
        /// Timeout for write operations.
        /// </summary>
        public override int WriteTimeout {
            get => this.m_Stream.WriteTimeout;
            set => this.m_Stream.WriteTimeout = value;
        }

        /// <summary>
        /// Closes the stream. Verifies that HTTP response is sent before closing.
        /// </summary>
        public override void Close()
        {
            if (this.m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                this.m_headersSend();
            }

            this.m_Stream.Close();
            this.m_Stream = null;
            this.m_Socket = null;
        }

        /// <summary>
        /// Flushes the stream. Verifies that HTTP response is sent before flushing.
        /// </summary>
        public override void Flush()
        {
            if (this.m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                this.m_headersSend();
            }

            this.m_Stream.Flush();
        }

        /// <summary>
        /// This putput stream, so read is not supported.
        /// </summary>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>
        /// This putput stream, so read is not supported.
        /// </summary>
        /// <returns></returns>
        public override int ReadByte() => throw new NotSupportedException();

        /// <summary>
        /// Seeking is not suported on network streams
        /// </summary>
        /// <param name="offset">Offset to seek</param>
        /// <param name="origin">Relative origin of the seek</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <summary>
        /// Setting length is not suported on network streams
        /// </summary>
        /// <param name="value">Length to set</param>
        /// <returns></returns>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// Writes single byte to the stream.
        /// </summary>
        /// <param name="value">Byte value to write.</param>
        public override void WriteByte(byte value) => this.m_Stream.WriteByte(value);

        /// <summary>
        /// Re-implements writing of data to network stream.
        /// The only functionality - on first write it sends HTTP headers.
        /// Then calls base
        /// </summary>
        /// <param name="buffer">Buffer with data to write to HTTP client</param>
        /// <param name="offset">Offset at which to use data from buffer</param>
        /// <param name="size">Count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int size)
        {
            if (this.m_headersSend != null)
            {
                // Calls HttpListenerResponse.SendHeaders. HttpListenerResponse.SendHeaders sets m_headersSend to null.
                this.m_headersSend();
            }

            this.m_Stream.Write(buffer, offset, size);
        }
    }
}


