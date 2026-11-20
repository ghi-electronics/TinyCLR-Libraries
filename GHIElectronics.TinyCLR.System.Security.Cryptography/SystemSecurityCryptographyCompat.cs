using System;
using System.IO;
using TinyCrypto = GHIElectronics.TinyCLR.Cryptography;

namespace System.Security.Cryptography {

    // .NET hash algorithm name selector. Matches System.Security.Cryptography.HashAlgorithmName
    // shape from .NET Framework 4.6+: a struct with static well-known names + Equals.
    /// <summary>Well-known hash-algorithm name selector matching .NET Framework's <c>System.Security.Cryptography.HashAlgorithmName</c>.</summary>
    public struct HashAlgorithmName {
        private readonly string name;

        /// <summary>Creates a name from the given algorithm string.</summary>
        public HashAlgorithmName(string name) => this.name = name;

        /// <summary>The algorithm name.</summary>
        public string Name => this.name;

        /// <summary>The MD5 algorithm name.</summary>
        public static HashAlgorithmName MD5 => new HashAlgorithmName("MD5");
        /// <summary>The SHA-1 algorithm name.</summary>
        public static HashAlgorithmName SHA1 => new HashAlgorithmName("SHA1");
        /// <summary>The SHA-256 algorithm name.</summary>
        public static HashAlgorithmName SHA256 => new HashAlgorithmName("SHA256");
        /// <summary>The SHA-384 algorithm name.</summary>
        public static HashAlgorithmName SHA384 => new HashAlgorithmName("SHA384");
        /// <summary>The SHA-512 algorithm name.</summary>
        public static HashAlgorithmName SHA512 => new HashAlgorithmName("SHA512");

        /// <summary>Returns true if the two names are equal.</summary>
        public bool Equals(HashAlgorithmName other) => this.name == other.name;
        /// <summary>Returns true if the object is an equal name.</summary>
        public override bool Equals(object obj) => obj is HashAlgorithmName other && this.Equals(other);
        /// <summary>Returns the hash code for this name.</summary>
        public override int GetHashCode() => this.name == null ? 0 : this.name.GetHashCode();
        /// <summary>Returns the algorithm name.</summary>
        public override string ToString() => this.name ?? string.Empty;

        /// <summary>Returns true if the two names are equal.</summary>
        public static bool operator ==(HashAlgorithmName left, HashAlgorithmName right) => left.Equals(right);
        /// <summary>Returns true if the two names differ.</summary>
        public static bool operator !=(HashAlgorithmName left, HashAlgorithmName right) => !left.Equals(right);
    }

    /// <summary>Padding scheme applied to RSA-encrypted blocks.</summary>
    public enum RSAEncryptionPaddingMode {
        /// <summary>PKCS#1 v1.5 padding.</summary>
        Pkcs1 = 0,
        /// <summary>OAEP (Optimal Asymmetric Encryption Padding).</summary>
        Oaep = 1,
    }

    // Matches .NET Framework: padding mode + (for OAEP) hash algorithm bundled together.
    /// <summary>Encryption-padding configuration — padding mode plus (for OAEP) the hash algorithm. Matches the .NET Framework type.</summary>
    public sealed class RSAEncryptionPadding {
        private static readonly RSAEncryptionPadding s_pkcs1 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Pkcs1, default(HashAlgorithmName));
        private static readonly RSAEncryptionPadding s_oaepSHA1 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, HashAlgorithmName.SHA1);
        private static readonly RSAEncryptionPadding s_oaepSHA256 = new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, HashAlgorithmName.SHA256);

        /// <summary>PKCS#1 v1.5 encryption padding.</summary>
        public static RSAEncryptionPadding Pkcs1 => s_pkcs1;
        /// <summary>OAEP padding using SHA-1.</summary>
        public static RSAEncryptionPadding OaepSHA1 => s_oaepSHA1;
        /// <summary>OAEP padding using SHA-256.</summary>
        public static RSAEncryptionPadding OaepSHA256 => s_oaepSHA256;

        /// <summary>The padding mode.</summary>
        public RSAEncryptionPaddingMode Mode { get; }
        /// <summary>The hash algorithm used for OAEP padding.</summary>
        public HashAlgorithmName OaepHashAlgorithm { get; }

        private RSAEncryptionPadding(RSAEncryptionPaddingMode mode, HashAlgorithmName oaepHash) {
            this.Mode = mode;
            this.OaepHashAlgorithm = oaepHash;
        }

        /// <summary>Creates OAEP padding using the given hash algorithm.</summary>
        public static RSAEncryptionPadding CreateOaep(HashAlgorithmName hashAlgorithm) =>
            new RSAEncryptionPadding(RSAEncryptionPaddingMode.Oaep, hashAlgorithm);

        /// <summary>Returns true if the two paddings are equal.</summary>
        public bool Equals(RSAEncryptionPadding other) =>
            other != null && this.Mode == other.Mode && this.OaepHashAlgorithm.Equals(other.OaepHashAlgorithm);
        /// <summary>Returns true if the object is an equal padding.</summary>
        public override bool Equals(object obj) => this.Equals(obj as RSAEncryptionPadding);
        /// <summary>Returns the hash code for this padding.</summary>
        public override int GetHashCode() => (int)this.Mode ^ this.OaepHashAlgorithm.GetHashCode();
        /// <summary>Returns a text description of this padding.</summary>
        public override string ToString() => this.Mode + (this.Mode == RSAEncryptionPaddingMode.Oaep ? "(" + this.OaepHashAlgorithm + ")" : "");

        /// <summary>Returns true if the two paddings are equal.</summary>
        public static bool operator ==(RSAEncryptionPadding left, RSAEncryptionPadding right) =>
            ReferenceEquals(left, right) || (left is object && left.Equals(right));
        /// <summary>Returns true if the two paddings differ.</summary>
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

        /// <summary>PKCS#1 v1.5 signature padding.</summary>
        public static RSASignaturePadding Pkcs1 => s_pkcs1;
        /// <summary>PSS signature padding.</summary>
        public static RSASignaturePadding Pss => s_pss;

        /// <summary>The padding mode.</summary>
        public RSASignaturePaddingMode Mode { get; }

        private RSASignaturePadding(RSASignaturePaddingMode mode) => this.Mode = mode;

        /// <summary>Returns true if the two paddings are equal.</summary>
        public bool Equals(RSASignaturePadding other) => other != null && this.Mode == other.Mode;
        /// <summary>Returns true if the object is an equal padding.</summary>
        public override bool Equals(object obj) => this.Equals(obj as RSASignaturePadding);
        /// <summary>Returns the hash code for this padding.</summary>
        public override int GetHashCode() => (int)this.Mode;
        /// <summary>Returns a text description of this padding.</summary>
        public override string ToString() => this.Mode.ToString();

        /// <summary>Returns true if the two paddings are equal.</summary>
        public static bool operator ==(RSASignaturePadding left, RSASignaturePadding right) =>
            ReferenceEquals(left, right) || (left is object && left.Equals(right));
        /// <summary>Returns true if the two paddings differ.</summary>
        public static bool operator !=(RSASignaturePadding left, RSASignaturePadding right) => !(left == right);
    }

    /// <summary>RSA key parameters (modulus, exponent, and optional private components) matching the .NET Framework struct.</summary>
    [Serializable]
    public struct RSAParameters {
        /// <summary>The private exponent (private key).</summary>
        public byte[] D;
        /// <summary>d mod (p-1).</summary>
        public byte[] DP;
        /// <summary>d mod (q-1).</summary>
        public byte[] DQ;
        /// <summary>The public exponent.</summary>
        public byte[] Exponent;
        /// <summary>The CRT coefficient (q^-1 mod p).</summary>
        public byte[] InverseQ;
        /// <summary>The modulus.</summary>
        public byte[] Modulus;
        /// <summary>The first prime factor.</summary>
        public byte[] P;
        /// <summary>The second prime factor.</summary>
        public byte[] Q;
    }

    // .NET hash algorithm hierarchy: HashAlgorithm -> SHA1/SHA256/MD5 (each abstract in BCL,
    // concrete here for simplicity). KeyedHashAlgorithm -> HMAC -> HMACSHA1/HMACSHA256.

    /// <summary>Abstract base for cryptographic hash algorithms (MD5, SHA1, SHA256). Matches the .NET BCL surface.</summary>
    public abstract class HashAlgorithm : IDisposable {
        /// <summary>Size of the computed hash, in bits.</summary>
        public virtual int HashSize { get; protected set; }
        /// <summary>The hash value computed by the last operation.</summary>
        public virtual byte[] Hash { get; protected set; }

        /// <summary>Computes the hash of the given data.</summary>
        public abstract byte[] ComputeHash(byte[] buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public abstract byte[] ComputeHash(byte[] buffer, int offset, int count);
        /// <summary>Computes the hash of a stream.</summary>
        public abstract byte[] ComputeHash(Stream inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public abstract void Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public void Clear() => this.Dispose();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public virtual void Dispose() { }
    }

    /// <summary>Abstract base for keyed hash algorithms (HMAC family).</summary>
    public abstract class KeyedHashAlgorithm : HashAlgorithm {
        /// <summary>The secret key used by the algorithm.</summary>
        public virtual byte[] Key { get; set; }
    }

    /// <summary>Abstract base for HMAC algorithms (HMAC-SHA1, HMAC-SHA256, etc.).</summary>
    public abstract class HMAC : KeyedHashAlgorithm {
        /// <summary>Name of the inner hash algorithm.</summary>
        public string HashName { get; set; }
    }

    /// <summary>Abstract base for asymmetric (public-key) algorithms.</summary>
    public abstract class AsymmetricAlgorithm : IDisposable {
        /// <summary>The key size, in bits.</summary>
        public virtual int KeySize { get; set; }
        /// <summary>Name of the key-exchange algorithm, or null.</summary>
        public virtual string KeyExchangeAlgorithm => null;
        /// <summary>Name of the signature algorithm, or null.</summary>
        public virtual string SignatureAlgorithm => null;
        /// <summary>Releases the resources used by the algorithm.</summary>
        public virtual void Dispose() { }
        /// <summary>Releases the resources used by the algorithm.</summary>
        public void Clear() => this.Dispose();
    }

    /// <summary>Abstract RSA implementation; create concrete instances via <see cref="RSACryptoServiceProvider"/>.</summary>
    public abstract class RSA : AsymmetricAlgorithm {
        /// <summary>Exports the RSA key, optionally including the private parameters.</summary>
        public abstract RSAParameters ExportParameters(bool includePrivateParameters);
        /// <summary>Imports the given RSA key parameters.</summary>
        public abstract void ImportParameters(RSAParameters parameters);

        /// <summary>Encrypts data with the public key using the given padding.</summary>
        public virtual byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) =>
            throw new NotImplementedException();
        /// <summary>Decrypts data with the private key using the given padding.</summary>
        public virtual byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) =>
            throw new NotImplementedException();
        /// <summary>Signs data with the private key.</summary>
        public virtual byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        /// <summary>Signs a precomputed hash with the private key.</summary>
        public virtual byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        /// <summary>Verifies a data signature against the public key.</summary>
        public virtual bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) =>
            throw new NotImplementedException();
        /// <summary>Verifies a hash signature against the public key.</summary>
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

        /// <summary>Creates a new SHA-1 instance.</summary>
        public static SHA1 Create() => new SHA1();

        /// <summary>The hash value computed by the last operation.</summary>
        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { /* derived state managed by impl */ }
        }

        /// <summary>Computes the hash of the given data.</summary>
        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        /// <summary>Computes the hash of a stream.</summary>
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public override void Initialize() => this.impl.Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>SHA-256 hash (256-bit).</summary>
    public sealed class SHA256 : HashAlgorithm {
        private readonly TinyCrypto.SHA256 impl;

        private SHA256() {
            this.impl = TinyCrypto.SHA256.Create();
            this.HashSize = this.impl.HashSize;
        }

        /// <summary>Creates a new SHA-256 instance.</summary>
        public static SHA256 Create() => new SHA256();

        /// <summary>The hash value computed by the last operation.</summary>
        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        /// <summary>Computes the hash of the given data.</summary>
        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        /// <summary>Computes the hash of a stream.</summary>
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public override void Initialize() => this.impl.Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>MD5 hash (128-bit). Cryptographically broken; use for checksums, not for security.</summary>
    public sealed class MD5 : HashAlgorithm {
        private readonly TinyCrypto.MD5 impl;

        private MD5() {
            this.impl = TinyCrypto.MD5.Create();
            this.HashSize = this.impl.HashSize;
        }

        /// <summary>Creates a new MD5 instance.</summary>
        public static MD5 Create() => new MD5();

        /// <summary>The hash value computed by the last operation.</summary>
        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        /// <summary>Computes the hash of the given data.</summary>
        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        /// <summary>Computes the hash of a stream.</summary>
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public override void Initialize() => this.impl.Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>HMAC-SHA1 keyed hash (160-bit output).</summary>
    public sealed class HMACSHA1 : HMAC {
        private readonly TinyCrypto.HMACSHA1 impl;

        /// <summary>Creates an HMAC-SHA1 with a random key.</summary>
        public HMACSHA1() {
            this.impl = new TinyCrypto.HMACSHA1();
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        /// <summary>Creates an HMAC-SHA1 with the given key.</summary>
        public HMACSHA1(byte[] key) {
            this.impl = new TinyCrypto.HMACSHA1(key);
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        /// <summary>The hash value computed by the last operation.</summary>
        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        /// <summary>The secret key used by the algorithm.</summary>
        public override byte[] Key {
            get => this.impl.Key;
            set => this.impl.Key = value;
        }

        /// <summary>Computes the hash of the given data.</summary>
        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        /// <summary>Computes the hash of a stream.</summary>
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public override void Initialize() => this.impl.Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>HMAC-SHA256 keyed hash (256-bit output).</summary>
    public sealed class HMACSHA256 : HMAC {
        private readonly TinyCrypto.HMACSHA256 impl;

        /// <summary>Creates an HMAC-SHA256 with a random key.</summary>
        public HMACSHA256() {
            this.impl = new TinyCrypto.HMACSHA256();
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        /// <summary>Creates an HMAC-SHA256 with the given key.</summary>
        public HMACSHA256(byte[] key) {
            this.impl = new TinyCrypto.HMACSHA256(key);
            this.HashSize = this.impl.HashSize;
            this.HashName = this.impl.HashName;
        }

        /// <summary>The hash value computed by the last operation.</summary>
        public override byte[] Hash {
            get => this.impl.Hash;
            protected set { }
        }

        /// <summary>The secret key used by the algorithm.</summary>
        public override byte[] Key {
            get => this.impl.Key;
            set => this.impl.Key = value;
        }

        /// <summary>Computes the hash of the given data.</summary>
        public override byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);
        /// <summary>Computes the hash of a region of the data.</summary>
        public override byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);
        /// <summary>Computes the hash of a stream.</summary>
        public override byte[] ComputeHash(Stream inputStream) => this.impl.ComputeHash(inputStream);

        /// <summary>Resets the algorithm to its initial state.</summary>
        public override void Initialize() => this.impl.Initialize();

        /// <summary>Releases the resources used by the algorithm.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    /// <summary>Concrete RSA implementation. Construct with the desired key size or with externally supplied <see cref="RSAParameters"/>.</summary>
    public sealed class RSACryptoServiceProvider : RSA {
        private readonly TinyCrypto.RSACryptoServiceProvider impl;

        /// <summary>Creates an RSA provider with the default key size.</summary>
        public RSACryptoServiceProvider() => this.impl = new TinyCrypto.RSACryptoServiceProvider();

        /// <summary>Creates an RSA provider with the given key size, in bits.</summary>
        public RSACryptoServiceProvider(int dwKeySize) => this.impl = new TinyCrypto.RSACryptoServiceProvider(dwKeySize);

        /// <summary>The key size, in bits. Set it via the constructor.</summary>
        public override int KeySize {
            get => this.impl.KeySize;
            set => throw new NotSupportedException("Set KeySize via constructor.");
        }

        /// <summary>Name of the key-exchange algorithm.</summary>
        public override string KeyExchangeAlgorithm => this.impl.KeyExchangeAlgorithm;

        /// <summary>Exports the RSA key, optionally including the private parameters.</summary>
        public override RSAParameters ExportParameters(bool includePrivateParameters) {
            var p = this.impl.ExportParameters(includePrivateParameters);
            return new RSAParameters {
                D = p.D, DP = p.DP, DQ = p.DQ,
                Exponent = p.Exponent, InverseQ = p.InverseQ,
                Modulus = p.Modulus, P = p.P, Q = p.Q
            };
        }

        /// <summary>Imports the given RSA key parameters.</summary>
        public override void ImportParameters(RSAParameters parameters) {
            this.impl.ImportParameters(new TinyCrypto.RSAParameters {
                D = parameters.D, DP = parameters.DP, DQ = parameters.DQ,
                Exponent = parameters.Exponent, InverseQ = parameters.InverseQ,
                Modulus = parameters.Modulus, P = parameters.P, Q = parameters.Q
            });
        }

        // ----- .NET-shape methods using HashAlgorithmName + RSASignaturePadding/RSAEncryptionPadding -----

        /// <summary>Encrypts data with the public key. Only PKCS#1 padding is supported.</summary>
        public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSAEncryptionPaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 encryption padding is supported.");
            return this.impl.Encrypt(data);
        }

        /// <summary>Decrypts data with the private key. Only PKCS#1 padding is supported.</summary>
        public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSAEncryptionPaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 encryption padding is supported.");
            return this.impl.Decrypt(data);
        }

        /// <summary>Signs data with the private key. Only PKCS#1 padding is supported.</summary>
        public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            return this.impl.SignData(data, hashAlgorithm.Equals(HashAlgorithmName.SHA256));
        }

        /// <summary>Signs a precomputed hash with the private key. Only PKCS#1 padding is supported.</summary>
        public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            var halg = hashAlgorithm.Equals(HashAlgorithmName.SHA256)
                ? TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha256
                : TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm.Sha1;
            return this.impl.SignHash(hash, halg);
        }

        /// <summary>Verifies a data signature against the public key. Only PKCS#1 padding is supported.</summary>
        public override bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (padding == null) throw new ArgumentNullException(nameof(padding));
            if (padding.Mode != RSASignaturePaddingMode.Pkcs1)
                throw new NotSupportedException("Only Pkcs1 signature padding is supported.");
            return this.impl.VerifyData(data, signature, hashAlgorithm.Equals(HashAlgorithmName.SHA256));
        }

        /// <summary>Verifies a hash signature against the public key. Only PKCS#1 padding is supported.</summary>
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

        /// <summary>Encrypts data with the public key (legacy overload). OAEP is not supported.</summary>
        public byte[] Encrypt(byte[] rgb, bool fOAEP) {
            if (fOAEP) throw new NotSupportedException("OAEP not supported.");
            return this.impl.Encrypt(rgb);
        }

        /// <summary>Decrypts data with the private key (legacy overload). OAEP is not supported.</summary>
        public byte[] Decrypt(byte[] rgb, bool fOAEP) {
            if (fOAEP) throw new NotSupportedException("OAEP not supported.");
            return this.impl.Decrypt(rgb);
        }

        /// <summary>Signs data with the private key (legacy overload). Set <paramref name="sha256"/> to use SHA-256 instead of SHA-1.</summary>
        public byte[] SignData(byte[] buffer, bool sha256 = false) => this.impl.SignData(buffer, sha256);
        /// <summary>Verifies a data signature (legacy overload). Set <paramref name="sha256"/> to use SHA-256 instead of SHA-1.</summary>
        public bool VerifyData(byte[] buffer, byte[] signature, bool sha256 = false) => this.impl.VerifyData(buffer, signature, sha256);

        /// <summary>Releases the resources used by the provider.</summary>
        public override void Dispose() => this.impl.Dispose();
    }

    // ----- Random number generator (already shaped to .NET) -----

    /// <summary>Cryptographically secure RNG. Create via <see cref="Create()"/>; do not seed manually.</summary>
    public abstract class RandomNumberGenerator : IDisposable {
        /// <summary>Creates a new secure random number generator.</summary>
        public static RandomNumberGenerator Create() => new TinyClrRandomNumberGenerator();

        /// <summary>Fills the buffer with cryptographically strong random bytes.</summary>
        public static void Fill(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            using (var rng = Create())
                rng.GetBytes(data);
        }

        /// <summary>Fills the buffer with cryptographically strong random bytes.</summary>
        public abstract void GetBytes(byte[] data);

        /// <summary>Releases the resources used by the generator.</summary>
        public abstract void Dispose();
    }

    // ===== AES (symmetric) =====
    //
    // Shape matches .NET BCL: CipherMode / PaddingMode enums, KeySizes, ICryptoTransform,
    // SymmetricAlgorithm abstract, Aes abstract, AesCryptoServiceProvider concrete.
    // Backed by GHIElectronics.TinyCLR.Cryptography.AesCryptoServiceProvider (mbedTLS AES).
    //
    // Supported subset: CBC + ECB; None / PKCS7 / Zeros padding; 128/192/256-bit keys.
    // OFB, CFB, CTS, ANSIX923, ISO10126 throw NotSupportedException from CreateEncryptor/Decryptor —
    // matches .NET's behavior that the algorithm can reject modes it doesn't implement.

    /// <summary>Block-cipher mode. Supported: <see cref="CBC"/>, <see cref="ECB"/>. Other values throw at use.</summary>
    public enum CipherMode {
        /// <summary>Cipher Block Chaining.</summary>
        CBC = 1,
        /// <summary>Electronic Codebook.</summary>
        ECB = 2,
        /// <summary>Output Feedback (not supported).</summary>
        OFB = 3,
        /// <summary>Cipher Feedback (not supported).</summary>
        CFB = 4,
        /// <summary>Ciphertext Stealing (not supported).</summary>
        CTS = 5,
    }

    /// <summary>Padding scheme for block ciphers. Supported: <see cref="None"/>, <see cref="PKCS7"/>, <see cref="Zeros"/>.</summary>
    public enum PaddingMode {
        /// <summary>No padding; the input must already be a whole number of blocks.</summary>
        None = 1,
        /// <summary>PKCS#7 padding.</summary>
        PKCS7 = 2,
        /// <summary>Pad with zero bytes (ambiguous — decryptor cannot strip).</summary>
        Zeros = 3,
        /// <summary>ANSI X.923 padding (not supported).</summary>
        ANSIX923 = 4,
        /// <summary>ISO 10126 padding (not supported).</summary>
        ISO10126 = 5,
    }

    /// <summary>Describes an allowed key/block size range as Min/Max with a Skip step. Matches the .NET type.</summary>
    public sealed class KeySizes {
        /// <summary>Minimum valid size, in bits.</summary>
        public int MinSize { get; }
        /// <summary>Maximum valid size, in bits.</summary>
        public int MaxSize { get; }
        /// <summary>Step between valid sizes, in bits.</summary>
        public int SkipSize { get; }

        /// <summary>Creates a new range.</summary>
        public KeySizes(int minSize, int maxSize, int skipSize) {
            this.MinSize = minSize;
            this.MaxSize = maxSize;
            this.SkipSize = skipSize;
        }
    }

    /// <summary>Cryptographic transformation interface. Matches the .NET BCL interface.</summary>
    public interface ICryptoTransform : IDisposable {
        /// <summary>True if the transform can be reused after a final block.</summary>
        bool CanReuseTransform { get; }
        /// <summary>True if the transform can process more than one block in a single call.</summary>
        bool CanTransformMultipleBlocks { get; }
        /// <summary>Input block size, in bytes.</summary>
        int InputBlockSize { get; }
        /// <summary>Output block size, in bytes.</summary>
        int OutputBlockSize { get; }
        /// <summary>Transforms the given region of the input and writes the result to the output buffer.</summary>
        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);
        /// <summary>Transforms the final region of the input and returns the (possibly padded) result.</summary>
        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
    }

    /// <summary>Base class for symmetric algorithms (AES on this platform). Matches the .NET BCL abstract class.</summary>
    public abstract class SymmetricAlgorithm : IDisposable {
        /// <summary>Block size, in bits.</summary>
        protected int BlockSizeValue;
        /// <summary>Feedback size, in bits.</summary>
        protected int FeedbackSizeValue;
        /// <summary>Initialization vector.</summary>
        protected byte[] IVValue;
        /// <summary>Key size, in bits.</summary>
        protected int KeySizeValue;
        /// <summary>Secret key bytes.</summary>
        protected byte[] KeyValue;
        /// <summary>Allowed block-size ranges.</summary>
        protected KeySizes[] LegalBlockSizesValue;
        /// <summary>Allowed key-size ranges.</summary>
        protected KeySizes[] LegalKeySizesValue;
        /// <summary>Cipher mode.</summary>
        protected CipherMode ModeValue;
        /// <summary>Padding mode.</summary>
        protected PaddingMode PaddingValue;

        /// <summary>Block size, in bits.</summary>
        public virtual int BlockSize {
            get => this.BlockSizeValue;
            set {
                if (!ValidSize(this.LegalBlockSizesValue, value)) throw new CryptographicException("Invalid block size.");
                this.BlockSizeValue = value;
            }
        }

        /// <summary>Feedback size, in bits (unused on this platform).</summary>
        public virtual int FeedbackSize {
            get => this.FeedbackSizeValue;
            set => this.FeedbackSizeValue = value;
        }

        /// <summary>Initialization vector. Auto-generated (random) on first read if not set.</summary>
        public virtual byte[] IV {
            get {
                if (this.IVValue == null) this.GenerateIV();
                return (byte[])this.IVValue.Clone();
            }
            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Length * 8 != this.BlockSizeValue) throw new CryptographicException("Invalid IV size.");
                this.IVValue = (byte[])value.Clone();
            }
        }

        /// <summary>Secret key. Auto-generated (random) on first read if not set.</summary>
        public virtual byte[] Key {
            get {
                if (this.KeyValue == null) this.GenerateKey();
                return (byte[])this.KeyValue.Clone();
            }
            set {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (!ValidSize(this.LegalKeySizesValue, value.Length * 8)) throw new CryptographicException("Invalid key size.");
                this.KeyValue = (byte[])value.Clone();
                this.KeySizeValue = value.Length * 8;
            }
        }

        /// <summary>Key size, in bits.</summary>
        public virtual int KeySize {
            get => this.KeySizeValue;
            set {
                if (!ValidSize(this.LegalKeySizesValue, value)) throw new CryptographicException("Invalid key size.");
                this.KeySizeValue = value;
                this.KeyValue = null;
            }
        }

        /// <summary>Allowed block-size ranges, in bits.</summary>
        public virtual KeySizes[] LegalBlockSizes => (KeySizes[])this.LegalBlockSizesValue.Clone();
        /// <summary>Allowed key-size ranges, in bits.</summary>
        public virtual KeySizes[] LegalKeySizes => (KeySizes[])this.LegalKeySizesValue.Clone();

        /// <summary>Cipher mode.</summary>
        public virtual CipherMode Mode {
            get => this.ModeValue;
            set => this.ModeValue = value;
        }

        /// <summary>Padding mode.</summary>
        public virtual PaddingMode Padding {
            get => this.PaddingValue;
            set => this.PaddingValue = value;
        }

        /// <summary>Creates an encryptor using the current <see cref="Key"/> and <see cref="IV"/>.</summary>
        public virtual ICryptoTransform CreateEncryptor() => this.CreateEncryptor(this.Key, this.IV);
        /// <summary>Creates a decryptor using the current <see cref="Key"/> and <see cref="IV"/>.</summary>
        public virtual ICryptoTransform CreateDecryptor() => this.CreateDecryptor(this.Key, this.IV);
        /// <summary>Creates an encryptor using the given key and IV.</summary>
        public abstract ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV);
        /// <summary>Creates a decryptor using the given key and IV.</summary>
        public abstract ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV);
        /// <summary>Generates a random initialization vector.</summary>
        public abstract void GenerateIV();
        /// <summary>Generates a random key.</summary>
        public abstract void GenerateKey();

        /// <summary>Returns true if the given size matches one of the given ranges.</summary>
        public bool ValidKeySize(int bitLength) => ValidSize(this.LegalKeySizesValue, bitLength);

        internal static bool ValidSize(KeySizes[] sizes, int bitLength) {
            if (sizes == null) return false;
            for (var i = 0; i < sizes.Length; i++) {
                var s = sizes[i];
                if (s.SkipSize == 0) {
                    if (s.MinSize == bitLength) return true;
                }
                else {
                    for (var size = s.MinSize; size <= s.MaxSize; size += s.SkipSize)
                        if (size == bitLength) return true;
                }
            }
            return false;
        }

        /// <summary>Releases the resources used by the algorithm.</summary>
        public virtual void Dispose() {
            if (this.KeyValue != null) Array.Clear(this.KeyValue, 0, this.KeyValue.Length);
            if (this.IVValue != null) Array.Clear(this.IVValue, 0, this.IVValue.Length);
            this.KeyValue = null;
            this.IVValue = null;
        }
        /// <summary>Releases the resources used by the algorithm.</summary>
        public void Clear() => this.Dispose();
    }

    /// <summary>Thrown for cryptographic errors (bad key/IV length, invalid padding, etc.). Matches the .NET type.</summary>
    [Serializable]
    public class CryptographicException : SystemException {
        /// <summary>Creates a new exception.</summary>
        public CryptographicException() { }
        /// <summary>Creates a new exception with a message.</summary>
        public CryptographicException(string message) : base(message) { }
        /// <summary>Creates a new exception with a message and inner exception.</summary>
        public CryptographicException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Abstract AES. Use <see cref="Create()"/> or construct <see cref="AesCryptoServiceProvider"/> directly.</summary>
    public abstract class Aes : SymmetricAlgorithm {
        /// <summary>Protected constructor initializing legal sizes (128-bit block, 128/192/256-bit key).</summary>
        protected Aes() {
            this.LegalKeySizesValue = new[] { new KeySizes(128, 256, 64) };
            this.LegalBlockSizesValue = new[] { new KeySizes(128, 128, 0) };
            this.KeySizeValue = 256;
            this.BlockSizeValue = 128;
            this.FeedbackSizeValue = 8;
            this.ModeValue = CipherMode.CBC;
            this.PaddingValue = PaddingMode.PKCS7;
        }

        /// <summary>Creates a new AES instance.</summary>
        public static Aes Create() => new AesCryptoServiceProvider();
    }

    /// <summary>Concrete AES backed by the firmware's mbedTLS AES implementation.</summary>
    public sealed class AesCryptoServiceProvider : Aes {
        /// <summary>Creates a new AES provider. Key and IV are generated on first access if not set.</summary>
        public AesCryptoServiceProvider() { }

        /// <summary>Generates a random initialization vector.</summary>
        public override void GenerateIV() {
            var iv = new byte[this.BlockSizeValue / 8];
            RandomNumberGenerator.Fill(iv);
            this.IVValue = iv;
        }

        /// <summary>Generates a random key.</summary>
        public override void GenerateKey() {
            var key = new byte[this.KeySizeValue / 8];
            RandomNumberGenerator.Fill(key);
            this.KeyValue = key;
        }

        /// <summary>Creates an encryptor using the given key and IV.</summary>
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) =>
            new AesTransform(rgbKey, rgbIV, this.ModeValue, this.PaddingValue, encrypt: true);

        /// <summary>Creates a decryptor using the given key and IV.</summary>
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) =>
            new AesTransform(rgbKey, rgbIV, this.ModeValue, this.PaddingValue, encrypt: false);
    }

    // AES transform — wraps the native bulk Encrypt/Decrypt. TransformBlock accumulates
    // bytes into an internal buffer; TransformFinalBlock calls native once on the whole
    // payload. This matches .NET semantics (final call produces ciphertext including padding)
    // and keeps interop round-trips to one per operation.
    internal sealed class AesTransform : ICryptoTransform {
        private readonly byte[] key;
        private readonly byte[] iv;
        private readonly TinyCrypto.AesCryptoServiceProvider.AesMode mode;
        private readonly TinyCrypto.AesCryptoServiceProvider.AesPadding padding;
        private readonly bool encrypt;
        private byte[] buffer;
        private int bufferLength;
        private bool disposed;

        public AesTransform(byte[] key, byte[] iv, CipherMode mode, PaddingMode padding, bool encrypt) {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new CryptographicException("Key must be 128, 192, or 256 bits.");

            switch (mode) {
                case CipherMode.CBC:
                    if (iv == null) throw new ArgumentNullException(nameof(iv));
                    if (iv.Length != 16) throw new CryptographicException("IV must be 128 bits.");
                    this.mode = TinyCrypto.AesCryptoServiceProvider.AesMode.Cbc;
                    this.iv = (byte[])iv.Clone();
                    break;
                case CipherMode.ECB:
                    this.mode = TinyCrypto.AesCryptoServiceProvider.AesMode.Ecb;
                    this.iv = null;
                    break;
                default:
                    throw new CryptographicException("Only CBC and ECB modes are supported.");
            }

            switch (padding) {
                case PaddingMode.None: this.padding = TinyCrypto.AesCryptoServiceProvider.AesPadding.None; break;
                case PaddingMode.PKCS7: this.padding = TinyCrypto.AesCryptoServiceProvider.AesPadding.Pkcs7; break;
                case PaddingMode.Zeros: this.padding = TinyCrypto.AesCryptoServiceProvider.AesPadding.Zeros; break;
                default: throw new CryptographicException("Only None, PKCS7, and Zeros padding are supported.");
            }

            this.key = (byte[])key.Clone();
            this.encrypt = encrypt;
            this.buffer = new byte[16];
            this.bufferLength = 0;
        }

        public bool CanReuseTransform => false;
        public bool CanTransformMultipleBlocks => true;
        public int InputBlockSize => 16;
        public int OutputBlockSize => 16;

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            if (this.disposed) throw new ObjectDisposedException(nameof(AesTransform));
            if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
            if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer));
            if (inputOffset < 0 || inputCount < 0 || inputOffset + inputCount > inputBuffer.Length)
                throw new ArgumentOutOfRangeException();

            this.Append(inputBuffer, inputOffset, inputCount);
            return 0;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            if (this.disposed) throw new ObjectDisposedException(nameof(AesTransform));
            if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
            if (inputOffset < 0 || inputCount < 0 || inputOffset + inputCount > inputBuffer.Length)
                throw new ArgumentOutOfRangeException();

            this.Append(inputBuffer, inputOffset, inputCount);

            using (var aes = new TinyCrypto.AesCryptoServiceProvider()) {
                return this.encrypt
                    ? aes.Encrypt(this.key, this.iv, this.buffer, 0, this.bufferLength, this.mode, this.padding)
                    : aes.Decrypt(this.key, this.iv, this.buffer, 0, this.bufferLength, this.mode, this.padding);
            }
        }

        private void Append(byte[] src, int offset, int count) {
            if (count == 0) return;
            var needed = this.bufferLength + count;
            if (needed > this.buffer.Length) {
                var grown = new byte[Math.Max(this.buffer.Length * 2, needed)];
                Array.Copy(this.buffer, 0, grown, 0, this.bufferLength);
                this.buffer = grown;
            }
            Array.Copy(src, offset, this.buffer, this.bufferLength, count);
            this.bufferLength += count;
        }

        public void Dispose() {
            if (this.disposed) return;
            if (this.key != null) Array.Clear(this.key, 0, this.key.Length);
            if (this.iv != null) Array.Clear(this.iv, 0, this.iv.Length);
            if (this.buffer != null) Array.Clear(this.buffer, 0, this.buffer.Length);
            this.disposed = true;
        }
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
