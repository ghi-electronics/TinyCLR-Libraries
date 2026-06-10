////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    /// <summary>Base class for objects that describe a file or directory.</summary>
    public abstract class FileSystemInfo : MarshalByRefObject
    {
        /// <summary>The fully qualified path of the file or directory.</summary>
        protected string m_fullPath;  // fully qualified path of the directory

        //--//

        /// <summary>The full path of the file or directory.</summary>
        public virtual string FullName => this.m_fullPath;

        /// <summary>The extension portion of the name, including the leading period.</summary>
        public string Extension => Path.GetExtension(this.FullName);

        /// <summary>The name of the file or directory.</summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>Whether the file or directory exists.</summary>
        public abstract bool Exists
        {
            get;
        }

        /// <summary>Deletes the file or directory.</summary>
        public abstract void Delete();

        /// <summary>The attributes of the file or directory.</summary>
        public FileAttributes Attributes
        {
            get
            {
                RefreshIfNull();
                return (FileAttributes)this._nativeFileInfo.Attributes;
            }
        }

        /// <summary>The creation time in local time.</summary>
        public DateTime CreationTime => this.CreationTimeUtc.ToLocalTime();

        /// <summary>The creation time in UTC.</summary>
        public DateTime CreationTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.CreationTime;
            }
        }

        /// <summary>The last access time in local time.</summary>
        public DateTime LastAccessTime => this.LastAccessTimeUtc.ToLocalTime();

        /// <summary>The last access time in UTC.</summary>
        public DateTime LastAccessTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.LastAccessTime;
            }
        }

        /// <summary>The last write time in local time.</summary>
        public DateTime LastWriteTime => this.LastWriteTimeUtc.ToLocalTime();

        /// <summary>The last write time in UTC.</summary>
        public DateTime LastWriteTimeUtc
        {
            get
            {
                RefreshIfNull();
                return this._nativeFileInfo.LastWriteTime;
            }
        }

        /// <summary>Reloads the cached metadata from the underlying file system.</summary>
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

        /// <summary>Loads the cached metadata if it has not been loaded yet.</summary>
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


