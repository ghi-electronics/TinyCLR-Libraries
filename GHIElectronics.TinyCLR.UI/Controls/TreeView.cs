using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A node in a <see cref="TreeView"/>: a header plus optional child nodes.</summary>
    public sealed class TreeNode {
        /// <summary>The text shown for this node.</summary>
        public string Header;
        /// <summary>Whether the node's children are shown.</summary>
        public bool IsExpanded = true;
        /// <summary>The child nodes.</summary>
        public readonly ArrayList Children = new ArrayList();

        /// <summary>Creates a node with the given header.</summary>
        public TreeNode(string header) => this.Header = header ?? string.Empty;

        /// <summary>Adds a child node.</summary>
        public TreeNode Add(TreeNode child) {
            this.Children.Add(child);
            return child;
        }
    }

    /// <summary>A hierarchical list of <see cref="TreeNode"/>s with expand/collapse. Tapping a node's chevron toggles
    /// its children; tapping the row selects it.</summary>
    public class TreeView : Control {
        private readonly ArrayList _roots = new ArrayList(); // TreeNode
        private readonly ArrayList _visible = new ArrayList(); // (TreeNode node, int depth) via VisibleRow
        private int _rowHeight = 22;
        private int _indent = 14;
        private TreeNode _selected;

        /// <summary>Row height in pixels.</summary>
        public int RowHeight {
            get => this._rowHeight;
            set { if (value > 4) { this._rowHeight = value; this.InvalidateMeasure(); } }
        }

        /// <summary>Colour of node text.</summary>
        public Color HeaderColor { get; set; } = Colors.Black;

        /// <summary>Fill behind the selected row.</summary>
        public Color SelectionColor { get; set; } = Color.FromRgb(0xCC, 0xE4, 0xF7);

        /// <summary>Adds a top-level node.</summary>
        public void AddNode(TreeNode node) {
            if (node != null) {
                this._roots.Add(node);
                this.InvalidateMeasure();
            }
        }

        private struct VisibleRow {
            public TreeNode Node;
            public int Depth;
        }

        // Rebuilds the flattened list of currently-visible rows (respecting each node's IsExpanded).
        private void RebuildVisible() {
            this._visible.Clear();
            for (var i = 0; i < this._roots.Count; i++) {
                this.AddVisible((TreeNode)this._roots[i], 0);
            }
        }

        private void AddVisible(TreeNode node, int depth) {
            this._visible.Add(new VisibleRow { Node = node, Depth = depth });
            if (node.IsExpanded) {
                for (var i = 0; i < node.Children.Count; i++) {
                    this.AddVisible((TreeNode)node.Children[i], depth + 1);
                }
            }
        }

        /// <summary>Reports the full height of all visible rows.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            this.RebuildVisible();
            desiredWidth = availableWidth < Media.Constants.MaxExtent ? availableWidth : 0;
            desiredHeight = this._visible.Count * this._rowHeight;
        }

        /// <summary>Toggles a node's expansion (chevron zone) or selects the tapped row.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            base.OnTouchUp(e); // raise the public TouchUp event for user/designer handlers

            e.GetPosition(this, 0, out var x, out var y);
            var row = y / this._rowHeight;
            if (row < 0 || row >= this._visible.Count) {
                return;
            }

            var vr = (VisibleRow)this._visible[row];
            var chevronX = vr.Depth * this._indent + 2;
            if (vr.Node.Children.Count > 0 && x >= chevronX && x < chevronX + this._indent) {
                vr.Node.IsExpanded = !vr.Node.IsExpanded;
                this.InvalidateMeasure();
                this.Invalidate();
            }
            else {
                this._selected = vr.Node;
                this.Invalidate();
            }

            e.Handled = true;
        }

        /// <summary>Draws each visible node with indentation, a chevron for parents, and the header text.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (this._visible.Count == 0) {
                this.RebuildVisible();
            }

            var w = this._renderWidth;
            var pen = new Media.Pen(this.HeaderColor, 1);

            for (var i = 0; i < this._visible.Count; i++) {
                var vr = (VisibleRow)this._visible[i];
                var y = i * this._rowHeight;
                var indentX = vr.Depth * this._indent;

                if (vr.Node == this._selected) {
                    dc.DrawRectangle(new SolidColorBrush(this.SelectionColor), null, 0, y, w, this._rowHeight);
                }

                if (vr.Node.Children.Count > 0) {
                    var cx = indentX + 5;
                    var cy = y + this._rowHeight / 2;
                    if (vr.Node.IsExpanded) {
                        dc.DrawLine(pen, cx - 3, cy - 2, cx, cy + 2);
                        dc.DrawLine(pen, cx, cy + 2, cx + 3, cy - 2);
                    }
                    else {
                        dc.DrawLine(pen, cx - 2, cy - 3, cx + 2, cy);
                        dc.DrawLine(pen, cx + 2, cy, cx - 2, cy + 3);
                    }
                }

                if (this.Font != null) {
                    var txt = vr.Node.Header;
                    var tx = indentX + this._indent;
                    dc.DrawText(ref txt, this.Font, this.HeaderColor, tx, y + (this._rowHeight - this.Font.Height) / 2, w - tx, this.Font.Height, TextAlignment.Left, TextTrimming.CharacterEllipsis);
                }
            }
        }
    }
}
