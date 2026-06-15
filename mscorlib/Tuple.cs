namespace System {
    public static class Tuple {
        public static Tuple<T1> Create<T1>(T1 item1) => new Tuple<T1>(item1);
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new Tuple<T1, T2>(item1, item2);
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new Tuple<T1, T2, T3>(item1, item2, item3);
        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) => new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) => new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);

        internal static int CombineHash(int h1, int h2) => ((h1 << 5) + h1) ^ h2;
        internal static int HashOf(object o) => o == null ? 0 : o.GetHashCode();
    }

    [Serializable]
    public class Tuple<T1> {
        public T1 Item1 { get; }

        public Tuple(T1 item1) { this.Item1 = item1; }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1);
        }

        public override int GetHashCode() => Tuple.HashOf(this.Item1);

        public override string ToString() => "(" + (this.Item1 == null ? "" : this.Item1.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }

        public Tuple(T1 item1, T2 item2) { this.Item1 = item1; this.Item2 = item2; }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1) && object.Equals(this.Item2, other.Item2);
        }

        public override int GetHashCode() =>
            Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2, T3> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }

        public Tuple(T1 item1, T2 item2, T3 item3) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
        }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2, T3>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2)
                && object.Equals(this.Item3, other.Item3);
        }

        public override int GetHashCode() {
            var h = Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));
            return Tuple.CombineHash(h, Tuple.HashOf(this.Item3));
        }

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString())
            + ", " + (this.Item3 == null ? "" : this.Item3.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3; this.Item4 = item4;
        }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2, T3, T4>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2)
                && object.Equals(this.Item3, other.Item3)
                && object.Equals(this.Item4, other.Item4);
        }

        public override int GetHashCode() {
            var h = Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item3));
            return Tuple.CombineHash(h, Tuple.HashOf(this.Item4));
        }

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString())
            + ", " + (this.Item3 == null ? "" : this.Item3.ToString())
            + ", " + (this.Item4 == null ? "" : this.Item4.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3; this.Item4 = item4; this.Item5 = item5;
        }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2, T3, T4, T5>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2)
                && object.Equals(this.Item3, other.Item3)
                && object.Equals(this.Item4, other.Item4)
                && object.Equals(this.Item5, other.Item5);
        }

        public override int GetHashCode() {
            var h = Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item3));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item4));
            return Tuple.CombineHash(h, Tuple.HashOf(this.Item5));
        }

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString())
            + ", " + (this.Item3 == null ? "" : this.Item3.ToString())
            + ", " + (this.Item4 == null ? "" : this.Item4.ToString())
            + ", " + (this.Item5 == null ? "" : this.Item5.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
            this.Item4 = item4; this.Item5 = item5; this.Item6 = item6;
        }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2, T3, T4, T5, T6>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2)
                && object.Equals(this.Item3, other.Item3)
                && object.Equals(this.Item4, other.Item4)
                && object.Equals(this.Item5, other.Item5)
                && object.Equals(this.Item6, other.Item6);
        }

        public override int GetHashCode() {
            var h = Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item3));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item4));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item5));
            return Tuple.CombineHash(h, Tuple.HashOf(this.Item6));
        }

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString())
            + ", " + (this.Item3 == null ? "" : this.Item3.ToString())
            + ", " + (this.Item4 == null ? "" : this.Item4.ToString())
            + ", " + (this.Item5 == null ? "" : this.Item5.ToString())
            + ", " + (this.Item6 == null ? "" : this.Item6.ToString()) + ")";
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7> {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }
        public T4 Item4 { get; }
        public T5 Item5 { get; }
        public T6 Item6 { get; }
        public T7 Item7 { get; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
            this.Item4 = item4; this.Item5 = item5; this.Item6 = item6; this.Item7 = item7;
        }

        public override bool Equals(object obj) {
            var other = obj as Tuple<T1, T2, T3, T4, T5, T6, T7>;
            if (other == null) return false;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2)
                && object.Equals(this.Item3, other.Item3)
                && object.Equals(this.Item4, other.Item4)
                && object.Equals(this.Item5, other.Item5)
                && object.Equals(this.Item6, other.Item6)
                && object.Equals(this.Item7, other.Item7);
        }

        public override int GetHashCode() {
            var h = Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item3));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item4));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item5));
            h = Tuple.CombineHash(h, Tuple.HashOf(this.Item6));
            return Tuple.CombineHash(h, Tuple.HashOf(this.Item7));
        }

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString())
            + ", " + (this.Item3 == null ? "" : this.Item3.ToString())
            + ", " + (this.Item4 == null ? "" : this.Item4.ToString())
            + ", " + (this.Item5 == null ? "" : this.Item5.ToString())
            + ", " + (this.Item6 == null ? "" : this.Item6.ToString())
            + ", " + (this.Item7 == null ? "" : this.Item7.ToString()) + ")";
    }
}
