using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    public struct TaskAwaiter : ICriticalNotifyCompletion {
        private readonly Task _task;

        internal TaskAwaiter(Task task) { this._task = task; }

        public bool IsCompleted => this._task == null || this._task.IsCompleted;

        public void GetResult() {
            if (this._task != null) this._task.Wait();
        }

        // Register the continuation on the Task. If the Task is already
        // complete, RegisterContinuation runs it immediately. Otherwise the
        // worker thread (or whoever calls SetCompleted/Canceled/Exception)
        // fires it. This matches the real .NET async pattern — the calling
        // thread doesn't block, and the state machine resumes on whatever
        // thread completes the Task.
        public void OnCompleted(Action continuation) {
            if (this._task == null) { continuation?.Invoke(); return; }
            this._task.RegisterContinuation(continuation);
        }

        public void UnsafeOnCompleted(Action continuation) {
            if (this._task == null) { continuation?.Invoke(); return; }
            this._task.RegisterContinuation(continuation);
        }
    }

    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion {
        private readonly Task<TResult> _task;

        internal TaskAwaiter(Task<TResult> task) { this._task = task; }

        public bool IsCompleted => this._task == null || this._task.IsCompleted;

        public TResult GetResult() {
            if (this._task == null) return default(TResult);
            this._task.Wait();
            return this._task._result;
        }

        public void OnCompleted(Action continuation) {
            if (this._task == null) { continuation?.Invoke(); return; }
            this._task.RegisterContinuation(continuation);
        }

        public void UnsafeOnCompleted(Action continuation) {
            if (this._task == null) { continuation?.Invoke(); return; }
            this._task.RegisterContinuation(continuation);
        }
    }
}
