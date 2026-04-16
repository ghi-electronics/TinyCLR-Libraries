using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    public struct TaskAwaiter : ICriticalNotifyCompletion {
        private readonly Task _task;

        internal TaskAwaiter(Task task) { this._task = task; }

        public bool IsCompleted => this._task == null || this._task.IsCompleted;

        public void GetResult() {
            if (this._task != null) this._task.Wait();
        }

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
