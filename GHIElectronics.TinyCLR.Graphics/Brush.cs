namespace System.Drawing {
    public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {
        public abstract object Clone();

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~Brush() => this.Dispose(false);
    }

    public class SolidBrush : Brush {
        public SolidBrush(Color color) => this.Color = color;

        public Color Color { get; set; }

        public override object Clone() => new SolidBrush(this.Color);
    }
}
