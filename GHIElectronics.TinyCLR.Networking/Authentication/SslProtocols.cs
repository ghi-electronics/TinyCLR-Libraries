namespace System.Security.Authentication {
    /// <summary>Defines the SSL and TLS protocol versions that can be used.</summary>
    [Flags]
    public enum SslProtocols {
        /// <summary>No protocol is specified.</summary>
        None = 0,
        /// <summary>The SSL 2.0 protocol.</summary>
        Ssl2 = SchProtocols.Ssl2,
        /// <summary>The SSL 3.0 protocol.</summary>
        Ssl3 = SchProtocols.Ssl3,
        /// <summary>The TLS 1.0 protocol.</summary>
        Tls = SchProtocols.Tls10,
        /// <summary>The TLS 1.1 protocol.</summary>
        Tls11 = SchProtocols.Tls11,
        /// <summary>The TLS 1.2 protocol.</summary>
        Tls12 = SchProtocols.Tls12,
        /// <summary>Allows the operating system to choose between SSL 3.0 and TLS 1.0.</summary>
        Default = Ssl3 | Tls
    }

    /// <summary>Specifies the algorithm used to create keys shared by the client and server.</summary>
    public enum ExchangeAlgorithmType {
        /// <summary>No key exchange algorithm is used.</summary>
        None = 0,
        /// <summary>The RSA public-key signature algorithm.</summary>
        RsaSign = (Alg.ClassSignture | Alg.TypeRSA | Alg.Any),
        /// <summary>The RSA public-key exchange algorithm.</summary>
        RsaKeyX = (Alg.ClassKeyXch | Alg.TypeRSA | Alg.Any),
        /// <summary>The Diffie-Hellman ephemeral key exchange algorithm.</summary>
        DiffieHellman = (Alg.ClassKeyXch | Alg.TypeDH | Alg.NameDH_Ephem),
    }


    /// <summary>Specifies the cipher algorithm used to encrypt data.</summary>
    public enum CipherAlgorithmType {
        /// <summary>No encryption algorithm is used.</summary>
        None = 0,
        /// <summary>The RC2 block cipher.</summary>
        Rc2 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameRC2),
        /// <summary>The RC4 stream cipher.</summary>
        Rc4 = (Alg.ClassEncrypt | Alg.TypeStream | Alg.NameRC4),
        /// <summary>The Data Encryption Standard (DES) block cipher.</summary>
        Des = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameDES),
        /// <summary>The Triple DES block cipher.</summary>
        TripleDes = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.Name3DES),
        /// <summary>The Advanced Encryption Standard (AES) block cipher.</summary>
        Aes = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES),
        /// <summary>The AES block cipher with a 128-bit key.</summary>
        Aes128 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_128),
        /// <summary>The AES block cipher with a 192-bit key.</summary>
        Aes192 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_192),
        /// <summary>The AES block cipher with a 256-bit key.</summary>
        Aes256 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_256),
        /// <summary>No encryption is performed.</summary>
        Null = (Alg.ClassEncrypt)
    }

    /// <summary>Specifies the hash algorithm used for message authentication.</summary>
    public enum HashAlgorithmType {
        /// <summary>No hash algorithm is used.</summary>
        None = 0,
        /// <summary>The MD5 hash algorithm.</summary>
        Md5 = (Alg.ClassHash | Alg.Any | Alg.NameMD5),
        /// <summary>The SHA-1 hash algorithm.</summary>
        Sha1 = (Alg.ClassHash | Alg.Any | Alg.NameSHA),
        /// <summary>The SHA-256 hash algorithm.</summary>
        Sha256 = (Alg.ClassHash | Alg.Any | Alg.NameSHA256),
        /// <summary>The SHA-384 hash algorithm.</summary>
        Sha384 = (Alg.ClassHash | Alg.Any | Alg.NameSHA384),
        /// <summary>The SHA-512 hash algorithm.</summary>
        Sha512 = (Alg.ClassHash | Alg.Any | Alg.NameSHA512)
    }

    /// <summary>Specifies how the remote certificate is verified during the SSL handshake.</summary>
    public enum SslVerification {
        /// <summary>No certificate verification is performed.</summary>
        None = 0,
        /// <summary>Certificate verification is optional.</summary>
        Optional = 1,
        /// <summary>Certificate verification is required.</summary>
        Required = 2,
        /// <summary>The certificate is verified only once.</summary>
        VerifyOnce = 3
    }

    [Flags]
    internal enum SchProtocols {
        Zero = 0,
        PctClient = 0x00000002,
        PctServer = 0x00000001,
        Pct = (PctClient | PctServer),
        Ssl2Client = 0x00000008,
        Ssl2Server = 0x00000004,
        Ssl2 = (Ssl2Client | Ssl2Server),
        Ssl3Client = 0x00000020,
        Ssl3Server = 0x00000010,
        Ssl3 = (Ssl3Client | Ssl3Server),
        Tls10Client = 0x00000080,
        Tls10Server = 0x00000040,
        Tls10 = (Tls10Client | Tls10Server),
        Tls11Client = 0x00000200,
        Tls11Server = 0x00000100,
        Tls11 = (Tls11Client | Tls11Server),
        Tls12Client = 0x00000800,
        Tls12Server = 0x00000400,
        Tls12 = (Tls12Client | Tls12Server),
        Ssl3Tls = (Ssl3 | Tls10),
        UniClient = unchecked((int)0x80000000),
        UniServer = 0x40000000,
        Unified = (UniClient | UniServer),
        ClientMask = (PctClient | Ssl2Client | Ssl3Client | Tls10Client | Tls11Client | Tls12Client | UniClient),
        ServerMask = (PctServer | Ssl2Server | Ssl3Server | Tls10Server | Tls11Server | Tls12Server | UniServer)
    }

    [Flags]
    internal enum Alg {
        Any = 0,
        ClassSignture = (1 << 13),
        ClassEncrypt = (3 << 13),
        ClassHash = (4 << 13),
        ClassKeyXch = (5 << 13),
        TypeRSA = (2 << 9),
        TypeBlock = (3 << 9),
        TypeStream = (4 << 9),
        TypeDH = (5 << 9),
        NameDES = 1,
        NameRC2 = 2,
        Name3DES = 3,
        NameAES_128 = 14,
        NameAES_192 = 15,
        NameAES_256 = 16,
        NameAES = 17,
        NameRC4 = 1,
        NameMD5 = 3,
        NameSHA = 4,
        NameSHA256 = 12,
        NameSHA384 = 13,
        NameSHA512 = 14,
        NameDH_Ephem = 2,
    }
}
