using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFWsStack")]

namespace System.IO {
    /// <summary>Writes characters to a stream using UTF-8 encoding.</summary>
    public class StreamWriter : TextWriter
    {
        private Stream m_stream;
        private bool m_disposed;
        private byte[] m_buffer;

        private int m_curBufPos;

        private const string c_NewLine = "\r\n";
        private const int c_BufferSize = 0xFFF;

        //--//

        /// <summary>Creates a writer over the given stream.</summary>
        public StreamWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException();
            }

            this.m_stream = stream;
            this.m_buffer = new byte[c_BufferSize];
            this.m_curBufPos = 0;
            this.m_disposed = false;
        }

        /// <summary>Creates a writer that creates or overwrites the file at the given path.</summary>
        public StreamWriter(string path)
            : this(path, false)
        {
        }

        /// <summary>Creates a writer for the file at the given path, optionally appending to it.</summary>
        public StreamWriter(string path, bool append)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
        {
        }

        /// <inheritdoc/>
        public override void Close() => Dispose();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.m_stream != null)
            {
                if (disposing)
                {
                    try
                    {
                        if (this.m_stream.CanWrite)
                        {
                            Flush();
                        }
                    }
                    catch { }

                    try
                    {
                        this.m_stream.Close();
                    }
                    catch {}
                }

                this.m_stream = null;
                this.m_buffer = null;
                this.m_curBufPos = 0;
            }

            this.m_disposed = true;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            if (this.m_disposed) throw new ObjectDisposedException();

            if (this.m_curBufPos > 0)
            {
                try
                {
                    this.m_stream.Write(this.m_buffer, 0, this.m_curBufPos);
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter Flush. ", e);
                }

                this.m_curBufPos = 0;
            }
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            var buffer = this.Encoding.GetBytes(value.ToString());

            WriteBytes(buffer, 0, buffer.Length);
        }

        /// <inheritdoc/>
        public override void WriteLine()
        {
            var tempBuf = this.Encoding.GetBytes(c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        /// <inheritdoc/>
        public override void WriteLine(string value)
        {
            var tempBuf = this.Encoding.GetBytes(value + c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        /// <summary>The underlying stream being written to.</summary>
        public virtual Stream BaseStream => this.m_stream;

        /// <inheritdoc/>
        public override Encoding Encoding => System.Text.Encoding.UTF8;

        //--//

        internal void WriteBytes(byte[] buffer, int index, int count)
        {
            if (this.m_disposed) throw new ObjectDisposedException();

            // If this write will overrun the buffer flush the current buffer to stream and
            // write remaining bytes directly to stream.
            if (this.m_curBufPos + count >= c_BufferSize)
            {
                // Flush the current buffer to the stream and write new bytes
                // directly to stream.
                try
                {
                    this.m_stream.Write(this.m_buffer, 0, this.m_curBufPos);
                    this.m_curBufPos = 0;

                    this.m_stream.Write(buffer, index, count);
                    return;
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter WriteBytes. ", e);
                }
            }

            // Else add bytes to the internal buffer
            Array.Copy(buffer, index, this.m_buffer, this.m_curBufPos, count);

            this.m_curBufPos += count;

            return;
        }
    }
}


