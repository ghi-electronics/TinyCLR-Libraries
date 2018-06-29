using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.IO {
    public interface IFileSystemEntryFinder {
        FileSystemEntry GetNext();
        void Close();
    }

    internal class NativeFileSystemEntryFinder : IFileSystemEntryFinder {
#pragma warning disable CS0169
        IntPtr implPtr;

        object m_ff;
#pragma warning restore CS0169

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern NativeFileSystemEntryFinder(string path, string searchPattern);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern void IFileSystemEntryFinder.Close();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern FileSystemEntry IFileSystemEntryFinder.GetNext();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern FileSystemEntry GetFileInfo(string path);
    }
}