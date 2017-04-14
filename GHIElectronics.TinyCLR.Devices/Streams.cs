using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Storage.Streams {
    public enum InputStreamOptions {
        None = 0,
        Partial = 1,
        ReadAhead = 2
    }

    public interface IOutputStream : IDisposable {
        bool Flush();
        uint Write(IBuffer buffer);
    }

    public interface IInputStream : IDisposable {
        IBuffer Read(IBuffer buffer, uint count, InputStreamOptions options);
    }

    public interface IBuffer {
        uint Capacity { get; }
        uint Length { get; set; }
    }

    public sealed class Buffer : IBuffer {
        internal byte[] data;
        internal int offset;
        private uint length;

        public uint Capacity { get; }

        public uint Length {
            get => this.length;
            set {
                if (value > this.Capacity) throw new ArgumentOutOfRangeException(nameof(value));

                this.length = value;
            }
        }

        public Buffer(uint capacity) {
            this.data = new byte[capacity];
            this.offset = 0;
            this.length = 0;
            this.Capacity = capacity;
        }

        internal Buffer(byte[] data, int offset, int length, int capacity) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (length > capacity || data.Length - offset < length || data.Length - offset < capacity) throw new ArgumentException();

            this.data = data;
            this.offset = offset;
            this.length = (uint)length;
            this.Capacity = (uint)capacity;
        }
    }

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
        public void WriteDateTime(/* TODO Needs to be DateTimeOffset */ DateTime value) => this.WriteInt64(value.Ticks - 504911232000000000);
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

        public UnicodeEncoding UnicodeEncoding { get => UnicodeEncoding.Utf8; set => throw new NotSupportedException(); }
        public ByteOrder ByteOrder { get => ByteOrder.LittleEndian; set => throw new NotSupportedException(); }
        public uint UnstoredBufferLength => this.position;
    }

#if false
    public sealed class DataReader : IDataReader, IDisposable {
        //
        // Summary:
        //     Creates and initializes a new instance of the data reader.
        //
        // Parameters:
        //   inputStream:
        //     The input stream.
        public DataReader(IInputStream inputStream);

        //
        // Summary:
        //     Reads a byte value from the input stream.
        //
        // Returns:
        //     The value.
        public byte ReadByte();
        //
        // Summary:
        //     Reads an array of byte values from the input stream.
        //
        // Parameters:
        //   value:
        //     The array that receives the byte values.
        public void ReadBytes(byte[] value);
        //
        // Summary:
        //     Reads a buffer from the input stream.
        //
        // Parameters:
        //   length:
        //     The length of the buffer, in bytes.
        //
        // Returns:
        //     The buffer.
        public IBuffer ReadBuffer(uint length);
        //
        // Summary:
        //     Reads a Boolean value from the input stream.
        //
        // Returns:
        //     The value.
        public bool ReadBoolean();
        //
        // Summary:
        //     Reads a GUID value from the input stream.
        //
        // Returns:
        //     The value.
        public Guid ReadGuid();
        //
        // Summary:
        //     Reads a 16-bit integer value from the input stream.
        //
        // Returns:
        //     The value.
        public short ReadInt16();
        //
        // Summary:
        //     Reads a 32-bit integer value from the input stream.
        //
        // Returns:
        //     The value.
        public int ReadInt32();
        //
        // Summary:
        //     Reads a 64-bit integer value from the input stream.
        //
        // Returns:
        //     The value.
        public long ReadInt64();
        //
        // Summary:
        //     Reads a 16-bit unsigned integer from the input stream.
        //
        // Returns:
        //     The value.
        public ushort ReadUInt16();
        //
        // Summary:
        //     Reads a 32-bit unsigned integer from the input stream.
        //
        // Returns:
        //     The value.
        public uint ReadUInt32();
        //
        // Summary:
        //     Reads a 64-bit unsigned integer from the input stream.
        //
        // Returns:
        //     The value.
        public ulong ReadUInt64();
        //
        // Summary:
        //     Reads a floating-point value from the input stream.
        //
        // Returns:
        //     The value.
        public float ReadSingle();
        //
        // Summary:
        //     Reads a floating-point value from the input stream.
        //
        // Returns:
        //     The value.
        public double ReadDouble();
        //
        // Summary:
        //     Reads a string value from the input stream.
        //
        // Parameters:
        //   codeUnitCount:
        //     The length of the string.
        //
        // Returns:
        //     The value.
        public string ReadString(uint codeUnitCount);
        //
        // Summary:
        //     Reads a date and time value from the input stream.
        //
        // Returns:
        //     The value.
        public /* TODO Needs to be DateTimeOffset */ DateTime ReadDateTime();
        //
        // Summary:
        //     Reads a time-interval value from the input stream.
        //
        // Returns:
        //     The value.
        public TimeSpan ReadTimeSpan();
        //
        // Summary:
        //     Loads data from the input stream.
        //
        // Parameters:
        //   count:
        //     The count of bytes to load into the intermediate buffer.
        //
        // Returns:
        //     The asynchronous load data request.
        public uint Load(uint count) {
            //In place of the DataReceived event, we can spin up another thread and block inside Load (when InputStreamOptions is set to Partial) until data is received or timed out
        }
        //
        // Summary:
        //     Detaches the buffer that is associated with the data reader.
        //
        // Returns:
        //     The detached buffer.
        public IBuffer DetachBuffer();
        //
        // Summary:
        //     Detaches the stream that is associated with the data reader.
        //
        // Returns:
        //     The detached stream.
        public IInputStream DetachStream();
        public void Dispose();
        //
        // Summary:
        //     Creates a new instance of the data reader with data from the specified buffer.
        //
        // Parameters:
        //   buffer:
        //     The buffer.
        //
        // Returns:
        //     The data reader.
        public static DataReader FromBuffer(IBuffer buffer);

        //
        // Summary:
        //     Gets or sets the Unicode character encoding for the input stream.
        //
        // Returns:
        //     One of the enumeration values.
        public UnicodeEncoding UnicodeEncoding { get; set; }
        //
        // Summary:
        //     Gets or sets the read options for the input stream.
        //
        // Returns:
        //     One of the enumeration values.
        public InputStreamOptions InputStreamOptions { get; set; }
        //
        // Summary:
        //     Gets or sets the byte order of the data in the input stream.
        //
        // Returns:
        //     One of the enumeration values.
        public ByteOrder ByteOrder { get; set; }
        //
        // Summary:
        //     Gets the size of the buffer that has not been read.
        //
        // Returns:
        //     The size of the buffer that has not been read, in bytes.
        public uint UnconsumedBufferLength { get; }
    }
#endif

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