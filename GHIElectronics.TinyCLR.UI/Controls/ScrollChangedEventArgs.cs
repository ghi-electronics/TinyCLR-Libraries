using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Provides data for the scroll-changed event, describing the new offsets and how far they moved.</summary>
    public class ScrollChangedEventArgs : EventArgs {
        /// <summary>The amount the horizontal offset changed since the last event.</summary>
        public readonly int HorizontalChange;
        /// <summary>The current horizontal scroll offset.</summary>
        public readonly int HorizontalOffset;

        /// <summary>The amount the vertical offset changed since the last event.</summary>
        public readonly int VerticalChange;
        /// <summary>The current vertical scroll offset.</summary>
        public readonly int VerticalOffset;

        /// <summary>Initializes a new instance of the <see cref="ScrollChangedEventArgs"/> class.</summary>
        public ScrollChangedEventArgs(int offsetX, int offsetY, int offsetChangeX, int offsetChangeY) {
            this.HorizontalOffset = offsetX;
            this.HorizontalChange = offsetChangeX;

            this.VerticalOffset = offsetY;
            this.VerticalChange = offsetChangeY;
        }
    }
}


