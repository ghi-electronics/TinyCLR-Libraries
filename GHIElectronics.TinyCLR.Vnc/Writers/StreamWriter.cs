// TinyCLS OS VNC Server Library
// Copyright (C) 2020 GHI Electronics
//
// This file is a heavy modified version from T1T4N, based on VncSharp project.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;

namespace GHIElectronics.TinyCLR.Vnc {
    internal sealed class StreamWriter : Stream {
        readonly Stream stream;

        public StreamWriter(Stream input) => this.stream = input;

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() => this.stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => this.stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public void Write(byte value) => this.stream.WriteByte(value);

        public void Write(ushort value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(short value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(uint value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(int value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(ulong value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(long value) => this.FlipAndWrite(BitConverter.GetBytes(value));

        public void Write(byte[] buffer) => this.stream.Write(buffer, 0, buffer.Length);

        public override void Write(byte[] buffer, int offset, int count) => this.stream.Write(buffer, offset, count);

        private void FlipAndWrite(byte[] b) {
            BitConverter.SwapEndianness(b, b.Length);

            this.stream.Write(b, 0, b.Length);
        }
    }
}
