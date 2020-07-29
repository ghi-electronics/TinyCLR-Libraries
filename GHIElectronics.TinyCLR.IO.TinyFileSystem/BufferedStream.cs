/*
 * TinyFileSystem driver for TinyCLR 2.0
 * 
 * Version 1.0
 *  - Initial revision, based on Chris Taylor (Taylorza) work
 *  - adaptations to conform to MikroBus.Net drivers design
 *  
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

// System.IO.BufferedStream
// Updated to work with .NET Micro Framework
// Changes by:
//     Chris Taylor (taylorza) - 26/08/2012

// System.IO.BufferedStream
//
// Author:
//   Matt Kimball (matt@kimball.net)
//   Ville Palo <vi64pa@kolumbus.fi>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public partial class TinyFileSystem
    {
        /// <summary>
        /// Main class for the BufferedStream object.
        /// </summary>
        internal sealed class BufferedStream : Stream
        {
            private readonly Stream mStream;
            private byte[] mBuffer;
            private int mBufferPos;
            private int mBufferReadAhead;
            private bool mBufferReading;
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedStream"/> class with a default buffer size of 4096 bytes.
            /// </summary>
            /// <param name="stream">The stream.</param>
            public BufferedStream(Stream stream)
                : this(stream, 4096)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedStream"/> class.
            /// </summary>
            /// <param name="stream">The stream.</param>
            /// <param name="bufferSize">The buffer_size.</param>
            /// <exception cref="System.ArgumentNullException">stream</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">buffer_size &lt;= 0</exception>
            /// <exception cref="System.ObjectDisposedException">Cannot access a closed Stream.</exception>
            public BufferedStream(Stream stream, int bufferSize)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");
                // LAMESPEC: documented as < 0
                if (bufferSize <= 0)
                    throw new ArgumentOutOfRangeException("bufferSize", "<= 0");
                if (!stream.CanRead && !stream.CanWrite)
                {
                    throw new ObjectDisposedException("Cannot access a closed Stream.");
                }

                this.mStream = stream;
                this.mBuffer = new byte[bufferSize];
            }

            public override bool CanRead => this.mStream.CanRead;

            public override bool CanWrite => this.mStream.CanWrite;

            public override bool CanSeek => this.mStream.CanSeek;

            public override long Length
            {
                get
                {
                    this.Flush();
                    return this.mStream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    this.CheckObjectDisposedException();
                    return this.mStream.Position - this.mBufferReadAhead + this.mBufferPos;
                }

                set
                {
                    if (value < this.Position && (this.Position - value <= this.mBufferPos) && this.mBufferReading)
                    {
                        this.mBufferPos -= (int) (this.Position - value);
                    }
                    else if (value > this.Position && (value - this.Position < this.mBufferReadAhead - this.mBufferPos) &&
                             this.mBufferReading)
                    {
                        this.mBufferPos += (int) (value - this.Position);
                    }
                    else
                    {
                        this.Flush();
                        this.mStream.Position = value;
                    }
                }
            }

            public override void Close()
            {
                try
                {
                    if (this.mBuffer != null)
                        this.Flush();
                }
                finally
                {
                    this.mStream.Close();
                    this.mBuffer = null;
                    this.disposed = true;
                }
            }

            public override void Flush()
            {
                this.CheckObjectDisposedException();

                if (this.mBufferReading)
                {
                    if (this.CanSeek)
                        this.mStream.Position = this.Position;
                }
                else if (this.mBufferPos > 0)
                {
                    this.mStream.Write(this.mBuffer, 0, this.mBufferPos);
                }

                this.mBufferReadAhead = 0;
                this.mBufferPos = 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                this.CheckObjectDisposedException();
                if (!this.CanSeek)
                {
                    throw new NotSupportedException("Non seekable stream.");
                }
                this.Flush();
                return this.mStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.CheckObjectDisposedException();

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                if (!this.mStream.CanWrite && !this.mStream.CanSeek)
                    throw new NotSupportedException("the stream cannot seek nor write.");

                if ((this.mStream == null) || (!this.mStream.CanRead && !this.mStream.CanWrite))
                    throw new IOException("the stream is not open");

                this.mStream.SetLength(value);
                if (this.Position > value)
                    this.Position = value;
            }

            public override int ReadByte()
            {
                this.CheckObjectDisposedException();

                var b = new byte[1];

                return this.Read(b, 0, 1) == 1 ? b[0] : -1;
            }

            public override void WriteByte(byte value)
            {
                this.CheckObjectDisposedException();
                var b = new byte[1];

                b[0] = value;
                this.Write(b, 0, 1);
            }

            public override int Read(byte[] array, int offset, int count)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                this.CheckObjectDisposedException();
                if (!this.mStream.CanRead)
                {
                    throw new NotSupportedException("Cannot read from stream");
                }
                if (offset < 0)
                    throw new ArgumentOutOfRangeException("offset", "< 0");
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "< 0");
                // re-ordered to avoid possible integer overflow
                if (array.Length - offset < count)
                    throw new ArgumentException("array.Length - offset < count");

                if (!this.mBufferReading)
                {
                    this.Flush();
                    this.mBufferReading = true;
                }

                if (count <= this.mBufferReadAhead - this.mBufferPos)
                {
                    Array.Copy(this.mBuffer, this.mBufferPos, array, offset, count);

                    this.mBufferPos += count;
                    if (this.mBufferPos == this.mBufferReadAhead)
                    {
                        this.mBufferPos = 0;
                        this.mBufferReadAhead = 0;
                    }

                    return count;
                }

                var ret = this.mBufferReadAhead - this.mBufferPos;
                Array.Copy(this.mBuffer, this.mBufferPos, array, offset, ret);
                this.mBufferPos = 0;
                this.mBufferReadAhead = 0;
                offset += ret;
                count -= ret;

                if (count >= this.mBuffer.Length)
                {
                    ret += this.mStream.Read(array, offset, count);
                }
                else
                {
                    this.mBufferReadAhead = this.mStream.Read(this.mBuffer, 0, this.mBuffer.Length);

                    if (count < this.mBufferReadAhead)
                    {
                        Array.Copy(this.mBuffer, 0, array, offset, count);
                        this.mBufferPos = count;
                        ret += count;
                    }
                    else
                    {
                        Array.Copy(this.mBuffer, 0, array, offset, this.mBufferReadAhead);
                        ret += this.mBufferReadAhead;
                        this.mBufferReadAhead = 0;
                    }
                }

                return ret;
            }

            public override void Write(byte[] array, int offset, int count)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                this.CheckObjectDisposedException();
                if (!this.mStream.CanWrite)
                {
                    throw new NotSupportedException("Cannot write to stream");
                }
                if (offset < 0)
                    throw new ArgumentOutOfRangeException("offset", "< 0");
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "< 0");
                // avoid possible integer overflow
                if (array.Length - offset < count)
                    throw new ArgumentException("array.Length - offset < count");

                if (this.mBufferReading)
                {
                    this.Flush();
                    this.mBufferReading = false;
                }

                // reordered to avoid possible integer overflow
                if (this.mBufferPos >= this.mBuffer.Length - count)
                {
                    this.Flush();
                    this.mStream.Write(array, offset, count);
                }
                else
                {
                    Array.Copy(array, offset, this.mBuffer, this.mBufferPos, count);
                    this.mBufferPos += count;
                }
            }

            private void CheckObjectDisposedException()
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("Stream is closed");
                }
            }
        }
    }
}
