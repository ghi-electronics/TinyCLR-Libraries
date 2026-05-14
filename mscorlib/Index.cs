namespace System {
    // C# 8. Represents an index into a sequence - either from the start
    // (`new Index(0)`, `0` via implicit conversion) or from the end (`^1`,
    // which becomes `new Index(1, fromEnd: true)`). Internally stored as
    // bitwise-complement: positive = from-start, negative = from-end.
    // IEquatable<Index> not yet in TinyCLR mscorlib (see Version.cs note); the
    // typed Equals(Index) method is enough for the compiler-generated paths.
    public readonly struct Index {
        private readonly int _value;

        public Index(int value, bool fromEnd = false) {
            if (value < 0) throw new ArgumentOutOfRangeException();
            this._value = fromEnd ? ~value : value;
        }

        private Index(int value) { this._value = value; }

        public static Index Start => new Index(0);
        public static Index End => new Index(~0);

        public static Index FromStart(int value) {
            if (value < 0) throw new ArgumentOutOfRangeException();
            return new Index(value);
        }

        public static Index FromEnd(int value) {
            if (value < 0) throw new ArgumentOutOfRangeException();
            return new Index(~value);
        }

        public int Value => this._value < 0 ? ~this._value : this._value;
        public bool IsFromEnd => this._value < 0;

        // length + ~value + 1 == length - value, but branchless via the
        // bit-complement encoding.
        public int GetOffset(int length) => this.IsFromEnd ? length + this._value + 1 : this._value;

        public override bool Equals(object value) => value is Index i && this._value == i._value;
        public bool Equals(Index other) => this._value == other._value;
        public override int GetHashCode() => this._value;

        public static implicit operator Index(int value) => FromStart(value);

        public override string ToString() => this.IsFromEnd ? "^" + this.Value : this.Value.ToString();
    }
}
