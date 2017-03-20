namespace System {

    using System.Globalization;

    [Serializable()]
    public struct Single : IFormattable {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        internal float m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        //
        // Public constants
        //
        public const float MinValue = (float)-3.40282346638528859e+38;
        public const float Epsilon = (float)1.4e-45;
        public const float MaxValue = (float)3.40282346638528859e+38;

        public override string ToString() {
            // Number.Format method is responsible for returning the correct string representation of the value; however, it does not work properly for special values.
            // Fixing the issue in Number.Format requires a significant amount of modification in both native and managed code.
            // In order to avoid that (at lease for now), we use the help of Double class to identify special values and use Number.Format for the others.
            var str = ((double)this.m_value).ToString();
            switch (str) {
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    return str;
                default:
                    return Number.Format(this.m_value, false, "G", NumberFormatInfo.CurrentInfo);
            }
        }

        public string ToString(string format) {
            var str = ((double)this.m_value).ToString();
            switch (str) {
                case "Infinity":
                case "-Infinity":
                case "NaN":
                    return str;
                default:
                    return Number.Format(this.m_value, false, format, NumberFormatInfo.CurrentInfo);
            }
        }

        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, false, format, NumberFormatInfo.GetInstance(provider));
    }
}


