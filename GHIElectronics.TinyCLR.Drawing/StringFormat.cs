namespace System.Drawing {
    /// <summary>Specifies how text is aligned within its layout rectangle.</summary>
    public enum StringAlignment {
        /// <summary>Text is aligned near the layout origin (left or top).</summary>
        Near = 0,
        /// <summary>Text is centered within the layout rectangle.</summary>
        Center = 1,
        /// <summary>Text is aligned far from the layout origin (right or bottom).</summary>
        Far = 2
    }

    /// <summary>Specifies how text is trimmed when it does not fit in the layout rectangle.</summary>
    public enum StringTrimming {
        /// <summary>Text is not trimmed.</summary>
        None = 0,
        /// <summary>Text is trimmed to the nearest character.</summary>
        Character = 1,
        /// <summary>Text is trimmed to the nearest word.</summary>
        Word = 2,
        /// <summary>Text is trimmed to the nearest character and an ellipsis is inserted.</summary>
        EllipsisCharacter = 3,
        /// <summary>Text is trimmed to the nearest word and an ellipsis is inserted.</summary>
        EllipsisWord = 4,
        /// <summary>The center of a path is removed and replaced with an ellipsis.</summary>
        EllipsisPath = 5
    }

    /// <summary>Bit flags that control text layout and rendering behavior.</summary>
    [Flags]
    public enum StringFormatFlags {
        /// <summary>Text is laid out from right to left.</summary>
        DirectionRightToLeft = 0x00000001,
        /// <summary>Text is laid out vertically.</summary>
        DirectionVertical = 0x00000002,
        /// <summary>Parts of characters are allowed to overhang the layout rectangle.</summary>
        FitBlackBox = 0x00000004,
        /// <summary>Control characters are shown with representative glyphs.</summary>
        DisplayFormatControl = 0x00000020,
        /// <summary>Fallback to alternate fonts for missing characters is disabled.</summary>
        NoFontFallback = 0x00000400,
        /// <summary>Trailing spaces are included when measuring text.</summary>
        MeasureTrailingSpaces = 0x00000800,
        /// <summary>Text wrapping between lines is disabled.</summary>
        NoWrap = 0x00001000,
        /// <summary>Only entire lines are laid out within the rectangle.</summary>
        LineLimit = 0x00002000,
        /// <summary>Glyph overhangs and unwrapped text are not clipped.</summary>
        NoClip = 0x00004000
    }


    /// <summary>Encapsulates text layout information such as alignment, trimming, and format flags.</summary>
    public sealed class StringFormat {
        /// <summary>Gets or sets the horizontal alignment of the text.</summary>
        public StringAlignment Alignment { get; set; }
        /// <summary>Gets or sets how text is trimmed when it does not fit.</summary>
        public StringTrimming Trimming { get; set; }
        /// <summary>Gets or sets the flags that control text layout and rendering.</summary>
        public StringFormatFlags FormatFlags { get; set; }
    }
}
