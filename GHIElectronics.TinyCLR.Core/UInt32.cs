////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    /**
     * * Wrapper for unsigned 32 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt32
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private uint m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public const uint MaxValue = (uint)0xffffffff;
        public const uint MinValue = 0U;

        public override String ToString()
        {
            return Number.Format(m_value, true, "G", NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.Format(m_value, true, format, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static uint Parse(String s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            return Convert.ToUInt32(s);
        }

        public static bool TryParse(string s, out uint b) {
            b = default(uint);

            try {
                b = uint.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


