namespace System {
    // ValueTuple is required by the C# compiler for tuple-literal syntax
    // `(int, string) p = (1, "x");` and for deconstruction `var (a, b) = p;`.
    // Without these types, the compiler emits CS8137 / CS8179 / CS8128 and the
    // user can't use any tuple syntax. Reference-class Tuple (in Tuple.cs)
    // remains for legacy users and explicit construction; ValueTuple is the
    // syntax target.
    //
    // Per .NET BCL these are STRUCTs with PUBLIC fields (not auto-properties)
    // so reflection / IL semantics match. Equals / GetHashCode / ToString match
    // the BCL formats so user code that compares or prints behaves identically.
    // Non-generic ValueTuple is a STRUCT per the predefined-types contract
    // (CS8182). It holds no data; it exists so `(int,string) p = (1,"x")` can
    // bind and so static `Create` helpers have a home. Match .NET BCL exactly.
    [Serializable]
    public struct ValueTuple {
        public static ValueTuple<T1> Create<T1>(T1 item1) => new ValueTuple<T1>(item1);
        public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new ValueTuple<T1, T2>(item1, item2);
        public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new ValueTuple<T1, T2, T3>(item1, item2, item3);
        public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        public static ValueTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        public static ValueTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) => new ValueTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) => new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
    }

    [Serializable]
    public struct ValueTuple<T1> {
        public T1 Item1;

        public ValueTuple(T1 item1) { this.Item1 = item1; }

        public void Deconstruct(out T1 item1) { item1 = this.Item1; }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1>)) return false;
            var other = (ValueTuple<T1>)obj;
            return object.Equals(this.Item1, other.Item1);
        }

        public override int GetHashCode() => Tuple.HashOf(this.Item1);

        public override string ToString() => "(" + (this.Item1 == null ? "" : this.Item1.ToString()) + ")";
    }

    [Serializable]
    public struct ValueTuple<T1, T2> {
        public T1 Item1;
        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2) { this.Item1 = item1; this.Item2 = item2; }

        public void Deconstruct(out T1 item1, out T2 item2) { item1 = this.Item1; item2 = this.Item2; }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2>)) return false;
            var other = (ValueTuple<T1, T2>)obj;
            return object.Equals(this.Item1, other.Item1)
                && object.Equals(this.Item2, other.Item2);
        }

        public override int GetHashCode() =>
            Tuple.CombineHash(Tuple.HashOf(this.Item1), Tuple.HashOf(this.Item2));

        public override string ToString() =>
            "(" + (this.Item1 == null ? "" : this.Item1.ToString())
            + ", " + (this.Item2 == null ? "" : this.Item2.ToString()) + ")";
    }

    [Serializable]
    public struct ValueTuple<T1, T2, T3> {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3) {
            item1 = this.Item1; item2 = this.Item2; item3 = this.Item3;
        }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2, T3>)) return false;
            var other = (ValueTuple<T1, T2, T3>)obj;
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
    public struct ValueTuple<T1, T2, T3, T4> {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3; this.Item4 = item4;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4) {
            item1 = this.Item1; item2 = this.Item2; item3 = this.Item3; item4 = this.Item4;
        }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2, T3, T4>)) return false;
            var other = (ValueTuple<T1, T2, T3, T4>)obj;
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
    public struct ValueTuple<T1, T2, T3, T4, T5> {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3; this.Item4 = item4; this.Item5 = item5;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5) {
            item1 = this.Item1; item2 = this.Item2; item3 = this.Item3; item4 = this.Item4; item5 = this.Item5;
        }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2, T3, T4, T5>)) return false;
            var other = (ValueTuple<T1, T2, T3, T4, T5>)obj;
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
    public struct ValueTuple<T1, T2, T3, T4, T5, T6> {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
            this.Item4 = item4; this.Item5 = item5; this.Item6 = item6;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6) {
            item1 = this.Item1; item2 = this.Item2; item3 = this.Item3;
            item4 = this.Item4; item5 = this.Item5; item6 = this.Item6;
        }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2, T3, T4, T5, T6>)) return false;
            var other = (ValueTuple<T1, T2, T3, T4, T5, T6>)obj;
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
    public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7> {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) {
            this.Item1 = item1; this.Item2 = item2; this.Item3 = item3;
            this.Item4 = item4; this.Item5 = item5; this.Item6 = item6; this.Item7 = item7;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7) {
            item1 = this.Item1; item2 = this.Item2; item3 = this.Item3;
            item4 = this.Item4; item5 = this.Item5; item6 = this.Item6; item7 = this.Item7;
        }

        public override bool Equals(object obj) {
            if (!(obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7>)) return false;
            var other = (ValueTuple<T1, T2, T3, T4, T5, T6, T7>)obj;
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
