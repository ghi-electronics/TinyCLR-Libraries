using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// Options which can be applied to a RegularExpression
    /// </summary>
    [Flags]    
    public enum RegexOptions
    {
        /// <summary>Compile the regular expression to a program for faster execution.</summary>
        Compiled = 8,
        /// <summary>Ignore cultural differences in language when matching.</summary>
        CultureInvariant = 0x200,
        /// <summary>Enable ECMAScript-compliant behavior for the expression.</summary>
        ECMAScript = 0x100,
        /// <summary>Capture only explicitly named or numbered groups.</summary>
        ExplicitCapture = 4,
        /// <summary>Match without regard to case.</summary>
        IgnoreCase = 1,
        /// <summary>Ignore unescaped whitespace in the pattern.</summary>
        IgnorePatternWhitespace = 0x20,
        /// <summary>Treat ^ and $ as matching at the start and end of any line.</summary>
        Multiline = 2,
        /// <summary>Use the default options.</summary>
        None = 0,
        /// <summary>Make the period (.) match every character, including newlines.</summary>
        Singleline = 0x10,
        /// <summary>Time the match and throw if it exceeds the allowed number of ticks.</summary>
        Timed = 0x400
    }
}
