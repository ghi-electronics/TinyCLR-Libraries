namespace System {
    public interface ICustomFormatter {
        // Interface does not need to be marked with the serializable attribute
        string Format(string format, object arg, IFormatProvider formatProvider);

    }
}


