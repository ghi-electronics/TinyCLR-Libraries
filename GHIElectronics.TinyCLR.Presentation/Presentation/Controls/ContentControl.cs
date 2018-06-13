////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



namespace GHIElectronics.TinyCLR.UI.Controls {
    public abstract class ContentControl : Control {
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


