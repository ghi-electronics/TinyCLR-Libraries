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

using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public partial class TinyFileSystem
    {
        /// <summary>
        /// Provides a stream for a file in the Tiny File System.
        /// </summary>
        public class TinyFileStream : Stream
        {
            private TinyFileSystem tfs;
            private FileRef fileRef;
            private long filePointer;

            /// <summary>
            /// Creates an instance of TinyFileStream.
            /// </summary>
            /// <param name="fileSystem">TinyFileSystem instance on which the file this stream exposes exists.</param>
            /// <param name="fileRef">The file to expose through the TinyFileStream.</param>
            /// <remarks>
            /// Instances of this class should never be created directly. The should be created by calls to Create or Open
            /// on the TinyFileSystem instance.
            /// </remarks>
            internal TinyFileStream(TinyFileSystem fileSystem, FileRef fileRef)
            {
                this.tfs = fileSystem;
                this.fileRef = fileRef;
                this.filePointer = 0;
                this.fileRef.openCount++;
            }

            /// <summary>
            /// Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead => true;

            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek => true;

            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite => true;

            /// <summary>
            /// Gets length of bytes in the stream.
            /// </summary>
            public override long Length
            {
                get
                {
                    this.CheckState();
                    return this.fileRef.fileSize;
                }
            }

            /// <summary>
            /// Gets or sets the current possition in the stream.
            /// </summary>
            public override long Position {
                get {
                    this.CheckState();
                    return this.filePointer;
                }
                set => this.Seek(value, SeekOrigin.Begin);
            }

            /// <summary>
            /// Writes unwritten data to the file.
            /// </summary>
            public override void Flush() => this.CheckState();

            /// <summary>
            /// Reads a block of bytes from the stream.
            /// </summary>
            /// <param name="array">The array to fill with the data read from the file.</param>
            /// <param name="offset">The byte offset in the array at which read bytes will be placed.</param>
            /// <param name="count">The maximun number of bytes to read.</param>
            /// <returns></returns>
            public override int Read(byte[] array, int offset, int count)
            {
                this.CheckState();

                if (array == null) throw new ArgumentNullException("data");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset");
                if (count < 0) throw new ArgumentOutOfRangeException("count");
                if (array.Length - offset < count) throw new ArgumentOutOfRangeException("count");

                var bytesRead = this.tfs.Read(this.fileRef, this.filePointer, array, offset, count);
                this.filePointer += bytesRead;
                return bytesRead;
            }

            /// <summary>
            /// Sets the current position of this stream to a given value.
            /// </summary>
            /// <param name="offset">The offset of the positon relative to the origin.</param>
            /// <param name="origin">Specified the beginning, end or current postion as a reference point to apply the offset.</param>
            /// <returns>The new postion in the stream.</returns>    
            public override long Seek(long offset, SeekOrigin origin)
            {
                this.CheckState();

                var newFilePointer = this.filePointer;

                switch (origin)
                {
                    case SeekOrigin.Begin:
                        newFilePointer = offset;
                        break;
                    case SeekOrigin.End:
                        newFilePointer = this.fileRef.fileSize + offset;
                        break;
                    case SeekOrigin.Current:
                        newFilePointer = this.filePointer + offset;
                        break;
                }

                if (newFilePointer < 0 || newFilePointer > this.fileRef.fileSize)
                    throw new IOException(StringTable.Error_OutOfBounds, (int) IOException.IOExceptionErrorCode.Others);

                this.filePointer = newFilePointer;

                return this.filePointer;
            }

            /// <summary>
            /// Sets the length of this stream to a given value.
            /// </summary>
            /// <param name="value">The new length of the stream</param>
            /// <remarks>
            /// If the length is less than the current length of the stream, the stream is truncated.
            /// </remarks>
            public override void SetLength(long value)
            {
                this.CheckState();
                if (value < 0 || value > this.fileRef.fileSize)
                {
                    throw new IOException(StringTable.Error_OutOfBounds, (int) IOException.IOExceptionErrorCode.Others);
                }
                this.Flush();
                this.tfs.Truncate(this.fileRef, value);
                this.Seek(0, SeekOrigin.End);
            }

            /// <summary>
            /// Writes a block of bytes to the file stream.
            /// </summary>
            /// <param name="array">The buffer containing the data to write to the stream.</param>
            /// <param name="offset">The byte offset in the array from which to start writing bytes to the stream.</param>
            /// <param name="count">The number of bytes to write.</param>
            public override void Write(byte[] array, int offset, int count)
            {
                this.CheckState();
                if (array == null) throw new ArgumentNullException("data");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset");
                if (count < 0) throw new ArgumentOutOfRangeException("count");
                if (array.Length - offset < count) throw new ArgumentOutOfRangeException("count");

                this.tfs.Write(this.fileRef, this.filePointer, array, offset, count);
                this.filePointer += count;
            }

            /// <summary>
            /// Dispose the TinyFileStream.
            /// </summary>
            /// <param name="disposing">true if being disposed from a call to Dispose otherwise false if called from the finalizer.</param>
            protected override void Dispose(bool disposing)
            {
                if (this.fileRef != null)
                {
                    this.fileRef.openCount--;
                    this.fileRef = null;
                    this.tfs = null;
                }

                base.Dispose(disposing);
            }

            private void CheckState()
            {
                if (this.tfs == null || this.fileRef == null || (this.fileRef != null && this.fileRef.openCount == 0))
                    throw new ObjectDisposedException(StringTable.Error_FileClosed);
            }
        }
    }
}
