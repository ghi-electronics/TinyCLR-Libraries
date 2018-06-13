////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GHIElectronics.TinyCLR.UI.Threading {
    /// <summary>
    ///     DispatcherOperation represents a delegate that has been
    ///     posted to the Dispatcher queue.
    /// </summary>
    public sealed class DispatcherOperation {

        internal DispatcherOperation(
            Dispatcher dispatcher,
            DispatcherOperationCallback method,
            object args) {
            this._dispatcher = dispatcher;
            this._method = method;
            this._args = args;
        }

        /// <summary>
        ///     Returns the Dispatcher that this operation was posted to.
        /// </summary>
        public Dispatcher Dispatcher => this._dispatcher;

        /// <summary>
        ///     The status of this operation.
        /// </summary>
        public DispatcherOperationStatus Status {
            get => this._status;

            internal set => this._status = value;
        }

        /// <summary>
        ///     Waits for this operation to complete.
        /// </summary>
        /// <returns>
        ///     The status of the operation.  To obtain the return value
        ///     of the invoked delegate, use the the Result property.
        /// </returns>
        public DispatcherOperationStatus Wait() => Wait(new TimeSpan(-TimeSpan.TicksPerMillisecond)); /// Negative one (-1) milliseconds to prevent the timer from starting. See documentation.


        /// <summary>
        ///     Waits for this operation to complete.
        /// </summary>
        /// <param name="timeout">
        ///     The maximum amount of time to wait.
        /// </param>
        /// <returns>
        ///     The status of the operation.  To obtain the return value
        ///     of the invoked delegate, use the the Result property.
        /// </returns>
        public DispatcherOperationStatus Wait(TimeSpan timeout) {
            if ((this._status == DispatcherOperationStatus.Pending || this._status == DispatcherOperationStatus.Executing) &&
                timeout.Ticks != 0) {
                if (this._dispatcher.Thread == Thread.CurrentThread) {
                    if (this._status == DispatcherOperationStatus.Executing) {
                        // We are the dispatching thread, and the current operation state is
                        // executing, which means that the operation is in the middle of
                        // executing (on this thread) and is trying to wait for the execution
                        // to complete.  Unfortunately, the thread will now deadlock, so
                        // we throw an exception instead.
                        throw new InvalidOperationException();
                    }

                    // We are the dispatching thread for this operation, so
                    // we can't block.  We will push a frame instead.
                    var frame = new DispatcherOperationFrame(this, timeout);
                    Dispatcher.PushFrame(frame);
                }
                else {
                    // We are some external thread, so we can just block.  Of
                    // course this means that the Dispatcher (queue)for this
                    // thread (if any) is now blocked.
                    // Because we have a single dispatcher per app domain, this thread
                    // must be from another app domain.  We will enforce semantics on
                    // dispatching between app domains so we don't lock up the system.

                    var wait = new DispatcherOperationEvent(this, timeout);
                    wait.WaitOne();
                }
            }

            return this._status;
        }

        /// <summary>
        ///     Aborts this operation.
        /// </summary>
        /// <returns>
        ///     False if the operation could not be aborted (because the
        ///     operation was already in  progress)
        /// </returns>
        public bool Abort() {
            var removed = this._dispatcher.Abort(this);

            if (removed) {
                this._status = DispatcherOperationStatus.Aborted;

                // Raise the Aborted so anyone who is waiting will wake up.
                Aborted?.Invoke(this, EventArgs.Empty);
            }

            return removed;
        }

        /// <summary>
        ///     Returns the result of the operation if it has completed.
        /// </summary>
        public object Result => this._result;

        /// <summary>
        ///     An event that is raised when the operation is aborted.
        /// </summary>
        public event EventHandler Aborted;

        /// <summary>
        ///     An event that is raised when the operation completes.
        /// </summary>
        public event EventHandler Completed;

        internal void OnCompleted() => Completed?.Invoke(this, EventArgs.Empty);

        private class DispatcherOperationFrame : DispatcherFrame, IDisposable {
            // Note: we pass "exitWhenRequested=false" to the base
            // DispatcherFrame construsctor because we do not want to exit
            // this frame if the dispatcher is shutting down. This is
            // because we may need to invoke operations during the shutdown process.
            public DispatcherOperationFrame(DispatcherOperation op, TimeSpan timeout)
                : base(false) {
                this._operation = op;

                // We will exit this frame once the operation is completed or aborted.
                this._operation.Aborted += new EventHandler(this.OnCompletedOrAborted);
                this._operation.Completed += new EventHandler(this.OnCompletedOrAborted);

                // We will exit the frame if the operation is not completed within
                // the requested timeout.
                if (timeout.Ticks > 0) {
                    this._waitTimer = new Timer(new TimerCallback(this.OnTimeout),
                                           null,
                                           timeout,
                                           new TimeSpan(-TimeSpan.TicksPerMillisecond)); /// Negative one (-1) milliseconds to disable periodic signaling.
                }

                // Some other thread could have aborted the operation while we were
                // setting up the handlers.  We check the state again and mark the
                // frame as "should not continue" if this happened.
                if (this._operation._status != DispatcherOperationStatus.Pending) {
                    Exit();
                }
            }

            private void OnCompletedOrAborted(object sender, EventArgs e) => Exit();

            private void OnTimeout(object arg) => Exit();

            private void Exit() {
                this.Continue = false;

                if (this._waitTimer != null) {
                    this._waitTimer.Dispose();
                }

                this._operation.Aborted -= new EventHandler(this.OnCompletedOrAborted);
                this._operation.Completed -= new EventHandler(this.OnCompletedOrAborted);
            }

            private DispatcherOperation _operation;
            private Timer _waitTimer;

            public virtual void Close() => Dispose();

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) => this._waitTimer.Dispose();

        }

        private class DispatcherOperationEvent : IDisposable {
            public DispatcherOperationEvent(DispatcherOperation op, TimeSpan timeout) {
                this._operation = op;
                this._timeout = timeout;
                this._event = new AutoResetEvent(false);

                // We will set our event once the operation is completed or aborted.
                this._operation.Aborted += new EventHandler(this.OnCompletedOrAborted);
                this._operation.Completed += new EventHandler(this.OnCompletedOrAborted);

                // Since some other thread is dispatching this operation, it could
                // have been dispatched while we were setting up the handlers.
                // We check the state again and set the event ourselves if this
                // happened.
                if (this._operation._status != DispatcherOperationStatus.Pending && this._operation._status != DispatcherOperationStatus.Executing) {
                    this._event.Set();
                }
            }

            private void OnCompletedOrAborted(object sender, EventArgs e) => this._event.Set();

            public void WaitOne() {
                this._waitTimer = new Timer(new TimerCallback(this.OnTimeout), null, this._timeout, new TimeSpan(-TimeSpan.TicksPerMillisecond));
                this._event.WaitOne();
                this._waitTimer.Dispose();

                // Cleanup the events.
                this._operation.Aborted -= new EventHandler(this.OnCompletedOrAborted);
                this._operation.Completed -= new EventHandler(this.OnCompletedOrAborted);
            }

            private void OnTimeout(object arg) => this._event.Set();

            private DispatcherOperation _operation;
            private TimeSpan _timeout;
            private AutoResetEvent _event;
            private Timer _waitTimer;

            public virtual void Close() => Dispose();

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) => this._waitTimer.Dispose();
        }

        private Dispatcher _dispatcher;
        internal DispatcherOperationCallback _method;
        internal object _args;
        internal object _result;
        internal DispatcherOperationStatus _status;
    }
}


