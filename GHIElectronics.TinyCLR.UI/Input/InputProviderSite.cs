////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {

    /// <summary>
    ///     The object which input providers use to report input to the input manager.
    /// </summary>
    public class InputProviderSite : IDisposable {
        internal InputProviderSite(InputManager inputManager, object inputProvider) {
            this._inputManager = inputManager ?? throw new ArgumentNullException("inputManager");
            this._inputProvider = inputProvider;
        }

        /// <summary>
        ///     Returns the input manager that this site is attached to.
        /// </summary>
        public InputManager InputManager => this._inputManager;

        /// <summary>
        ///     Unregisters this input provider.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this._isDisposed) {
                this._isDisposed = true;

                if (this._inputManager != null && this._inputProvider != null) {
                    this._inputManager.UnregisterInputProvider(this._inputProvider);
                }

                this._inputManager = null;
                this._inputProvider = null;
            }

        }

        /// <summary>
        /// Returns true if we are disposed.
        /// </summary>
        public bool IsDisposed => this._isDisposed;

        /// <summary>
        ///     Reports input to the input manager.
        /// </summary>
        /// <returns>
        ///     Whether or not any event generated as a consequence of this
        ///     event was handled.
        /// </returns>
        // do we really need this?  Make the "providers" call InputManager.ProcessInput themselves.
        // we currently need to map back to providers for other reasons.
        public bool ReportInput(InputDevice device, InputReport inputReport) {
            if (this._isDisposed) {
                throw new InvalidOperationException();
            }

            var handled = false;

            var input = new InputReportEventArgs(device, inputReport) {
                RoutedEvent = InputManager.PreviewInputReportEvent
            };

            if (this._inputManager != null) {
                handled = this._inputManager.ProcessInput(input);
            }

            return handled;
        }

        private bool _isDisposed;

        private InputManager _inputManager;

        private object _inputProvider;
    }
}


