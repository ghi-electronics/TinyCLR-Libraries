////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using Microsoft.SPOT.IO;

namespace System.IO
{

    public class FileStream : Stream
    {
        // Driver data

        private NativeFileStream _nativeFileStream;
        private FileSystemManager.FileRecord _fileRecord;
        private string _fileName;
        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private long _seekLimit;

        private bool _disposed;

        //--//

        public FileStream(string path, FileMode mode)
            : this(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            // This will perform validation on path
            this._fileName = Path.GetFullPath(path);

            // make sure mode, access, and share are within range
            if (mode < FileMode.CreateNew || mode > FileMode.Append ||
                access < FileAccess.Read || access > FileAccess.ReadWrite ||
                share < FileShare.None || share > FileShare.ReadWrite)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            var wantsRead = (access & FileAccess.Read) == FileAccess.Read;
            var wantsWrite = (access & FileAccess.Write) == FileAccess.Write;

            // You can't open for readonly access (wantsWrite == false) when
            // mode is CreateNew, Create, Truncate or Append (when it's not Open or OpenOrCreate)
            if (mode != FileMode.Open && mode != FileMode.OpenOrCreate && !wantsWrite)
            {
                throw new ArgumentException();
            }

            // We need to register the share information prior to the actual file open call (the NativeFileStream ctor)
            // so subsequent file operation on the same file will behave correctly
            this._fileRecord = FileSystemManager.AddToOpenList(this._fileName, (int)access, (int)share);

            try
            {
                var attributes = NativeIO.GetAttributes(this._fileName);
                var exists = (attributes != 0xFFFFFFFF);
                var isReadOnly = (exists) ? (((FileAttributes)attributes) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly : false;

                // If the path specified is an existing directory, fail
                if (exists && ((((FileAttributes)attributes) & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // The seek limit is 0 (the beginning of the file) for all modes except Append
                this._seekLimit = 0;

                switch (mode)
                {
                    case FileMode.CreateNew: // if the file exists, IOException is thrown
                        if (exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        break;

                    case FileMode.Create: // if the file exists, it should be overwritten
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        if (exists) this._nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Open: // if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        break;

                    case FileMode.OpenOrCreate: // if the file does not exist, it is created
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        break;

                    case FileMode.Truncate: // the file would be overwritten. if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        this._nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Append: // Opens the file if it exists and seeks to the end of the file. Append can only be used in conjunction with FileAccess.Write
                        // Attempting to seek to a position before the end of the file will throw an IOException and any attempt to read fails and throws an NotSupportedException
                        if (access != FileAccess.Write) throw new ArgumentException();
                        this._nativeFileStream = new NativeFileStream(this._fileName, bufferSize);
                        this._seekLimit = this._nativeFileStream.Seek(0, (uint)SeekOrigin.End);
                        break;

                    // We've already checked the mode value previously, so no need for default
                    //default:
                    //    throw new ArgumentOutOfRangeException();
                }

                // Now that we have a valid NativeFileStream, we add it to the FileRecord, so it could gets clean up
                // in case an eject or force format
                this._fileRecord.NativeFileStream = this._nativeFileStream;

                // Retrive the filesystem capabilities
                this._nativeFileStream.GetStreamProperties(out this._canRead, out this._canWrite, out this._canSeek);

                // If the file is readonly, regardless of the filesystem capability, we'll turn off write
                if (isReadOnly)
                {
                    this._canWrite = false;
                }

                // Make sure the requests (wantsRead / wantsWrite) matches the filesystem capabilities (canRead / canWrite)
                if ((wantsRead && !this._canRead) || (wantsWrite && !this._canWrite))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // finally, adjust the _canRead / _canWrite to match the requests
                if (!wantsWrite)
                {
                    this._canWrite = false;
                }
                else if (!wantsRead)
                {
                    this._canRead = false;
                }
            }
            catch
            {
                // something went wrong, clean up and re-throw the exception
                if (this._nativeFileStream != null)
                {
                    this._nativeFileStream.Close();
                }

                FileSystemManager.RemoveFromOpenList(this._fileRecord);

                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (disposing)
                    {
                        this._canRead = false;
                        this._canWrite = false;
                        this._canSeek = false;
                    }

                    if (this._nativeFileStream != null)
                    {
                        this._nativeFileStream.Close();
                    }
                }
                finally
                {
                    if (this._fileRecord != null)
                    {
                        FileSystemManager.RemoveFromOpenList(this._fileRecord);
                        this._fileRecord = null;
                    }

                    this._nativeFileStream = null;
                    this._disposed = true;
                }
            }
        }

        ~FileStream()
        {
            Dispose(false);
        }

        // This is for internal use to support proper atomic CopyAndDelete
        internal void DisposeAndDelete()
        {
            this._nativeFileStream.Close();
            this._nativeFileStream = null; // so Dispose(true) won't close the stream again
            NativeIO.Delete(this._fileName);

            Dispose(true);
        }

        public override void Flush()
        {
            if (this._disposed) throw new ObjectDisposedException();
            this._nativeFileStream.Flush();
        }

        public override void SetLength(long value)
        {
            if (this._disposed) throw new ObjectDisposedException();
            if (!this._canWrite || !this._canSeek) throw new NotSupportedException();

            // argument validation in interop layer
            this._nativeFileStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this._disposed) throw new ObjectDisposedException();
            if (!this._canRead) throw new NotSupportedException();

            lock (this._nativeFileStream)
            {
                // argument validation in interop layer
                return this._nativeFileStream.Read(buffer, offset, count, NativeFileStream.TimeoutDefault);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this._disposed) throw new ObjectDisposedException();
            if (!this._canSeek) throw new NotSupportedException();

            var oldPosition = this.Position;
            var newPosition = this._nativeFileStream.Seek(offset, (uint)origin);

            if (newPosition < this._seekLimit)
            {
                this.Position = oldPosition;
                throw new IOException();
            }

            return newPosition;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this._disposed) throw new ObjectDisposedException();
            if (!this._canWrite) throw new NotSupportedException();

            // argument validation in interop layer
            int bytesWritten;

            lock (this._nativeFileStream)
            {
                // we check for count being != 0 because we want to handle negative cases
                // as well in the interop layer
                while (count != 0)
                {
                    bytesWritten = this._nativeFileStream.Write(buffer, offset, count, NativeFileStream.TimeoutDefault);

                    if (bytesWritten == 0) throw new IOException();

                    offset += bytesWritten;
                    count -= bytesWritten;
                }
            }
        }

        public override bool CanRead => this._canRead;

        public override bool CanWrite => this._canWrite;

        public override bool CanSeek => this._canSeek;

        public virtual bool IsAsync => false;

        public override long Length
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException();
                if (!this._canSeek) throw new NotSupportedException();

                return this._nativeFileStream.GetLength();
            }
        }

        public string Name => this._fileName;

        public override long Position
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException();
                if (!this._canSeek) throw new NotSupportedException();

                // argument validation in interop layer
                return this._nativeFileStream.Seek(0, (uint)SeekOrigin.Current);
            }

            set
            {
                if (this._disposed) throw new ObjectDisposedException();
                if (!this._canSeek) throw new NotSupportedException();
                if (value < this._seekLimit) throw new IOException();

                // argument validation in interop layer
                this._nativeFileStream.Seek(value, (uint)SeekOrigin.Begin);
            }
        }
    }
}


