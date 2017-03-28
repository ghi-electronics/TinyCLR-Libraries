using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Drawing {
    public sealed class Font : MarshalByRefObject, ICloneable, IDisposable {
        private object m_font;

        // Must keep in sync with CLR_GFX_Font::c_DefaultKerning
        private const int DefaultKerning = 1024;

        private Font() { }

        public object Clone() => throw new NotImplementedException();
        public void Dispose() { }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int CharWidth(char c);

        private extern int Height { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern int AverageWidth { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern int MaxWidth { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        private extern int Ascent { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern int Descent { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        private extern int InternalLeading { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern int ExternalLeading { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ComputeExtent(string text, out int width, out int height, int kerning);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int xRelStart, int yRelStart, int availableWidth, int availableHeight, uint dtFlags);

        private void ComputeExtent(string text, out int width, out int height) => ComputeExtent(text, out width, out height, DefaultKerning);
        private void ComputeTextInRect(string text, out int renderWidth, out int renderHeight) => ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, 65536, 0, Internal.Bitmap.DT_IgnoreHeight | Internal.Bitmap.DT_WordWrap);
        private void ComputeTextInRect(string text, out int renderWidth, out int renderHeight, int availableWidth) => ComputeTextInRect(text, out renderWidth, out renderHeight, 0, 0, availableWidth, 0, Internal.Bitmap.DT_IgnoreHeight | Internal.Bitmap.DT_WordWrap);
    }
}


