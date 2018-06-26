using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.IO {
    public interface IFileSystemEntryfinder {
        FileSystemEntry GetNext();
        void Close();
    }

    public class NativeFileSystemEntryfinder : IFileSystemEntryfinder {
        IntPtr implPtr;

        object m_ff;

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern NativeFileSystemEntryfinder(string path, string searchPattern);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern void IFileSystemEntryfinder.Close();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern FileSystemEntry IFileSystemEntryfinder.GetNext();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern FileSystemEntry GetFileInfo(string path);
    }
}