
using System;
using System.IO;

namespace GHIElectronics.TinyCLR.Vnc {

    internal sealed class StreamReader : Stream {
        private readonly byte[] buff = new byte[4];

        private readonly Stream stream;

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StreamReader(Stream input) => this.stream = input;

        public StreamReader(Stream input, System.Text.Encoding encoding) => this.stream = input;

        public ushort ReadUInt16() {
            this.FillBuff(2);
            return (ushort)(((uint)this.buff[1]) | ((uint)this.buff[0]) << 8);
        }

        public short ReadInt16() {
            this.FillBuff(2);
            return (short)(this.buff[1] & 0xFF | this.buff[0] << 8);
        }

        public uint ReadUInt32() {
            this.FillBuff(4);
            return (uint)(((uint)this.buff[3]) & 0xFF | ((uint)this.buff[2]) << 8 | ((uint)this.buff[1]) << 16 | ((uint)this.buff[0]) << 24);
        }

        public int ReadInt32() {
            this.FillBuff(4);
            return (int)(this.buff[3] | this.buff[2] << 8 | this.buff[1] << 16 | this.buff[0] << 24);
        }

        public byte[] ReadBytes(int count) {
            var data = new byte[count];

            var total = 0;
            while (total < count) {
                total += this.stream.Read(data, 0, count - total);
            }

            return data;
        }

        private void FillBuff(int totalBytes) {
            var bytesRead = 0;
            do {
                var n = this.stream.Read(this.buff, bytesRead, totalBytes - bytesRead);
                if (n == 0)
                    throw new IOException("Unable to read next byte(s).");

                bytesRead += n;
            } while (bytesRead < totalBytes);
        }

        public override void Flush() => this.stream.Flush();

        public override long Seek(long offset, SeekOrigin origin) => this.stream.Seek(offset, origin);

        public override void SetLength(long value) => this.stream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => this.stream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => this.stream.Write(buffer, offset, count);
    }
}
