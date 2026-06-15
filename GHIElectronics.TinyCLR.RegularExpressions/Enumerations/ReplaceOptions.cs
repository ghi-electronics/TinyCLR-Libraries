using System;

namespace System.Text.RegularExpressions
{
    /// <summary>Options that control how a replacement is applied to matches.</summary>
    [Flags]
    public enum ReplaceOptions
    {
        /**
         * Flag bit that indicates that subst should replace all occurrences of this
         * regular expression.
         */
        /// <summary>Replace all occurrences of the regular expression.</summary>
        ReplaceAll = 0x0000,

        /**
         * Flag bit that indicates that subst should only replace the first occurrence
         * of this regular expression.
         */
        /// <summary>Replace only the first occurrence of the regular expression.</summary>
        ReplaceFirst = 0x0001,

        /**
         * Flag bit that indicates that subst should replace backreferences
         */
        /// <summary>Process backreferences in the replacement string.</summary>
        ReplaceBackrefrences = 0x0002
    }
}
