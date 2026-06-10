using System.Drawing.Drawing2D;

namespace System.Drawing {
    /// <summary>Defines an object used to draw lines and outlines — color, width, and brush.</summary>
    public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
        /// <summary>Gets or sets the width of this pen in pixels.</summary>
        public float Width { get; set; }
        /// <summary>Gets or sets the color of this pen.</summary>
        public Color Color { get; set; }
        /// <summary>Gets the style of this pen.</summary>
        public PenType PenType { get; }

        /// <summary>Gets or sets the brush used by this pen; only a solid brush is supported.</summary>
        public Brush Brush {
            get => new SolidBrush(this.Color);
            set {
                if (value is SolidBrush brush) {
                    this.Color = brush.Color;
                }
                else {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>Initializes a new pen of width 1 with the specified color.</summary>
        public Pen(Color color) : this(color, 1.0f) { }
        /// <summary>Initializes a new pen of width 1 with the specified brush.</summary>
        public Pen(Brush brush) : this(brush, 1.0f) { }

        /// <summary>Initializes a new pen with the specified color and width.</summary>
        public Pen(Color color, float width) {
            this.Width = width;
            this.Color = color;
            this.PenType = PenType.SolidColor;
        }

        /// <summary>Initializes a new pen with the specified brush and width.</summary>
        public Pen(Brush brush, float width) {
            this.Width = width;
            this.Brush = brush;
            this.PenType = PenType.SolidColor;
        }

        /// <summary>Releases the resources used by this pen.</summary>
        public void Dispose() { }

        /// <summary>Creates an exact copy of this pen.</summary>
        public object Clone() => new Pen(this.Color, this.Width);
    }

    namespace Drawing2D {
        /// <summary>Specifies the type of fill a pen uses.</summary>
        public enum PenType {
            /// <summary>The pen draws using a single solid color.</summary>
            SolidColor
        }
    }
}
