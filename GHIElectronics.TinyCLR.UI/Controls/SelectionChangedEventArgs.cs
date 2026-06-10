using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Provides data for the selection-changed event, identifying the previous and new selected indexes.</summary>
    public class SelectionChangedEventArgs : EventArgs {
        /// <summary>The index that was selected before the change.</summary>
        public readonly int PreviousSelectedIndex;
        /// <summary>The index that is selected after the change.</summary>
        public readonly int SelectedIndex;

        /// <summary>Initializes a new instance of the <see cref="SelectionChangedEventArgs"/> class.</summary>
        public SelectionChangedEventArgs(int previousIndex, int newIndex) {
            this.PreviousSelectedIndex = previousIndex;
            this.SelectedIndex = newIndex;
        }
    }
}


