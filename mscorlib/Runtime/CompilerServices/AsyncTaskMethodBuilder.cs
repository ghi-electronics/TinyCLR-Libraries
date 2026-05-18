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

    public struct AsyncVoidMethodBuilder {
        public static AsyncVoidMethodBuilder Create() => new AsyncVoidMethodBuilder();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult() { }

        public void SetException(Exception exception) { throw exception; }

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
}
