namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Panel : UIElement {
        public UIElementCollection Children => this.LogicalChildren;

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


