////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    /// <summary>Specifies the direction in which a line is drawn.</summary>
    public enum Direction {
        /// <summary>The line is drawn from the top-left to the bottom-right.</summary>
        TopToBottom,
        /// <summary>The line is drawn from the bottom-left to the top-right.</summary>
        BottomToTop
    }

    /// <summary>Draws a straight line.</summary>
    public class Line : Shape {
        /// <summary>Creates a line with no length.</summary>
        public Line()
            : this(0, 0) {
        }

        /// <summary>Creates a line spanning the given horizontal and vertical extents.</summary>
        public Line(int dx, int dy) {
            if (dx < 0 || dy < 0) {
                throw new ArgumentException();
            }

            this.Width = dx + 1;
            this.Height = dy + 1;
        }

        /// <summary>The direction in which the line is drawn.</summary>
        public Direction Direction {
            get => this._direction;

            set {
                this._direction = value;
                Invalidate();
            }
        }

        /// <summary>Renders the line to the drawing context.</summary>
        public override void OnRender(Media.DrawingContext dc) {
            var width = this._renderWidth;
            var height = this._renderHeight;

            if (this._direction == Direction.TopToBottom) {
                dc.DrawLine(this.Stroke, 0, 0, width - 1, height - 1);
            }
            else {
                dc.DrawLine(this.Stroke, 0, height - 1, width - 1, 0);
            }
        }

        private Direction _direction;
    }
}


