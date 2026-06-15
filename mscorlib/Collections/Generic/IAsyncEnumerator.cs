namespace System.Collections.Generic {
    // C# 8. Async counterpart to IEnumerator<T>. Returned by
    // IAsyncEnumerable<T>.GetAsyncEnumerator().
    //
    // NOTE: full .NET uses ValueTask<bool> for MoveNextAsync; TinyCLR has no
    // ValueTask so this trio uses Task / Task<bool>. Under the current sync
    // Task.Delay (see [[task-async-state-machine-limitation]]) the awaits
    // complete inline anyway - shapes work, semantics are "sequential".
    public interface IAsyncEnumerator<out T> : System.IAsyncDisposable {
        System.Threading.Tasks.Task<bool> MoveNextAsync();
        T Current { get; }
    }
}
