extern alias bcl;
using System;
using System.Collections;
using System.IO;
using BclFile = bcl::System.IO.File;
using BclDirectory = bcl::System.IO.Directory;
using BclFileStream = bcl::System.IO.FileStream;
using BclPath = bcl::System.IO.Path;
using BclFileInfo = bcl::System.IO.FileInfo;
using BclDirectoryInfo = bcl::System.IO.DirectoryInfo;
using BclEnvironment = bcl::System.Environment;

namespace GHIElectronics.TinyCLR.IO {
    public static class FileSystem {
        private static readonly IDictionary mounted = new Hashtable();

        internal static string BaseFolder {
            get {
                var folder = BclEnvironment.GetEnvironmentVariable("TINYCLR_DUALMODE_FS_ROOT");
                if (string.IsNullOrEmpty(folder))
                    folder = BclPath.Combine(BclPath.GetTempPath(), "TinyCLRDrives");
                BclDirectory.CreateDirectory(folder);
                return folder;
            }
        }

        public static IDriveProvider Mount(IntPtr hdc) {
            if (FileSystem.mounted.Contains(hdc))
                throw new InvalidOperationException("Already mounted");

            var drive = new DesktopDriveProvider();
            var provider = DriveInfo.RegisterDriveProvider(drive);
            mounted[hdc] = drive;
            return provider;
        }

        public static bool Unmount(IntPtr hdc) {
            if (!FileSystem.mounted.Contains(hdc))
                throw new InvalidOperationException("Not mounted");

            var drive = (IDriveProvider)FileSystem.mounted[hdc];
            FileSystem.mounted.Remove(hdc);
            DriveInfo.DeregisterDriveProvider(drive);
            return true;
        }

        public static void Flush(IntPtr hdc) {
            // No-op on Desktop: System.IO.FileStream.Flush is per-stream; nothing to flush at the volume level.
        }

        public static bool Format(IntPtr hdc, string volume = null, uint parameter = 0, byte forceSize = 0) {
            if (forceSize > 2)
                throw new ArgumentOutOfRangeException("forceSize", "Valid values: 0 (100%), 1 (75%), 2 (50%).");

            if (!FileSystem.mounted.Contains(hdc))
                return false;

            var drive = (DesktopDriveProvider)FileSystem.mounted[hdc];
            drive.FormatBackingFolder();
            return true;
        }

        internal static string TranslateToDesktop(string driveRoot, string tinyClrPath) {
            // driveRoot is e.g. "Z:\", tinyClrPath is e.g. "Z:\foo\bar.txt".
            // Map to <BaseFolder>\<DriveLetter>\foo\bar.txt
            if (string.IsNullOrEmpty(tinyClrPath)) return tinyClrPath;
            var letter = driveRoot.Length > 0 ? driveRoot[0].ToString() : "_";
            string remainder;
            if (tinyClrPath.Length >= 3 && tinyClrPath[1] == ':' && tinyClrPath[2] == '\\')
                remainder = tinyClrPath.Substring(3);
            else if (tinyClrPath.StartsWith("\\"))
                remainder = tinyClrPath.Substring(1);
            else
                remainder = tinyClrPath;
            return BclPath.Combine(BaseFolder, letter, remainder);
        }
    }

    internal sealed class DesktopDriveProvider : IDriveProvider {
        private string driveRoot;
        private string backingFolder;

        public string Name => this.driveRoot;
        public DriveType DriveType => DriveType.Fixed;
        public string DriveFormat => "Desktop";
        public bool IsReady => this.backingFolder != null && BclDirectory.Exists(this.backingFolder);
        public string VolumeLabel => "TinyCLR Desktop " + this.driveRoot;

        public long AvailableFreeSpace {
            get {
                try { return new bcl::System.IO.DriveInfo(BclPath.GetPathRoot(this.backingFolder)).AvailableFreeSpace; }
                catch { return 0; }
            }
        }
        public long TotalFreeSpace => this.AvailableFreeSpace;
        public long TotalSize {
            get {
                try { return new bcl::System.IO.DriveInfo(BclPath.GetPathRoot(this.backingFolder)).TotalSize; }
                catch { return 0; }
            }
        }

        public void Initialize(string name) {
            this.driveRoot = name;
            var letter = name.Length > 0 ? name[0].ToString() : "_";
            this.backingFolder = BclPath.Combine(FileSystem.BaseFolder, letter);
            BclDirectory.CreateDirectory(this.backingFolder);
        }

        internal void FormatBackingFolder() {
            if (this.backingFolder == null) return;
            if (BclDirectory.Exists(this.backingFolder)) {
                BclDirectory.Delete(this.backingFolder, true);
            }
            BclDirectory.CreateDirectory(this.backingFolder);
        }

        private string Map(string path) => FileSystem.TranslateToDesktop(this.driveRoot, path);

        public IFileStream OpenFile(string path, int bufferSize) {
            var dt = this.Map(path);
            var dir = BclPath.GetDirectoryName(dt);
            if (!string.IsNullOrEmpty(dir)) BclDirectory.CreateDirectory(dir);
            // The TinyCLR FileStream layer has already validated mode/access/share and decided to call OpenFile.
            // We open with permissive mode here and let the upper layer manage Length/Seek as needed.
            var fs = new BclFileStream(dt,
                bcl::System.IO.FileMode.OpenOrCreate,
                bcl::System.IO.FileAccess.ReadWrite,
                bcl::System.IO.FileShare.ReadWrite,
                bufferSize > 0 ? bufferSize : 4096);
            return new DesktopFileStream(fs);
        }

        public void Delete(string path) {
            var dt = this.Map(path);
            if (BclFile.Exists(dt)) BclFile.Delete(dt);
            else if (BclDirectory.Exists(dt)) BclDirectory.Delete(dt, true);
        }

        public bool Move(string source, string destination) {
            var src = this.Map(source);
            var dst = this.Map(destination);
            var dstDir = BclPath.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(dstDir)) BclDirectory.CreateDirectory(dstDir);
            if (BclFile.Exists(src)) { BclFile.Move(src, dst); return true; }
            if (BclDirectory.Exists(src)) { BclDirectory.Move(src, dst); return true; }
            return false;
        }

        public void CreateDirectory(string path) {
            var dt = this.Map(path);
            BclDirectory.CreateDirectory(dt);
        }

        public FileAttributes GetAttributes(string path) {
            var dt = this.Map(path);
            if (!BclFile.Exists(dt) && !BclDirectory.Exists(dt))
                return unchecked((FileAttributes)0xFFFFFFFF);
            return (FileAttributes)(int)BclFile.GetAttributes(dt);
        }

        public void SetAttributes(string path, FileAttributes attributes) {
            var dt = this.Map(path);
            BclFile.SetAttributes(dt, (bcl::System.IO.FileAttributes)(int)attributes);
        }

        public FileSystemEntry GetFileSystemEntry(string path) {
            var dt = this.Map(path);
            if (BclFile.Exists(dt)) {
                var fi = new BclFileInfo(dt);
                return new FileSystemEntry {
                    FileName = BclPath.GetFileName(path),
                    Attributes = (FileAttributes)(int)fi.Attributes,
                    CreationTime = fi.CreationTime,
                    LastAccessTime = fi.LastAccessTime,
                    LastWriteTime = fi.LastWriteTime,
                    Size = fi.Length,
                };
            }
            if (BclDirectory.Exists(dt)) {
                var di = new BclDirectoryInfo(dt);
                return new FileSystemEntry {
                    FileName = BclPath.GetFileName(path),
                    Attributes = (FileAttributes)(int)di.Attributes,
                    CreationTime = di.CreationTime,
                    LastAccessTime = di.LastAccessTime,
                    LastWriteTime = di.LastWriteTime,
                    Size = 0,
                };
            }
            return null;
        }

        public IFileSystemEntryFinder Find(string path, string searchPattern) {
            var dt = this.Map(path);
            return new DesktopFileSystemEntryFinder(dt, searchPattern);
        }
    }

    internal sealed class DesktopFileStream : IFileStream, IDisposable {
        private BclFileStream inner;

        public DesktopFileStream(BclFileStream inner) => this.inner = inner;

        public bool CanWrite => this.inner.CanWrite;
        public bool CanRead => this.inner.CanRead;
        public bool CanSeek => this.inner.CanSeek;

        public long Length {
            get => this.inner.Length;
            set => this.inner.SetLength(value);
        }

        public void Close() { this.inner?.Close(); this.inner = null; }
        public void Flush() => this.inner?.Flush();

        public int Read(byte[] buffer, int offset, int count, TimeSpan timeout) =>
            this.inner.Read(buffer, offset, count);

        public int Write(byte[] buffer, int offset, int count, TimeSpan timeout) {
            this.inner.Write(buffer, offset, count);
            return count;
        }

        public long Seek(long offset, SeekOrigin origin) => this.inner.Seek(offset, (bcl::System.IO.SeekOrigin)(int)origin);

        public void Dispose() { this.Close(); GC.SuppressFinalize(this); }
        ~DesktopFileStream() => this.Close();
    }
}
