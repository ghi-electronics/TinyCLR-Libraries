using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System {
    public delegate void Action();

    // Action<T..> family matching the standard .NET BCL up to 4 args. LINQ's
    // ForEach-style operators and event-style callbacks both use these.
    public delegate void Action<in T>(T arg);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
