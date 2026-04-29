using System;
using TinyCrypto = GHIElectronics.TinyCLR.Cryptography;

namespace System.Security.Cryptography {
    public sealed class SHA1 : IDisposable {
        private readonly TinyCrypto.SHA1 impl;

        private SHA1() => this.impl = TinyCrypto.SHA1.Create();

        public byte[] Hash => this.impl.Hash;

        public static SHA1 Create() => new SHA1();

        public byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);

        public void Dispose() { }
    }

    public sealed class SHA256 : IDisposable {
        private readonly TinyCrypto.SHA256 impl;

        private SHA256() => this.impl = TinyCrypto.SHA256.Create();

        public byte[] Hash => this.impl.Hash;

        public static SHA256 Create() => new SHA256();

        public byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);

        public void Dispose() { }
    }

    public sealed class MD5 : IDisposable {
        private readonly TinyCrypto.MD5 impl;

        private MD5() => this.impl = TinyCrypto.MD5.Create();

        public byte[] Hash => this.impl.Hash;
        public int HashSize => this.impl.HashSize;

        public static MD5 Create() => new MD5();

        public byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);

        public void Dispose() => this.impl.Dispose();
    }

    public sealed class HMACSHA1 : IDisposable {
        private readonly TinyCrypto.HMACSHA1 impl;

        public HMACSHA1() => this.impl = new TinyCrypto.HMACSHA1();

        public HMACSHA1(byte[] key) => this.impl = new TinyCrypto.HMACSHA1(key);

        public byte[] Hash => this.impl.Hash;
        public byte[] Key => this.impl.Key;
        public string HashName => this.impl.HashName;

        public byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);

        public void Dispose() { }
    }

    public sealed class HMACSHA256 : IDisposable {
        private readonly TinyCrypto.HMACSHA256 impl;

        public HMACSHA256() => this.impl = new TinyCrypto.HMACSHA256();

        public HMACSHA256(byte[] key) => this.impl = new TinyCrypto.HMACSHA256(key);

        public byte[] Hash => this.impl.Hash;
        public byte[] Key => this.impl.Key;
        public string HashName => this.impl.HashName;

        public byte[] ComputeHash(byte[] buffer) => this.impl.ComputeHash(buffer);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) => this.impl.ComputeHash(buffer, offset, count);

        public void Dispose() { }
    }

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

    public sealed class RSACryptoServiceProvider : IDisposable {
        private readonly TinyCrypto.RSACryptoServiceProvider impl;

        public enum RSAHashAlgorithm {
            Sha1 = 0,
            Sha256 = 1
        }

        public enum RSASignaturePaddingMode {
            Pkcs1 = 0
        }

        public enum RSAEncryptionPaddingMode {
            Pkcs1 = 0,
            OaepSha1 = 1,
            OaepSha256 = 2
        }

        public RSACryptoServiceProvider() => this.impl = new TinyCrypto.RSACryptoServiceProvider();

        public RSACryptoServiceProvider(int dwKeySize) => this.impl = new TinyCrypto.RSACryptoServiceProvider(dwKeySize);

        public int KeySize => this.impl.KeySize;

        public string KeyExchangeAlgorithm => this.impl.KeyExchangeAlgorithm;

        public RSAParameters ExportParameters(bool includePrivateParameters) {
            var p = this.impl.ExportParameters(includePrivateParameters);

            return new RSAParameters {
                D = p.D,
                DP = p.DP,
                DQ = p.DQ,
                Exponent = p.Exponent,
                InverseQ = p.InverseQ,
                Modulus = p.Modulus,
                P = p.P,
                Q = p.Q
            };
        }

        public void ImportParameters(RSAParameters parameters) {
            this.impl.ImportParameters(new TinyCrypto.RSAParameters {
                D = parameters.D,
                DP = parameters.DP,
                DQ = parameters.DQ,
                Exponent = parameters.Exponent,
                InverseQ = parameters.InverseQ,
                Modulus = parameters.Modulus,
                P = parameters.P,
                Q = parameters.Q
            });
        }

        public byte[] Encrypt(byte[] data) => this.impl.Encrypt(data);

        public byte[] Encrypt(byte[] data, bool fOAEP) =>
            this.impl.Encrypt(data, fOAEP ? TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode.OaepSha1 : TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode.Pkcs1);

        public byte[] Encrypt(byte[] data, RSAEncryptionPaddingMode padding) =>
            this.impl.Encrypt(data, (TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode)padding);

        public byte[] Decrypt(byte[] data) => this.impl.Decrypt(data);

        public byte[] Decrypt(byte[] data, bool fOAEP) =>
            this.impl.Decrypt(data, fOAEP ? TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode.OaepSha1 : TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode.Pkcs1);

        public byte[] Decrypt(byte[] data, RSAEncryptionPaddingMode padding) =>
            this.impl.Decrypt(data, (TinyCrypto.RSACryptoServiceProvider.RSAEncryptionPaddingMode)padding);

        public byte[] SignData(byte[] data, bool sha256 = false) => this.impl.SignData(data, sha256);

        public bool VerifyData(byte[] data, byte[] signedData, bool sha256 = false) => this.impl.VerifyData(data, signedData, sha256);

        public byte[] SignHash(byte[] hash, RSAHashAlgorithm hashAlgorithm, RSASignaturePaddingMode padding = RSASignaturePaddingMode.Pkcs1) =>
            this.impl.SignHash(hash, (TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm)hashAlgorithm, (TinyCrypto.RSACryptoServiceProvider.RSASignaturePaddingMode)padding);

        public bool VerifyHash(byte[] hash, byte[] signature, RSAHashAlgorithm hashAlgorithm, RSASignaturePaddingMode padding = RSASignaturePaddingMode.Pkcs1) =>
            this.impl.VerifyHash(hash, signature, (TinyCrypto.RSACryptoServiceProvider.RSAHashAlgorithm)hashAlgorithm, (TinyCrypto.RSACryptoServiceProvider.RSASignaturePaddingMode)padding);

        public void Dispose() => this.impl.Dispose();
    }

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
