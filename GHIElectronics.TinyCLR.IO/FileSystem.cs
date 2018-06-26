using System;
using System.IO;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.SdCard;

namespace GHIElectronics.TinyCLR.IO {
    public static class FileSystem {
        public static IDriveProvider Mount(SdCardController sdCard) {
            var drive = new NativeDriveProvider();

            var provider = DriveInfo.RegisterDriveProvider(drive);

            FileSystem.Initialize(sdCard.Hdc, sdCard.ControllerIndex, provider.Name);

            return drive;
        }

        public static bool Unmount(SdCardController sdCard) => FileSystem.Uninitialize(sdCard.Hdc, sdCard.ControllerIndex);

        public static void Flush(SdCardController sdCard) => FileSystem.FlushAll(sdCard.Hdc, sdCard.ControllerIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void FlushAll(IntPtr nativeProvider, int controllerIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void Initialize(IntPtr nativeProvider, int controllerIndex, string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool Uninitialize(IntPtr nativeProvider, int controllerIndex);

        private class NativeDriveProvider : IDriveProvider {
            public extern DriveType DriveType { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern string DriveFormat { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool IsReady { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long AvailableFreeSpace { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long TotalFreeSpace { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long TotalSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern string VolumeLabel { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public string Name { get; set; }

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

            public IFileSystemEntryfinder Find(string path, string searchPattern) => new NativeFileSystemEntryfinder(path, searchPattern);

            public IFileStream OpenFile(string path, int bufferSize) => new NativeFileStream(path, bufferSize);
        }

        private class NativeFileStream : IFileStream {
            private IntPtr impl;

            private object obj;

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern NativeFileStream(string path, int bufferSize);

            public extern bool CanWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool CanRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool CanSeek { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern long Length { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern long Seek(long offset, SeekOrigin origin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] buffer, int offset, int count, int timeout);
        }
    }
}
