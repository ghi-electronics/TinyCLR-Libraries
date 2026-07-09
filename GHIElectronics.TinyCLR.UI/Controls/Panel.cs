namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Base class for controls that arrange a collection of child elements.</summary>
    public class Panel : UIElement {
        private Media.Brush _background;

        /// <summary>The collection of child elements contained in this panel.</summary>
        public UIElementCollection Children => this.LogicalChildren;

        /// <summary>
        /// The brush used to paint the panel's background, matching WPF's <c>Panel.Background</c>
        /// (Canvas/Grid/StackPanel/DockPanel). Null (the default) leaves the panel transparent, so
        /// whatever is behind it shows through.
        /// </summary>
        public Media.Brush Background {
            get {
                VerifyAccess();

                return this._background;
            }

            set {
                VerifyAccess();

                this._background = value;
                Invalidate();
            }
        }

        /// <summary>Corner radius in pixels for the panel background (0 = square, the default). Cheap — only
        /// the corner pixels rasterize.</summary>
        public int CornerRadius {
            get => this._cornerRadius;
            set { this._cornerRadius = value < 0 ? 0 : value; Invalidate(); }
        }

        /// <summary>Paints the background across the panel's render area (WPF Panel parity).</summary>
        public override void OnRender(Media.DrawingContext dc) {
            if (this._background != null) {
                if (this._cornerRadius > 0)
                    dc.FillRoundedRectangle(this._background, 0, 0, this._renderWidth, this._renderHeight, this._cornerRadius);
                else
                    dc.DrawRectangle(this._background, null, 0, 0, this._renderWidth, this._renderHeight);
            }
        }

        private int _cornerRadius;

        /// <summary>Measures the panel as the bounding size of all its children.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = desiredHeight = 0;
            var children = this._logicalChildren;
            if (children != null) {
                for (var i = 0; i < children.Count; i++) {
                    var child = children[i];
                    child.Measure(availableWidth, availableHeight);
                    child.GetDesiredSize(out var childDesiredWidth, out var childDesiredHeight);
                    desiredWidth = System.Math.Max(desiredWidth, childDesiredWidth);
                    desiredHeight = System.Math.Max(desiredHeight, childDesiredHeight);
                }
            }
        }
    }
}
