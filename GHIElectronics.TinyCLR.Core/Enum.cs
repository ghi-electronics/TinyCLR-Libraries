namespace System {
    [Serializable]
    public abstract class Enum : ValueType, IFormattable {
        public string ToString(string format, IFormatProvider formatProvider) => this.ToString();
        public override string ToString() => this.GetType().GetField("value__").GetValue(this).ToString();

        public static Type GetUnderlyingType(Type enumType) => enumType.GetType().GetField("value__").FieldType;
    }
}
