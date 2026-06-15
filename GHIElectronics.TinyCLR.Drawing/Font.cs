using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GHIElectronics.TinyCLR.UI")]

namespace System.Drawing {
    /// <summary>Unit of measurement for text and graphics sizes.</summary>
    public enum GraphicsUnit {
        /// <summary>The world coordinate system unit as the unit of measure.</summary>
        World = 0,
        /// <summary>The unit of measure of the display device.</summary>
        Display = 1,
        /// <summary>A device pixel as the unit of measure.</summary>
        Pixel = 2,
        /// <summary>A printer's point (1/72 inch) as the unit of measure.</summary>
        Point = 3,
        /// <summary>An inch as the unit of measure.</summary>
        Inch = 4,
        /// <summary>A document unit (1/300 inch) as the unit of measure.</summary>
        Document = 5,
        /// <summary>A millimeter as the unit of measure.</summary>
        Millimeter = 6
    }

    /// <summary>
    /// A bitmap font loaded from a TinyCLR resource. Use the resource designer to
    /// embed .tinyfnt files, then construct via <c>Resources.GetFont</c>.
    /// </summary>
    //The name and namespace of this must match the definition in c_TypeIndexLookup in TypeSystem.cpp
    public sealed class Font : MarshalByRefObject, ICloneable, IDisposable {
#pragma warning disable CS0169 // The field is never used
        IntPtr implPtr;
        IntPtr dataPtr;
#pragma warning restore CS0169 // The field is never used

        // Must keep in sync with CLR_GFX_Font::c_DefaultKerning
        private const int DefaultKerning = 1024;

        private Font() { }

        /// <summary>Initializes a new font from raw .tinyfnt font data.</summary>
        public Font(byte[] data) => new Font(data, 0, data.Length);

        /// <summary>Initializes a new font from a range of raw .tinyfnt font data.</summary>
        public Font(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            this.CreateInstantFromBuffer(data, offset, count);
        }

        /// <summary>Initializes a new font from a family name and em size (only the built-in GHIMono8x5 family is supported).</summary>
        public Font(string familyName, float emSize) {
            var sz = (int)emSize;

            this.IsGHIMono8x5 = familyName == "GHIMono8x5" && (sz % 8) == 0 ? true : throw new NotSupportedException();
            this.Size = sz;
        }

        ~Font() => this.Dispose();

        internal int Size { get; }
        internal bool IsGHIMono8x5 { get; }

        /// <summary>Creates a copy of this font.</summary>
        public object Clone() => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int CharWidth(char c);

        /// <summary>Gets the unit of measure for this font, which is always pixels.</summary>
        public GraphicsUnit Unit => GraphicsUnit.Pixel;

        /// <summary>Gets the height of this font in pixels.</summary>
        public extern int Height { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        internal extern int AverageWidth { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        internal extern int MaxWidth { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        internal extern int Ascent { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        internal extern int Descent { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        internal extern int InternalLeading { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        internal extern int ExternalLeading { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ComputeExtent(string text, out int width, out int height, int kerning);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int xRelStart, int yRelStart, int availableWidth, int availableHeight, uint dtFlags);

        /// <summary>Computes the pixel width and height needed to render the given text.</summary>
        public void ComputeExtent(string text, out int width, out int height) => this.ComputeExtent(text, out width, out height, DefaultKerning);
        /// <summary>Computes the pixel width and height needed to render the given text with word wrapping.</summary>
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, 65536, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);
        /// <summary>Computes the pixel width and height needed to render the given text wrapped within the available width.</summary>
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int availableWidth) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, availableWidth, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void CreateInstantFromResources(uint buffer, uint size, uint assembly);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void CreateInstantFromBuffer(byte[] data, int offset, int size);

        /// <summary>Releases the resources used by this font.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Dispose();
    }
}


