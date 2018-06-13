////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    public class Polygon : Shape {

        public Polygon() {
        }

        public Polygon(int[] pts) => this.Points = pts;

        public int[] Points {
            get => this._pts;

            set {
                if (value == null || value.Length == 0) {
                    throw new ArgumentException();
                }

                this._pts = value;

                InvalidateMeasure();
            }
        }

        public override void OnRender(Media.DrawingContext dc) {
            if (this._pts != null) {
                dc.DrawPolygon(this.Fill, this.Stroke, this._pts);
            }
        }

        private int[] _pts;
    }
}


