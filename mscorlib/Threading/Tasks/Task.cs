using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    public class Task {
        internal Exception _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        public TaskStatus Status => this._status;
        public bool IsCompleted => this._status == TaskStatus.RanToCompletion || this._status == TaskStatus.Faulted || this._status == TaskStatus.Canceled;
        public bool IsFaulted => this._status == TaskStatus.Faulted;
        public Exception Exception => this._exception;

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
