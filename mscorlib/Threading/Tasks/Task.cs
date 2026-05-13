using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    public class Task {
        internal Exception _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        // NOTE: `await Task.Delay(N)` blocks the calling thread for N ms via
        // Thread.Sleep, rather than yielding control via a Timer-based async
        // resume. Other threads (system threads, user-spawned threads, ISRs,
        // peripherals) are unaffected. End-to-end timing of `await`-using code
        // is identical to "real" async — only the semantics of "what does the
        // thread do during the wait" differ. For embedded targets where you'd
        // spawn a Thread for parallel work anyway, this is functionally
        // equivalent and avoids the deep runtime work needed to make struct
        // state-machine resume safe under TinyCLR's generic-type erasure.
        // See [[task-async-state-machine-limitation]] memory for the
        // investigation that arrived at this trade-off.

        public TaskStatus Status => this._status;
        public bool IsCompleted => this._status == TaskStatus.RanToCompletion || this._status == TaskStatus.Faulted || this._status == TaskStatus.Canceled;
        public bool IsFaulted => this._status == TaskStatus.Faulted;
        public bool IsCanceled => this._status == TaskStatus.Canceled;
        public Exception Exception => this._exception;

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
