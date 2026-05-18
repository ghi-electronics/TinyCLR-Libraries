using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    public class Task {
        internal Exception _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        // Completion plumbing — both lazy, so the legacy "make a pre-completed
        // Task with `new Task()` and hand it back" paths (FromResult,
        // CompletedTask, the awaiter on Task.Delay) pay zero overhead.
        //
        // _completion fires when the Task transitions to a terminal state.
        // Used by Wait(), WhenAll (via WaitHandle.WaitAll), WhenAny (via
        // WaitHandle.WaitAny).
        //
        // _continuation is the single-slot continuation registered by
        // TaskAwaiter.OnCompleted (i.e. the rest-of-method lambda from an
        // `await task`). Multiple registrations on the same Task chain
        // existing-then-new, preserving registration order on completion.
        // Single slot keeps the common case (one awaiter per Task)
        // allocation-free.
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
        public Exception Exception => this._exception;

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
            if (this._status == TaskStatus.Canceled) throw new OperationCanceledException();
        }

        public bool Wait(int millisecondsTimeout) {
            if (!this.IsCompleted) {
                if (this._completion != null) {
                    if (!this._completion.WaitOne(millisecondsTimeout, false)) return false;
                }
                else {
                    // No completion event ever installed and not completed —
                    // shouldn't happen for any of our internal Task creation
                    // paths, but degrade gracefully.
                    return false;
                }
            }
            if (this._status == TaskStatus.Faulted && this._exception != null) throw this._exception;
            if (this._status == TaskStatus.Canceled) throw new OperationCanceledException();
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
        // background job," wrong for "fork-join a thousand small units." Real
        // .NET pools workers and recycles; we don't yet.
        //
        // The returned Task transitions WaitingToRun → Running → terminal as
        // the worker thread progresses. An awaiter that hits IsCompleted=false
        // registers a continuation that the worker thread invokes on
        // completion — matching the real .NET async pattern.

        public static Task Run(Action action) => Run(action, CancellationToken.None);

        public static Task Run(Action action, CancellationToken cancellationToken) {
            if (action == null) throw new ArgumentNullException();
            if (cancellationToken.IsCancellationRequested) return FromCanceled(cancellationToken);

            var t = new Task();
            t._status = TaskStatus.WaitingToRun;
            t._completion = new ManualResetEvent(false);

            var worker = new Thread(() => {
                if (cancellationToken.IsCancellationRequested) { t.SetCanceled(); return; }
                t._status = TaskStatus.Running;
                try {
                    action();
                    t.SetCompleted();
                }
                catch (OperationCanceledException) {
                    t.SetCanceled();
                }
                catch (Exception ex) {
                    t.SetException(ex);
                }
            });
            worker.Start();
            return t;
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function) => Run(function, CancellationToken.None);

        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) {
            if (function == null) throw new ArgumentNullException();
            if (cancellationToken.IsCancellationRequested) return FromCanceled<TResult>(cancellationToken);

            var t = new Task<TResult>();
            t._status = TaskStatus.WaitingToRun;
            t._completion = new ManualResetEvent(false);

            var worker = new Thread(() => {
                if (cancellationToken.IsCancellationRequested) { t.SetCanceled(); return; }
                t._status = TaskStatus.Running;
                try {
                    t._result = function();
                    t.SetCompleted();
                }
                catch (OperationCanceledException) {
                    t.SetCanceled();
                }
                catch (Exception ex) {
                    t.SetException(ex);
                }
            });
            worker.Start();
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
            t.SetException(exception);
            return t;
        }

        public static Task<TResult> FromException<TResult>(Exception exception) {
            if (exception == null) throw new ArgumentNullException();
            var t = new Task<TResult>();
            t.SetException(exception);
            return t;
        }

        public static Task FromCanceled(CancellationToken cancellationToken) {
            var t = new Task();
            t._status = TaskStatus.Canceled;
            return t;
        }

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) {
            var t = new Task<TResult>();
            t._status = TaskStatus.Canceled;
            return t;
        }

        // ============ WhenAll / WhenAny ============
        //
        // Both spawn ONE coordinator thread that uses WaitHandle.WaitAll /
        // WaitAny to sleep in the kernel until input tasks signal. Beats
        // polling, and stays at one thread regardless of how many inputs
        // there are.

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

            new Thread(() => {
                var handles = new WaitHandle[tasks.Length];
                for (var i = 0; i < tasks.Length; i++) handles[i] = tasks[i].GetCompletionHandle();
                WaitHandle.WaitAll(handles);

                Exception firstFault = null;
                var anyCanceled = false;
                foreach (var t in tasks) {
                    if (t.IsFaulted && firstFault == null) firstFault = t._exception;
                    else if (t.IsCanceled) anyCanceled = true;
                }
                if (firstFault != null) agg.SetException(firstFault);
                else if (anyCanceled) agg.SetCanceled();
                else agg.SetCompleted();
            }).Start();
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

            new Thread(() => {
                var handles = new WaitHandle[tasks.Length];
                for (var i = 0; i < tasks.Length; i++) handles[i] = tasks[i].GetCompletionHandle();
                WaitHandle.WaitAll(handles);

                Exception firstFault = null;
                var anyCanceled = false;
                var results = new TResult[tasks.Length];
                for (var i = 0; i < tasks.Length; i++) {
                    var t = tasks[i];
                    if (t.IsFaulted && firstFault == null) firstFault = t._exception;
                    else if (t.IsCanceled) anyCanceled = true;
                    else results[i] = t._result;
                }
                if (firstFault != null) agg.SetException(firstFault);
                else if (anyCanceled) agg.SetCanceled();
                else { agg._result = results; agg.SetCompleted(); }
            }).Start();
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

            new Thread(() => {
                var handles = new WaitHandle[tasks.Length];
                for (var i = 0; i < tasks.Length; i++) handles[i] = tasks[i].GetCompletionHandle();
                var idx = WaitHandle.WaitAny(handles);
                agg._result = tasks[idx];
                agg.SetCompleted();
            }).Start();
            return agg;
        }

        private static Task<Task<TResult>> WhenAnyCore<TResult>(Task<TResult>[] tasks) {
            var agg = new Task<Task<TResult>>();
            agg._status = TaskStatus.WaitingForActivation;
            agg._completion = new ManualResetEvent(false);

            new Thread(() => {
                var handles = new WaitHandle[tasks.Length];
                for (var i = 0; i < tasks.Length; i++) handles[i] = tasks[i].GetCompletionHandle();
                var idx = WaitHandle.WaitAny(handles);
                agg._result = tasks[idx];
                agg.SetCompleted();
            }).Start();
            return agg;
        }

        // ============ Completion + continuation dispatch ============
        //
        // All three terminal-state transitions go through the same locked
        // pattern: flip status → signal event → grab continuation → unlock →
        // invoke continuation outside the lock. The lock guards the race
        // between RegisterContinuation (which may register just as we're
        // transitioning) and us — the loser of that race observes IsCompleted
        // inside the lock and runs the continuation itself.

        internal void SetCompleted() {
            Action c;
            lock (this._completionLock) {
                if (this.IsCompleted) return;
                this._status = TaskStatus.RanToCompletion;
                if (this._completion != null) this._completion.Set();
                c = this._continuation;
                this._continuation = null;
            }
            c?.Invoke();
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
            c?.Invoke();
        }

        internal void SetException(Exception ex) {
            Action c;
            lock (this._completionLock) {
                if (this.IsCompleted) return;
                this._exception = ex;
                this._status = TaskStatus.Faulted;
                if (this._completion != null) this._completion.Set();
                c = this._continuation;
                this._continuation = null;
            }
            c?.Invoke();
        }

        // Called by TaskAwaiter.OnCompleted. If the Task has already
        // completed, runs the continuation immediately on the calling thread.
        // Otherwise stashes it for SetCompleted/SetCanceled/SetException to
        // fire from the worker thread.
        internal void RegisterContinuation(Action continuation) {
            if (continuation == null) return;
            var runNow = false;
            lock (this._completionLock) {
                if (this.IsCompleted) {
                    runNow = true;
                }
                else {
                    // Chain if there's already one registered (rare). Preserves
                    // FIFO order; matches BCL behavior where multiple awaiters
                    // on the same Task all get to resume.
                    var existing = this._continuation;
                    this._continuation = existing == null ? continuation : (Action)(() => { existing(); continuation(); });
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
            // Not completed, no event yet — install one. Race-safe because
            // SetCompleted reads _completion under the lock.
            lock (this._completionLock) {
                if (this._completion == null) this._completion = new ManualResetEvent(this.IsCompleted);
            }
            return this._completion;
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
