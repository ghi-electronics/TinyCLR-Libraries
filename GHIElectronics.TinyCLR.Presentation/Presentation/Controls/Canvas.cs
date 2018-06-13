////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Canvas : Panel {
        public Canvas() {
        }

        private const int Edge_Left = 0x1;
        private const int Edge_Right = 0x2;
        private const int Edge_Top = 0x4;
        private const int Edge_Bottom = 0x8;
        private const int Edge_LeftRight = Edge_Left | Edge_Right;
        private const int Edge_TopBottom = Edge_Top | Edge_Bottom;

        private static int GetAnchorValue(UIElement e, int edge) {
            var anchorInfo = e._anchorInfo;
            if (anchorInfo != null) {
                if ((anchorInfo._status & edge) != 0) {
                    return ((edge & Edge_LeftRight) != 0) ? anchorInfo._first : anchorInfo._second;
                }
            }

            return 0;
        }

        private static void SetAnchorValue(UIElement e, int edge, int val) {
            e.VerifyAccess();

            var anchorInfo = e._anchorInfo;
            if (anchorInfo == null) {
                anchorInfo = new UIElement.Pair();
                e._anchorInfo = anchorInfo;
            }

            if ((edge & Edge_LeftRight) != 0) {
                anchorInfo._first = val;
                anchorInfo._status &= ~Edge_LeftRight;
            }
            else {
                anchorInfo._second = val;
                anchorInfo._status &= ~Edge_TopBottom;
            }

            anchorInfo._status |= edge;

            if (e.Parent != null) {
                e.Parent.InvalidateArrange();
            }
        }

        public static int GetBottom(UIElement e) => GetAnchorValue(e, Edge_Bottom);

        public static void SetBottom(UIElement e, int bottom) => SetAnchorValue(e, Edge_Bottom, bottom);

        public static int GetLeft(UIElement e) => GetAnchorValue(e, Edge_Left);

        public static void SetLeft(UIElement e, int left) => SetAnchorValue(e, Edge_Left, left);

        public static int GetRight(UIElement e) => GetAnchorValue(e, Edge_Right);

        public static void SetRight(UIElement e, int right) => SetAnchorValue(e, Edge_Right, right);

        public static int GetTop(UIElement e) => GetAnchorValue(e, Edge_Top);

        public static void SetTop(UIElement e, int top) => SetAnchorValue(e, Edge_Top, top);

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            VerifyAccess();

            var children = this._logicalChildren;
            if (children != null) {
                var count = children.Count;
                for (var i = 0; i < count; i++) {
                    var child = children[i];
                    child.GetDesiredSize(out var childWidth, out var childHeight);

                    var anchorInfo = child._anchorInfo;
                    if (anchorInfo != null) {
                        var status = anchorInfo._status;
                        child.Arrange(
                            ((status & Edge_Right) != 0) ? arrangeWidth - childWidth - anchorInfo._first : anchorInfo._first,
                            ((status & Edge_Bottom) != 0) ? arrangeHeight - childHeight - anchorInfo._second : anchorInfo._second,
                            childWidth,
                            childHeight);
                    }
                    else {
                        child.Arrange(0, 0, childWidth, childHeight);
                    }
                }
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var children = this._logicalChildren;
            if (children != null) {
                for (var i = 0; i < children.Count; i++) {
                    children[i].Measure(Media.Constants.MaxExtent, Media.Constants.MaxExtent);
                }
            }

            desiredWidth = 0;
            desiredHeight = 0;
        }

    }
}


