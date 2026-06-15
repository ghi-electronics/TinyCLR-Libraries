namespace System {
    // Required by C# 9 records (compiler auto-implements IEquatable<TSelf> on
    // every record / record struct), and useful in its own right for any type
    // that wants a typed Equals method without boxing the parameter.
    public interface IEquatable<T> {
        bool Equals(T other);
    }
}
