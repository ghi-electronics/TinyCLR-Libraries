using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public enum BindingErrorDirection {
        /// <summary>Source → control. The control couldn't read the bound property.</summary>
        Pull,
        /// <summary>Control → source. The control couldn't write the bound property.</summary>
        Push,
    }

    public delegate void BindingErrorEventHandler(object sender, BindingErrorEventArgs e);

    /// <summary>
    /// Reported by controls (currently <see cref="TextBox"/>) when a reflection-
    /// based binding read or write fails. Subscribing is optional — the
    /// framework defaults to silent so a misspelled property name doesn't crash
    /// the UI, but a subscriber can log or surface the error.
    /// </summary>
    public sealed class BindingErrorEventArgs {
        public BindingErrorEventArgs(BindingErrorDirection direction, string propertyName, Exception exception) {
            this.Direction = direction;
            this.PropertyName = propertyName;
            this.Exception = exception;
        }

        public BindingErrorDirection Direction { get; }
        public string PropertyName { get; }
        public Exception Exception { get; }
    }
}
