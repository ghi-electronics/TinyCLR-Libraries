using System;
using System.Text;
using System.IO;
using System.Collections;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFWsStack")]

namespace System.IO
{
    public class StreamReader : TextReader
    {
        private const int c_MaxReadLineLen = 0xFFFF;
        private const int c_BufferSize = 512;

        // Larger chunk used only in ReadNonSeekableStream.
        // Why not reuse c_BufferSize (512): on a body of N chars, the old
        // 512-char chunks produced ceil(N/512) ArrayList entries — for a
        // ~80 KB response that's ~160 small char[] objects, each with its
        // own GC header. The cumulative fragmentation made it impossible
        // to allocate the final contiguous char[N] concat buffer on devices
        // without external heap (failed at ~161 KB with the heap holding
        // plenty of total free space — just no contiguous slab). 4096-char
        // (8 KB) chunks cost the same total bytes but produce ~8× fewer
        // allocations and ~8× fewer holes, so the final concat slab fits.
        private const int c_ReadToEndChunkSize = 4096;

        //--//

        private Stream m_stream;
        // Initialized in constructor by CurrentEncoding scheme.
        // Encoding can be changed by resetting this variable.
        Decoder m_decoder;

        // Temporary buffer used for decoder in Read() function.
        // Made it class member to save creation of buffer on each call to  Read()
        // Initialized in StreamReader(String path)
        char[] m_singleCharBuff;

        // Disposeed flag
        private bool m_disposed;

        // Internal stream read buffer
        private byte[] m_buffer;
        private int m_curBufPos;
        private int m_curBufLen;

        public StreamReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException();
            }

            this.m_singleCharBuff = new char[1];
            this.m_buffer = new byte[c_BufferSize];
            this.m_curBufPos = 0;
            this.m_curBufLen = 0;
            this.m_stream = stream;
            this.m_decoder = this.CurrentEncoding.GetDecoder();
            this.m_disposed = false;
        }

        public StreamReader(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        public override void Close() => Dispose();

        protected override void Dispose(bool disposing)
        {
            if (this.m_stream != null)
            {
                if (disposing)
                {
                    this.m_stream.Close();
                }

                this.m_stream = null;
                this.m_buffer = null;
                this.m_curBufPos = 0;
                this.m_curBufLen = 0;
            }

            this.m_disposed = true;
        }

        public override int Peek()
        {
            var tempPos = this.m_curBufPos;
            int nextChar;

            // If buffer need refresh take into account max UTF8 bytes if the next character is UTF8 encoded
            // Note: In some occasions, m_curBufPos may go beyond m_curBufLen-1 (for example, when trying to peek after reading the last character of the buffer), so we need to refresh the buffer in these cases too
            if ((this.m_curBufPos                       >= (this.m_curBufLen -1)) ||
               ((this.m_buffer[this.m_curBufPos + 1] & 0x80) !=  0 &&
                (this.m_curBufPos + 3                   >= this.m_curBufLen)))
            {
                // Move any bytes read for this character to front of new buffer
                int totRead;
                for (totRead = 0; totRead < this.m_curBufLen - this.m_curBufPos; ++totRead)
                {
                    this.m_buffer[totRead] = this.m_buffer[this.m_curBufPos + totRead];
                }

                // Get the new buffer
                try
                {
                    // Standard .NET pattern: ask Stream.Read for the full
                    // remaining buffer space and let the stream return what
                    // it has. The previous `m_stream.Length` guard threw
                    // NotSupportedException on BCL NetworkStream/SslStream.
                    while (totRead < this.m_buffer.Length)
                    {
                        var len = this.m_buffer.Length - totRead;

                        len = this.m_stream.Read(this.m_buffer, totRead, len);

                        if (len <= 0) break;

                        totRead += len;
                    }
                }
                catch (Exception e)
                {
                    throw new IOException("m_stream.Read", e);
                }

                tempPos = 0;
                this.m_curBufPos = 0;
                this.m_curBufLen = totRead;
            }

            // Get the next character and reset m_curBufPos
            nextChar = Read();
            this.m_curBufPos = tempPos;
            return nextChar;
        }

        public override int Read()
        {
            var completed = false;

            while (true)
            {
                this.m_decoder.Convert(this.m_buffer, this.m_curBufPos, this.m_curBufLen - this.m_curBufPos, this.m_singleCharBuff, 0, 1, false, out var byteUsed, out var charUsed, out completed);

                this.m_curBufPos += byteUsed;

                if (charUsed == 1) // done
                {
                    break;
                }
                else
                {
                    // get more data to feed the decoder and try again.
                    // Ask for the full buffer; Stream.Read returns whatever
                    // it has (>=1 for blocking streams, 0 for EOF). Previous
                    // `m_stream.Length` cap threw on BCL network streams.
                    var readCount = this.m_buffer.Length;

                    // If there is no data, then return -1
                    if (FillBufferAndReset(readCount) == 0)
                    {
                        return -1;
                    }
                }
            }

            return (int)this.m_singleCharBuff[0];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (count < 0)
                throw new ArgumentOutOfRangeException();
            if (buffer.Length - index < count)
                throw new ArgumentException();
            if (this.m_disposed == true)
                throw new ObjectDisposedException("");

            var charUsed = 0;
            var completed = false;

            if (this.m_curBufLen == 0) FillBufferAndReset(count);

            var offset = 0;

            while (true)
            {
                this.m_decoder.Convert(this.m_buffer, this.m_curBufPos, this.m_curBufLen - this.m_curBufPos, buffer, offset, count, false, out var byteUsed, out charUsed, out completed);

                count -= charUsed;
                this.m_curBufPos += byteUsed;
                offset += charUsed;

                if (count == 0 || (FillBufferAndReset(count) == 0))
                {
                    break;
                }
            }

            return charUsed;
        }

        public override string ReadLine()
        {

            var bufLen = c_BufferSize;
            var readLineBuff = new char[bufLen];
            var growSize = c_BufferSize;
            var curPos = 0;
            int newChar;
            var startPos = this.m_curBufPos;

            // Look for \r\n
            while ((newChar = Read()) != -1)
            {
                // Grow the line buffer if needed
                if (curPos == bufLen)
                {
                    if (bufLen + growSize > c_MaxReadLineLen)
                        throw new Exception();

                    var tempBuf = new char[bufLen + growSize];
                    Array.Copy(readLineBuff, 0, tempBuf, 0, bufLen);
                    readLineBuff = tempBuf;
                    bufLen += growSize;
                }

                // Store the new character
                readLineBuff[curPos] = (char)newChar;

                if (readLineBuff[curPos] == '\n')
                {
                    return new string(readLineBuff, 0, curPos);
                }

                // Check for \r and \r\n
                if (readLineBuff[curPos] == '\r')
                {
                    // If the next character is \n eat it
                    if (Peek() == '\n')
                        Read();

                    return new string(readLineBuff, 0, curPos);
                }

                // Move to the next byte
                ++curPos;
            }

            // Reached end of stream. Send line upto EOS
            if(curPos == 0) return null;

            return new string(readLineBuff, 0, curPos);
        }

        public override string ReadToEnd()
        {
            char[] result = null;

            if (this.m_stream.CanSeek)
            {
                result = ReadSeekableStream();
            }
            else
            {
                result = ReadNonSeekableStream();
            }

            return new string(result);
        }

        private char[] ReadSeekableStream()
        {
            var chars = new char[(int)this.m_stream.Length];

            var read = Read(chars, 0, chars.Length);

            return chars;
        }

        private char[] ReadNonSeekableStream()
        {
            // Larger 8 KB chunks (vs the old 512-byte ones) keep the
            // small-allocation count low so the heap stays defragmented
            // enough to allocate the final concat buffer below — critical
            // on devices without external heap. See c_ReadToEndChunkSize.
            //
            // Feed each chunk by calling Read() in c_BufferSize-sized
            // batches into a small reusable temp buffer, then copy into
            // the current chunk at the running write position. Why not
            // call Read(chunk, 0, chunk.Length) directly: the underlying
            // Read overload returns the *last Convert iteration's* char
            // count rather than the cumulative total when count exceeds
            // the internal m_buffer size — for ASCII bodies > 512 chars
            // it would silently truncate to the tail's char count. Reading
            // in m_buffer-sized batches keeps Read on its single-iteration
            // happy path so the returned count is reliable.
            var buffers = new ArrayList();
            var totalRead = 0;
            int chunkRead;
            var temp = new char[c_BufferSize];

            do
            {
                var chunk = new char[c_ReadToEndChunkSize];
                chunkRead = 0;
                while (chunkRead < chunk.Length)
                {
                    var n = Read(temp, 0, temp.Length);
                    if (n <= 0) break;
                    Array.Copy(temp, 0, chunk, chunkRead, n);
                    chunkRead += n;
                }
                if (chunkRead > 0)
                {
                    buffers.Add(chunk);
                    totalRead += chunkRead;
                }
            }
            while (chunkRead == c_ReadToEndChunkSize);

            if (totalRead == 0) return new char[0];

            if (buffers.Count == 1)
            {
                var only = (char[])buffers[0];
                if (totalRead == only.Length) return only;

                // Single partial chunk — trim to exact length.
                var trimmed = new char[totalRead];
                Array.Copy(only, 0, trimmed, 0, totalRead);
                return trimmed;
            }

            // Multi-chunk concat. Null each source slot as we copy from it
            // so the GC has a chance to reclaim individual chunks if the
            // caller's `new string(...)` later triggers GC under pressure.
            var text = new char[totalRead];
            var pos = 0;
            var count = buffers.Count;
            for (var i = 0; i < count; i++)
            {
                var buf = (char[])buffers[i];
                var copyLen = totalRead - pos;
                if (copyLen > buf.Length) copyLen = buf.Length;
                Array.Copy(buf, 0, text, pos, copyLen);
                pos += copyLen;
                buffers[i] = null;
            }
            return text;
        }

        //--//

        public virtual Stream BaseStream => this.m_stream;

        public virtual Encoding CurrentEncoding => System.Text.Encoding.UTF8;

        public bool EndOfStream => this.m_curBufLen == this.m_curBufPos;

        private int FillBufferAndReset(int count)
        {
            if (this.m_curBufPos != 0) Reset();

            var totalRead = 0;

            try
            {
                while (count > 0 && this.m_curBufLen < this.m_buffer.Length)
                {
                    var spaceLeft = this.m_buffer.Length - this.m_curBufLen;

                    if (count > spaceLeft) count = spaceLeft;

                    var read = this.m_stream.Read(this.m_buffer, this.m_curBufLen, count);

                    if (read <= 0) break;

                    totalRead += read;
                    this.m_curBufLen += read;

                    count -= read;
                }
            }
            catch (Exception e)
            {
                throw new IOException("m_stream.Read", e);
            }

            return totalRead;
        }

        private void Reset()
        {
            var bytesAvailable = this.m_curBufLen - this.m_curBufPos;

            // here we trust that the copy in place doe not overwrites data
            Array.Copy(this.m_buffer, this.m_curBufPos, this.m_buffer, 0, bytesAvailable);

            this.m_curBufPos = 0;
            this.m_curBufLen = bytesAvailable;
        }
    }
}


