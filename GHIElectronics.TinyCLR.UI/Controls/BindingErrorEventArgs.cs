using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Direction of a binding operation that failed.</summary>
    public enum BindingErrorDirection {
        /// <summary>Source → control. The control couldn't read the bound property.</summary>
        Pull,
        /// <summary>Control → source. The control couldn't write the bound property.</summary>
        Push,
    }

    /// <summary>Handles a binding error reported by a control.</summary>
    public delegate void BindingErrorEventHandler(object sender, BindingErrorEventArgs e);

    /// <summary>
    /// Reported by controls (currently <see cref="TextBox"/>) when a reflection-
    /// based binding read or write fails. Subscribing is optional — the
    /// framework defaults to silent so a misspelled property name doesn't crash
    /// the UI, but a subscriber can log or surface the error.
    /// </summary>
    public sealed class BindingErrorEventArgs {
        /// <summary>Creates a new BindingErrorEventArgs.</summary>
        public BindingErrorEventArgs(BindingErrorDirection direction, string propertyName, Exception exception) {
            this.Direction = direction;
            this.PropertyName = propertyName;
            this.Exception = exception;
        }

        /// <summary>Whether the failure was a read (pull) or write (push).</summary>
        public BindingErrorDirection Direction { get; }
        /// <summary>Name of the bound property that failed.</summary>
        public string PropertyName { get; }
        /// <summary>The exception that caused the binding to fail.</summary>
        public Exception Exception { get; }
    }
}
