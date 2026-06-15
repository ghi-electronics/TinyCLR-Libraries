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
