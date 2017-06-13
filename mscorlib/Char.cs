namespace System {
    [Serializable]
    public struct Char {
        //
        // Member Variables
        //
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        internal char m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        //
        // Public Constants
        //
        /**
         * The maximum character value.
         */
        public const char MaxValue = (char)0xFFFF;
        /**
         * The minimum character value.
         */
        public const char MinValue = (char)0x00;

        public override string ToString() => new string(this.m_value, 1);

        public char ToLower() {
            if ('A' <= this.m_value && this.m_value <= 'Z') {
                return (char)(this.m_value - ('A' - 'a'));
            }

            return this.m_value;
        }

        public char ToUpper() {
            if ('a' <= this.m_value && this.m_value <= 'z') {
                return (char)(this.m_value + ('A' - 'a'));
            }

            return this.m_value;
        }
    }
}


