using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.IO {
    /// <summary>Iterates over the entries returned by a drive provider's search.</summary>
    public interface IFileSystemEntryFinder {
        /// <summary>Returns the next entry, or null when there are no more.</summary>
        FileSystemEntry GetNext();
        /// <summary>Closes the finder and releases its resources.</summary>
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