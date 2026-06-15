namespace System {
    // C# 8. A pair of Index values describing a half-open slice [Start, End).
    // The `1..5` syntax becomes `new Range(1, 5)`; `..` is `Range.All`; `^3..^0`
    // is `new Range(Index.FromEnd(3), Index.FromEnd(0))`. Resolved into a
    // concrete (offset, length) pair at use time via GetOffsetAndLength.
    // IEquatable<Range> not yet in TinyCLR mscorlib (see Version.cs note).
    public readonly struct Range {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end) {
            this.Start = start;
            this.End = end;
        }

        public static Range All => new Range(Index.Start, Index.End);
        public static Range StartAt(Index start) => new Range(start, Index.End);
        public static Range EndAt(Index end) => new Range(Index.Start, end);

        public ValueTuple<int, int> GetOffsetAndLength(int length) {
            var s = this.Start.GetOffset(length);
            var e = this.End.GetOffset(length);
            if ((uint)e > (uint)length || (uint)s > (uint)e)
                throw new ArgumentOutOfRangeException();
            return new ValueTuple<int, int>(s, e - s);
        }

        public override bool Equals(object value) => value is Range r && this.Start.Equals(r.Start) && this.End.Equals(r.End);
        public bool Equals(Range other) => this.Start.Equals(other.Start) && this.End.Equals(other.End);
        public override int GetHashCode() => this.Start.GetHashCode() * 31 + this.End.GetHashCode();

        public override string ToString() => this.Start.ToString() + ".." + this.End.ToString();
    }
}
