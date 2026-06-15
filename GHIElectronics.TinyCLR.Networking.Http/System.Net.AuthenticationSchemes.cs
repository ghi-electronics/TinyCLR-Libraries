using System;

namespace System.Net {
    /// <summary>Specifies the protocols used to authenticate client requests.</summary>
    // Mirrors System.Net.AuthenticationSchemes from .NET Framework BCL.
    // Values are bit-flags so callers can combine schemes (matches BCL).
    [Flags]
    public enum AuthenticationSchemes {
        /// <summary>No authentication is allowed.</summary>
        None = 0x00000000,
        /// <summary>Specifies digest authentication.</summary>
        Digest = 0x00000001,
        /// <summary>Negotiates with the client to determine the authentication scheme.</summary>
        Negotiate = 0x00000002,
        /// <summary>Specifies NTLM authentication.</summary>
        Ntlm = 0x00000004,
        /// <summary>Specifies basic authentication.</summary>
        Basic = 0x00000008,
        /// <summary>Specifies anonymous authentication.</summary>
        Anonymous = 0x00008000,
        /// <summary>Specifies Windows authentication using Negotiate or NTLM.</summary>
        IntegratedWindowsAuthentication = Negotiate | Ntlm,
    }
}
