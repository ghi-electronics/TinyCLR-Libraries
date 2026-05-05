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

        public object Clone() => throw new NotImplementedException();

        private int CharWidth(char c) => throw new System.NotSupportedException("TODO - Not supported");
        public GraphicsUnit Unit => GraphicsUnit.Pixel;

        public int Height => throw new System.NotSupportedException("TODO - Not supported");
        internal int AverageWidth => throw new System.NotSupportedException("TODO - Not supported");
        internal int MaxWidth => throw new System.NotSupportedException("TODO - Not supported");
        internal int Ascent => throw new System.NotSupportedException("TODO - Not supported");
        internal int Descent => throw new System.NotSupportedException("TODO - Not supported");
        internal int InternalLeading => throw new System.NotSupportedException("TODO - Not supported");
        internal int ExternalLeading => throw new System.NotSupportedException("TODO - Not supported");
        private void ComputeExtent(string text, out int width, out int height, int kerning) => throw new System.NotSupportedException("TODO - Not supported");
        internal void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int xRelStart, int yRelStart, int availableWidth, int availableHeight, uint dtFlags) => throw new System.NotSupportedException("TODO - Not supported");
        public void ComputeExtent(string text, out int width, out int height) => this.ComputeExtent(text, out width, out height, DefaultKerning);
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, 65536, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);
        public void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int availableWidth) => this.ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, availableWidth, 0, (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight | (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap);

        private void CreateInstantFromResources(uint buffer, uint size, uint assembly) => throw new System.NotSupportedException("TODO - Not supported");
        private void CreateInstantFromBuffer(byte[] data, int offset, int size) => throw new System.NotSupportedException("TODO - Not supported");
        public void Dispose() => throw new System.NotSupportedException("TODO - Not supported");
    }
}


