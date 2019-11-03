////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>
    /// The MediaContext class controls the rendering
    /// </summary>
    internal class MediaContext : DispatcherObject, IDisposable {
        /// <summary>
        /// The MediaContext lives in the Dispatcher and is the MediaSystem's class that keeps
        /// per Dispatcher state.
        /// </summary>
        internal MediaContext() {
            this._renderMessage = new DispatcherOperationCallback(this.RenderMessageHandler);

            // we have one render target, the window manager
            var disp = WindowManager.Instance.DisplayController;

            this._target = WindowManager.Instance;

            if (disp.Hdc != IntPtr.Zero) 
                this._screen = new Bitmap(Graphics.FromHdc(disp.Hdc));
            else 
                this._screen = new Bitmap(Graphics.FromImage(new System.Drawing.Bitmap(disp.ActiveConfiguration.Width, disp.ActiveConfiguration.Height)));

            this._screenW = (int)disp.ActiveConfiguration.Width;
            this._screenH = (int)disp.ActiveConfiguration.Height;
            this._dirtyX0 = (int)disp.ActiveConfiguration.Width;
            this._dirtyY0 = (int)disp.ActiveConfiguration.Height;
        }

        /// <summary>
        /// Gets the MediaContext from the context passed in as argument.
        /// </summary>
        internal static MediaContext From(Dispatcher dispatcher) {
            Debug.Assert(dispatcher != null, "Dispatcher required");

            var cm = dispatcher._mediaContext;

            if (cm == null) {
                lock (typeof(GlobalLock)) {
                    cm = dispatcher._mediaContext;

                    if (cm == null) {
                        cm = new MediaContext();
                        dispatcher._mediaContext = cm;
                    }
                }
            }

            return cm;
        }

        private class InvokeOnRenderCallback {
            private DispatcherOperationCallback _callback;
            private object _arg;

            public InvokeOnRenderCallback(
                DispatcherOperationCallback callback,
                object arg) {
                this._callback = callback;
                this._arg = arg;
            }

            public void DoWork() => this._callback(this._arg);
        }

        internal void BeginInvokeOnRender(DispatcherOperationCallback callback, object arg) {
            Debug.Assert(callback != null);

            // While technically it could be OK for the arg to be null, for now
            // I know that arg represents the this reference for the layout
            // process and should never be null.

            Debug.Assert(arg != null);

            if (this._invokeOnRenderCallbacks == null) {
                lock (this) {
                    if (this._invokeOnRenderCallbacks == null) {
                        this._invokeOnRenderCallbacks = new ArrayList();
                    }
                }
            }

            lock (this._invokeOnRenderCallbacks) {
                this._invokeOnRenderCallbacks.Add(new InvokeOnRenderCallback(callback, arg));
            }

            PostRender();
        }

        /// <summary>
        /// If there is already a render operation in the Dispatcher queue, this
        /// method will do nothing.  If not, it will add a
        /// render operation.
        /// </summary>
        /// <remarks>
        /// This method should only be called when a render is necessary "right
        /// now."  Events such as a change to the visual tree would result in
        /// this method being called.
        /// </remarks>
        internal void PostRender() {
            VerifyAccess();

            if (!this._isRendering) {
                if (this._currentRenderOp == null) {
                    // If we don't have a render operation in the queue, add one
                    this._currentRenderOp = this.Dispatcher.BeginInvoke(this._renderMessage, null);
                }
            }
        }

        internal void AddDirtyArea(int x, int y, int w, int h) {
            if (x < 0) x = 0;
            if (x + w > this._screenW) w = this._screenW - x;
            if (w <= 0) return;

            if (y < 0) y = 0;
            if (y + h > this._screenH) h = this._screenH - y;
            if (h <= 0) return;

            var x1 = x + w;
            var y1 = y + h;

            if (x < this._dirtyX0) this._dirtyX0 = x;
            if (y < this._dirtyY0) this._dirtyY0 = y;
            if (x1 > this._dirtyX1) this._dirtyX1 = x1;
            if (y1 > this._dirtyY1) this._dirtyY1 = y1;
        }

        private int _screenW = 0, _screenH = 0;
        private int _dirtyX0 = 0, _dirtyY0 = 0, _dirtyX1 = 0, _dirtyY1 = 0;

        /// <summary>
        /// This is the standard RenderMessageHandler callback, posted via PostRender()
        /// and Resize().  This wraps RenderMessageHandlerCore and emits an ETW events
        /// to trace its execution.
        /// </summary>
        internal object RenderMessageHandler(object arg) {
            try {
                this._isRendering = true;

                //_screen.Clear();

                if (this._invokeOnRenderCallbacks != null) {
                    var callbackLoopCount = 0;
                    var count = this._invokeOnRenderCallbacks.Count;

                    while (count > 0) {
                        callbackLoopCount++;
                        if (callbackLoopCount > 153) {
                            throw new InvalidOperationException("infinite loop");
                        }

                        InvokeOnRenderCallback[] callbacks;

                        lock (this._invokeOnRenderCallbacks) {
                            count = this._invokeOnRenderCallbacks.Count;
                            callbacks = new InvokeOnRenderCallback[count];

                            this._invokeOnRenderCallbacks.CopyTo(callbacks);
                            this._invokeOnRenderCallbacks.Clear();
                        }

                        for (var i = 0; i < count; i++) {
                            callbacks[i].DoWork();
                        }

                        count = this._invokeOnRenderCallbacks.Count;
                    }
                }

                var dc = new DrawingContext(this._screen);

                /* The dirty rectange MUST be read after the InvokeOnRender callbacks are
                 * complete, as they can trigger layout changes or invalidate controls
                 * which are expected to be redrawn. */
                var x = this._dirtyX0;
                var y = this._dirtyY0;
                var w = this._dirtyX1 - this._dirtyX0;
                var h = this._dirtyY1 - this._dirtyY0;
                this._dirtyX0 = this._screenW; this._dirtyY0 = this._screenH;
                this._dirtyX1 = this._dirtyY1 = 0;

                try {
                    if (w > 0 && h > 0) {
                        //
                        // This is the big Render!
                        //
                        // We've now updated layout and the updated scene will be
                        // rendered.
                        dc.PushClippingRectangle(x, y, w, h);
                        this._target.RenderRecursive(dc);
                        dc.PopClippingRectangle();
                    }
                }
                finally {
                    dc.Close();
                    if (w > 0 && h > 0) {
                        this._screen.Flush(x, y, w, h);
                    }








         }
            }
            finally {
                this._currentRenderOp = null;
                this._isRendering = false;
            }

            return null;
        }

        /// <summary>
        /// Message delegate.
        /// </summary>
        private DispatcherOperation _currentRenderOp;
        private DispatcherOperationCallback _renderMessage;

        /// <summary>
        /// Indicates that we are in the middle of processing a render message.
        /// </summary>
        private bool _isRendering;

        private ArrayList _invokeOnRenderCallbacks;

        private UIElement _target;
        private Bitmap _screen;

        private class GlobalLock { }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => this._screen.Dispose();

    }
}


