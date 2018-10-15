using System;
using System.IO;

namespace GHIElectronics.TinyCLR.IO {
    public interface IFileStream {
        bool CanWrite { get; }
        bool CanRead { get; }
        bool CanSeek { get; }
        long Length { get; set; }

        int Read(byte[] buffer, int offset, int count, TimeSpan timeout);
        int Write(byte[] buffer, int offset, int count, TimeSpan timeout);
        long Seek(long offset, SeekOrigin origin);
        void Flush();
        void Close();
    }
}