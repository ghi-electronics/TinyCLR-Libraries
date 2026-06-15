////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A panel that stacks its children in a single row or column.</summary>
    public class StackPanel : Panel {
        /// <summary>Initializes a new stack panel that stacks children vertically.</summary>
        public StackPanel()
            : this(Orientation.Vertical) {
        }

        /// <summary>Initializes a new stack panel with the given stacking orientation.</summary>
        public StackPanel(Orientation orientation) => this.Orientation = orientation;

        /// <summary>The direction in which children are stacked.</summary>
        public Orientation Orientation {
            get => this._orientation;

            set {
                VerifyAccess();

                this._orientation = value;
                InvalidateMeasure();
            }
        }

        /// <summary>Measures the panel as the sum of child sizes along the stacking axis.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = 0;
            desiredHeight = 0;

            var fHorizontal = (this.Orientation == Orientation.Horizontal);

            //  Iterate through children.
            //
            var nChildren = this.Children.Count;
            for (var i = 0; i < nChildren; i++) {
                var child = this.Children[i];

                if (child.Visibility != Visibility.Collapsed) {
                    // Measure the child.
                    // - according to Avalon specs, the stack panel should not constrain
                    //   a child's measure in the direction of the stack
                    //
                    if (fHorizontal) {
                        child.Measure(Media.Constants.MaxExtent, availableHeight);
                    }
                    else {
                        child.Measure(availableWidth, Media.Constants.MaxExtent);
                    }

                    child.GetDesiredSize(out var childDesiredWidth, out var childDesiredHeight);

                    if (fHorizontal) {
                        desiredWidth += childDesiredWidth;
                        desiredHeight = System.Math.Max(desiredHeight, childDesiredHeight);
                    }
                    else {
                        desiredWidth = System.Math.Max(desiredWidth, childDesiredWidth);
                        desiredHeight += childDesiredHeight;
                    }
                }
            }
        }

        /// <summary>Positions children one after another along the stacking axis.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var fHorizontal = (this.Orientation == Orientation.Horizontal);
            var previousChildSize = 0;
            var childPosition = 0;

            // Arrange and Position Children.
            //
            var nChildren = this.Children.Count;
            for (var i = 0; i < nChildren; ++i) {
                var child = this.Children[i];
                if (child.Visibility != Visibility.Collapsed) {
                    childPosition += previousChildSize;
                    child.GetDesiredSize(out var childDesiredWidth, out var childDesiredHeight);

                    if (fHorizontal) {
                        previousChildSize = childDesiredWidth;
                        child.Arrange(childPosition, 0, previousChildSize, System.Math.Max(arrangeHeight, childDesiredHeight));
                    }
                    else {
                        previousChildSize = childDesiredHeight;
                        child.Arrange(0, childPosition, System.Math.Max(arrangeWidth, childDesiredWidth), previousChildSize);
                    }
                }
            }
        }

        private Orientation _orientation;
    }
}


