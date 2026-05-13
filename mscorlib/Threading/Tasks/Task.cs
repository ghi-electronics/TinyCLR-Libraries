using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    public class Task {
        internal Exception _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        public TaskStatus Status => this._status;
        public bool IsCompleted => this._status == TaskStatus.RanToCompletion || this._status == TaskStatus.Faulted || this._status == TaskStatus.Canceled;
        public bool IsFaulted => this._status == TaskStatus.Faulted;
        public bool IsCanceled => this._status == TaskStatus.Canceled;
        public Exception Exception => this._exception;

        // Pre-completed singleton. Safe to return for any sync method that promises a Task
        // result; matches .NET BCL behavior of being a shared instance.
        public static readonly Task CompletedTask = new Task();

        public TaskAwaiter GetAwaiter() => new TaskAwaiter(this);

        public void Wait() {
            if (this._status == TaskStatus.Faulted && this._exception != null) throw this._exception;
        }

        public static Task Delay(int millisecondsDelay) {
            if (millisecondsDelay > 0) Thread.Sleep(millisecondsDelay);
            return new Task();
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

        internal void SetException(Exception ex) {
            this._exception = ex;
            this._status = TaskStatus.Faulted;
        }
    }

    public class Task<TResult> : Task {
        internal TResult _result;

        public TResult Result => this._result;

        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);
    }
}
