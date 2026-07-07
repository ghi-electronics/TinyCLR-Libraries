using System.Collections;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>The edge of a <see cref="DockPanel"/> that a child is docked to.</summary>
    public enum Dock {
        /// <summary>Dock to the left edge.</summary>
        Left,
        /// <summary>Dock to the top edge.</summary>
        Top,
        /// <summary>Dock to the right edge.</summary>
        Right,
        /// <summary>Dock to the bottom edge.</summary>
        Bottom,
    }

    /// <summary>
    /// Panel that docks each child to one edge (Left/Top/Right/Bottom) of the space left by the previously docked
    /// children; when <see cref="LastChildFill"/> is true (the default) the last child fills whatever remains in the
    /// centre. WPF-style. Choose an edge per child with <see cref="GetDock"/> / <see cref="SetDock"/> (default Left).
    /// </summary>
    public class DockPanel : Panel {
        // Attached Dock values keyed by child element — the same static-store pattern Grid uses for Row/Column, so no
        // per-element field is added to UIElement (that would change its layout / MMP tables).
        private static readonly Hashtable DockStore = new Hashtable();

        /// <summary>Whether the last child fills the remaining central area instead of being docked (default true).</summary>
        public bool LastChildFill { get; set; } = true;

        /// <summary>Creates a new DockPanel.</summary>
        public DockPanel() {
        }

        /// <summary>Gets the edge an element is docked to (default <see cref="Dock.Left"/>).</summary>
        public static Dock GetDock(UIElement e) => DockStore.Contains(e) ? (Dock)DockStore[e] : Dock.Left;

        /// <summary>Docks an element to an edge of its parent DockPanel.</summary>
        public static void SetDock(UIElement e, Dock dock) {
            e.VerifyAccess();

            DockStore[e] = dock;

            for (var p = e.Parent; p != null; p = p.Parent) {
                if (p is DockPanel d) {
                    d.InvalidateMeasure();
                    break;
                }
            }
        }

        /// <summary>Measures children, accumulating docked extents along each axis (WPF DockPanel algorithm).</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var accumulatedW = 0; // width already consumed by Left/Right-docked children
            var accumulatedH = 0; // height already consumed by Top/Bottom-docked children
            var parentW = 0;      // widest cross-axis requirement seen
            var parentH = 0;

            var count = this.Children.Count;
            for (var i = 0; i < count; i++) {
                var child = this.Children[i];
                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                var cw = availableWidth < Media.Constants.MaxExtent ? System.Math.Max(0, availableWidth - accumulatedW) : Media.Constants.MaxExtent;
                var ch = availableHeight < Media.Constants.MaxExtent ? System.Math.Max(0, availableHeight - accumulatedH) : Media.Constants.MaxExtent;
                child.Measure(cw, ch);
                child.GetDesiredSize(out var dw, out var dh);

                switch (GetDock(child)) {
                    case Dock.Left:
                    case Dock.Right:
                        parentH = System.Math.Max(parentH, accumulatedH + dh);
                        accumulatedW += dw;
                        break;
                    default: // Top / Bottom
                        parentW = System.Math.Max(parentW, accumulatedW + dw);
                        accumulatedH += dh;
                        break;
                }
            }

            desiredWidth = System.Math.Max(parentW, accumulatedW);
            desiredHeight = System.Math.Max(parentH, accumulatedH);
        }

        /// <summary>Arranges each child against its docked edge; the last child fills the remainder when enabled.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var count = this.Children.Count;
            var fillIndex = this.LastChildFill ? count - 1 : -1;

            var left = 0;
            var top = 0;
            var right = 0;
            var bottom = 0;

            for (var i = 0; i < count; i++) {
                var child = this.Children[i];
                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                child.GetDesiredSize(out var dw, out var dh);

                var x = left;
                var y = top;
                var w = System.Math.Max(0, arrangeWidth - left - right);
                var h = System.Math.Max(0, arrangeHeight - top - bottom);

                if (i == fillIndex) {
                    child.Arrange(x, y, w, h); // fills the remaining central area, ignoring its Dock
                    continue;
                }

                switch (GetDock(child)) {
                    case Dock.Left:
                        child.Arrange(x, y, System.Math.Min(dw, w), h);
                        left += dw;
                        break;
                    case Dock.Right:
                        child.Arrange(System.Math.Max(0, arrangeWidth - right - dw), y, System.Math.Min(dw, w), h);
                        right += dw;
                        break;
                    case Dock.Top:
                        child.Arrange(x, y, w, System.Math.Min(dh, h));
                        top += dh;
                        break;
                    default: // Bottom
                        child.Arrange(x, System.Math.Max(0, arrangeHeight - bottom - dh), w, System.Math.Min(dh, h));
                        bottom += dh;
                        break;
                }
            }
        }
    }
}
