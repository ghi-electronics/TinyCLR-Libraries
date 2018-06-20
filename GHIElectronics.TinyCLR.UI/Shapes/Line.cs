////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    public enum Direction {
        TopToBottom,
        BottomToTop
    }

    public class Line : Shape {
        public Line()
            : this(0, 0) {
        }

        public Line(int dx, int dy) {
            if (dx < 0 || dy < 0) {
                throw new ArgumentException();
            }

            this.Width = dx + 1;
            this.Height = dy + 1;
        }

        public Direction Direction {
            get => this._direction;

            set {
                this._direction = value;
                Invalidate();
            }
        }

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


