using System;

// need move this into mscorlib, also get the real implementation.
namespace GHIElectronics.TinyCLR.UI {
    /// <summary>Represents a method that handles a general event with no event data.</summary>
    public delegate void EventHandler(object sender, EventArgs e);

    /// <summary>Represents a method that handles a cancelable event.</summary>
    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    /// <summary>Provides data for a cancelable event.</summary>
    public class CancelEventArgs : EventArgs {
        /// <summary>Set to true to cancel the operation; otherwise false.</summary>
        public bool Cancel;
    }
}


