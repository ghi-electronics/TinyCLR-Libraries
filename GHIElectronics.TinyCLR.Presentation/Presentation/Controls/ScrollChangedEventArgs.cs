using System;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class ScrollChangedEventArgs : EventArgs {
        public readonly int HorizontalChange;
        public readonly int HorizontalOffset;

        public readonly int VerticalChange;
        public readonly int VerticalOffset;

        public ScrollChangedEventArgs(int offsetX, int offsetY, int offsetChangeX, int offsetChangeY) {
            this.HorizontalOffset = offsetX;
            this.HorizontalChange = offsetChangeX;

            this.VerticalOffset = offsetY;
            this.VerticalChange = offsetChangeY;
        }
    }
}


