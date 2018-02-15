using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFWsStack")]

namespace System.IO {
    public class StreamWriter : TextWriter
    {
        private Stream m_stream;
        private bool m_disposed;
        private byte[] m_buffer;

        private int m_curBufPos;

        private const string c_NewLine = "\r\n";
        private const int c_BufferSize = 0xFFF;

        //--//

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

        public StreamWriter(string path)
            : this(path, false)
        {
        }

        public StreamWriter(string path, bool append)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
        {
        }

        public override void Close() => Dispose();

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

        public override void Write(char value)
        {
            var buffer = this.Encoding.GetBytes(value.ToString());

            WriteBytes(buffer, 0, buffer.Length);
        }

        public override void WriteLine()
        {
            var tempBuf = this.Encoding.GetBytes(c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        public override void WriteLine(string value)
        {
            var tempBuf = this.Encoding.GetBytes(value + c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        public virtual Stream BaseStream => this.m_stream;

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


