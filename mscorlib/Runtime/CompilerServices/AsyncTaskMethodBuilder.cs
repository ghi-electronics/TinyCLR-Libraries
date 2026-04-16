using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    public struct AsyncTaskMethodBuilder {
        private Task _task;

        public static AsyncTaskMethodBuilder Create() => new AsyncTaskMethodBuilder();

        public Task Task => this._task != null ? this._task : (this._task = new Task());

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult() {
            if (this._task == null) this._task = new Task();
        }

        public void SetException(Exception exception) {
            if (this._task == null) this._task = new Task();
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

        public static AsyncTaskMethodBuilder<TResult> Create() => new AsyncTaskMethodBuilder<TResult>();

        public Task<TResult> Task => this._task != null ? this._task : (this._task = new Task<TResult>());

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void SetResult(TResult result) {
            if (this._task == null) this._task = new Task<TResult>();
            this._task._result = result;
        }

        public void SetException(Exception exception) {
            if (this._task == null) this._task = new Task<TResult>();
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
