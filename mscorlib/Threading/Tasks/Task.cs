using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    // ============ KNOWN LIMITATION: `await Task<T>` ============
    //
    // `await Task<T>` (awaiting a generic Task) currently crashes with
    // CLR_E_WRONG_TYPE inside the compiler-generated state machine. Root
    // cause is in TinyCLR's interpreter: generic type parameters are lost
    // when a struct state machine is boxed for an await suspension, so
    // reading the `TaskAwaiter<T> <>u__N` field back from the box after
    // the continuation resumes mis-types the access.
    //
    // Workaround patterns:
    //
    //   // BAD - hits the bug:
    //   int v = await Task.Run<int>(() => Compute());
    //
    //   // GOOD - use .Result after starting:
    //   var t = Task.Run<int>(() => Compute());
    //   ...
    //   int v = t.Result;       // blocks, but no async-await type loss
    //
    //   // ALSO GOOD - await non-generic Task instead:
    //   await Task.Delay(0);     // or any non-generic awaitable
    //   int v = t.Result;
    //
    // Awaiting non-generic `Task` (Task.Delay, Task.WhenAll(Task[])) works
    // correctly. Only generic Task<T> awaits are affected.
    //
    // Defensive fault: AsyncTaskMethodBuilder's continuation trampoline
    // catches the MoveNext crash and faults the outer Task, so the caller
    // sees an exception rather than a deadlock. But the operation still
    // fails — this is escape-the-hang, not make-it-work.
    //
    // Full fix is CLR-level (interpreter generic-erasure tracking), Tier 3.
    // See feedback_task_async_generic_state_machine memory note.
    public class Task {
        // AggregateException, not Exception, to match BCL Task.get_Exception()
        // signature. On Desktop the user-code IL typeref resolves through to
        // BCL Task; if the return type were Exception, the JIT would raise
        // MissingMethodException.
        internal AggregateException _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        // Completion plumbing — both lazy, so the legacy "make a pre-completed
        // Task with `new Task()` and hand it back" paths (FromResult,
        // CompletedTask, the awaiter on Task.Delay) pay zero overhead.
        //
        // _completion fires when the Task transitions to a terminal state.
        // Used by Wait(), WhenAll (via WaitHandle.WaitAll), WhenAny (via
        // WaitHandle.WaitAny).
        //
        // _continuation is the continuation list registered by
        // TaskAwaiter.OnCompleted (i.e. the rest-of-method lambda from an
        // `await task`). Multiple registrations on the same Task are chained
        // via Delegate.Combine — invocation order matches registration order.
        internal ManualResetEvent _completion;
        private Action _continuation;
        private readonly object _completionLock = new object();

        // Shared signaled event for already-completed Tasks. Used as a stand-in
        // handle when a caller (WhenAll/WhenAny) needs a WaitHandle for a
        // pre-completed Task that never had _completion allocated.
        private static readonly ManualResetEvent _alreadySignaled = new ManualResetEvent(true);

        public TaskStatus Status => this._status;
        public bool IsCompleted => this._status == TaskStatus.RanToCompletion || this._status == TaskStatus.Faulted || this._status == TaskStatus.Canceled;
        public bool IsFaulted => this._status == TaskStatus.Faulted;
        public bool IsCanceled => this._status == TaskStatus.Canceled;
        public AggregateException Exception => this._exception;

        public static readonly Task CompletedTask = new Task();

        public TaskAwaiter GetAwaiter() => new TaskAwaiter(this);

        // ConfigureAwait is a no-op on TinyCLR: no SynchronizationContext to
        // capture or marshal back to, so `false` and `true` are identical.
        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
            => new ConfiguredTaskAwaitable(this);

        public void Wait() {
            if (!this.IsCompleted) {
                if (this._completion != null) this._completion.WaitOne();
            }
            if (this._status == TaskStatus.Faulted && this._exception != null) throw this._exception;
            // BCL Task.Wait() on a canceled Task throws AggregateException
            // wrapping a TaskCanceledException — not a raw OperationCanceledException.
            // Match that here so device and Desktop agree on the throw type.
            if (this._status == TaskStatus.Canceled) throw new AggregateException(new OperationCanceledException());
        }

        public bool Wait(int millisecondsTimeout) {
            if (!this.IsCompleted) {
                if (this._completion != null) {
                    if (!this._completion.WaitOne(millisecondsTimeout, false)) return false;
                }
                else {
                    return false;
                }
            }
            if (this._status == TaskStatus.Faulted && this._exception != null) throw this._exception;
            // BCL Task.Wait() on a canceled Task throws AggregateException
            // wrapping a TaskCanceledException — not a raw OperationCanceledException.
            // Match that here so device and Desktop agree on the throw type.
            if (this._status == TaskStatus.Canceled) throw new AggregateException(new OperationCanceledException());
            return true;
        }

        public static Task Delay(int millisecondsDelay) => Delay(millisecondsDelay, CancellationToken.None);

        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) return FromCanceled(cancellationToken);
            if (millisecondsDelay <= 0) return CompletedTask;

            // Synchronous sleep on calling thread, polling cancellation token
            // every ~50ms. See [[task-async-state-machine-limitation]] for why
            // we don't use timer-driven async resume here.
            const int sliceMs = 50;
            var remaining = millisecondsDelay;
            while (remaining > 0) {
                if (cancellationToken.IsCancellationRequested) return FromCanceled(cancellationToken);
                var slice = remaining < sliceMs ? remaining : sliceMs;
                Thread.Sleep(slice);
                remaining -= slice;
            }
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : CompletedTask;
        }

        // ============ Task.Run ============
        //
        // Spawns a Thread per call. No ThreadPool, so this is "honest
        // parallelism with a cost": appropriate for "start a long-running
        // background job," wrong for "fork-join a thousand small units."
        //
        // The returned Task transitions WaitingToRun → Running → terminal as
        // the worker thread progresses. An awaiter that hits IsCompleted=false
        // registers a continuation that the worker thread invokes on
        // completion — matching the real .NET async pattern.
        //
        // IMPORTANT: we deliberately avoid lambdas that capture local variables.
        // The C# compiler generates `<>c__DisplayClass<N>_<M>` types for those,
        // and TinyCLR's MetadataProcessor doesn't sanitize the `<>` characters
        // when it emits FIELD___ constants in mscorlib.h, producing
        // syntactically-invalid C++. All thread-bound work is wrapped in named
        // worker classes whose fields hold the would-be captures explicitly.

        public static Task Run(Action action) => Run(action, CancellationToken.None);

        public static Task Run(Action action, CancellationToken cancellationToken) {
            if (action == null) throw new ArgumentNullException();
            if (cancellationToken.IsCancellationRequested) return FromCanceled(cancellationToken);

            var t = new Task();
            t._status = TaskStatus.WaitingToRun;
            t._completion = new ManualResetEvent(false);

            var worker = new RunActionWorker(action, cancellationToken, t);
            new Thread(new ThreadStart(worker.Run)).Start();
            return t;
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function) => Run(function, CancellationToken.None);

        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) {
            if (function == null) throw new ArgumentNullException();
            if (cancellationToken.IsCancellationRequested) return FromCanceled<TResult>(cancellationToken);

            var t = new Task<TResult>();
            t._status = TaskStatus.WaitingToRun;
            t._completion = new ManualResetEvent(false);

            var worker = new RunFuncWorker<TResult>(function, cancellationToken, t);
            new Thread(new ThreadStart(worker.Run)).Start();
            return t;
        }

        public static Task<TResult> FromResult<TResult>(TResult result) {
            var t = new Task<TResult>();
            t._result = result;
            return t;
        }

        public static Task FromException(Exception exception) {
            if (exception == null) throw new ArgumentNullException();
            var t = new Task();
            // Direct field write — SetException's IsCompleted guard would bail
            // because `new Task()` defaults to RanToCompletion. The guard exists
            // for race safety on Task.Run workers, not for construction.
            // FromCanceled follows the same pattern below.
            t._exception = WrapForTask(exception);
            t._status = TaskStatus.Faulted;
            return t;
        }

        public static Task<TResult> FromException<TResult>(Exception exception) {
            if (exception == null) throw new ArgumentNullException();
            var t = new Task<TResult>();
            t._exception = WrapForTask(exception);
            t._status = TaskStatus.Faulted;
            return t;
        }

        // Storing exceptions as AggregateException matches BCL's Task contract.
        // If the caller already handed us an AggregateException, keep it as-is
        // rather than double-wrapping; otherwise wrap the raw Exception.
        internal static AggregateException WrapForTask(Exception ex) =>
            ex is AggregateException ae ? ae : new AggregateException(ex);

        public static Task FromCanceled(CancellationToken cancellationToken) {
            // BCL requires the token to be already-canceled; passing
            // CancellationToken.None throws ArgumentOutOfRangeException. Mirror
            // that so device behavior matches Desktop.
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException("cancellationToken");
            var t = new Task();
            t._status = TaskStatus.Canceled;
            return t;
        }

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) {
            if (!cancellationToken.IsCancellationRequested)
                throw new ArgumentOutOfRangeException("cancellationToken");
            var t = new Task<TResult>();
            t._status = TaskStatus.Canceled;
            return t;
        }

        // ============ WhenAll / WhenAny ============
        //
        // Both spawn ONE coordinator thread that uses WaitHandle.WaitAll /
        // WaitAny to sleep in the kernel until input tasks signal. Beats
        // polling, and stays at one thread regardless of how many inputs.
        // Coordinator logic lives in named worker classes for the same MMP
        // reason as Task.Run.

        public static Task WhenAll(params Task[] tasks) {
            if (tasks == null) throw new ArgumentNullException();
            foreach (var t in tasks) if (t == null) throw new ArgumentException();
            return WhenAllCore(tasks);
        }

        public static Task WhenAll(IEnumerable<Task> tasks) {
            if (tasks == null) throw new ArgumentNullException();
            var list = new List<Task>();
            foreach (var t in tasks) {
                if (t == null) throw new ArgumentException();
                list.Add(t);
            }
            return WhenAllCore(list.ToArray());
        }

        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) {
            if (tasks == null) throw new ArgumentNullException();
            foreach (var t in tasks) if (t == null) throw new ArgumentException();
            return WhenAllCore(tasks);
        }

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) {
            if (tasks == null) throw new ArgumentNullException();
            var list = new List<Task<TResult>>();
            foreach (var t in tasks) {
                if (t == null) throw new ArgumentException();
                list.Add(t);
            }
            return WhenAllCore(list.ToArray());
        }

        private static Task WhenAllCore(Task[] tasks) {
            var agg = new Task();
            agg._status = TaskStatus.WaitingForActivation;
            agg._completion = new ManualResetEvent(false);

            if (tasks.Length == 0) {
                agg.SetCompleted();
                return agg;
            }

            var worker = new WhenAllWorker(tasks, agg);
            new Thread(new ThreadStart(worker.Run)).Start();
            return agg;
        }

        private static Task<TResult[]> WhenAllCore<TResult>(Task<TResult>[] tasks) {
            var agg = new Task<TResult[]>();
            agg._status = TaskStatus.WaitingForActivation;
            agg._completion = new ManualResetEvent(false);

            if (tasks.Length == 0) {
                agg._result = new TResult[0];
                agg.SetCompleted();
                return agg;
            }

            var worker = new WhenAllWorker<TResult>(tasks, agg);
            new Thread(new ThreadStart(worker.Run)).Start();
            return agg;
        }

        public static Task<Task> WhenAny(params Task[] tasks) {
            if (tasks == null) throw new ArgumentNullException();
            if (tasks.Length == 0) throw new ArgumentException();
            foreach (var t in tasks) if (t == null) throw new ArgumentException();
            return WhenAnyCore(tasks);
        }

        public static Task<Task> WhenAny(IEnumerable<Task> tasks) {
            if (tasks == null) throw new ArgumentNullException();
            var list = new List<Task>();
            foreach (var t in tasks) {
                if (t == null) throw new ArgumentException();
                list.Add(t);
            }
            if (list.Count == 0) throw new ArgumentException();
            return WhenAnyCore(list.ToArray());
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) {
            if (tasks == null) throw new ArgumentNullException();
            if (tasks.Length == 0) throw new ArgumentException();
            foreach (var t in tasks) if (t == null) throw new ArgumentException();
            return WhenAnyCore(tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks) {
            if (tasks == null) throw new ArgumentNullException();
            var list = new List<Task<TResult>>();
            foreach (var t in tasks) {
                if (t == null) throw new ArgumentException();
                list.Add(t);
            }
            if (list.Count == 0) throw new ArgumentException();
            return WhenAnyCore(list.ToArray());
        }

        private static Task<Task> WhenAnyCore(Task[] tasks) {
            var agg = new Task<Task>();
            agg._status = TaskStatus.WaitingForActivation;
            agg._completion = new ManualResetEvent(false);

            var worker = new WhenAnyWorker(tasks, agg);
            new Thread(new ThreadStart(worker.Run)).Start();
            return agg;
        }

        private static Task<Task<TResult>> WhenAnyCore<TResult>(Task<TResult>[] tasks) {
            var agg = new Task<Task<TResult>>();
            agg._status = TaskStatus.WaitingForActivation;
            agg._completion = new ManualResetEvent(false);

            var worker = new WhenAnyWorker<TResult>(tasks, agg);
            new Thread(new ThreadStart(worker.Run)).Start();
            return agg;
        }

        // ============ Completion + continuation dispatch ============

        internal void SetCompleted() {
            Action c;
            lock (this._completionLock) {
                if (this.IsCompleted) return;
                this._status = TaskStatus.RanToCompletion;
                if (this._completion != null) this._completion.Set();
                c = this._continuation;
                this._continuation = null;
            }
            if (c != null) c();
        }

        internal void SetCanceled() {
            Action c;
            lock (this._completionLock) {
                if (this.IsCompleted) return;
                this._status = TaskStatus.Canceled;
                if (this._completion != null) this._completion.Set();
                c = this._continuation;
                this._continuation = null;
            }
            if (c != null) c();
        }

        internal void SetException(Exception ex) {
            Action c;
            lock (this._completionLock) {
                if (this.IsCompleted) return;
                this._exception = WrapForTask(ex);
                this._status = TaskStatus.Faulted;
                if (this._completion != null) this._completion.Set();
                c = this._continuation;
                this._continuation = null;
            }
            if (c != null) c();
        }

        // Called by TaskAwaiter.OnCompleted. If the Task has already completed,
        // runs the continuation immediately on the calling thread. Otherwise
        // stashes it for SetCompleted/SetCanceled/SetException to fire from
        // the worker thread. Multiple registrations chain via MulticastDelegate
        // — no lambda needed (which would create a `<>c__DisplayClass` and
        // trip MetadataProcessor).
        internal void RegisterContinuation(Action continuation) {
            if (continuation == null) return;
            var runNow = false;
            lock (this._completionLock) {
                if (this.IsCompleted) {
                    runNow = true;
                }
                else {
                    this._continuation = (Action)Delegate.Combine(this._continuation, continuation);
                }
            }
            if (runNow) continuation();
        }

        // Returns a WaitHandle that becomes signaled when this Task completes.
        // For pre-completed Tasks (no _completion event ever allocated) returns
        // a shared always-signaled event so WaitAll/WaitAny work uniformly.
        internal WaitHandle GetCompletionHandle() {
            if (this._completion != null) return this._completion;
            if (this.IsCompleted) return _alreadySignaled;
            lock (this._completionLock) {
                if (this._completion == null) this._completion = new ManualResetEvent(this.IsCompleted);
            }
            return this._completion;
        }

        // ============ Internal worker classes (replace capturing lambdas) ============
        //
        // Each holds the would-be captured variables as explicit fields. Run()
        // is the method invoked from the worker thread. Named classes are
        // immune to the MMP `<>c__DisplayClass` mangling bug because their
        // identifiers contain no `<` or `>` characters.

        private sealed class RunActionWorker {
            private readonly Action _action;
            private readonly CancellationToken _token;
            private readonly Task _result;

            internal RunActionWorker(Action action, CancellationToken token, Task result) {
                this._action = action;
                this._token = token;
                this._result = result;
            }

            internal void Run() {
                if (this._token.IsCancellationRequested) { this._result.SetCanceled(); return; }
                this._result._status = TaskStatus.Running;
                try {
                    this._action();
                    this._result.SetCompleted();
                }
                catch (OperationCanceledException) { this._result.SetCanceled(); }
                catch (Exception ex) { this._result.SetException(ex); }
            }
        }

        private sealed class RunFuncWorker<TResult> {
            private readonly Func<TResult> _function;
            private readonly CancellationToken _token;
            private readonly Task<TResult> _result;

            internal RunFuncWorker(Func<TResult> function, CancellationToken token, Task<TResult> result) {
                this._function = function;
                this._token = token;
                this._result = result;
            }

            internal void Run() {
                if (this._token.IsCancellationRequested) { this._result.SetCanceled(); return; }
                this._result._status = TaskStatus.Running;
                try {
                    this._result._result = this._function();
                    this._result.SetCompleted();
                }
                catch (OperationCanceledException) { this._result.SetCanceled(); }
                catch (Exception ex) { this._result.SetException(ex); }
            }
        }

        private sealed class WhenAllWorker {
            private readonly Task[] _tasks;
            private readonly Task _agg;

            internal WhenAllWorker(Task[] tasks, Task agg) {
                this._tasks = tasks;
                this._agg = agg;
            }

            internal void Run() {
                var handles = new WaitHandle[this._tasks.Length];
                for (var i = 0; i < this._tasks.Length; i++) handles[i] = this._tasks[i].GetCompletionHandle();
                WaitHandle.WaitAll(handles);

                Exception firstFault = null;
                var anyCanceled = false;
                foreach (var t in this._tasks) {
                    if (t.IsFaulted && firstFault == null) firstFault = t._exception;
                    else if (t.IsCanceled) anyCanceled = true;
                }
                if (firstFault != null) this._agg.SetException(firstFault);
                else if (anyCanceled) this._agg.SetCanceled();
                else this._agg.SetCompleted();
            }
        }

        private sealed class WhenAllWorker<TResult> {
            private readonly Task<TResult>[] _tasks;
            private readonly Task<TResult[]> _agg;

            internal WhenAllWorker(Task<TResult>[] tasks, Task<TResult[]> agg) {
                this._tasks = tasks;
                this._agg = agg;
            }

            internal void Run() {
                var handles = new WaitHandle[this._tasks.Length];
                for (var i = 0; i < this._tasks.Length; i++) handles[i] = this._tasks[i].GetCompletionHandle();
                WaitHandle.WaitAll(handles);

                Exception firstFault = null;
                var anyCanceled = false;
                var results = new TResult[this._tasks.Length];
                for (var i = 0; i < this._tasks.Length; i++) {
                    var t = this._tasks[i];
                    if (t.IsFaulted && firstFault == null) firstFault = t._exception;
                    else if (t.IsCanceled) anyCanceled = true;
                    else results[i] = t._result;
                }
                if (firstFault != null) this._agg.SetException(firstFault);
                else if (anyCanceled) this._agg.SetCanceled();
                else { this._agg._result = results; this._agg.SetCompleted(); }
            }
        }

        private sealed class WhenAnyWorker {
            private readonly Task[] _tasks;
            private readonly Task<Task> _agg;

            internal WhenAnyWorker(Task[] tasks, Task<Task> agg) {
                this._tasks = tasks;
                this._agg = agg;
            }

            internal void Run() {
                var handles = new WaitHandle[this._tasks.Length];
                for (var i = 0; i < this._tasks.Length; i++) handles[i] = this._tasks[i].GetCompletionHandle();
                var idx = WaitHandle.WaitAny(handles);
                this._agg._result = this._tasks[idx];
                this._agg.SetCompleted();
            }
        }

        private sealed class WhenAnyWorker<TResult> {
            private readonly Task<TResult>[] _tasks;
            private readonly Task<Task<TResult>> _agg;

            internal WhenAnyWorker(Task<TResult>[] tasks, Task<Task<TResult>> agg) {
                this._tasks = tasks;
                this._agg = agg;
            }

            internal void Run() {
                var handles = new WaitHandle[this._tasks.Length];
                for (var i = 0; i < this._tasks.Length; i++) handles[i] = this._tasks[i].GetCompletionHandle();
                var idx = WaitHandle.WaitAny(handles);
                this._agg._result = this._tasks[idx];
                this._agg.SetCompleted();
            }
        }
    }

    public class Task<TResult> : Task {
        internal TResult _result;

        public TResult Result {
            get {
                this.Wait();
                return this._result;
            }
        }

        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);

        public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            => new ConfiguredTaskAwaitable<TResult>(this);
    }
}
