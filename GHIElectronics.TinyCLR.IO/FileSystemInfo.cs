////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    public abstract class FileSystemInfo : MarshalByRefObject
    {
        protected string m_fullPath;  // fully qualified path of the directory

        //--//

        public virtual string FullName => this.m_fullPath;

        public string Extension => Path.GetExtension(this.FullName);

        public abstract string Name
        {
            get;
        }

        public abstract bool Exists
        {
            get;
        }

        public abstract void Delete();

        public FileAttributes Attributes
        {
            get
            {
                RefreshIfNull();
                return (FileAttributes)this._nativeFileInfo.Attributes;
            }
        }

        public DateTime CreationTime => this.CreationTimeUtc.ToLocalTime();

        public DateTime CreationTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.CreationTime;
            }
        }

        public DateTime LastAccessTime => this.LastAccessTimeUtc.ToLocalTime();

        public DateTime LastAccessTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.LastAccessTime;
            }
        }

        public DateTime LastWriteTime => this.LastWriteTimeUtc.ToLocalTime();

        public DateTime LastWriteTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.LastWriteTime;
            }
        }

        public void Refresh()
        {
            var record = FileSystemManager.AddToOpenListForRead(this.m_fullPath);

            try
            {
                this._nativeFileInfo = DriveInfo.GetForPath(this.m_fullPath).GetFileSystemEntry(this.m_fullPath);

                if (this._nativeFileInfo == null)
                {
                    var errorCode = (this is FileInfo) ? IOException.IOExceptionErrorCode.FileNotFound : IOException.IOExceptionErrorCode.DirectoryNotFound;
                    throw new IOException("", (int)errorCode);
                }
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        protected void RefreshIfNull()
        {
            if (this._nativeFileInfo == null)
            {
                Refresh();
            }
        }

        internal FileSystemEntry _nativeFileInfo;
    }
}


