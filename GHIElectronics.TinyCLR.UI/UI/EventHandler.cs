using System;

// need move this into mscorlib, also get the real implementation.
namespace GHIElectronics.TinyCLR.UI {
    public delegate void EventHandler(object sender, EventArgs e);

    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    public class CancelEventArgs : EventArgs {
        public bool Cancel;
    }
}


