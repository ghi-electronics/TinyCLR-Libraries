namespace System.Drawing {
    /// <summary>Abstract base for objects that fill graphics shapes (rectangles, ellipses, paths).</summary>
    public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {
        public abstract object Clone();

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~Brush() => this.Dispose(false);
    }

    /// <summary>A brush that fills with a single solid <see cref="Color"/>.</summary>
    public class SolidBrush : Brush {
        public SolidBrush(Color color) => this.Color = color;

        public Color Color { get; set; }

        public override object Clone() => new SolidBrush(this.Color);
    }
}
