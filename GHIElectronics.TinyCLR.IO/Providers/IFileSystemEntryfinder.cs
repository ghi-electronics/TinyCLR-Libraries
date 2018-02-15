namespace GHIElectronics.TinyCLR.IO {
    public interface IFileSystemEntryfinder {
        FileSystemEntry GetNext();
        void Close();
    }
}