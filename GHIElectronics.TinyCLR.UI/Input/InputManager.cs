////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    ///     The InputManager class is responsible for coordinating all of the
    ///     input system in TinyCore.
    ///
    ///     The input manager exists per Dispatcher
    /// </summary>
    public sealed class InputManager : DispatcherObject {

        /// <summary>
        ///     A routed event indicating that an input report arrived.
        /// </summary>
        public static readonly RoutedEvent PreviewInputReportEvent = new RoutedEvent("PreviewInputReport", RoutingStrategy.Tunnel, typeof(InputReportEventHandler));

        /// <summary>
        ///     A routed event indicating that an input report arrived.
        /// </summary>
        public static readonly RoutedEvent InputReportEvent = new RoutedEvent("InputReport", RoutingStrategy.Bubble, typeof(InputReportEventHandler));

        /// <summary>
        ///     Return the input manager associated with the current context.
        /// </summary>
        /// <remarks>
        ///     This class will not be available in internet zone.
        /// </remarks>
        public static InputManager CurrentInputManager {
            get {
                var dispatcher = Dispatcher.CurrentDispatcher;

                if (dispatcher == null) throw new InvalidOperationException("no dispatcher");

                if (dispatcher._inputManager == null) {
                    lock (typeof(GlobalLock)) {
                        if (dispatcher._inputManager == null) {
                            dispatcher._inputManager = new InputManager();
                        }
                    }
                }

                return dispatcher._inputManager;
            }
        }

        private InputManager() {
            this._stagingArea = new Queue();
            this._currentStagingStack = new Stack();
            this._frameStagingArea = new ArrayList();
            this.InputDeviceEvents = new DeviceEvents[(int)InputDeviceType.Last];

            for (var i = 0; i < (int)InputDeviceType.Last; i++) {
                this.InputDeviceEvents[i] = new DeviceEvents();
            }

            this._continueProcessingStagingAreaCallback = new DispatcherOperationCallback(this.ProcessStagingArea);
            this._buttonDevice = new ButtonDevice(this);
            this._touchDevice = new TouchDevice(this);
            this._genericDevice = new GenericDevice(this);
        }

        public ButtonDevice ButtonDevice => this._buttonDevice;

        public TouchDevice TouchDevice => this._touchDevice;

        public GenericDevice GenericDevice => this._genericDevice;

        /// <summary>
        ///     Registers an input provider with the input manager.
        /// </summary>
        /// <param name="inputProvider">
        ///     The input provider to register.
        /// </param>
        public InputProviderSite RegisterInputProvider(object inputProvider) {
            VerifyAccess();

            // Create a site for this provider, and keep track of it.
            var site = new InputProviderSite(this, inputProvider);

            var idx = this._inputProviders.IndexOf(inputProvider);
            if (idx < 0) {
                this._inputProviders.Add(inputProvider);
                this._inputProviderSites.Add(site);
            }
            else {
                // NOTE -- should we dispose the old one?
                this._inputProviders[idx] = inputProvider;
                this._inputProviderSites[idx] = site;
            }

            return site;
        }

        internal void UnregisterInputProvider(object inputProvider) {
            var i = this._inputProviders.IndexOf(inputProvider);
            if (i >= 0) {
                this._inputProviders.RemoveAt(i);
                this._inputProviderSites.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Returns a collection of input providers registered with the input manager.
        /// </summary>
        public ICollection InputProviders {
            get {
                VerifyAccess();

                return this._inputProviders;
            }
        }

        /// <summary>
        ///     The MostRecentInputDevice represents the last input device to
        ///     report an "interesting" user action.  What exactly constitutes
        ///     such an action is up to each device to implement.
        /// </summary>
        public InputDevice MostRecentInputDevice {
            get { //If GenericInputDevice/TouchDevice use VerifyAccess in SetTarget, this verify can be removed.
                VerifyAccess(); return this._mostRecentInputDevice;
            }
            set { VerifyAccess(); this._mostRecentInputDevice = value; }
        }

        /// <summary>
        ///     Synchronously processes the specified input.
        /// </summary>
        /// <remarks>
        ///     The specified input is processed by all of the filters and
        ///     monitors, and is finally dispatched to the appropriate
        ///     element as an input event.
        /// </remarks>
        /// <returns>
        ///     Whether or not any event generated as a consequence of this
        ///     event was handled.
        /// </returns>
        public bool ProcessInput(InputEventArgs input) {
            VerifyAccess();

            if (input == null) {
                throw new ArgumentNullException("input");
            }

            // Push a marker indicating the portion of the staging area
            // that needs to be processed.
            var stk = new Stack();

            // Push the input to be processed onto the staging area.
            stk.Push(new StagingAreaInputItem(input, null));

            this._stagingArea.Enqueue(stk);

            // Post a work item to continue processing the staging area
            // in case someone pushes a dispatcher frame in the middle
            // of input processing.
            var frame = this.Dispatcher.CurrentFrame;
            if (frame != null) {
                if (!this._frameStagingArea.Contains(frame)) {
                    this._frameStagingArea.Add(frame);
                    this.Dispatcher.BeginInvoke(this._continueProcessingStagingAreaCallback, frame);
                }
            }

            return true;
        }

        private object ProcessStagingArea(object frame) {
            var handled = false;

            // NOTE -- avalon caches the XXXEventArgs.  In our system, the costs are different,
            // so it is probably cheaper for us to just create them, since everything gets created
            // on the heap anyways, and IL is expensive.  we should reconsider this if
            // its a performance impact.

            // Because we can be reentered, we can't just enumerate over the
            // staging area - that could throw an exception if the queue
            // changes underneath us.  Instead, just loop until we find a
            // frame marker or until the staging area is empty.

            try {
                while (this._stagingArea.Count > 0) {
                    this._currentStagingStack = this._stagingArea.Dequeue() as Stack;

                    do {
                        var item = (StagingAreaInputItem)this._currentStagingStack.Pop();

                        // Pre-Process the input.  This could modify the staging
                        // area, and it could cancel the processing of this
                        // input event.
                        //

                        var fCanceled = false;

                        var devType = (int)item.Input._inputDevice.DeviceType;

                        if (this.InputDeviceEvents[devType]._preProcessInput != null) {
                            PreProcessInputEventArgs preProcessInputEventArgs;
                            this.InputDeviceEvents[devType]._preProcessInput(this, preProcessInputEventArgs = new PreProcessInputEventArgs(item));

                            fCanceled = preProcessInputEventArgs._canceled;
                        }

                        if (!fCanceled) {
                            // Pre-Notify the input.
                            //
                            this.InputDeviceEvents[devType]._preNotifyInput?.Invoke(this, new NotifyInputEventArgs(item));

                            // Raise the input event being processed.
                            var input = item.Input;

                            // Some input events are explicitly associated with
                            // an element.  Those that are not instead are associated with
                            // the target of the input device for this event.
                            var eventSource = input._source as UIElement;

                            if (eventSource == null && input._inputDevice != null) {
                                eventSource = input._inputDevice.Target;
                            }

                            if (eventSource != null) {
                                eventSource.RaiseEvent(input);
                            }

                            // Post-Notify the input.
                            //
                            this.InputDeviceEvents[devType]._postNotifyInput?.Invoke(this, new NotifyInputEventArgs(item));

                            // Post-Process the input.  This could modify the staging
                            // area.
                            this.InputDeviceEvents[devType]._postProcessInput?.Invoke(this, new ProcessInputEventArgs(item));

                            // PreviewInputReport --> InputReport
                            if (item.Input._routedEvent == InputManager.PreviewInputReportEvent) {
                                if (!item.Input.Handled) {
                                    var previewInputReport = (InputReportEventArgs)item.Input;
                                    var inputReport = new InputReportEventArgs(previewInputReport.Device, previewInputReport.Report) {
                                        RoutedEvent = InputManager.InputReportEvent
                                    };

                                    this._currentStagingStack.Push(new StagingAreaInputItem(inputReport, item));
                                }
                            }

                            if (input.Handled) {
                                handled = true;
                            }
                        }
                    } while (this._currentStagingStack.Count > 0);
                }
            }
            finally {
                // It is possible that we can be re-entered by a nested
                // dispatcher frame.  Continue processing the staging
                // area if we need to.
                if (this._stagingArea.Count > 0) {
                    // Before we actually start to drain the staging area, we need
                    // to post a work item to process more input.  This enables us
                    // to process more input if we enter a nested pump.
                    this.Dispatcher.BeginInvoke(this._continueProcessingStagingAreaCallback, this.Dispatcher.CurrentFrame);
                }

                this._frameStagingArea.Remove(frame);
            }

            return handled;
        }

        private DispatcherOperationCallback _continueProcessingStagingAreaCallback;
        private ArrayList _frameStagingArea;

        public enum InputDeviceType : int {
            Button = 0,
            Touch,
            Generic,
            Last,
        }

        public class DeviceEvents : DispatcherObject {
            /// <summary>Subscribe for all input before it is processed</summary>
            public event PreProcessInputEventHandler PreProcessInput {
                add {
                    VerifyAccess();

                    // Add the handlers in reverse order so that handlers that
                    // users add are invoked before handlers in the system.

                    this._preProcessInput = (PreProcessInputEventHandler)Delegate.Combine(value, this._preProcessInput);
                }

                remove {
                    VerifyAccess();

                    this._preProcessInput = (PreProcessInputEventHandler)Delegate.Remove(this._preProcessInput, value);
                }
            }

            /// <summary>Subscribe for all input before it is notified</summary>
            public event NotifyInputEventHandler PreNotifyInput {
                add {
                    VerifyAccess();

                    // Add the handlers in reverse order so that handlers that
                    // users add are invoked before handlers in the system.

                    this._preNotifyInput = (NotifyInputEventHandler)Delegate.Combine(value, this._preNotifyInput);
                }

                remove {
                    VerifyAccess();
                    this._preNotifyInput = (NotifyInputEventHandler)Delegate.Remove(this._preNotifyInput, value);
                }

            }

            /// <summary>Subscribe to all input after it is notified</summary>
            public event NotifyInputEventHandler PostNotifyInput {
                add {
                    VerifyAccess();

                    // Add the handlers in reverse order so that handlers that
                    // users add are invoked before handlers in the system.

                    this._postNotifyInput = (NotifyInputEventHandler)Delegate.Combine(value, this._postNotifyInput);
                }

                remove {
                    VerifyAccess();

                    this._postNotifyInput = (NotifyInputEventHandler)Delegate.Remove(this._postNotifyInput, value);
                }
            }

            /// <summary>subscribe to all input after it is processed</summary>
            public event ProcessInputEventHandler PostProcessInput {
                add {
                    VerifyAccess();

                    // Add the handlers in reverse order so that handlers that
                    // users add are invoked before handlers in the system.

                    this._postProcessInput = (ProcessInputEventHandler)Delegate.Combine(value, this._postProcessInput);
                }

                remove {
                    VerifyAccess();

                    this._postProcessInput = (ProcessInputEventHandler)Delegate.Remove(this._postProcessInput, value);
                }
            }

            internal PreProcessInputEventHandler _preProcessInput;
            internal NotifyInputEventHandler _preNotifyInput;
            internal NotifyInputEventHandler _postNotifyInput;
            internal ProcessInputEventHandler _postProcessInput;

        }

        public DeviceEvents[] InputDeviceEvents;

        private ArrayList _inputProviders = new ArrayList();
        private ArrayList _inputProviderSites = new ArrayList();

        internal Stack _currentStagingStack;
        internal Queue _stagingArea;
        private InputDevice _mostRecentInputDevice;

        internal ButtonDevice _buttonDevice;
        internal TouchDevice _touchDevice;
        internal GenericDevice _genericDevice;

        private class GlobalLock { };
    }
}


