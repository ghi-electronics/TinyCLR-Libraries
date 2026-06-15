using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    /// <summary>Backing store for a mounted drive that the IO library calls into.</summary>
    public interface IDriveProvider {
        /// <summary>The type of the drive.</summary>
        DriveType DriveType { get; }
        /// <summary>The name of the file system format on the drive.</summary>
        string DriveFormat { get; }
        /// <summary>Whether the drive is ready for access.</summary>
        bool IsReady { get; }
        /// <summary>The amount of free space available on the drive in bytes.</summary>
        long AvailableFreeSpace { get; }
        /// <summary>The total free space on the drive in bytes.</summary>
        long TotalFreeSpace { get; }
        /// <summary>The total size of the drive in bytes.</summary>
        long TotalSize { get; }
        /// <summary>The volume label of the drive.</summary>
        string VolumeLabel { get; }
        /// <summary>The root name assigned to the drive.</summary>
        string Name { get; }

        /// <summary>Returns a finder for the entries under the path that match the search pattern.</summary>
        IFileSystemEntryFinder Find(string path, string searchPattern);
        /// <summary>Returns metadata for the entry at the path, or null if it does not exist.</summary>
        FileSystemEntry GetFileSystemEntry(string path);
        /// <summary>Opens the file at the path and returns a stream to it.</summary>
        IFileStream OpenFile(string path, int bufferSize);
        /// <summary>Deletes the file or directory at the path.</summary>
        void Delete(string path);
        /// <summary>Moves an entry from the source path to the destination path.</summary>
        bool Move(string source, string destination);
        /// <summary>Creates a directory at the path.</summary>
        void CreateDirectory(string path);
        /// <summary>Returns the attributes of the entry at the path.</summary>
        FileAttributes GetAttributes(string path);
        /// <summary>Sets the attributes of the entry at the path.</summary>
        void SetAttributes(string path, FileAttributes attributes);
        /// <summary>Initializes the provider with the given root name.</summary>
        void Initialize(string name);
    }
}