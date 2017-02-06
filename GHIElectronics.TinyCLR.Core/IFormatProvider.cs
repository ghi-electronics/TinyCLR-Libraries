namespace System {
    public interface IFormatProvider {
        // Interface does not need to be marked with the serializable attribute
        object GetFormat(Type formatType);
    }
}


