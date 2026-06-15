using System.Runtime.CompilerServices;

namespace System.Drawing {
    /// <summary>A floating-point width × height size.</summary>
    public struct SizeF {
        /// <summary>Represents a size with zero width and height.</summary>
        public static readonly SizeF Empty = new SizeF();

        /// <summary>Initializes a new size as a copy of an existing size.</summary>
        public SizeF(SizeF size) {
            this.Width = size.Width;
            this.Height = size.Height;
        }

        /// <summary>Initializes a new size with the given width and height.</summary>
        public SizeF(float width, float height) {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>Gets a value indicating whether both width and height are zero.</summary>
        public bool IsEmpty => this.Width == 0 && this.Height == 0;

        /// <summary>Gets or sets the width.</summary>
        public float Width { get; set; }
        /// <summary>Gets or sets the height.</summary>
        public float Height { get; set; }
    }
}
