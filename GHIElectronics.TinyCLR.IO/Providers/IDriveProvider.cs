using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    public interface IDriveProvider {
        DriveType DriveType { get; }
        string DriveFormat { get; }
        bool IsReady { get; }
        long AvailableFreeSpace { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
        string VolumeLabel { get; }
        string Name { get; }

        IFileSystemEntryFinder Find(string path, string searchPattern);
        FileSystemEntry GetFileSystemEntry(string path);
        IFileStream OpenFile(string path, int bufferSize);
        void Delete(string path);
        bool Move(string source, string destination);
        void CreateDirectory(string path);
        FileAttributes GetAttributes(string path);
        void SetAttributes(string path, FileAttributes attributes);
        void Initialize(string name);
    }
}