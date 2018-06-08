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

    public sealed class StringFormat {
        public StringAlignment Alignment { get; set; }
        public StringTrimming Trimming { get; set; }
    }
}
