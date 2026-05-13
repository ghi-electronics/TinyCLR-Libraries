using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    public struct TaskAwaiter : ICriticalNotifyCompletion {
        private readonly Task _task;

        internal TaskAwaiter(Task task) { this._task = task; }

        public bool IsCompleted => this._task == null || this._task.IsCompleted;

        public void GetResult() {
            if (this._task != null) this._task.Wait();
        }

        // Runs continuation inline. Real continuation queueing requires a
        // working async/await state-machine resume path, which TinyCLR
        // doesn't have yet (see Task.cs note).
        public void OnCompleted(Action continuation) {
            if (continuation != null) continuation();
        }

        public void UnsafeOnCompleted(Action continuation) {
            if (continuation != null) continuation();
        }
    }

    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion {
        private readonly Task<TResult> _task;

        internal TaskAwaiter(Task<TResult> task) { this._task = task; }

        public bool IsCompleted => this._task == null || this._task.IsCompleted;

        public TResult GetResult() {
            if (this._task == null) return default(TResult);
            this._task.Wait();
            return this._task.Result;
        }

        public void OnCompleted(Action continuation) {
            if (continuation != null) continuation();
        }

        public void UnsafeOnCompleted(Action continuation) {
            if (continuation != null) continuation();
        }
    }
}
