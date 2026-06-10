using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    /// <summary>A low-level file stream supplied by a drive provider.</summary>
    public interface IFileStream {
        /// <summary>Whether the stream supports writing.</summary>
        bool CanWrite { get; }
        /// <summary>Whether the stream supports reading.</summary>
        bool CanRead { get; }
        /// <summary>Whether the stream supports seeking.</summary>
        bool CanSeek { get; }
        /// <summary>The length of the stream in bytes.</summary>
        long Length { get; set; }

        /// <summary>Reads up to count bytes into the buffer, returning the number read.</summary>
        int Read(byte[] buffer, int offset, int count, TimeSpan timeout);
        /// <summary>Writes count bytes from the buffer, returning the number written.</summary>
        int Write(byte[] buffer, int offset, int count, TimeSpan timeout);
        /// <summary>Moves the stream position relative to the given origin and returns the new position.</summary>
        long Seek(long offset, SeekOrigin origin);
        /// <summary>Flushes any buffered data to the underlying store.</summary>
        void Flush();
        /// <summary>Closes the stream.</summary>
        void Close();
    }
}