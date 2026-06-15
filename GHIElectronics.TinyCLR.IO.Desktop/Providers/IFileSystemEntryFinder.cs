extern alias bcl;
using System;
using System.Collections;
using System.IO;
using BclDirectory = bcl::System.IO.Directory;
using BclPath = bcl::System.IO.Path;
using BclFile = bcl::System.IO.File;
using BclFileInfo = bcl::System.IO.FileInfo;
using BclDirectoryInfo = bcl::System.IO.DirectoryInfo;
using BclSearchOption = bcl::System.IO.SearchOption;

namespace GHIElectronics.TinyCLR.IO {
    public interface IFileSystemEntryFinder {
        FileSystemEntry GetNext();
        void Close();
    }

    internal sealed class DesktopFileSystemEntryFinder : IFileSystemEntryFinder, IDisposable {
        private readonly string searchRoot;
        private IEnumerator entries;
        private bool closed;

        public DesktopFileSystemEntryFinder(string desktopPath, string searchPattern) {
            this.searchRoot = desktopPath;
            if (BclDirectory.Exists(desktopPath)) {
                var pattern = string.IsNullOrEmpty(searchPattern) ? "*" : searchPattern;
                this.entries = BclDirectory.EnumerateFileSystemEntries(desktopPath, pattern, BclSearchOption.TopDirectoryOnly).GetEnumerator();
            }
            else {
                this.entries = new ArrayList().GetEnumerator();
            }
        }

        public FileSystemEntry GetNext() {
            if (this.closed || this.entries == null) return null;
            if (!this.entries.MoveNext()) return null;
            var fullPath = (string)this.entries.Current;
            if (BclFile.Exists(fullPath)) {
                var fi = new BclFileInfo(fullPath);
                return new FileSystemEntry {
                    FileName = fi.Name,
                    Attributes = (FileAttributes)(int)fi.Attributes,
                    CreationTime = fi.CreationTime,
                    LastAccessTime = fi.LastAccessTime,
                    LastWriteTime = fi.LastWriteTime,
                    Size = fi.Length,
                };
            }
            if (BclDirectory.Exists(fullPath)) {
                var di = new BclDirectoryInfo(fullPath);
                return new FileSystemEntry {
                    FileName = di.Name,
                    Attributes = (FileAttributes)(int)di.Attributes,
                    CreationTime = di.CreationTime,
                    LastAccessTime = di.LastAccessTime,
                    LastWriteTime = di.LastWriteTime,
                    Size = 0,
                };
            }
            return null;
        }

        public void Close() { this.closed = true; this.entries = null; }

        public void Dispose() { this.Close(); GC.SuppressFinalize(this); }

        ~DesktopFileSystemEntryFinder() => this.Close();

        // Used by FileEnum and other internals when looking up a single path's metadata.
        public static FileSystemEntry GetFileInfo(string desktopPath) {
            if (BclFile.Exists(desktopPath)) {
                var fi = new BclFileInfo(desktopPath);
                return new FileSystemEntry {
                    FileName = fi.Name,
                    Attributes = (FileAttributes)(int)fi.Attributes,
                    CreationTime = fi.CreationTime,
                    LastAccessTime = fi.LastAccessTime,
                    LastWriteTime = fi.LastWriteTime,
                    Size = fi.Length,
                };
            }
            if (BclDirectory.Exists(desktopPath)) {
                var di = new BclDirectoryInfo(desktopPath);
                return new FileSystemEntry {
                    FileName = di.Name,
                    Attributes = (FileAttributes)(int)di.Attributes,
                    CreationTime = di.CreationTime,
                    LastAccessTime = di.LastAccessTime,
                    LastWriteTime = di.LastWriteTime,
                    Size = 0,
                };
            }
            return null;
        }
    }
}
