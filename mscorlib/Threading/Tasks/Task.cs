using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
    public class Task {
        internal Exception _exception;
        internal TaskStatus _status = TaskStatus.RanToCompletion;

        // NOTE: Real async/await with timer-driven Task.Delay is not yet
        // supported. Two TinyCLR runtime gaps prevent it:
        //   (1) `ldobj !!T` / `box !!T` for VAR/MVAR-typed params don't
        //       actually value-copy / box a struct — they just slot-copy an
        //       object reference, which is garbage for a struct on stack.
        //   (2) Even with AsyncTaskMethodBuilder as a class (fixing the
        //       builder's interior-pointer issue), the state machine STRUCT
        //       captured at the first await cannot be resumed correctly from
        //       the Timer thread because its `this` reference becomes stale.
        // Until MMP can rewrite async state machines from struct to class (or
        // the interpreter learns to handle these cases), Task.Delay stays a
        // synchronous Thread.Sleep and `await` over a non-completed Task is
        // broken. The flow that DID work pre-2026-05-13 (sync Delay, inline
        // continuation, awaiter.IsCompleted always true at the check) is
        // preserved.

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
