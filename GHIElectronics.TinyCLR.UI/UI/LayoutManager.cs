////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI {
    // LayoutManager is responsible for all layout operations. It maintains the Arrange,
    // Invalidate and Measure queues.
    //
    internal class LayoutManager : DispatcherObject {
        public class LayoutQueue {
            public LayoutQueue(LayoutManager layoutManager) {
                this._layoutManager = layoutManager;
                this._elements = new ArrayList();
            }

            public bool IsEmpty => this._elements.Count == 0;

            public void Add(UIElement e) {
                if (!this._elements.Contains(e)) {
                    RemoveOrphans(e);
                    this._elements.Add(e);
                }

                this._layoutManager.NeedsRecalc();
            }

            public UIElement GetTopMost() {
                UIElement found = null;
                var treeLevel = int.MaxValue;

                var count = this._elements.Count;
                for (var index = 0; index < count; index++) {
                    var currentElement = (UIElement)this._elements[index];
                    var parent = currentElement._parent;

                    var cnt = 0;
                    while (parent != null && cnt < treeLevel) {
                        cnt++;
                        parent = parent._parent;
                    }

                    if (cnt < treeLevel) {
                        treeLevel = cnt;
                        found = currentElement;
                    }
                }

                return found;
            }

            public void Remove(UIElement e) => this._elements.Remove(e);

            public void RemoveOrphans(UIElement parent) {
                var count = this._elements.Count;
                for (var index = count - 1; index >= 0; index--) {
                    var child = (UIElement)this._elements[index];
                    if (child._parent == parent) {
                        this._elements.RemoveAt(index);
                    }
                }
            }

            private LayoutManager _layoutManager;

            private ArrayList _elements;
        }

        private class SingletonLock {
        }

        private LayoutManager() {
            // This constructor exists to prevent instantiation of a LayoutManager by any
            // means other than through LayoutManager.CurrentLayoutManager.
            this._updateLayoutBackground = new DispatcherOperationCallback(this.UpdateLayoutBackground);
            this._updateCallback = new DispatcherOperationCallback(this.UpdateLayoutCallback);
        }

        // posts a layout update
        private void NeedsRecalc() {
            if (!this._layoutRequestPosted && !this._isUpdating) {
                this._layoutRequestPosted = true;
                MediaContext.From(this.Dispatcher).BeginInvokeOnRender(this._updateCallback, this);
            }
        }

        private object UpdateLayoutBackground(object arg) {
            this.NeedsRecalc();
            return null;
        }

        private object UpdateLayoutCallback(object arg) {
            this.UpdateLayout();
            return null;
        }

        public LayoutQueue ArrangeQueue {
            get {
                if (this._arrangeQueue == null) {
                    lock (typeof(SingletonLock)) {
                        if (this._arrangeQueue == null) {
                            this._arrangeQueue = new LayoutQueue(this);
                        }
                    }
                }

                return this._arrangeQueue;
            }
        }

        // Returns the LayoutManager singleton
        //
        public static LayoutManager CurrentLayoutManager => LayoutManager.From(Dispatcher.CurrentDispatcher);

        public static LayoutManager From(Dispatcher dispatcher) {
            if (dispatcher == null) throw new ArgumentException();

            if (dispatcher._layoutManager == null) {
                lock (typeof(SingletonLock)) {
                    if (dispatcher._layoutManager == null) {
                        dispatcher._layoutManager = new LayoutManager();
                    }
                }
            }

            return dispatcher._layoutManager;
        }

        public LayoutQueue MeasureQueue {
            get {
                if (this._measureQueue == null) {
                    lock (typeof(SingletonLock)) {
                        if (this._measureQueue == null) {
                            this._measureQueue = new LayoutQueue(this);
                        }
                    }
                }

                return this._measureQueue;
            }
        }

        public void UpdateLayout() {
            VerifyAccess();

            //make UpdateLayout to be a NOP if called during UpdateLayout.
            if (this._isUpdating) return;

            this._isUpdating = true;

            WindowManager.Instance.Invalidate();

            var measureQueue = this.MeasureQueue;
            var arrangeQueue = this.ArrangeQueue;

            var cnt = 0;
            var gotException = true;
            UIElement currentElement = null;

            //NOTE:
            //
            //There are a bunch of checks here that break out of and re-queue layout if
            //it looks like things are taking too long or we have somehow gotten into an
            //infinite loop.   In the TinyCLR we will probably have better ways of
            //dealing with a bad app through app domain separation, but keeping this
            //robustness can't hurt.  In a single app domain scenario, it could
            //give the opportunity to get out to the system if something is misbehaving,
            //we like this kind of reliability in embedded systems.
            //

            try {
                invalidateTreeIfRecovering();

                while ((!this.MeasureQueue.IsEmpty) || (!this.ArrangeQueue.IsEmpty)) {
                    if (++cnt > 153) {
                        //loop detected. Lets re-queue and let input/user to correct the situation.
                        //
                        this.Dispatcher.BeginInvoke(this._updateLayoutBackground, this);
                        currentElement = null;
                        gotException = false;
                        return;
                    }

                    //loop for Measure
                    //We limit the number of loops here by time - normally, all layout
                    //calculations should be done by this time, this limit is here for
                    //emergency, "infinite loop" scenarios - yielding in this case will
                    //provide user with ability to continue to interact with the app, even though
                    //it will be sluggish. If we don't yield here, the loop is goign to be a deadly one

                    var loopCounter = 0;
                    long loopStartTime = 0;

                    while (true) {
                        if (++loopCounter > 153) {
                            loopCounter = 0;
                            if (LimitExecution(ref loopStartTime)) {
                                currentElement = null;
                                gotException = false;
                                return;
                            }
                        }

                        currentElement = measureQueue.GetTopMost();

                        if (currentElement == null) break; //exit if no more Measure candidates

                        currentElement.Measure(
                            currentElement._previousAvailableWidth,
                            currentElement._previousAvailableHeight
                            );

                        measureQueue.RemoveOrphans(currentElement);
                    }

                    //loop for Arrange
                    //if Arrange dirtied the tree go clean it again

                    //We limit the number of loops here by time - normally, all layout
                    //calculations should be done by this time, this limit is here for
                    //emergency, "infinite loop" scenarios - yielding in this case will
                    //provide user with ability to continue to interact with the app, even though
                    //it will be sluggish. If we don't yield here, the loop is goign to be a deadly one
                    loopCounter = 0;
                    loopStartTime = 0;

                    while (true) {
                        if (++loopCounter > 153) {
                            loopCounter = 0;
                            if (LimitExecution(ref loopStartTime)) {
                                currentElement = null;
                                gotException = false;
                                return;
                            }
                        }

                        currentElement = arrangeQueue.GetTopMost();

                        if (currentElement == null) break; //exit if no more Arrange candidates

                        getProperArrangeRect(currentElement, out var arrangeX, out var arrangeY, out var arrangeWidth, out var arrangeHeight);

#if TINYCLR_DEBUG_LAYOUT
                        Trace.Print("arrangeWidth = " + arrangeWidth);
                        Trace.Print("arrangeHeight = " + arrangeWidth);
#endif

                        currentElement.Arrange(arrangeX, arrangeY, arrangeWidth, arrangeHeight);
                        arrangeQueue.RemoveOrphans(currentElement);
                    }

                    /* REFACTOR -- do we need Layout events and Size changed events?

                                        //let LayoutUpdated handlers to call UpdateLayout
                                        //note that it means we can get reentrancy into UpdateLayout past this point,
                                        //if any of event handlers call UpdateLayout sync. Need to protect from reentrancy
                                        //in the firing methods below.

                                        fireSizeChangedEvents();
                                        if ((!MeasureQueue.IsEmpty) || (!ArrangeQueue.IsEmpty)) continue;
                                        fireLayoutUpdateEvent();
                    */
                }

                currentElement = null;
                gotException = false;
            }
            finally {
                this._isUpdating = false;
                this._layoutRequestPosted = false;

                if (gotException) {
                    //set indicator
                    this._gotException = true;
                    this._forceLayoutElement = currentElement;

                    //make attempt to request the subsequent layout calc
                    this.Dispatcher.BeginInvoke(this._updateLayoutBackground, this);
                }
            }
        }

        //
        // ensures we don't spend all day doing layout, and
        // give the system the chance to do something else.
        private bool LimitExecution(ref long loopStartTime) {
            if (loopStartTime == 0) {
                loopStartTime = DateTime.Now.Ticks;
            }
            else {
                if ((DateTime.Now.Ticks - loopStartTime) > 153 * 2 * TimeSpan.TicksPerMillisecond) // 153*2 = magic*science
                {
                    //loop detected. Lets go over to background to let input work.
                    this.Dispatcher.BeginInvoke(this._updateLayoutBackground, this);
                    return true;
                }
            }

            return false;
        }

        private void getProperArrangeRect(UIElement element, out int x, out int y, out int width, out int height) {
            x = element._finalX;
            y = element._finalY;
            width = element._finalWidth;
            height = element._finalHeight;

            // ELements without a parent (top level) get Arrange at DesiredSize
            // if they were measured "to content" (as Constants.MaxExtent indicates).
            // If we arrange the element that is temporarily disconnected
            // so it is not a top-level one, the assumption is that it will be
            // layout-invalidated and/or recomputed by the parent when reconnected.
            if (element.Parent == null) {
                x = y = 0;
                element.GetDesiredSize(out var desiredWidth, out var desiredHeight);

                if (element._previousAvailableWidth == Media.Constants.MaxExtent)
                    width = desiredWidth;

                if (element._previousAvailableHeight == Media.Constants.MaxExtent)
                    height = desiredHeight;
            }
        }

        private void invalidateTreeIfRecovering() {
            if ((this._forceLayoutElement != null) || this._gotException) {
                if (this._forceLayoutElement != null) {
                    var e = this._forceLayoutElement.RootUIElement;

                    markTreeDirtyHelper(e);
                    this.MeasureQueue.Add(e);
                }

                this._forceLayoutElement = null;
                this._gotException = false;
            }
        }

        private void markTreeDirtyHelper(UIElement e) {
            //now walk down and mark all UIElements dirty
            if (e != null) {
                e._flags |= (UIElement.Flags.InvalidMeasure | UIElement.Flags.InvalidArrange);

                var uiec = e._logicalChildren;

                if (uiec != null) {
                    for (var i = uiec.Count; i-- > 0;) {
                        markTreeDirtyHelper(uiec[i]);
                    }
                }
            }
        }

        private bool _isUpdating;
        private bool _gotException; //true if UpdateLayout exited with exception
        private bool _layoutRequestPosted;

        private UIElement _forceLayoutElement; //set in extreme situations, forces the update of the whole tree containing the element

        // measure & arrange queues.
        private LayoutQueue _arrangeQueue;
        private LayoutQueue _measureQueue;

        private DispatcherOperationCallback _updateLayoutBackground;
        private DispatcherOperationCallback _updateCallback;

    }

}


