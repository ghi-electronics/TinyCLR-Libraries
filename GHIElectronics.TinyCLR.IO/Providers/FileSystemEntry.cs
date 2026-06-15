using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    /// <summary>Describes a single file or directory returned by a drive provider.</summary>
    public class FileSystemEntry {
        /// <summary>The attributes of the entry.</summary>
        public FileAttributes Attributes { get; set; }
        /// <summary>The time the entry was created.</summary>
        public DateTime CreationTime { get; set; }
        /// <summary>The time the entry was last accessed.</summary>
        public DateTime LastAccessTime { get; set; }
        /// <summary>The time the entry was last written.</summary>
        public DateTime LastWriteTime { get; set; }
        /// <summary>The size of the entry in bytes.</summary>
        public long Size { get; set; }
        /// <summary>The name of the entry.</summary>
        public string FileName { get; set; }
    }
}