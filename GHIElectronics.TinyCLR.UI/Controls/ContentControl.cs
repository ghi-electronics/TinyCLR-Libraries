////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Base class for controls that host a single child element.</summary>
    public abstract class ContentControl : Control {
        /// <summary>Horizontal placement of <see cref="Child"/> within this control's content area.
        /// <see cref="HorizontalAlignment.Stretch"/> (the default) gives the child the full width and lets the
        /// child's own <see cref="UIElement.HorizontalAlignment"/> take effect (back-compat). Derived controls
        /// such as <see cref="Button"/> default this to <see cref="HorizontalAlignment.Center"/>.</summary>
        public HorizontalAlignment HorizontalContentAlignment { get; set; } = HorizontalAlignment.Stretch;

        /// <summary>Vertical placement of <see cref="Child"/> within this control's content area.
        /// See <see cref="HorizontalContentAlignment"/> for the Stretch/back-compat semantics.</summary>
        public VerticalAlignment VerticalContentAlignment { get; set; } = VerticalAlignment.Stretch;

        /// <summary>The single child element hosted by this control.</summary>
        public UIElement Child {
            get {
                if (this.LogicalChildren.Count > 0) {
                    return this._logicalChildren[0];
                }
                else {
                    return null;
                }
            }

            set {
                VerifyAccess();

                this.LogicalChildren.Clear();
                this.LogicalChildren.Add(value);
            }
        }

        /// <summary>Measures the child element.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var child = this.Child;
            if (child != null) {
                child.Measure(availableWidth, availableHeight);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
            }
            else {
                desiredWidth = desiredHeight = 0;
            }
        }
    }
}


