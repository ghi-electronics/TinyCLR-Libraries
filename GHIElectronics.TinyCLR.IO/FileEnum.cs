////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    public enum FileEnumFlags
    {
        Files = 0x0001,
        Directories = 0x0002,
        FilesAndDirectories = Files | Directories,
    }

    public class FileEnum : IEnumerator, IDisposable
    {
        private IFileSystemEntryFinder  m_findFile;
        private FileSystemEntry  m_currentFile;
        private FileEnumFlags   m_flags;
        private string          m_path;
        private bool            m_disposed;
        private object          m_openForReadHandle;

        public FileEnum(string path, FileEnumFlags flags)
        {
            this.m_flags = flags;
            this.m_path  = path;

            this.m_openForReadHandle = FileSystemManager.AddToOpenListForRead(this.m_path);
            this.m_findFile          = DriveInfo.GetForPath(this.m_path).Find(this.m_path, "*");
        }

        #region IEnumerator Members

        public object Current
        {
            get
            {
                if (this.m_disposed) throw new ObjectDisposedException();

                return this.m_currentFile.FileName;
            }
        }

        public bool MoveNext()
        {
            if (this.m_disposed) throw new ObjectDisposedException();

            var fileinfo = this.m_findFile.GetNext();

            while (fileinfo != null)
            {
                if (this.m_flags != FileEnumFlags.FilesAndDirectories)
                {
                    var targetAttribute = (0 != (this.m_flags & FileEnumFlags.Directories) ? FileAttributes.Directory : 0);

                    if ((fileinfo.Attributes & FileAttributes.Directory) == targetAttribute)
                    {
                        this.m_currentFile = fileinfo;
                        break;
                    }
                }
                else
                {
                    this.m_currentFile = fileinfo;
                    break;
                }

                fileinfo = this.m_findFile.GetNext();
            }

            if (fileinfo == null)
            {
                this.m_findFile.Close();
                this.m_findFile = null;

                FileSystemManager.RemoveFromOpenList(this.m_openForReadHandle);
                this.m_openForReadHandle = null;
            }

            return fileinfo != null;
        }

        public void Reset()
        {
            if (this.m_disposed) throw new ObjectDisposedException();

            if (this.m_findFile != null)
            {
                this.m_findFile.Close();
            }

            if(this.m_openForReadHandle == null)
            {
                this.m_openForReadHandle = FileSystemManager.AddToOpenListForRead(this.m_path);
            }

            this.m_findFile = DriveInfo.GetForPath(this.m_path).Find(this.m_path, "*");
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (this.m_findFile != null)
            {
                this.m_findFile.Close();
                this.m_findFile = null;
            }

            if (this.m_openForReadHandle != null)
            {
                FileSystemManager.RemoveFromOpenList(this.m_openForReadHandle);
                this.m_openForReadHandle = null;
            }

            this.m_disposed = true;
        }

        ~FileEnum()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class FileEnumerator : IEnumerable
    {
        private string m_path;
        private FileEnumFlags m_flags;

        public FileEnumerator(string path, FileEnumFlags flags)
        {
            this.m_path  = Path.GetFullPath(path);
            this.m_flags = flags;

            if (!Directory.Exists(this.m_path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator() => new FileEnum(this.m_path, this.m_flags);

        #endregion
    }
}