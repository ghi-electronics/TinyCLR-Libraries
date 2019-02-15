using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.IO {
    public interface IFileSystemEntryFinder {
        FileSystemEntry GetNext();
        void Close();
    }

    internal class NativeFileSystemEntryFinder : IFileSystemEntryFinder, IDisposable {
#pragma warning disable CS0169
        IntPtr implPtr;

#pragma warning restore CS0169
        ~NativeFileSystemEntryFinder() => this.Dispose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern NativeFileSystemEntryFinder(string path, string searchPattern);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Dispose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern void IFileSystemEntryFinder.Close();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern FileSystemEntry IFileSystemEntryFinder.GetNext();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern FileSystemEntry GetFileInfo(string path);
    }
}