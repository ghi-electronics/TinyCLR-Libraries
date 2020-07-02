using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Glide.Ext
{
    public class Bitmaps
    {
        public const ushort OpacityOpaque = 0xff;
        public const ushort OpacityTransparent = 0;
        public const int SRCCOPY = 1;
        public const int PATINVERT = 2;
        public const int DSTINVERT = 3;
        public const int BLACKNESS = 4;
        public const int WHITENESS = 5;
        public const int DSTGRAY = 6;
        public const int DSTLTGRAY = 7;
        public const int DSTDKGRAY = 8;
        public const int SINGLEPIXEL = 9;
        public const int RANDOM = 10;
        public const uint DT_None = 0;
        public const uint DT_WordWrap = 1;
        public const uint DT_TruncateAtBottom = 4;
        [Obsolete("Use DT_TrimmingWordEllipsis or DT_TrimmingCharacterEllipsis to specify the type of trimming needed.", false)]
        public const uint DT_Ellipsis = 8;
        public const uint DT_IgnoreHeight = 0x10;
        public const uint DT_AlignmentLeft = 0;
        public const uint DT_AlignmentCenter = 2;
        public const uint DT_AlignmentRight = 0x20;
        public const uint DT_AlignmentMask = 0x22;
        public const uint DT_TrimmingNone = 0;
        public const uint DT_TrimmingWordEllipsis = 8;
        public const uint DT_TrimmingCharacterEllipsis = 0x40;
        public const uint DT_TrimmingMask = 0x48;
        public enum BitmapImageType : byte
        {
            TinyCLRBitmap = 0,
            Gif = 1,
            Jpeg = 2,
            Bmp = 3
        }
    }
}
