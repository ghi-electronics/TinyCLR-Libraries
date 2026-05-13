namespace System {
    // C# 8. Enables `await using` on resources that need an asynchronous
    // cleanup phase. Real .NET defines DisposeAsync as returning ValueTask;
    // TinyCLR doesn't ship ValueTask, so we use Task. Roslyn accepts any
    // GetAwaiter-bearing return type for the pattern, but the WELL-KNOWN
    // INTERFACE binding does care about the signature - if you find a future
    // need for ValueTask here, swap it then.
    public interface IAsyncDisposable {
        System.Threading.Tasks.Task DisposeAsync();
    }
}
