using System.Drawing.Drawing2D;

namespace System.Drawing {
    public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
        public float Width { get; set; }
        public Color Color { get; set; }
        public PenType PenType { get; }

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

        public Pen(Color color) : this(color, 1.0f) { }
        public Pen(Brush brush) : this(brush, 1.0f) { }

        public Pen(Color color, float width) {
            this.Width = width;
            this.Color = color;
            this.PenType = PenType.SolidColor;
        }

        public Pen(Brush brush, float width) {
            this.Width = width;
            this.Brush = brush;
            this.PenType = PenType.SolidColor;
        }

        public void Dispose() { }

        public object Clone() => new Pen(this.Color, this.Width);
    }

    namespace Drawing2D {
        public enum PenType {
            SolidColor
        }
    }
}
