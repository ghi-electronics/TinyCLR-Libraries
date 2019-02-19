using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.IO {
    public static class FileSystem {
        private static readonly IDictionary mounted = new Hashtable();

        public static IDriveProvider Mount(IntPtr hdc) {
            if (FileSystem.mounted.Contains(hdc))
                throw new InvalidOperationException("Already mounted");

            var drive = new NativeDriveProvider();

            var provider = DriveInfo.RegisterDriveProvider(drive);

            FileSystem.Initialize(hdc, provider.Name);

            mounted[hdc] = drive;

            return drive;
        }

        public static bool Unmount(IntPtr hdc) {
            if (!FileSystem.mounted.Contains(hdc))
                throw new InvalidOperationException("Not mounted");

            var drive = (IDriveProvider)FileSystem.mounted[hdc];

            FileSystem.mounted.Remove(hdc);

            DriveInfo.DeregisterDriveProvider(drive);

            return FileSystem.Uninitialize(hdc);
        }

        public static void Flush(IntPtr hdc) => FileSystem.FlushAll(hdc);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void FlushAll(IntPtr nativeProvider);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void Initialize(IntPtr nativeProvider, string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool Uninitialize(IntPtr nativeProvider);

        private class NativeDriveProvider : IDriveProvider {
            private bool initialized;

            public extern DriveType DriveType { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern string DriveFormat { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool IsReady { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long AvailableFreeSpace { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long TotalFreeSpace { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long TotalSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern string VolumeLabel { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public string Name { get; private set; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CreateDirectory(string path);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Delete(string path);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern FileAttributes GetAttributes(string path);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern FileSystemEntry GetFileSystemEntry(string path);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool Move(string source, string destination);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetAttributes(string path, FileAttributes attributes);

            public IFileSystemEntryFinder Find(string path, string searchPattern) => new NativeFileSystemEntryFinder(path, searchPattern);

            public IFileStream OpenFile(string path, int bufferSize) => new NativeFileStream(path, bufferSize);

            public void Initialize(string name) {
                if (this.initialized) throw new InvalidOperationException();

                this.initialized = true;
                this.Name = name;
            }
        }

        private class NativeFileStream : IFileStream, IDisposable {
#pragma warning disable CS0169
            private IntPtr impl;

#pragma warning restore CS0169
            ~NativeFileStream() => this.Dispose();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern NativeFileStream(string path, int bufferSize);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Dispose();

            public extern bool CanWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool CanRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool CanSeek { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long Length { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] buffer, int offset, int count, TimeSpan timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern long Seek(long offset, SeekOrigin origin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] buffer, int offset, int count, TimeSpan timeout);
        }
    }
}
