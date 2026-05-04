using System;

namespace System.Net {
    // Mirrors System.Net.AuthenticationSchemes from .NET Framework BCL.
    // Values are bit-flags so callers can combine schemes (matches BCL).
    [Flags]
    public enum AuthenticationSchemes {
        None = 0x00000000,
        Digest = 0x00000001,
        Negotiate = 0x00000002,
        Ntlm = 0x00000004,
        Basic = 0x00000008,
        Anonymous = 0x00008000,
        IntegratedWindowsAuthentication = Negotiate | Ntlm,
    }
}
