////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GHIElectronics.TinyCLR.UI.Threading {
    /// <summary>
    ///     A timer that is integrated into the Dispatcher queues, and will
    ///     be processed after a given amount of time
    /// </summary>
    public class DispatcherTimer : IDisposable {
        /// <summary>
        ///     Creates a timer that uses the current thread's Dispatcher to
        ///     process the timer event
        /// </summary>
        public DispatcherTimer()
            : this(Dispatcher.CurrentDispatcher) {
        }

        /// <summary>
        ///     Creates a timer that uses the specified Dispatcher to
        ///     process the timer event.
        /// </summary>
        /// <param name="dispatcher">
        ///     The dispatcher to use to process the timer.
        /// </param>
        public DispatcherTimer(Dispatcher dispatcher) {
            this._dispatcher = dispatcher ?? throw new ArgumentNullException("dispatcher");

            this._timer = new Timer(new TimerCallback(this.Callback), null, Timeout.Infinite, Timeout.Infinite);

        }

        /// <summary>
        ///     Gets the dispatcher this timer is associated with.
        /// </summary>
        public Dispatcher Dispatcher => this._dispatcher;

        /// <summary>
        ///     Gets or sets whether the timer is running.
        /// </summary>
        public bool IsEnabled {
            get => this._isEnabled;

            set {
                lock (this._instanceLock) {
                    if (!value && this._isEnabled) {
                        Stop();
                    }
                    else if (value && !this._isEnabled) {
                        Start();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the time between timer ticks.
        /// </summary>
        public TimeSpan Interval {
            get => new TimeSpan(this._interval * TimeSpan.TicksPerMillisecond);

            set {
                var updateTimer = false;

                var ticks = value.Ticks;

                if (ticks < 0)
                    throw new ArgumentOutOfRangeException("value", "too small");

                if (ticks > int.MaxValue * TimeSpan.TicksPerMillisecond)
                    throw new ArgumentOutOfRangeException("value", "too large");

                lock (this._instanceLock) {
                    this._interval = (int)(ticks / TimeSpan.TicksPerMillisecond);

                    if (this._isEnabled) {
                        updateTimer = true;
                    }
                }

                if (updateTimer) {
                    this._timer.Change(this._interval, this._interval);
                }
            }
        }

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        public void Start() {
            lock (this._instanceLock) {
                if (!this._isEnabled) {
                    this._isEnabled = true;

                    this._timer.Change(this._interval, this._interval);
                }
            }
        }

        /// <summary>
        ///     Stops the timer.
        /// </summary>
        public void Stop() {
            lock (this._instanceLock) {
                if (this._isEnabled) {
                    this._isEnabled = false;

                    this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        ///     Occurs when the specified timer interval has elapsed and the
        ///     timer is enabled.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        ///     Any data that the caller wants to pass along with the timer.
        /// </summary>
        public object Tag {
            get => this._tag;

            set => this._tag = value;
        }

        private void Callback(object state) =>
            // BeginInvoke a new operation.
            this._dispatcher.BeginInvoke(
                new DispatcherOperationCallback(this.FireTick),
                null);

        private object FireTick(object unused) {
            Tick?.Invoke(this, EventArgs.Empty);

            return null;
        }

        private object _instanceLock = new object();
        private Dispatcher _dispatcher;
        private int _interval;
        private object _tag;
        private bool _isEnabled;
        private Timer _timer;

        public virtual void Close() => Dispose();

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => this._timer.Dispose();
    }
}


