using System;

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
        uint Read(IBuffer buffer, uint count, InputStreamOptions options);
    }

    public interface IBuffer {
        uint Capacity { get; }
        uint Length { get; set; }
    }

    public class Buffer : IBuffer {
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

}
