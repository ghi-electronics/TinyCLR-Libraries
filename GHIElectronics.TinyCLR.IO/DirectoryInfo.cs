////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


namespace System.IO
{
    /// <summary>Provides information about a directory and the means to create or delete it.</summary>
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private DirectoryInfo()
        {
        }

        /// <summary>Creates a new instance for the directory at the given path.</summary>
        public DirectoryInfo(string path) =>
            // path validation in Path.GetFullPath()

            this.m_fullPath = Path.GetFullPath(path);

        /// <inheritdoc/>
        public override string Name {
            get => Path.GetFileName(this.m_fullPath).Length == 0 ? this.m_fullPath : Path.GetFileName(this.m_fullPath);
        }

        /// <summary>The parent directory, or null if this is a root.</summary>
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

        /// <summary>Creates a subdirectory under this directory and returns it.</summary>
        public DirectoryInfo CreateSubdirectory(string path)
        {
            // path validatation in Path.Combine()

            var subDirPath = Path.Combine(this.m_fullPath, path);

            // This will also ensure "path" is valid.
            subDirPath = Path.GetFullPath(subDirPath);

            return Directory.CreateDirectory(subDirPath);
        }

        /// <summary>Creates the directory.</summary>
        public void Create() => Directory.CreateDirectory(this.m_fullPath);

        /// <inheritdoc/>
        public override bool Exists => Directory.Exists(this.m_fullPath);

        /// <summary>Returns the files contained in the directory.</summary>
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

        /// <summary>Returns the subdirectories contained in the directory.</summary>
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

        /// <summary>The root portion of the directory's path.</summary>
        public DirectoryInfo Root => new DirectoryInfo(Path.GetPathRoot(this.m_fullPath));

        /// <summary>Moves the directory and its contents to a new path.</summary>
        public void MoveTo(string destDirName) =>
            // destDirName validation in Directory.Move()

            Directory.Move(this.m_fullPath, destDirName);

        /// <inheritdoc/>
        public override void Delete() => Directory.Delete(this.m_fullPath);

        /// <summary>Deletes the directory, optionally including its contents.</summary>
        public void Delete(bool recursive) => Directory.Delete(this.m_fullPath, recursive);

        /// <summary>Returns the full path of the directory.</summary>
        public override string ToString() => this.m_fullPath;
    }
}


