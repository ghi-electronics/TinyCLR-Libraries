namespace System.Runtime.CompilerServices {
    public static class FormattableStringFactory {
        public static FormattableString Create(string format, params object[] arguments) => new ConcreteFormattableString(format, arguments);

        private sealed class ConcreteFormattableString : FormattableString {
            private readonly string format;
            private readonly object[] arguments;

            internal ConcreteFormattableString(string format, object[] arguments) {
                this.format = format;
                this.arguments = arguments;
            }

            public override string Format => this.format;
            public override object[] GetArguments() => this.arguments;
            public override int ArgumentCount => this.arguments.Length;
            public override object GetArgument(int index) => this.arguments[index];
            public override string ToString(IFormatProvider formatProvider) => string.Format(formatProvider, this.format, this.arguments);
        }

    }
}