using System.Globalization;
using System.Runtime.InteropServices;

namespace System {
    [Serializable]
    [CLSCompliant(false)]
    [ComVisible(true)]
    public struct UIntPtr {
        private readonly uint value;

        public static readonly UIntPtr Zero;

        public UIntPtr(uint value) => this.value = value;

        public UIntPtr(ulong value) {
            if (value > uint.MaxValue) throw new OverflowException();

            this.value = (uint)value;
        }

        public override int GetHashCode() => unchecked((int)this.value) & 0x7FFFFFFF;

        public override bool Equals(object obj) => obj is UIntPtr o && this.value == o.value;

        public uint ToUInt32() => this.value;
        public ulong ToUInt64() => this.value;

        public override string ToString() => this.ToString(null);
        public string ToString(string format) => this.value.ToString(format, CultureInfo.InvariantCulture);

        public static explicit operator UIntPtr(uint value) => new UIntPtr(value);
        public static explicit operator UIntPtr(ulong value) => new UIntPtr(value);

        public static explicit operator uint(UIntPtr value) => value.value;
        public static explicit operator ulong(UIntPtr value) => value.value;

        public static bool operator ==(UIntPtr value1, UIntPtr value2) => value1.value == value2.value;
        public static bool operator !=(UIntPtr value1, UIntPtr value2) => value1.value != value2.value;
        public static UIntPtr operator +(UIntPtr pointer, int offset) => new UIntPtr(pointer.ToUInt32() + (uint)offset);
        public static UIntPtr operator -(UIntPtr pointer, int offset) => new UIntPtr(pointer.ToUInt32() - (uint)offset);

        public static UIntPtr Add(UIntPtr pointer, int offset) => pointer + offset;
        public static UIntPtr Subtract(UIntPtr pointer, int offset) => pointer - offset;

        public static int Size => 4;
    }
}
