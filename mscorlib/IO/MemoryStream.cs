// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  MemoryStream
**
** <OWNER>Microsoft</OWNER>
**
**
** Purpose: A Stream whose backing store is memory.  Great
** for temporary storage without creating a temp file.  Also
** lets users expose a byte[] as a stream.
**
**
===========================================================*/

using System.Runtime.InteropServices;
#if FEATURE_ASYNC_IO
using System.Threading;
using System.Threading.Tasks;
using System.Security.Permissions;
#endif

namespace System.IO {
    // A MemoryStream represents a Stream in memory (ie, it has no backing store).
    // This stream may reduce the need for temporary buffers and files in
    // an application.
    //
    // There are two ways to create a MemoryStream.  You can initialize one
    // from an unsigned byte array, or you can create an empty one.  Empty
    // memory streams are resizable, while ones created with a byte array provide
    // a stream "view" of the data.
    [Serializable]
    [ComVisible(true)]
    public class MemoryStream : Stream {
        private byte[] _buffer;    // Either allocated internally or externally.
        private int _origin;       // For user-provided arrays, start at this origin
        private int _position;     // read/write head.
        //[ContractPublicPropertyName("Length")]
        private int _length;       // Number of bytes within the memory stream
        private int _capacity;     // length of usable portion of buffer for stream
                                   // Note that _capacity == _buffer.Length for non-user-provided byte[]'s

        private bool _expandable;  // User-provided buffers aren't expandable.
        private bool _writable;    // Can user write to this stream?
        private bool _exposable;   // Whether the array can be returned to the user.
        private bool _isOpen;      // Is this stream open or closed?

#if FEATURE_ASYNC_IO
        [NonSerialized]
        private Task<int> _lastReadTask; // The last successful task returned from ReadAsync
#endif

        // <

        private const int MemStreamMaxLength = int.MaxValue;

        public MemoryStream()
            : this(0) {
        }

        public MemoryStream(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity", ("ArgumentOutOfRange_NegativeCapacity"));
            }
            //Contract.EndContractBlock();

            this._buffer = new byte[capacity];
            this._capacity = capacity;
            this._expandable = true;
            this._writable = true;
            this._exposable = true;
            this._origin = 0;      // Must be 0 for byte[]'s created by MemoryStream
            this._isOpen = true;
        }

        public MemoryStream(byte[] buffer)
            : this(buffer, true) {
        }

        public MemoryStream(byte[] buffer, bool writable) {
            //Contract.EndContractBlock();
            this._buffer = buffer ?? throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            this._length = this._capacity = buffer.Length;
            this._writable = writable;
            this._exposable = false;
            this._origin = 0;
            this._isOpen = true;
        }

        public MemoryStream(byte[] buffer, int index, int count)
            : this(buffer, index, count, true, false) {
        }

        public MemoryStream(byte[] buffer, int index, int count, bool writable)
            : this(buffer, index, count, writable, false) {
        }

        public MemoryStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible) {
            if (buffer == null)
                throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - index < count)
                throw new ArgumentException(("Argument_InvalidOffLen"));
            //Contract.EndContractBlock();

            this._buffer = buffer;
            this._origin = this._position = index;
            this._length = this._capacity = index + count;
            this._writable = writable;
            this._exposable = publiclyVisible;  // Can TryGetBuffer/GetBuffer return the array?
            this._expandable = false;
            this._isOpen = true;
        }

        public override bool CanRead {
            //[Pure]
            get => this._isOpen;
        }

        public override bool CanSeek {
            //[Pure]
            get => this._isOpen;
        }

        public override bool CanWrite {
            //[Pure]
            get => this._writable;
        }

        private void EnsureWriteable() {
            if (!this.CanWrite) __Error.WriteNotSupported();
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    this._isOpen = false;
                    this._writable = false;
                    this._expandable = false;
                    // Don't set buffer to null - allow TryGetBuffer, GetBuffer & ToArray to work.
#if FEATURE_ASYNC_IO
                    _lastReadTask = null;
#endif
                }
            }
            finally {
                // Call base.Close() to cleanup async IO resources
                base.Dispose(disposing);
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value) {
            // Check for overflow
            if (value < 0)
                throw new IOException(("IO.IO_StreamTooLong"));
            if (value > this._capacity) {
                var newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                // We are ok with this overflowing since the next statement will deal
                // with the cases where _capacity*2 overflows.
                if (newCapacity < this._capacity * 2)
                    newCapacity = this._capacity * 2;
                // We want to expand the array up to Array.MaxArrayLengthOneDimensional
                // And we want to give the user the value that they asked for
                if ((uint)(this._capacity * 2) > Array.MaxByteArrayLength)
                    newCapacity = value > Array.MaxByteArrayLength ? value : Array.MaxByteArrayLength;

                this.Capacity = newCapacity;
                return true;
            }
            return false;
        }

        public override void Flush() {
        }

#if FEATURE_ASYNC_IO
        [HostProtection(ExternalThreading=true)]
        [ComVisible(false)]
        public override Task FlushAsync(CancellationToken cancellationToken) {

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCancellation(cancellationToken);

            try {

                Flush();
                return Task.CompletedTask;

            } catch(Exception ex) {

                return Task.FromException(ex);
            }
        }
#endif // FEATURE_ASYNC_IO


        public virtual byte[] GetBuffer() {
            if (!this._exposable)
                throw new InvalidOperationException(("UnauthorizedAccess_MemStreamBuffer"));
            return this._buffer;
        }

        //public virtual bool TryGetBuffer(out ArraySegment<byte> buffer) {
        //    if (!this._exposable) {
        //        buffer = default(ArraySegment<byte>);
        //        return false;
        //    }
        //
        //    buffer = new ArraySegment<byte>(_buffer, offset: _origin, count: (_length - _origin));
        //    return true;
        //}

        // -------------- PERF: Internal functions for fast direct access of MemoryStream buffer (cf. BinaryReader for usage) ---------------

        // PERF: Internal sibling of GetBuffer, always returns a buffer (cf. GetBuffer())
        internal byte[] InternalGetBuffer() => this._buffer;

        // PERF: Get origin and length - used in ResourceWriter.
        //[FriendAccessAllowed]
        internal void InternalGetOriginAndLength(out int origin, out int length) {
            if (!this._isOpen) __Error.StreamIsClosed();
            origin = this._origin;
            length = this._length;
        }

        // PERF: True cursor position, we don't need _origin for direct access
        internal int InternalGetPosition() {
            if (!this._isOpen) __Error.StreamIsClosed();
            return this._position;
        }

        // PERF: Takes out Int32 as fast as possible
        internal int InternalReadInt32() {
            if (!this._isOpen)
                __Error.StreamIsClosed();

            var pos = (this._position += 4); // use temp to avoid ----
            if (pos > this._length) {
                this._position = this._length;
                __Error.EndOfFile();
            }
            return (int)(this._buffer[pos - 4] | this._buffer[pos - 3] << 8 | this._buffer[pos - 2] << 16 | this._buffer[pos - 1] << 24);
        }

        // PERF: Get actual length of bytes available for read; do sanity checks; shift position - i.e. everything except actual copying bytes
        internal int InternalEmulateRead(int count) {
            if (!this._isOpen) __Error.StreamIsClosed();

            var n = this._length - this._position;
            if (n > count) n = count;
            if (n < 0) n = 0;

            //Contract.Assert(this._position + n >= 0, "_position + n >= 0");  // len is less than 2^31 -1.
            this._position += n;
            return n;
        }

        // Gets & sets the capacity (number of bytes allocated) for this stream.
        // The capacity cannot be set to a value less than the current length
        // of the stream.
        //
        public virtual int Capacity {
            get {
                if (!this._isOpen) __Error.StreamIsClosed();
                return this._capacity - this._origin;
            }
            set {
                // Only update the capacity if the MS is expandable and the value is different than the current capacity.
                // Special behavior if the MS isn't expandable: we don't throw if value is the same as the current capacity
                if (value < this.Length) throw new ArgumentOutOfRangeException("value", ("ArgumentOutOfRange_SmallCapacity"));
                //Contract.Ensures(this._capacity - this._origin == value);
                //Contract.EndContractBlock();

                if (!this._isOpen) __Error.StreamIsClosed();
                if (!this._expandable && (value != this.Capacity)) __Error.MemoryStreamNotExpandable();

                // MemoryStream has this invariant: _origin > 0 => !expandable (see ctors)
                if (this._expandable && value != this._capacity) {
                    if (value > 0) {
                        var newBuffer = new byte[value];
                        if (this._length > 0) Buffer.InternalBlockCopy(this._buffer, 0, newBuffer, 0, this._length);
                        this._buffer = newBuffer;
                    }
                    else {
                        this._buffer = null;
                    }
                    this._capacity = value;
                }
            }
        }

        public override long Length {
            get {
                if (!this._isOpen) __Error.StreamIsClosed();
                return this._length - this._origin;
            }
        }

        public override long Position {
            get {
                if (!this._isOpen) __Error.StreamIsClosed();
                return this._position - this._origin;
            }
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", ("ArgumentOutOfRange_NeedNonNegNum"));
                //Contract.Ensures(this.Position == value);
                //Contract.EndContractBlock();

                if (!this._isOpen) __Error.StreamIsClosed();

                if (value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException("value", ("ArgumentOutOfRange_StreamLength"));
                this._position = this._origin + (int)value;
            }
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count) {
            if (buffer == null)
                throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - offset < count)
                throw new ArgumentException(("Argument_InvalidOffLen"));
            //Contract.EndContractBlock();

            if (!this._isOpen) __Error.StreamIsClosed();

            var n = this._length - this._position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            //Contract.Assert(this._position + n >= 0, "_position + n >= 0");  // len is less than 2^31 -1.

            if (n <= 8) {
                var byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = this._buffer[this._position + byteCount];
            }
            else
                Buffer.InternalBlockCopy(this._buffer, this._position, buffer, offset, n);
            this._position += n;

            return n;
        }

#if FEATURE_ASYNC_IO
        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task<int> ReadAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer==null)
                throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - offset < count)
                throw new ArgumentException(("Argument_InvalidOffLen"));
            //Contract.EndContractBlock(); // contract validation copied from Read(...)

            // If cancellation was requested, bail early
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCancellation<int>(cancellationToken);

            try
            {
                int n = Read(buffer, offset, count);
                var t = _lastReadTask;
                //Contract.Assert(t == null || t.Status == TaskStatus.RanToCompletion,
                    "Expected that a stored last task completed successfully");
                return (t != null && t.Result == n) ? t : (_lastReadTask = Task.FromResult<int>(n));
            }
            catch (OperationCanceledException oce)
            {
                return Task.FromCancellation<int>(oce);
            }
            catch (Exception exception)
            {
                return Task.FromException<int>(exception);
            }
        }
#endif //FEATURE_ASYNC_IO


        public override int ReadByte() {
            if (!this._isOpen) __Error.StreamIsClosed();

            if (this._position >= this._length) return -1;

            return this._buffer[this._position++];
        }


#if FEATURE_ASYNC_IO
        public override Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken) {

            // This implementation offers beter performance compared to the base class version.

            // The parameter checks must be in sync with the base version:
            if (destination == null)
                throw new ArgumentNullException("destination");

            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", ("ArgumentOutOfRange_NeedPosNum"));

            if (!CanRead && !CanWrite)
                throw new ObjectDisposedException(null, ("ObjectDisposed_StreamClosed"));

            if (!destination.CanRead && !destination.CanWrite)
                throw new ObjectDisposedException("destination", ("ObjectDisposed_StreamClosed"));

            if (!CanRead)
                throw new NotSupportedException(("NotSupported_UnreadableStream"));

            if (!destination.CanWrite)
                throw new NotSupportedException(("NotSupported_UnwritableStream"));

            //Contract.EndContractBlock();

            // If we have been inherited into a subclass, the following implementation could be incorrect
            // since it does not call through to Read() or Write() which a subclass might have overriden.
            // To be safe we will only use this implementation in cases where we know it is safe to do so,
            // and delegate to our base class (which will call into Read/Write) when we are not sure.
            if (this.GetType() != typeof(MemoryStream))
                return base.CopyToAsync(destination, bufferSize, cancellationToken);

            // If cancelled - return fast:
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCancellation(cancellationToken);

            // Avoid copying data from this buffer into a temp buffer:
            //   (require that InternalEmulateRead does not throw,
            //    otherwise it needs to be wrapped into try-catch-Task.FromException like memStrDest.Write below)

            Int32 pos = _position;
            Int32 n = InternalEmulateRead(_length - _position);

            // If destination is not a memory stream, write there asynchronously:
            MemoryStream memStrDest = destination as MemoryStream;
            if (memStrDest == null)
                return destination.WriteAsync(_buffer, pos, n, cancellationToken);

            try {

                // If destination is a MemoryStream, CopyTo synchronously:
                memStrDest.Write(_buffer, pos, n);
                return Task.CompletedTask;

            } catch(Exception ex) {
                return Task.FromException(ex);
            }
        }
#endif //FEATURE_ASYNC_IO


        public override long Seek(long offset, SeekOrigin loc) {
            if (!this._isOpen) __Error.StreamIsClosed();

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException("offset", ("ArgumentOutOfRange_StreamLength"));
            switch (loc) {
                case SeekOrigin.Begin: {
                        var tempPosition = unchecked(this._origin + (int)offset);
                        if (offset < 0 || tempPosition < this._origin)
                            throw new IOException(("IO.IO_SeekBeforeBegin"));
                        this._position = tempPosition;
                        break;
                    }
                case SeekOrigin.Current: {
                        var tempPosition = unchecked(this._position + (int)offset);
                        if (unchecked(this._position + offset) < this._origin || tempPosition < this._origin)
                            throw new IOException(("IO.IO_SeekBeforeBegin"));
                        this._position = tempPosition;
                        break;
                    }
                case SeekOrigin.End: {
                        var tempPosition = unchecked(this._length + (int)offset);
                        if (unchecked(this._length + offset) < this._origin || tempPosition < this._origin)
                            throw new IOException(("IO.IO_SeekBeforeBegin"));
                        this._position = tempPosition;
                        break;
                    }
                default:
                    throw new ArgumentException(("Argument_InvalidSeekOrigin"));
            }

            //Contract.Assert(this._position >= 0, "_position >= 0");
            return this._position;
        }

        // Sets the length of the stream to a given value.  The new
        // value must be nonnegative and less than the space remaining in
        // the array, Int32.MaxValue - origin
        // Origin is 0 in all cases other than a MemoryStream created on
        // top of an existing array and a specific starting offset was passed
        // into the MemoryStream constructor.  The upper bounds prevents any
        // situations where a stream may be created on top of an array then
        // the stream is made longer than the maximum possible length of the
        // array (Int32.MaxValue).
        //
        public override void SetLength(long value) {
            if (value < 0 || value > int.MaxValue) {
                throw new ArgumentOutOfRangeException("value", ("ArgumentOutOfRange_StreamLength"));
            }
            //Contract.Ensures(this._length - this._origin == value);
            //Contract.EndContractBlock();
            EnsureWriteable();

            // Origin wasn't publicly exposed above.
            //Contract.Assert(MemStreamMaxLength == int.MaxValue);  // Check parameter validation logic in this method if this fails.
            if (value > (int.MaxValue - this._origin)) {
                throw new ArgumentOutOfRangeException("value", ("ArgumentOutOfRange_StreamLength"));
            }

            var newLength = this._origin + (int)value;
            var allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > this._length)
                Array.Clear(this._buffer, this._length, newLength - this._length);
            this._length = newLength;
            if (this._position > newLength) this._position = newLength;

        }

        public virtual byte[] ToArray() {
            //BCLDebug.Perf(_exposable, "MemoryStream::GetBuffer will let you avoid a copy.");
            var copy = new byte[this._length - this._origin];
            Buffer.InternalBlockCopy(this._buffer, this._origin, copy, 0, this._length - this._origin);
            return copy;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (buffer == null)
                throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - offset < count)
                throw new ArgumentException(("Argument_InvalidOffLen"));
            //Contract.EndContractBlock();

            if (!this._isOpen) __Error.StreamIsClosed();
            EnsureWriteable();

            var i = this._position + count;
            // Check for overflow
            if (i < 0)
                throw new IOException(("IO.IO_StreamTooLong"));

            if (i > this._length) {
                var mustZero = this._position > this._length;
                if (i > this._capacity) {
                    var allocatedNewArray = EnsureCapacity(i);
                    if (allocatedNewArray)
                        mustZero = false;
                }
                if (mustZero)
                    Array.Clear(this._buffer, this._length, i - this._length);
                this._length = i;
            }
            if ((count <= 8) && (buffer != this._buffer)) {
                var byteCount = count;
                while (--byteCount >= 0)
                    this._buffer[this._position + byteCount] = buffer[offset + byteCount];
            }
            else
                Buffer.InternalBlockCopy(buffer, offset, this._buffer, this._position, count);
            this._position = i;

        }

#if FEATURE_ASYNC_IO
        [HostProtection(ExternalThreading = true)]
        [ComVisible(false)]
        public override Task WriteAsync(Byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", ("ArgumentNull_Buffer"));
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", ("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - offset < count)
                throw new ArgumentException(("Argument_InvalidOffLen"));
            //Contract.EndContractBlock(); // contract validation copied from Write(...)

            // If cancellation is already requested, bail early
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCancellation(cancellationToken);

            try
            {
                Write(buffer, offset, count);
                return Task.CompletedTask;
            }
            catch (OperationCanceledException oce)
            {
                return Task.FromCancellation<VoidTaskResult>(oce);
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }
#endif // FEATURE_ASYNC_IO

        public override void WriteByte(byte value) {
            if (!this._isOpen) __Error.StreamIsClosed();
            EnsureWriteable();

            if (this._position >= this._length) {
                var newLength = this._position + 1;
                var mustZero = this._position > this._length;
                if (newLength >= this._capacity) {
                    var allocatedNewArray = EnsureCapacity(newLength);
                    if (allocatedNewArray)
                        mustZero = false;
                }
                if (mustZero)
                    Array.Clear(this._buffer, this._length, this._position - this._length);
                this._length = newLength;
            }
            this._buffer[this._position++] = value;

        }

        // Writes this MemoryStream to another stream.
        public virtual void WriteTo(Stream stream) {
            if (stream == null)
                throw new ArgumentNullException("stream", ("ArgumentNull_Stream"));
            //Contract.EndContractBlock();

            if (!this._isOpen) __Error.StreamIsClosed();
            stream.Write(this._buffer, this._origin, this._length - this._origin);
        }

#if CONTRACTS_FULL
        [ContractInvariantMethod]
        private void ObjectInvariantMS() {
            //Contract.Invariant(_origin >= 0);
            //Contract.Invariant(_origin <= _position);
            //Contract.Invariant(_length <= _capacity);
            // equivalent to _origin > 0 => !expandable, and using fact that _origin is non-negative.
            //Contract.Invariant(_origin == 0 || !_expandable);
        }
#endif
    }

    internal class Buffer {
        internal static void InternalBlockCopy(Array src, int srcOffsetBytes, Array dst, int dstOffsetBytes, int byteCount) => Array.Copy(src, srcOffsetBytes, dst, dstOffsetBytes, byteCount);
    }
}

namespace System.IO {
    //[Pure]
    internal static class __Error {
        internal static void EndOfFile() => throw new IOException(("IO.EOF_ReadBeyondEOF"));

        internal static void FileNotOpen() => throw new ObjectDisposedException(("ObjectDisposed_FileClosed"));

        internal static void StreamIsClosed() => throw new ObjectDisposedException(("ObjectDisposed_StreamClosed"));

        internal static void MemoryStreamNotExpandable() => throw new NotSupportedException(("NotSupported_MemStreamNotExpandable"));

        internal static void ReaderClosed() => throw new ObjectDisposedException(("ObjectDisposed_ReaderClosed"));

        internal static void ReadNotSupported() => throw new NotSupportedException(("NotSupported_UnreadableStream"));

        internal static void SeekNotSupported() => throw new NotSupportedException(("NotSupported_UnseekableStream"));

        internal static void WrongAsyncResult() => throw new ArgumentException(("Arg_WrongAsyncResult"));

        internal static void EndReadCalledTwice() =>
            // Should ideally be InvalidOperationExc but we can't maitain parity with Stream and FileStream without some work
            throw new ArgumentException(("InvalidOperation_EndReadCalledMultiple"));

        internal static void EndWriteCalledTwice() =>
            // Should ideally be InvalidOperationExc but we can't maintain parity with Stream and FileStream without some work
            throw new ArgumentException(("InvalidOperation_EndWriteCalledMultiple"));
#if false
        // Given a possible fully qualified path, ensure that we have path
        // discovery permission to that path.  If we do not, return just the
        // file name.  If we know it is a directory, then don't return the
        // directory name.
        [System.Security.SecurityCritical]  // auto-generated
        internal static String GetDisplayablePath(String path, bool isInvalidPath) {
            if (String.IsNullOrEmpty(path))
                return String.Empty;

            if (path.Length < 2)
                return path;

            // Return the path as is if we're relative (not fully qualified) and not a bad path
            if (PathInternal.IsPartiallyQualified(path) && !isInvalidPath)
                return path;

            bool safeToReturn = false;
            try {
                if (!isInvalidPath) {
#if !FEATURE_CORECLR
                    FileIOPermission.QuickDemand(FileIOPermissionAccess.PathDiscovery, path, false, false);
#endif
                    safeToReturn = true;
                }
            }
            catch (SecurityException) {
            }
            catch (ArgumentException) {
                // ? and * characters cause ArgumentException to be thrown from HasIllegalCharacters
                // inside FileIOPermission.AddPathList
            }
            catch (NotSupportedException) {
                // paths like "!Bogus\\dir:with/junk_.in it" can cause NotSupportedException to be thrown
                // from Security.Util.StringExpressionSet.CanonicalizePath when ':' is found in the path
                // beyond string index position 1.
            }

            if (!safeToReturn) {
                if (Path.IsDirectorySeparator(path[path.Length - 1]))
                    path = ("IO.IO_NoPermissionToDirectoryName");
                else
                    path = Path.GetFileName(path);
            }

            return path;
        }
#endif

        //[System.Security.SecuritySafeCritical]  // auto-generated
        //internal static void WinIOError() {
        //    int errorCode = Marshal.GetLastWin32Error();
        //    WinIOError(errorCode, String.Empty);
        //}

        // After calling GetLastWin32Error(), it clears the last error field,
        // so you must save the HResult and pass it to this method.  This method
        // will determine the appropriate exception to throw dependent on your
        // error, and depending on the error, insert a string into the message
        // gotten from the ResourceManager.
        //[System.Security.SecurityCritical]  // auto-generated
        //internal static void WinIOError(int errorCode, String maybeFullPath) {
        //    // This doesn't have to be perfect, but is a perf optimization.
        //    bool isInvalidPath = errorCode == Win32Native.ERROR_INVALID_NAME || errorCode == Win32Native.ERROR_BAD_PATHNAME;
        //    String str = GetDisplayablePath(maybeFullPath, isInvalidPath);
        //
        //    switch (errorCode) {
        //        case Win32Native.ERROR_FILE_NOT_FOUND:
        //            if (str.Length == 0)
        //                throw new FileNotFoundException(("IO.FileNotFound"));
        //            else
        //                throw new FileNotFoundException(("IO.FileNotFound_FileName", str), str);
        //
        //        case Win32Native.ERROR_PATH_NOT_FOUND:
        //            if (str.Length == 0)
        //                throw new DirectoryNotFoundException(("IO.PathNotFound_NoPathName"));
        //            else
        //                throw new DirectoryNotFoundException(("IO.PathNotFound_Path", str));
        //
        //        case Win32Native.ERROR_ACCESS_DENIED:
        //            if (str.Length == 0)
        //                throw new UnauthorizedAccessException(("UnauthorizedAccess_IODenied_NoPathName"));
        //            else
        //                throw new UnauthorizedAccessException(("UnauthorizedAccess_IODenied_Path", str));
        //
        //        case Win32Native.ERROR_ALREADY_EXISTS:
        //            if (str.Length == 0)
        //                goto default;
        //            throw new IOException(("IO.IO_AlreadyExists_Name", str), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //
        //        case Win32Native.ERROR_FILENAME_EXCED_RANGE:
        //            throw new PathTooLongException(("IO.PathTooLong"));
        //
        //        case Win32Native.ERROR_INVALID_DRIVE:
        //            throw new DriveNotFoundException(("IO.DriveNotFound_Drive", str));
        //
        //        case Win32Native.ERROR_INVALID_PARAMETER:
        //            throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //
        //        case Win32Native.ERROR_SHARING_VIOLATION:
        //            if (str.Length == 0)
        //                throw new IOException(("IO.IO_SharingViolation_NoFileName"), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //            else
        //                throw new IOException(("IO.IO_SharingViolation_File", str), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //
        //        case Win32Native.ERROR_FILE_EXISTS:
        //            if (str.Length == 0)
        //                goto default;
        //            throw new IOException(("IO.IO_FileExists_Name", str), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //
        //        case Win32Native.ERROR_OPERATION_ABORTED:
        //            throw new OperationCanceledException();
        //
        //        default:
        //            throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        //    }
        //}

        // An alternative to WinIOError with friendlier messages for drives
        //[System.Security.SecuritySafeCritical]  // auto-generated
        //internal static void WinIODriveError(String driveName) {
        //    int errorCode = Marshal.GetLastWin32Error();
        //    WinIODriveError(driveName, errorCode);
        //}
        //
        //[System.Security.SecurityCritical]  // auto-generated
        //internal static void WinIODriveError(String driveName, int errorCode) {
        //    switch (errorCode) {
        //        case Win32Native.ERROR_PATH_NOT_FOUND:
        //        case Win32Native.ERROR_INVALID_DRIVE:
        //            throw new DriveNotFoundException(("IO.DriveNotFound_Drive", driveName));
        //
        //        default:
        //            WinIOError(errorCode, driveName);
        //            break;
        //    }
        //}

        internal static void WriteNotSupported() => throw new NotSupportedException(("NotSupported_UnwritableStream"));

        internal static void WriterClosed() => throw new ObjectDisposedException(("ObjectDisposed_WriterClosed"));

        // From WinError.h
        //internal const int ERROR_FILE_NOT_FOUND = Win32Native.ERROR_FILE_NOT_FOUND;
        //internal const int ERROR_PATH_NOT_FOUND = Win32Native.ERROR_PATH_NOT_FOUND;
        //internal const int ERROR_ACCESS_DENIED = Win32Native.ERROR_ACCESS_DENIED;
        //internal const int ERROR_INVALID_PARAMETER = Win32Native.ERROR_INVALID_PARAMETER;
    }
}