namespace System.Drawing {
    public enum StringAlignment {
        Near = 0,
        Center = 1,
        Far = 2
    }

    public enum StringTrimming {
        None = 0,
        Character = 1,
        Word = 2,
        EllipsisCharacter = 3,
        EllipsisWord = 4,
        EllipsisPath = 5
    }

    [Flags]
    public enum StringFormatFlags {
        DirectionRightToLeft = 0x00000001,
        DirectionVertical = 0x00000002,
        FitBlackBox = 0x00000004,
        DisplayFormatControl = 0x00000020,
        NoFontFallback = 0x00000400,
        MeasureTrailingSpaces = 0x00000800,
        NoWrap = 0x00001000,
        LineLimit = 0x00002000,
        NoClip = 0x00004000
    }


    public sealed class StringFormat {
        public StringAlignment Alignment { get; set; }
        public StringTrimming Trimming { get; set; }
        public StringFormatFlags FormatFlags { get; set; }
    }
}
