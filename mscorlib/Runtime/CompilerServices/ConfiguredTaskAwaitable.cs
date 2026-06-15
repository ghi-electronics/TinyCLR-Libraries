using System.Threading.Tasks;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Result of <c>task.ConfigureAwait(bool)</c>. On TinyCLR this is a no-op
    /// wrapper around the underlying Task — there is no SynchronizationContext
    /// to capture or skip, so `continueOnCapturedContext` is informational
    /// only. The type exists for source compatibility with portable code that
    /// calls <c>.ConfigureAwait(false)</c> defensively.
    /// </summary>
    public struct ConfiguredTaskAwaitable {
        private readonly Task _task;

        internal ConfiguredTaskAwaitable(Task task) { this._task = task; }

        public ConfiguredTaskAwaiter GetAwaiter() => new ConfiguredTaskAwaiter(this._task);

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion {
            private readonly Task _task;

            internal ConfiguredTaskAwaiter(Task task) { this._task = task; }

            public bool IsCompleted => this._task == null || this._task.IsCompleted;

            public void GetResult() {
                if (this._task != null) this._task.Wait();
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

    /// <summary>
    /// Generic counterpart of <see cref="ConfiguredTaskAwaitable"/> for
    /// <see cref="Task{TResult}"/>.
    /// </summary>
    public struct ConfiguredTaskAwaitable<TResult> {
        private readonly Task<TResult> _task;

        internal ConfiguredTaskAwaitable(Task<TResult> task) { this._task = task; }

        public ConfiguredTaskAwaiter GetAwaiter() => new ConfiguredTaskAwaiter(this._task);

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion {
            private readonly Task<TResult> _task;

            internal ConfiguredTaskAwaiter(Task<TResult> task) { this._task = task; }

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
}
