using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Storage.Streams {
    public interface IDataWriter {
        void WriteByte(byte value);
        void WriteBytes(byte[] value);
        void WriteBuffer(IBuffer buffer);
        void WriteBuffer(IBuffer buffer, uint start, uint count);
        void WriteBoolean(bool value);
        void WriteGuid(Guid value);
        void WriteInt16(short value);
        void WriteInt32(int value);
        void WriteInt64(long value);
        void WriteUInt16(ushort value);
        void WriteUInt32(uint value);
        void WriteUInt64(ulong value);
        void WriteSingle(float value);
        void WriteDouble(double value);
        void WriteDateTime(/* TODO Needs to be DateTimeOffset */ DateTime value);
        void WriteTimeSpan(TimeSpan value);
        uint WriteString(string value);
        uint MeasureString(string value);
        uint Store();
        bool Flush();
        IBuffer DetachBuffer();
        IOutputStream DetachStream();

        ByteOrder ByteOrder { get; set; }
        UnicodeEncoding UnicodeEncoding { get; set; }
        uint UnstoredBufferLength { get; }
    }

    public interface IDataReader {
        byte ReadByte();
        void ReadBytes(byte[] value);
        IBuffer ReadBuffer(uint length);
        bool ReadBoolean();
        Guid ReadGuid();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        float ReadSingle();
        double ReadDouble();
        string ReadString(uint codeUnitCount);
        /* TODO Needs to be DateTimeOffset */
        DateTime ReadDateTime();
        TimeSpan ReadTimeSpan();
        uint Load(uint count);
        IBuffer DetachBuffer();
        IInputStream DetachStream();

        ByteOrder ByteOrder { get; set; }
        InputStreamOptions InputStreamOptions { get; set; }
        uint UnconsumedBufferLength { get; }
        UnicodeEncoding UnicodeEncoding { get; set; }
    }

    public sealed class DataWriter : IDataWriter, IDisposable {
        private byte[] data;
        private uint position;
        private IOutputStream stream;

        public DataWriter(IOutputStream outputStream) : this() => this.stream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));

        public DataWriter() => this.ResetData();

        private void ResetData() {
            this.data = new byte[128];
            this.position = 0;
        }

        private IBuffer GetBuffer() => new Buffer(this.data, 0, (int)this.position, this.data.Length);

        private void EnsureSpace(uint count) {
            if (count > this.data.Length - this.position) {
                var newArr = new byte[this.data.Length * 2];

                Array.Copy(this.data, newArr, this.data.Length);

                this.data = newArr;
            }
        }

        private void Write(byte[] data, uint start, uint count) {
            this.EnsureSpace(count);

            Array.Copy(data, (int)start, this.data, (int)this.position, (int)count);

            this.position += count;
        }

        public void WriteByte(byte value) {
            this.EnsureSpace(1);

            this.data[this.position++] = value;
        }

        public void WriteBytes(byte[] value) {
            if (value == null) throw new ArgumentNullException(nameof(value));

            this.Write(value, 0, (uint)value.Length);
        }

        public void WriteBuffer(IBuffer buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            var buf = (Buffer)buffer;

            this.Write(buf.data, (uint)buf.offset, buf.Length);
        }

        public void WriteBuffer(IBuffer buffer, uint start, uint count) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            var buf = (Buffer)buffer;

            if (start > buffer.Capacity - buf.offset || count > buf.Capacity - buf.offset - start) throw new ArgumentException();

            this.Write(buf.data, (uint)buf.offset + start, count);
        }

        public void WriteBoolean(bool value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteGuid(Guid value) => this.WriteBytes(value.ToByteArray());
        public void WriteInt16(short value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteInt32(int value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteInt64(long value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteUInt16(ushort value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteUInt32(uint value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteUInt64(ulong value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteSingle(float value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteDouble(double value) => this.WriteBytes(BitConverter.GetBytes(value));
        public void WriteDateTime(/* TODO Needs to be DateTimeOffset */ DateTime value) => this.WriteInt64(value.Ticks);
        public void WriteTimeSpan(TimeSpan value) => this.WriteInt64(value.Ticks);

        public uint WriteString(string value) {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var arr = Encoding.UTF8.GetBytes(value);

            this.WriteBytes(arr);

            return (uint)arr.Length;
        }

        public uint MeasureString(string value) => (uint)Encoding.UTF8.GetBytes(value ?? throw new ArgumentNullException(nameof(value))).Length;

        public uint Store() {
            if (this.stream == null) throw new InvalidOperationException();

            var written = this.stream.Write(this.GetBuffer());

            if (written == this.position) {
                this.position = 0;
            }
            else {
                Array.Copy(this.data, (int)written, this.data, 0, (int)(this.position - written));
                this.position -= written;
            }

            return written;
        }

        public bool Flush() {
            if (this.stream == null) throw new InvalidOperationException();

            return this.stream.Flush();
        }

        public IBuffer DetachBuffer() {
            var buf = this.GetBuffer();

            this.ResetData();

            return buf;
        }

        public IOutputStream DetachStream() {
            var val = this.stream;

            this.stream = null;

            return val;
        }

        public void Dispose() {
            this.stream?.Dispose();
            this.data = null;
        }

        public UnicodeEncoding UnicodeEncoding { get => UnicodeEncoding.Utf8; set { if (value != UnicodeEncoding.Utf8) throw new NotSupportedException(); } }
        public ByteOrder ByteOrder { get => ByteOrder.LittleEndian; set { if (value != ByteOrder.LittleEndian) throw new NotSupportedException(); } }
        public uint UnstoredBufferLength => this.position;
    }

    public sealed class DataReader : IDataReader, IDisposable {
        private byte[] data;
        private Buffer buffer;
        private int validLength;
        private int position;
        private IInputStream stream;

        public DataReader(IInputStream inputStream) {
            this.stream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));

            this.ResetData();
        }

        private DataReader(byte[] data, int length) {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.validLength = length <= data.Length ? length : throw new ArgumentException(nameof(length));
            this.position = 0;
        }

        private void ResetData() {
            this.data = new byte[128];
            this.validLength = 0;
            this.position = 0;

            this.SyncBuffer();
        }

        private Buffer SyncBuffer() {
            this.buffer = this.buffer ?? new Buffer(this.data, 0, 0, this.data.Length);

            this.buffer.data = this.data;
            this.buffer.offset = this.validLength;
            this.buffer.Capacity = (uint)(this.data.Length - this.validLength);
            this.buffer.Length = 0;

            return this.buffer;
        }

        private int Advance(int length) {
            if (this.UnconsumedBufferLength < length) throw new IndexOutOfRangeException();

            var orig = this.position;

            this.position += length;

            return orig;
        }

        public Guid ReadGuid() {
            var data = new byte[16];

            this.ReadBytes(data);

            return new Guid(data);
        }

        public IBuffer ReadBuffer(uint length) {
            var data = new byte[length];

            this.ReadBytes(data);

            return new Buffer(data, 0, (int)length, (int)length);
        }

        public void ReadBytes(byte[] value) { if (value != null) Array.Copy(this.data, this.Advance(value.Length), value, 0, value.Length); else throw new ArgumentNullException(nameof(value)); }
        public byte ReadByte() => this.data[this.Advance(1)];
        public bool ReadBoolean() => this.data[this.Advance(1)] != 0;
        public short ReadInt16() => BitConverter.ToInt16(this.data, this.Advance(2));
        public int ReadInt32() => BitConverter.ToInt32(this.data, this.Advance(4));
        public long ReadInt64() => BitConverter.ToInt64(this.data, this.Advance(8));
        public ushort ReadUInt16() => BitConverter.ToUInt16(this.data, this.Advance(2));
        public uint ReadUInt32() => BitConverter.ToUInt32(this.data, this.Advance(4));
        public ulong ReadUInt64() => BitConverter.ToUInt64(this.data, this.Advance(8));
        public float ReadSingle() => BitConverter.ToSingle(this.data, this.Advance(4));
        public double ReadDouble() => BitConverter.ToDouble(this.data, this.Advance(8));
        public /* TODO Needs to be DateTimeOffset */ DateTime ReadDateTime() => new DateTime(this.ReadInt64());
        public TimeSpan ReadTimeSpan() => new TimeSpan(this.ReadInt64());
        public string ReadString(uint codeUnitCount) => new string(Encoding.UTF8.GetChars(this.data, this.Advance((int)codeUnitCount), (int)codeUnitCount)); //TODO should this return based on number of chars, not bytes?

        public uint Load(uint count) {
            if (this.stream == null) throw new InvalidOperationException();

            if (this.data.Length - this.validLength < count) {
                var newLen = 128;

                while (newLen < count + this.UnconsumedBufferLength)
                    newLen *= 2;

                var arr = new byte[newLen];

                Array.Copy(this.data, this.position, arr, 0, (int)this.UnconsumedBufferLength);

                this.validLength = (int)this.UnconsumedBufferLength;
                this.position = 0;
                this.data = arr;
            }

            var read = this.stream.Read(this.SyncBuffer(), count, this.InputStreamOptions);

            this.validLength += (int)read.Length;

            return read.Length;
        }

        public IBuffer DetachBuffer() {
            var data = new byte[this.UnconsumedBufferLength];

            this.ReadBytes(data);
            this.ResetData();

            return new Buffer(data, 0, data.Length, data.Length);
        }

        public IInputStream DetachStream() {
            var val = this.stream;

            this.stream = null;

            return val;
        }

        public void Dispose() {
            this.stream?.Dispose();
            this.data = null;
        }

        public static DataReader FromBuffer(IBuffer buffer) => ((Buffer)buffer).offset == 0 ? new DataReader(((Buffer)buffer).data, (int)buffer.Length) : throw new NotSupportedException();

        public UnicodeEncoding UnicodeEncoding { get => UnicodeEncoding.Utf8; set { if (value != UnicodeEncoding.Utf8) throw new NotSupportedException(); } }
        public ByteOrder ByteOrder { get => ByteOrder.LittleEndian; set { if (value != ByteOrder.LittleEndian) throw new NotSupportedException(); } }
        public InputStreamOptions InputStreamOptions { get; set; }
        public uint UnconsumedBufferLength => (uint)(this.validLength - this.position);
    }

    public enum ByteOrder {
        LittleEndian = 0,
        BigEndian = 1
    }

    public enum UnicodeEncoding {
        Utf8 = 0,
        Utf16LE = 1,
        Utf16BE = 2
    }
}