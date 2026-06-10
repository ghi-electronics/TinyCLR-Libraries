namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Base class for controls that arrange a collection of child elements.</summary>
    public class Panel : UIElement {
        /// <summary>The collection of child elements contained in this panel.</summary>
        public UIElementCollection Children => this.LogicalChildren;

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


