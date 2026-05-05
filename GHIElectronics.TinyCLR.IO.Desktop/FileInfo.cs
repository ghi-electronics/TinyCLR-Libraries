////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace System.IO {
    [Serializable]
    public sealed class FileInfo : FileSystemInfo
    {
        public FileInfo(string fileName) =>
            // path validation in Path.GetFullPath()

            this.m_fullPath = Path.GetFullPath(fileName);

        public override string Name => Path.GetFileName(this.m_fullPath);

        public long Length
        {
            get
            {
                RefreshIfNull();
                return (long)this._nativeFileInfo.Size;
            }
        }

        public string DirectoryName => Path.GetDirectoryName(this.m_fullPath);

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

        public FileStream Create() => File.Create(this.m_fullPath);

        public override void Delete() => File.Delete(this.m_fullPath);

        public override bool Exists => File.Exists(this.m_fullPath);

        public override string ToString() => this.m_fullPath;
    }
}


