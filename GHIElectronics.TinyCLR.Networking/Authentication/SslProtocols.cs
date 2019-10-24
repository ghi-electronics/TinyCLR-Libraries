namespace System.Security.Authentication {
    [Flags]
    public enum SslProtocols {
        None = 0,
        Ssl2 = SchProtocols.Ssl2,
        Ssl3 = SchProtocols.Ssl3,
        Tls = SchProtocols.Tls10,
        Tls11 = SchProtocols.Tls11,
        Tls12 = SchProtocols.Tls12,
        Default = Ssl3 | Tls
    }

    public enum ExchangeAlgorithmType {
        None = 0,
        RsaSign = (Alg.ClassSignture | Alg.TypeRSA | Alg.Any),
        RsaKeyX = (Alg.ClassKeyXch | Alg.TypeRSA | Alg.Any),
        DiffieHellman = (Alg.ClassKeyXch | Alg.TypeDH | Alg.NameDH_Ephem),
    }


    public enum CipherAlgorithmType {
        None = 0,
        Rc2 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameRC2),
        Rc4 = (Alg.ClassEncrypt | Alg.TypeStream | Alg.NameRC4),
        Des = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameDES),
        TripleDes = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.Name3DES),
        Aes = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES),
        Aes128 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_128),
        Aes192 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_192),
        Aes256 = (Alg.ClassEncrypt | Alg.TypeBlock | Alg.NameAES_256),
        Null = (Alg.ClassEncrypt)
    }

    public enum HashAlgorithmType {
        None = 0,
        Md5 = (Alg.ClassHash | Alg.Any | Alg.NameMD5),
        Sha1 = (Alg.ClassHash | Alg.Any | Alg.NameSHA),
        Sha256 = (Alg.ClassHash | Alg.Any | Alg.NameSHA256),
        Sha384 = (Alg.ClassHash | Alg.Any | Alg.NameSHA384),
        Sha512 = (Alg.ClassHash | Alg.Any | Alg.NameSHA512)
    }

    public enum SslVerification {
        None = 0,
        Optional = 1,
        Required = 2,
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
