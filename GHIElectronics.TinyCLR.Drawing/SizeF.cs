using System.Runtime.CompilerServices;

namespace System.Drawing {
    /// <summary>A floating-point width × height size.</summary>
    public struct SizeF {
        public static readonly SizeF Empty = new SizeF();

        public SizeF(SizeF size) {
            this.Width = size.Width;
            this.Height = size.Height;
        }

        public SizeF(float width, float height) {
            this.Width = width;
            this.Height = height;
        }

        public bool IsEmpty => this.Width == 0 && this.Height == 0;

        public float Width { get; set; }
        public float Height { get; set; }
    }
}
