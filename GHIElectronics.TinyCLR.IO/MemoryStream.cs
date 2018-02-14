using System;

namespace System.IO
{
    public class MemoryStream : Stream
    {
        private byte[] _buffer;    // Either allocated internally or externally.
        private int _origin;       // For user-provided arrays, start at this origin
        private int _position;     // read/write head.
        private int _length;       // Number of bytes within the memory stream
        private int _capacity;     // length of usable portion of buffer for stream
        private bool _expandable;  // User-provided buffers aren't expandable.
        private bool _isOpen;      // Is this stream open or closed?

        private const int MemStreamMaxLength = 0xFFFF;

        public MemoryStream()
        {
            this._buffer = new byte[256];
            this._capacity = 256;
            this._expandable = true;
            this._origin = 0;      // Must be 0 for byte[]'s created by MemoryStream
            this._isOpen = true;
        }

        public MemoryStream(byte[] buffer)
        {
            this._buffer = buffer ?? throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            this._length = this._capacity = buffer.Length;
            this._expandable = false;
            this._origin = 0;
            this._isOpen = true;
        }

        public override bool CanRead => this._isOpen;

        public override bool CanSeek => this._isOpen;

        public override bool CanWrite => this._isOpen;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._isOpen = false;
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value)
        {
            if (value > this._capacity)
            {
                var newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                if (newCapacity < this._capacity * 2)
                    newCapacity = this._capacity * 2;

                if (!this._expandable && newCapacity > this._capacity) throw new NotSupportedException();
                if (newCapacity > 0)
                {
                    var newBuffer = new byte[newCapacity];
                    if (this._length > 0) Array.Copy(this._buffer, 0, newBuffer, 0, this._length);
                    this._buffer = newBuffer;
                }
                else
                {
                    this._buffer = null;
                }

                this._capacity = newCapacity;

                return true;
            }

            return false;
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get
            {
                if (!this._isOpen) throw new ObjectDisposedException();
                return this._length - this._origin;
            }
        }

        public override long Position
        {
            get
            {
                if (!this._isOpen) throw new ObjectDisposedException();
                return this._position - this._origin;
            }

            set
            {
                if (!this._isOpen) throw new ObjectDisposedException();
                if (value < 0 || value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
                this._position = this._origin + (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            var n = this._length - this._position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            Array.Copy(this._buffer, this._position, buffer, offset, n);
            this._position += n;
            return n;
        }

        public override int ReadByte()
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (this._position >= this._length) return -1;
            return this._buffer[this._position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    this._position = this._origin + (int)offset;
                    break;

                case SeekOrigin.Current:
                    if (offset + this._position < this._origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    this._position += (int)offset;
                    break;

                case SeekOrigin.End:
                    if (this._length + offset < this._origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    this._position = this._length + (int)offset;
                    break;

                default:
                    throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidSeekOrigin")*/);
            }

            return this._position;
        }

        /*
         * Sets the length of the stream to a given value.  The new
         * value must be nonnegative and less than the space remaining in
         * the array, <var>Int32.MaxValue</var> - <var>origin</var>
         * Origin is 0 in all cases other than a MemoryStream created on
         * top of an existing array and a specific starting offset was passed
         * into the MemoryStream constructor.  The upper bounds prevents any
         * situations where a stream may be created on top of an array then
         * the stream is made longer than the maximum possible length of the
         * array (<var>Int32.MaxValue</var>).
         *
         * @exception ArgumentException Thrown if value is negative or is
         * greater than Int32.MaxValue - the origin
         * @exception NotSupportedException Thrown if the stream is readonly.
         */
        public override void SetLength(long value)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (value > MemStreamMaxLength || value < 0)
                throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);

            var newLength = this._origin + (int)value;
            var allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > this._length)
                Array.Clear(this._buffer, this._length, newLength - this._length);
            this._length = newLength;
            if (this._position > newLength) this._position = newLength;
        }

        public virtual byte[] ToArray()
        {
            var copy = new byte[this._length - this._origin];
            Array.Copy(this._buffer, this._origin, copy, 0, this._length - this._origin);
            return copy;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            var i = this._position + count;
            // Check for overflow

            if (i > this._length)
            {
                if (i > this._capacity) EnsureCapacity(i);
                this._length = i;
            }

            Array.Copy(buffer, offset, this._buffer, this._position, count);
            this._position = i;
            return;
        }

        public override void WriteByte(byte value)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (this._position >= this._capacity)
            {
                EnsureCapacity(this._position + 1);
            }

            this._buffer[this._position++] = value;

            if (this._position > this._length)
            {
                this._length = this._position;
            }
        }

        /*
         * Writes this MemoryStream to another stream.
         * @param stream Stream to write into
         * @exception ArgumentNullException if stream is null.
         */
        public virtual void WriteTo(Stream stream)
        {
            if (!this._isOpen) throw new ObjectDisposedException();

            if (stream == null)
                throw new ArgumentNullException(/*"stream", Environment.GetResourceString("ArgumentNull_Stream")*/);
            stream.Write(this._buffer, this._origin, this._length - this._origin);
        }
    }
}


