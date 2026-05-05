////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


namespace System.IO
{
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private DirectoryInfo()
        {
        }

        public DirectoryInfo(string path) =>
            // path validation in Path.GetFullPath()

            this.m_fullPath = Path.GetFullPath(path);

        public override string Name {
            get => Path.GetFileName(this.m_fullPath).Length == 0 ? this.m_fullPath : Path.GetFileName(this.m_fullPath);
        }

        public DirectoryInfo Parent
        {
            get
            {
                var parentDirPath = Path.GetDirectoryName(this.m_fullPath);
                if (parentDirPath == null)
                    return null;

                return new DirectoryInfo(parentDirPath);
            }
        }

        public DirectoryInfo CreateSubdirectory(string path)
        {
            // path validatation in Path.Combine()

            var subDirPath = Path.Combine(this.m_fullPath, path);

            /// This will also ensure "path" is valid.
            subDirPath = Path.GetFullPath(subDirPath);

            return Directory.CreateDirectory(subDirPath);
        }

        public void Create() => Directory.CreateDirectory(this.m_fullPath);

        public override bool Exists => Directory.Exists(this.m_fullPath);

        public FileInfo[] GetFiles()
        {
            var fileNames = Directory.GetFiles(this.m_fullPath);

            var files = new FileInfo[fileNames.Length];

            for (var i = 0; i < fileNames.Length; i++)
            {
                files[i] = new FileInfo(fileNames[i]);
            }

            return files;
        }

        public DirectoryInfo[] GetDirectories()
        {
            // searchPattern validation in Directory.GetDirectories()

            var dirNames = Directory.GetDirectories(this.m_fullPath);

            var dirs = new DirectoryInfo[dirNames.Length];

            for (var i = 0; i < dirNames.Length; i++)
            {
                dirs[i] = new DirectoryInfo(dirNames[i]);
            }

            return dirs;
        }

        public DirectoryInfo Root => new DirectoryInfo(Path.GetPathRoot(this.m_fullPath));

        public void MoveTo(string destDirName) =>
            // destDirName validation in Directory.Move()

            Directory.Move(this.m_fullPath, destDirName);

        public override void Delete() => Directory.Delete(this.m_fullPath);

        public void Delete(bool recursive) => Directory.Delete(this.m_fullPath, recursive);

        public override string ToString() => this.m_fullPath;
    }
}


