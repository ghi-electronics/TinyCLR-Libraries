////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    /// <summary>Draws a polygon defined by a set of points.</summary>
    public class Polygon : Shape {

        /// <summary>Creates an empty polygon.</summary>
        public Polygon() {
        }

        /// <summary>Creates a polygon from the given points.</summary>
        public Polygon(int[] pts) => this.Points = pts;

        /// <summary>The points that define the polygon as alternating x and y coordinates.</summary>
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

        /// <summary>Renders the polygon to the drawing context.</summary>
        public override void OnRender(Media.DrawingContext dc) {
            if (this._pts != null) {
                dc.DrawPolygon(this.Fill, this.Stroke, this._pts);
            }
        }

        private int[] _pts;
    }
}


