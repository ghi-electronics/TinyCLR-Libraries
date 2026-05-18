using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    public struct AsyncTaskMethodBuilder {
        private Task _task;

        // Eager Task allocation is critical for real async (Task.Run + await).
        // The compiler-generated state machine copies this builder (struct) on
        // every await suspension; without eager allocation, _task is null at
        // copy time, and a later SetResult on the boxed copy allocates a
        // DIFFERENT Task than the caller already received from the original
        // builder. Synchronous awaits (Task.Delay) never trip this because
        // SetResult runs before any copy; Task.Run does. Allocating up front
        // means every copy shares the same Task reference, so SetResult on
        // any copy marks the caller's Task complete.
        public static AsyncTaskMethodBuilder Create() {
            var b = new AsyncTaskMethodBuilder();
            b._task = new Task();
            b._task._status = TaskStatus.WaitingForActivation;
            b._task._completion = new ManualResetEvent(false);
            return b;
        }

        public Task Task => this._task;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult() {
            this._task.SetCompleted();
        }

        public void SetException(Exception exception) {
            this._task.SetException(exception);
        }

        // Defensive trampoline: wraps MoveNext in try/catch so that if the
        // continuation crashes (e.g. TinyCLR's generic-erasure trips
        // CLR_E_WRONG_TYPE reading a `TaskAwaiter<T>` field back from a boxed
        // state machine after `await Task<T>`), the outer Task gets faulted
        // instead of hanging forever in Wait()/await on its completion event.
        // Without this, a crashed MoveNext leaves the builder's Task in
        // WaitingForActivation status with no signal — the caller deadlocks.
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var trampoline = new MoveNextTrampoline(stateMachine, this._task);
            awaiter.OnCompleted(trampoline.Run);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var trampoline = new MoveNextTrampoline(stateMachine, this._task);
            awaiter.UnsafeOnCompleted(trampoline.Run);
        }
    }

    public struct AsyncTaskMethodBuilder<TResult> {
        private Task<TResult> _task;

        public static AsyncTaskMethodBuilder<TResult> Create() {
            var b = new AsyncTaskMethodBuilder<TResult>();
            b._task = new Task<TResult>();
            b._task._status = TaskStatus.WaitingForActivation;
            b._task._completion = new ManualResetEvent(false);
            return b;
        }

        public Task<TResult> Task => this._task;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult(TResult result) {
            this._task._result = result;
            this._task.SetCompleted();
        }

        public void SetException(Exception exception) {
            this._task.SetException(exception);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var trampoline = new MoveNextTrampoline(stateMachine, this._task);
            awaiter.OnCompleted(trampoline.Run);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var trampoline = new MoveNextTrampoline(stateMachine, this._task);
            awaiter.UnsafeOnCompleted(trampoline.Run);
        }
    }

    public struct AsyncVoidMethodBuilder {
        public static AsyncVoidMethodBuilder Create() => new AsyncVoidMethodBuilder();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult() { }

        public void SetException(Exception exception) { throw exception; }

        // Void builders have no Task to fault, so a MoveNext crash here still
        // throws on whichever thread is running the continuation. The user
        // sees an unhandled exception rather than a hang — bad but visible.
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var sm = stateMachine;
            awaiter.OnCompleted(sm.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine {
            var sm = stateMachine;
            awaiter.UnsafeOnCompleted(sm.MoveNext);
        }
    }

    // Named worker class — must not be a capturing lambda (MMP can't sanitize
    // the C# compiler's `<>c__DisplayClass` characters; see Task.cs's worker
    // classes for the same pattern).
    //
    // Stores IAsyncStateMachine (boxed if struct) and the Task to fault on
    // failure. Run() is the continuation delegate passed to the awaiter.
    internal sealed class MoveNextTrampoline {
        private readonly IAsyncStateMachine _sm;
        private readonly Task _task;

        internal MoveNextTrampoline(IAsyncStateMachine sm, Task task) {
            this._sm = sm;
            this._task = task;
        }

        internal void Run() {
            try {
                this._sm.MoveNext();
            }
            catch (Exception ex) {
                // Only fault the outer task if it hasn't already transitioned —
                // a well-behaved MoveNext that called SetResult before throwing
                // would already be terminal; we don't want to overwrite.
                if (!this._task.IsCompleted) this._task.SetException(ex);
            }
        }
    }
}
