using System;
using System.IO;
using TinyCrypto = GHIElectronics.TinyCLR.Cryptography;

namespace System.Security.Cryptography {

    /// <summary>Well-known hash-algorithm name selector matching .NET Framework's <c>System.Security.Cryptography.HashAlgorithmName</c>.</summary>
    // .NET hash algorithm name selector. Matches System.Security.Cryptography.HashAlgorithmName
    // shape from .NET Framework 4.6+: a struct with static well-known names + Equals.
    public struct HashAlgorithmName {
        private readonly string name;

        public HashAlgorithmName(string name) => this.name = name;

        public string Name => this.name;

        public static HashAlgorithmName MD5 => new HashAlgorithmName("MD5");
        public static HashAlgorithmName SHA1 => new HashAlgorithmName("SHA1");
        public static HashAlgorithmName SHA256 => new HashAlgorithmName("SHA256");
        public static HashAlgorithmName SHA384 => new HashAlgorithmName("SHA384");
        public static HashAlgorithmName SHA512 => new HashAlgorithmName("SHA512");

        public bool Equals(HashAlgorithmName other) => this.name == other.name;
        public override bool Equals(object obj) => obj is HashAlgorithmName other && this.Equals(other);
        public override int GetHashCode() => this.name == null ? 0 : this.name.GetHashCode();
        public override string ToString() => this.name ?? string.Empty;

        public static bool operator ==(HashAlgorithmName left, HashAlgorithmName right) => left.Equals(right);
        public static bool operator !=(HashAlgorithmName left, HashAlgorithmName right) => !left.Equals(right);
    }

    /// <summary>Padding scheme applied to RSA-encrypted blocks.</summary>
    public enum RSAEncryptionPaddingMode {
        /// <summary>PKCS#1 v1.5 padding.</summary>
        Pkcs1 = 0,
        /// <summary>OAEP (Optimal Asymmetric Encryption Padding).</summary>
        Oaep = 1,
    }

    /// <summary>Encryption-padding configuration — padding mode plus (for OAEP) the hash algorithm. Matches the .NET Framework type.</summary>
    // Matches .NET Framework: padding mode + (for OAEP) hash algorithm bundled together.
    public sealed class RSAEncryptionPadding {
        private static readonly RSAEncryptionPadding s_pkcs1 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Pkcs1, default(HashAlgorithmName));
        private static readonly RSAEncryptionPadding s_oaepSHA1 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, HashAlgorithmName.SHA1);
        private static readonly RSAEncryptionPadding s_oaepSHA256 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, HashAlgorithmName.SHA256);

        public static RSAEncryptionPadding Pkcs1 => s_pkcs1;
        public static RSAEncryptionPadding OaepSHA1 => s_oaepSHA1;
        public static RSAEncryptionPadding OaepSHA256 => s_oaepSHA256;

        public RSAEncryptionPaddingMode Mode { get; }
        public HashAlgorithmName OaepHashAlgorithm { get; }

        private RSAEncryptionPadding(RSAEncryptionPaddingMode mode, HashAlgorithmName oaepHash) {
            this.Mode = mode;
            this.OaepHashAlgorithm = oaepHash;
        }

        public static RSAEncryptionPadding CreateOaep(HashAlgorithmName hashAlgorithm) =>
            new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, hashAlgorithm);

        public bool Equals(RSAEncryptionPadding other) =>
            other != null && this.Mode == other.Mode && this.OaepHashAlgorithm.Equals(other.OaepHashAlgorithm);
        public override bool Equals(object obj) => this.Equals(obj as RSAEncryptionPadding);
        public override int GetHashCode() => (int)this.Mode ^ this.OaepHashAlgorithm.GetHashCode();
        public override string ToString() => this.Mode + (this.Mode == RSAEncryptionPaddingMode.Oaep ? "(" + this.OaepHashAlgorithm + ")" : "");

        public static bool operator ==(RSAEncryptionPadding left, RSAEncryptionPadding right) =>
            ReferenceEquals(left, right) || (left is object && left.Equals(right));
        public static bool operator !=(RSAEncryptionPadding left, RSAEncryptionPadding right) => !(left == right);
    }

    /// <summary>Padding scheme applied to RSA signatures.</summary>
    public enum RSASignaturePaddingMode {
        /// <summary>PKCS#1 v1.5 signature padding.</summary>
        Pkcs1 = 0,
        /// <summary>PSS (Probabilistic Signature Scheme) padding.</summary>
        Pss = 1,
    }

    /// <summary>Signature-padding configuration. Matches the .NET Framework type.</summary>
    public sealed class RSASignaturePadding {
        private static readonly RSASignaturePadding s_pkcs1 = new RSASignaturePadding(RSASignaturePaddingMode.Pkcs1);
        private static readonly RSASignaturePadding s_pss = new RSASignaturePadding(RSASignaturePaddingMode.Pss);

        public static RSASignaturePadding Pkcs1 => s_pkcs1;
        public static RSASignaturePadding Pss => s_pss;

        public RSASignaturePaddingMode Mode { get; }

        private RSASignaturePadding(RSASignaturePaddingMode mode) => this.Mode = mode;

        public bool Equals(RSASignaturePadding other) => other != null && this.Mode == other.Mode;
        public override bool Equals(object obj) => this.Equals(obj as RSASignaturePadding);
        public override int GetHashCode() => (int)this.Mode;
        public override string ToString() => this.Mode.ToString();

        public static bool operator ==(RSASignaturePadding left, RSASignaturePadding right) =>
            ReferenceEquals(left, right) || (left is object && left.Equals(right));
        public static bool operator !=(RSASignaturePadding left, RSASignaturePadding right) => !(left == right);
    }

    /// <summary>RSA key parameters (modulus, exponent, and optional private components) matching the .NET Framework struct.</summary>
    [Serializable]
    public struct RSAParameters {
        public byte[] D;
        public byte[] DP;
        public byte[] DQ;
        public byte[] Exponent;
        public byte[] InverseQ;
        public byte[] Modulus;
        public byte[] P;
        public byte[] Q;
    }

    // .NET hash algorithm hierarchy: HashAlgorithm -> SHA1/SHA256/MD5 (each abstract in BCL,
    // concrete here for simplicity). KeyedHashAlgorithm -> HMAC -> HMACSHA1/HMACSHA256.

    /// <summary>Abstract base for cryptographic hash algorithms (MD5, SHA1, SHA256). Matches the .NET BCL surface.</summary>
    public abstract class HashAlgorithm : IDisposable {
        public virtual int HashSize { get; protected set; }
        public virtual byte[] Hash { get; protected set; }

        public abstract byte[] ComputeHash(byte[] buffer);
        public abstract byte[] ComputeHash(byte[] buffer, int offset, int count);
        public abstract byte[] ComputeHash(Stream inputStream);

        public abstract void Initialize();

        public void Clear() => this.Dispose();

        public virtual void Dispose() { }
    }

    /// <summary>Abstract base for keyed hash algorithms (HMAC family).</summary>
    public abstract class KeyedHashAlgorithm : HashAlgorithm {
        public virtual byte[] Key { get; set; }
    }

    /// <summary>Abstract base for HMAC algorithms (HMAC-SHA1, HMAC-SHA256, etc.).</summary>
    public abstract class HMAC : KeyedHashAlgorithm {
        public string HashName { get; set; }
    }

    /// <summary>Abstract base for asymmetric (public-key) algorithms.</summary>
    public abstract class AsymmetricAlgorithm : IDisposable {
        public virtual int KeySize { get; set; }
        public virtual string KeyExchangeAlgorithm => null;
        public virtual string SignatureAlgorithm => null;
        public virtual void Dispose() { }
        public void Clear() => this.Dispose();
    }

    /// <summary>Abstract RSA implementation; create concrete instances via <see cref="RSACryptoServiceProvider"/>.</summary>
    public abstract class RSA : AsymmetricAlgorithm {
        public abstract RSAParameters ExportParameters(bool includePrivateParameters);
        public abstract void ImportParameters(RSAParameters parameters);

        public virtual byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) =>
            throw new NotImplementedException();
        public virtual byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) =>
            throw new NotImplementedException();
        public virtual byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        public virtual byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        public virtual bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        public virtual bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
    }

    // ----- Concrete algorithms (delegate to TinyCLR.Cryptography) -----

    /// <summary>SHA-1 hash (160-bit). Use <see cref="HashAlgorithm.Create()"/> overloads or instantiate directly.</summary>
    public sealed class SHA1 : HashAlgorithm {
        private readonly TinyCrypto.SHA1 impl;

        private SHA1() {
            this.impl = TinyCrypto.SHA1.Create();
            this.HashSize = this.impl.HashSize;
        }

        public static SHA1 Create() => new SHA1();

        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { /* derived state managed by impl */ }
        }

        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        public override void Initialize() => this.impl.Initialize();

        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>SHA-256 hash (256-bit).</summary>
    public sealed class SHA256 : HashAlgorithm {
        private readonly TinyCrypto.SHA256 impl;

        private SHA256() {
            this.impl = TinyCrypto.SHA256.Create();
            this.HashSize = this.impl.HashSize;
        }

        public static SHA256 Create() => new SHA256();

        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        public override void Initialize() => this.impl.Initialize();

        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>MD5 hash (128-bit). Cryptographically broken; use for checksums, not for security.</summary>
    public sealed class MD5 : HashAlgorithm {
        private readonly TinyCrypto.MD5 impl;

        private MD5() {
            this.impl = TinyCrypto.MD5.Create();
            this.HashSize = this.impl.HashSize;
        }

        public static MD5 Create() => new MD5();

        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        public override void Initialize() => this.impl.Initialize();

        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>HMAC-SHA1 keyed hash (160-bit output).</summary>
    public sealed class HMACSHA1 : HMAC {
        private readonly TinyCrypto.HMACSHA1 impl;

        public HMACSHA1() {
            this.impl = new TinyCrypto.HMACSHA1();
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        public HMACSHA1(byte[] key) {
            this.impl = new TinyCrypto.HMACSHA1(key);
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        public override byte[] Key {
            get => this.impl.Key;
            set => this.impl.Key = value;
        }

        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        public override void Initialize() => this.impl.Initialize();

        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>HMAC-SHA256 keyed hash (256-bit output).</summary>
    public sealed class HMACSHA256 : HMAC {
        private readonly TinyCrypto.HMACSHA256 impl;

        public HMACSHA256() {
            this.impl = new TinyCrypto.HMACSHA256();
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        public HMACSHA256(byte[] key) {
            this.impl = new TinyCrypto.HMACSHA256(key);
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        public override byte[] Key {
            get => this.impl.Key;
            set => this.impl.Key = value;
        }

        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        public override void Initialize() => this.impl.Initialize();

        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>Concrete RSA implementation. Construct with the desired key size or with externally supplied <see cref="RSAParameters"/>.</summary>
    public sealed class RSACryptoServiceProvider : RSA {
        private readonly TinyCrypto.RSACryptoServiceProvider impl;

        public RSACryptoServiceProvider() => this.impl = new TinyCrypto.RSACryptoServiceProvider();

        public RSACryptoServiceProvider(int dwKeySize) => this.impl = new TinyCrypto.RSACryptoServiceProvider(dwKeySize);

        public override int KeySize {
            get => this.impl.KeySize;
            set => throw new NotSupportedException("Set KeySize via constructor.");
        }

        public override string KeyExchangeAlgorithm => this.impl.KeyExchangeAlgorithm;

        public override RSAParameters ExportParameters(bool includePrivateParameters) {
            var p = this.impl.ExportParameters(includePrivateParameters);
            return new RSAParameters {
                D = p.D, DP = p.DP, DQ = p.DQ,
                Exponent = p.Exponent, InverseQ = p.InverseQ,
                Modulus = p.Modulus, P = p.P, Q = p.Q
            };
        }

        public override void ImportParameters(RSAParameters parameters) {
            this.impl.ImportParameters(new TinyCrypto.RSAParameters {
                D = parameters.D, DP = parameters.DP, DQ = parameters.DQ,
                Exponent = parameters.Exponent, InverseQ = parameters.InverseQ,
                Modulus = parameters.Modulus, P = parameters.P, Q = parameters.Q
            });
        }

        // ----- .NET-shape methods using HashAlgorithmName + RSASignaturePadding/RSAEncryptionPadding -----

        public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSAEncryptionPaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 encryption padding is supported.");
            return this.impl.Encrypt(data);
        }

        public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSAEncryptionPaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 encryption padding is supported.");
            return this.impl.Decrypt(data);
        }

        public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            return this.impl.SignData(data, hashAlgorithm.Equals(HashAlgorithmName.SHA256));
        }

        public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            var halg = hashAlgorithm.Equals(HashAlgorithmName.SHA256)
                ? TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha256
                : TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha1;
            return this.impl.SignHash(hash, halg);
        }

        public override bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            return this.impl.VerifyData(data, signature, hashAlgorithm.Equals(HashAlgorithmName.SHA256));
        }

        public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            var halg = hashAlgorithm.Equals(HashAlgorithmName.SHA256)
                ? TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha256
                : TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha1;
            return this.impl.VerifyHash(hash, signature, halg);
        }

        // ----- Backward-compat overloads matching .NET RSACryptoServiceProvider's legacy surface -----

        public byte[] Encrypt(byte[] rgb, bool fOAEP) {
            if (fOAEP) throw new NotSupportedException("OAEP not supported.");
            return this.impl.Encrypt(rgb);
        }

        public byte[] Decrypt(byte[] rgb, bool fOAEP) {
            if (fOAEP) throw new NotSupportedException("OAEP not supported.");
            return this.impl.Decrypt(rgb);
        }

        public byte[] SignData(byte[] buffer, bool sha256 = false) => this.impl.SignData(buffer, sha256);
        public bool VerifyData(byte[] buffer, byte[] signature, bool sha256 = false) => this.impl.VerifyData(buffer, signature, sha256);

        public override void Dispose() => this.impl.Dispose();
    }

    // ----- Random number generator (already shaped to .NET) -----

    /// <summary>Cryptographically secure RNG. Create via <see cref="Create()"/>; do not seed manually.</summary>
    public abstract class RandomNumberGenerator : IDisposable {
        public static RandomNumberGenerator Create() => new TinyClrRandomNumberGenerator();

        public static void Fill(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            using (var rng = Create())
                rng.GetBytes(data);
        }

        public abstract void GetBytes(byte[] data);

        public abstract void Dispose();
    }

    internal sealed class TinyClrRandomNumberGenerator : RandomNumberGenerator {
        public override void GetBytes(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            var offset = 0;
            var counter = 0;
            while (offset < data.Length) {
                var block = GetEntropyBlock(counter++);
                var copy = Math.Min(block.Length, data.Length - offset);
                Array.Copy(block, 0, data, offset, copy);
                offset += copy;
            }
        }

        public override void Dispose() { }

        private static byte[] GetEntropyBlock(int counter) {
            using (var rsa = new TinyCrypto.RSACryptoServiceProvider(1024)) {
                var seed = rsa.ExportParameters(false).Modulus;
                if (seed == null || seed.Length == 0)
                    throw new InvalidOperationException("Unable to obtain entropy source.");

                var input = new byte[seed.Length + 4];
                Array.Copy(seed, 0, input, 0, seed.Length);
                input[seed.Length] = (byte)(counter >> 24);
                input[seed.Length + 1] = (byte)(counter >> 16);
                input[seed.Length + 2] = (byte)(counter >> 8);
                input[seed.Length + 3] = (byte)counter;

                return TinyCrypto.SHA256.Create().ComputeHash(input);
            }
        }
    }
}
