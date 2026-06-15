////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    /// <summary>Static helpers for directory management — create, delete, enumerate, get current directory.</summary>
    public sealed class Directory {

        private Directory() {
        }

        //--//

        /// <summary>Returns the names of all mounted logical drives.</summary>
        public static string[] GetLogicalDrives() => DriveInfo.GetLogicalDrives();

        /// <summary>Creates the directory at the path, including any missing parents, and returns it.</summary>
        public static DirectoryInfo CreateDirectory(string path) {
            // path validation in Path.GetFullPath()

            path = Path.GetFullPath(path);

            // According to MSDN, Directory.CreateDirectory on an existing
            // directory is no-op.
            DriveInfo.GetForPath(path).CreateDirectory(path);

            return new DirectoryInfo(path);
        }

        /// <summary>Returns whether the directory at the path exists.</summary>
        public static bool Exists(string path) {
            // path validation in Path.GetFullPath()

            path = Path.GetFullPath(path);

            // Is this the absolute root? this always exists.
            if (path.Length == 3 && path[0] >= 'A' && path[0] <= 'Z' && path[1] == ':' && (path[2] == Path.DirectorySeparatorChar)) {
                return true;
            }
            else {
                try {
                    var attributes = DriveInfo.GetForPath(path).GetAttributes(path);

                    // This is essentially file not found.
                    if ((uint)attributes == 0xFFFFFFFF)
                        return false;

                    // Need to make sure these are not FAT16 or FAT32 specific.
                    if ((((FileAttributes)attributes) & FileAttributes.Directory) == FileAttributes.Directory) {
                        // It is a directory.
                        return true;
                    }
                }
                catch (Exception) {
                    return false;
                }
            }

            return false;
        }

        /// <summary>Returns an enumerable over the file names in the directory.</summary>
        public static IEnumerable EnumerateFiles(string path) {
            if (!Directory.Exists(path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);

            return new FileEnumerator(path, FileEnumFlags.Files);
        }

        /// <summary>Returns an enumerable over the subdirectory names in the directory.</summary>
        public static IEnumerable EnumerateDirectories(string path) {
            if (!Directory.Exists(path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);

            return new FileEnumerator(path, FileEnumFlags.Directories);
        }

        /// <summary>Returns an enumerable over the file and subdirectory names in the directory.</summary>
        public static IEnumerable EnumerateFileSystemEntries(string path) {
            if (!Directory.Exists(path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);

            return new FileEnumerator(path, FileEnumFlags.FilesAndDirectories);
        }

        /// <summary>Returns the names of the files in the directory.</summary>
        public static string[] GetFiles(string path) => GetChildren(path, "*", false);

        /// <summary>Returns the names of the subdirectories in the directory.</summary>
        public static string[] GetDirectories(string path) => GetChildren(path, "*", true);

        /// <summary>Returns the application's current working directory.</summary>
        public static string GetCurrentDirectory() => FileSystemManager.CurrentDirectory;

        /// <summary>Sets the application's current working directory.</summary>
        public static void SetCurrentDirectory(string path) {
            // path validation in Path.GetFullPath()

            path = Path.GetFullPath(path);

            // We lock the directory for read-access first, to ensure path won't get deleted
            var record = FileSystemManager.AddToOpenListForRead(path);

            try {
                if (!Directory.Exists(path)) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // This will put the actual lock on path. (also read-access)
                FileSystemManager.SetCurrentDirectory(path);
            }
            finally {
                // We take our lock off.
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        /// <summary>Moves a directory and its contents to a new location.</summary>
        public static void Move(string sourceDirName, string destDirName) {
            if (Path.GetPathRoot(sourceDirName) != Path.GetPathRoot(destDirName)) throw new ArgumentException();

            // sourceDirName and destDirName validation in Path.GetFullPath()

            sourceDirName = Path.GetFullPath(sourceDirName);
            destDirName = Path.GetFullPath(destDirName);

            var tryCopyAndDelete = false;
            var srcRecord = FileSystemManager.AddToOpenList(sourceDirName);

            try {
                // Make sure sourceDir is actually a directory
                if (!Exists(sourceDirName)) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // If Move() returns false, we'll try doing copy and delete to accomplish the move
                tryCopyAndDelete = !DriveInfo.GetForPath(sourceDirName).Move(sourceDirName, destDirName);
            }
            finally {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }

            if (tryCopyAndDelete) {
                RecursiveCopyAndDelete(sourceDirName, destDirName);
            }
        }

        private static void RecursiveCopyAndDelete(string sourceDirName, string destDirName) {
            string[] files;
            int filesCount, i;
            var relativePathIndex = sourceDirName.Length + 1; // relative path starts after the sourceDirName and a path seperator
            // We have to make sure no one else can modify it (for example, delete the directory and
            // create a file of the same name) while we're moving
            var recordSrc = FileSystemManager.AddToOpenList(sourceDirName);

            try {
                // Make sure sourceDir is actually a directory
                if (!Exists(sourceDirName)) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // Make sure destDir does not yet exist
                if (Exists(destDirName)) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                }

                DriveInfo.GetForPath(destDirName).CreateDirectory(destDirName);

                files = Directory.GetFiles(sourceDirName);
                filesCount = files.Length;

                for (i = 0; i < filesCount; i++) {
                    File.Copy(files[i], Path.Combine(destDirName, files[i].Substring(relativePathIndex)), false, true);
                }

                files = Directory.GetDirectories(sourceDirName);
                filesCount = files.Length;

                for (i = 0; i < filesCount; i++) {
                    RecursiveCopyAndDelete(files[i], Path.Combine(destDirName, files[i].Substring(relativePathIndex)));
                }

                DriveInfo.GetForPath(sourceDirName).Delete(sourceDirName);
            }
            finally {
                FileSystemManager.RemoveFromOpenList(recordSrc);
            }
        }

        /// <summary>Deletes the empty directory at the path.</summary>
        public static void Delete(string path) => Delete(path, false);

        /// <summary>Deletes the directory at the path, optionally including its contents.</summary>
        public static void Delete(string path, bool recursive) {
            path = Path.GetFullPath(path);

            var record = FileSystemManager.LockDirectory(path);
            var drive = DriveInfo.GetForPath(path);

            try {
                var attributes = drive.GetAttributes(path);

                if ((uint)attributes == 0xFFFFFFFF) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                if (((attributes & (FileAttributes.Directory)) == 0) ||
                    ((attributes & (FileAttributes.ReadOnly)) != 0)) {
                    // it's readonly or not a directory
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                if (!Exists(path)) // make sure it is indeed a directory (and not a file)
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                if (!recursive) {
                    var ff = drive.Find(path, "*");

                    try {
                        if (ff.GetNext() != null) {
                            throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotEmpty);
                        }
                    }
                    finally {
                        ff.Close();
                    }
                }

                drive.Delete(path);
            }
            finally {
                // regardless of what happened, we need to release the directory when we're done
                FileSystemManager.UnlockDirectory(record);
            }
        }

        private static string[] GetChildren(string path, string searchPattern, bool isDirectory) {
            // path and searchPattern validation in Path.GetFullPath() and Path.NormalizePath()

            path = Path.GetFullPath(path);

            if (!Directory.Exists(path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);

            Path.NormalizePath(searchPattern, true);

            var fileNames = new ArrayList();

            var root = Path.GetPathRoot(path);
            if (false && string.Equals(root, path)) { //TODO check to see it always go here
                // This is special case. Return all the volumes.
                // Note this will not work, once we start having \\server\share like paths.

                if (isDirectory) {
                    var volumes = DriveInfo.GetDrives();
                    var count = volumes.Length;
                    for (var i = 0; i < count; i++) {
                        fileNames.Add(volumes[i].RootDirectory.Name);
                    }
                }
            }
            else {
                var record = FileSystemManager.AddToOpenListForRead(path);
                IFileSystemEntryFinder ff = null;
                try {
                    ff = DriveInfo.GetForPath(path).Find(path, searchPattern);

                    var targetAttribute = (isDirectory ? FileAttributes.Directory : 0);

                    var fileinfo = ff.GetNext();

                    while (fileinfo != null) {
                        if ((fileinfo.Attributes & FileAttributes.Directory) == targetAttribute) {
                            fileNames.Add(fileinfo.FileName);
                        }

                        fileinfo = ff.GetNext();
                    }
                }
                finally {
                    if (ff != null) ff.Close();
                    FileSystemManager.RemoveFromOpenList(record);
                }
            }

            return (string[])fileNames.ToArray(typeof(string));
        }
    }
}


