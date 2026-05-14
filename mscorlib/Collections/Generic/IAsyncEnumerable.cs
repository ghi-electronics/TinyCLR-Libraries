namespace System.Collections.Generic {
    // C# 8. Source of an async iteration. The compiler recognizes this exact
    // type as the binding target of `await foreach`. Pure managed; CancellationToken
    // parameter is optional per spec (compiler passes default if omitted).
    public interface IAsyncEnumerable<out T> {
        IAsyncEnumerator<T> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default);
    }
}
