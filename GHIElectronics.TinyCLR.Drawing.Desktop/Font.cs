using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GHIElectronics.TinyCLR.UI")]

namespace System.Drawing {
    public enum GraphicsUnit {
        World = 0,
        Display = 1,
        Pixel = 2,
        Point = 3,
        Inch = 4,
        Document = 5,
        Millimeter = 6
    }

    //The name and namespace of this must match the definition in c_TypeIndexLookup in TypeSystem.cpp
    public sealed class Font : MarshalByRefObject, ICloneable, IDisposable {
#pragma warning disable CS0169 // The field is never used
        IntPtr implPtr;
        IntPtr dataPtr;
#pragma warning restore CS0169 // The field is never used

        // Must keep in sync with CLR_GFX_Font::c_DefaultKerning
        private const int DefaultKerning = 1024;

        private Font() { }

        public Font(byte[] data) => new Font(data, 0, data.Length);

        public Font(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            this.CreateInstantFromBuffer(data, offset, count);
        }

        public Font(string familyName, float emSize) {
            var sz = (int)emSize;

            this.IsGHIMono8x5 = familyName == "GHIMono8x5" && (sz % 8) == 0 ? true : throw new NotSupportedException();
            this.Size = sz;
        }

        ~Font() => this.Dispose();

        internal int Size { get; }
        internal bool IsGHIMono8x5 { get; }

        // Safe no-op on Desktop: there's no real glyph data, but UI controls
        // compute layout from these metrics and would divide-by-zero on Height=0.
        // Defaults approximate a typical small bitmap font (~12px high, ~8px wide).
        // Visual output is intentionally absent on Desktop; only layout math runs.
        public object Clone() => new Font();

        private int CharWidth(char c) => 8;
        public GraphicsUnit Unit => GraphicsUnit.Pixel;

        public int Height => 12;
        internal int AverageWidth => 8;
        internal int MaxWidth => 8;
        internal int Ascent => 9;
        internal int Descent => 3;
        internal int InternalLeading => 0;
        internal int ExternalLeading => 0;
        private void ComputeExtent(string text, out int width, out int height, int kerning) {
            width = (text?.Length ?? 0) * 8;
            height = 12;
        }
        internal void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int xRelStart, int yRelStart, int availableWidth, int availableHeight, uint dtFlags) {
            renderWidth = (text?.Length ?? 0) * 8;
            renderHeight = 12;
        }
        public void ComputeExtent(string text, out int width, out int height) => this.ComputeExtent(text, out width, out height, DefaultKerning);
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, 65536, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int availableWidth) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, availableWidth, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);

        private void CreateInstantFromResources(uint buffer, uint size, uint assembly) { }
        private void CreateInstantFromBuffer(byte[] data, int offset, int size) { }
        public void Dispose() { }
    }
}


