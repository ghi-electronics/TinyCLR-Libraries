namespace System {
    // Generic Func delegates - up to 4 input args + 1 result. Matches the
    // standard .NET BCL family; LINQ uses Func<T,bool>, Func<T,TResult>, and
    // Func<TAccumulate,T,TAccumulate> the most.

    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
