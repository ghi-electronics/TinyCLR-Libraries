////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace System.IO {
    /// <summary>Provides information about a file and the means to create or delete it.</summary>
    [Serializable]
    public sealed class FileInfo : FileSystemInfo
    {
        /// <summary>Creates a new instance for the file at the given path.</summary>
        public FileInfo(string fileName) =>
            // path validation in Path.GetFullPath()

            this.m_fullPath = Path.GetFullPath(fileName);

        /// <inheritdoc/>
        public override string Name => Path.GetFileName(this.m_fullPath);

        /// <summary>The size of the file in bytes.</summary>
        public long Length
        {
            get
            {
                RefreshIfNull();
                return (long)this._nativeFileInfo.Size;
            }
        }

        /// <summary>The full path of the directory that contains the file.</summary>
        public string DirectoryName => Path.GetDirectoryName(this.m_fullPath);

        /// <summary>The directory that contains the file, or null if there is none.</summary>
        public DirectoryInfo Directory
        {
            get
            {
                var dirName = this.DirectoryName;

                if (dirName == null)
                {
                    return null;
                }

                return new DirectoryInfo(dirName);
            }
        }

        /// <summary>Creates the file and returns a stream to it.</summary>
        public FileStream Create() => File.Create(this.m_fullPath);

        /// <inheritdoc/>
        public override void Delete() => File.Delete(this.m_fullPath);

        /// <inheritdoc/>
        public override bool Exists => File.Exists(this.m_fullPath);

        /// <summary>Returns the full path of the file.</summary>
        public override string ToString() => this.m_fullPath;
    }
}


