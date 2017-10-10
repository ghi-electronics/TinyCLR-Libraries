using System.Globalization;
using System.Runtime.InteropServices;

namespace System {
    [Serializable]
    [ComVisible(true)]
    public struct IntPtr {
        private readonly int value;

        public static readonly IntPtr Zero;

        public IntPtr(int value) => this.value = value;

        public IntPtr(long value) {
            if (value > int.MaxValue) throw new OverflowException();

            this.value = (int)value;
        }

        public override int GetHashCode() => this.value;

        public override bool Equals(object obj) => obj is IntPtr o && this.value == o.value;

        public int ToInt32() => this.value;
        public long ToInt64() => this.value;

        public override string ToString() => this.ToString(null);
        public string ToString(string format) => this.value.ToString(format, CultureInfo.InvariantCulture);

        public static explicit operator IntPtr(int value) => new IntPtr(value);
        public static explicit operator IntPtr(long value) => new IntPtr(value);

        public static explicit operator int(IntPtr value) => value.value;
        public static explicit operator long(IntPtr value) => value.value;

        public static bool operator ==(IntPtr value1, IntPtr value2) => value1.value == value2.value;
        public static bool operator !=(IntPtr value1, IntPtr value2) => value1.value != value2.value;
        public static IntPtr operator +(IntPtr pointer, int offset) => new IntPtr(pointer.ToInt32() + offset);
        public static IntPtr operator -(IntPtr pointer, int offset) => new IntPtr(pointer.ToInt32() - offset);

        public static IntPtr Add(IntPtr pointer, int offset) => pointer + offset;
        public static IntPtr Subtract(IntPtr pointer, int offset) => pointer - offset;

        public static int Size => 4;
    }
}
