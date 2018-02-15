using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    public class FileSystemEntry {
        public FileAttributes Attributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Size { get; set; }
        public string FileName { get; set; }
    }
}