namespace System.Drawing {
    /// <summary>Abstract base for objects that fill graphics shapes (rectangles, ellipses, paths).</summary>
    public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {
        /// <summary>Creates an exact copy of this brush.</summary>
        public abstract object Clone();

        /// <summary>Releases the resources used by this brush.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources used by this brush.</summary>
        protected virtual void Dispose(bool disposing) { }

        ~Brush() => this.Dispose(false);
    }

    /// <summary>A brush that fills with a single solid <see cref="Color"/>.</summary>
    public class SolidBrush : Brush {
        /// <summary>Initializes a new solid brush with the specified fill color.</summary>
        public SolidBrush(Color color) => this.Color = color;

        /// <summary>Gets or sets the color used to fill shapes.</summary>
        public Color Color { get; set; }

        /// <summary>Creates an exact copy of this solid brush.</summary>
        public override object Clone() => new SolidBrush(this.Color);
    }
}
