using System;
using BclRSA = System.Security.Cryptography.RSA;
using BclRSAParameters = System.Security.Cryptography.RSAParameters;
using BclHashAlgorithmName = System.Security.Cryptography.HashAlgorithmName;
using BclRSASignaturePadding = System.Security.Cryptography.RSASignaturePadding;
using BclRSAEncryptionPadding = System.Security.Cryptography.RSAEncryptionPadding;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class RSACryptoServiceProvider : IDisposable {

        public enum RSAMode {
            Public = 0,
            Private = 1,
        }

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

        public CryptoServiceProvider.ICryptoServiceProvider Provider { get; }

        public RSACryptoServiceProvider() => this.Provider = new CryptoServiceProvider.CryptoServiceApiWrapper(1024);
        public RSACryptoServiceProvider(int dwKeySize) => this.Provider = new CryptoServiceProvider.CryptoServiceApiWrapper(dwKeySize);

        public void Dispose() => this.Provider.Dispose();

        public int KeySize => this.Provider.KeySize;

        public string KeyExchangeAlgorithm => this.Provider.KeyExchangeAlgorithm;

        public RSAParameters ExportParameters(bool includePrivateParameters) => this.Provider.ExportParameters(includePrivateParameters);

        public void ImportParameters(RSAParameters parameters) => this.Provider.ImportParameters(parameters);

        public byte[] Encrypt(byte[] data) {
            if (data == null) throw new ArgumentNullException();
            return this.Encrypt(data, 0, data.Length, RSAMode.Public);
        }

        public byte[] Encrypt(byte[] data, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Public) {
            if (data == null) throw new ArgumentNullException();
            return this.Encrypt(data, 0, data.Length, padding, mode);
        }

        public byte[] Decrypt(byte[] data) {
            if (data == null) throw new ArgumentNullException();
            return this.Decrypt(data, 0, data.Length, RSAMode.Private);
        }

        public byte[] Decrypt(byte[] data, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Private) {
            if (data == null) throw new ArgumentNullException();
            return this.Decrypt(data, 0, data.Length, padding, mode);
        }

        public byte[] SignData(byte[] data, bool sha256 = false) {
            if (data == null) throw new ArgumentNullException();
            return this.SignData(data, 0, data.Length, sha256);
        }

        public byte[] SignHash(byte[] hash, RSAHashAlgorithm hashAlgorithm, RSASignaturePaddingMode padding = RSASignaturePaddingMode.Pkcs1, RSAMode mode = RSAMode.Private) {
            ValidateHash(hash, hashAlgorithm);
            if (padding != RSASignaturePaddingMode.Pkcs1)
                throw CreateTodoNotSupportedException("RSA signature padding " + padding.ToString() + ".");
            return this.Provider.SignData(hash, 0, hash.Length, (int)mode, hashAlgorithm == RSAHashAlgorithm.Sha256);
        }

        public bool VerifyData(byte[] data, byte[] signedData, bool sha256 = false) {
            if (data == null) throw new ArgumentNullException();
            if (signedData == null) throw new ArgumentNullException();
            return this.VerifyData(data, 0, data.Length, signedData, 0, signedData.Length, sha256);
        }

        public bool VerifyHash(byte[] hash, byte[] signature, RSAHashAlgorithm hashAlgorithm, RSASignaturePaddingMode padding = RSASignaturePaddingMode.Pkcs1, RSAMode mode = RSAMode.Public) {
            ValidateHash(hash, hashAlgorithm);
            ValidateBuffer(signature, 0, signature.Length);
            if (padding != RSASignaturePaddingMode.Pkcs1)
                throw CreateTodoNotSupportedException("RSA signature padding " + padding.ToString() + ".");
            return this.Provider.VerifyData(hash, 0, hash.Length, signature, 0, signature.Length, (int)mode, hashAlgorithm == RSAHashAlgorithm.Sha256);
        }

        public byte[] Encrypt(byte[] data, int offset, int count, RSAMode mode = RSAMode.Public) {
            ValidateBuffer(data, offset, count);
            return this.Provider.Encrypt(data, offset, count, (int)mode);
        }

        public byte[] Encrypt(byte[] data, int offset, int count, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Public) {
            if (padding != RSAEncryptionPaddingMode.Pkcs1)
                throw CreateTodoNotSupportedException("RSA encryption padding " + padding.ToString() + ".");
            return this.Encrypt(data, offset, count, mode);
        }

        public byte[] Decrypt(byte[] data, int offset, int count, RSAMode mode = RSAMode.Private) {
            ValidateBuffer(data, offset, count);
            return this.Provider.Decrypt(data, offset, count, (int)mode);
        }

        public byte[] Decrypt(byte[] data, int offset, int count, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Private) {
            if (padding != RSAEncryptionPaddingMode.Pkcs1)
                throw CreateTodoNotSupportedException("RSA encryption padding " + padding.ToString() + ".");
            return this.Decrypt(data, offset, count, mode);
        }

        public byte[] SignData(byte[] data, int offset, int count, bool sha256 = false, RSAMode mode = RSAMode.Private) {
            ValidateBuffer(data, offset, count);
            return this.Provider.SignData(data, offset, count, (int)mode, sha256);
        }

        public bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, bool sha256 = false, RSAMode mode = RSAMode.Public) {
            ValidateBuffer(data, offset, count);
            ValidateBuffer(signedData, signedDataOffset, signedDataLength);
            return this.Provider.VerifyData(data, offset, count, signedData, signedDataOffset, signedDataLength, (int)mode, sha256);
        }

        private static void ValidateBuffer(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset + count > data.Length)
                throw new ArgumentOutOfRangeException();
        }

        private static void ValidateHash(byte[] hash, RSAHashAlgorithm hashAlgorithm) {
            ValidateBuffer(hash, 0, hash.Length);
            switch (hashAlgorithm) {
                case RSAHashAlgorithm.Sha1:
                    if (hash.Length != 20) throw new ArgumentException("SHA1 hash must be 20 bytes.");
                    break;
                case RSAHashAlgorithm.Sha256:
                    if (hash.Length != 32) throw new ArgumentException("SHA256 hash must be 32 bytes.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgorithm));
            }
        }

        private static NotSupportedException CreateTodoNotSupportedException(string feature) =>
            new NotSupportedException("TODO-Not supported: " + feature);
    }

    namespace CryptoServiceProvider {
        public interface ICryptoServiceProvider : IDisposable {
            int KeySize { get; }

            string KeyExchangeAlgorithm { get; }

            RSAParameters ExportParameters(bool includePrivateParameters);

            void ImportParameters(RSAParameters parameters);

            byte[] Encrypt(byte[] data, int offset, int count, int mode);

            byte[] Decrypt(byte[] data, int offset, int count, int mode);

            byte[] SignData(byte[] data, int offset, int count, int mode, bool sha256);

            bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256);
        }

        internal sealed class CryptoServiceApiWrapper : ICryptoServiceProvider {
            private BclRSA inner;

            public int KeySize { get; }

            public string KeyExchangeAlgorithm => "RSA";

            public CryptoServiceApiWrapper(int dwKeySize) {
                this.KeySize = dwKeySize;
                this.inner = BclRSA.Create();
                this.inner.KeySize = dwKeySize;
            }

            public void Dispose() => this.inner?.Dispose();

            public RSAParameters ExportParameters(bool includePrivateParameters) {
                var bcl = this.inner.ExportParameters(includePrivateParameters);
                return new RSAParameters {
                    Modulus = bcl.Modulus,
                    Exponent = bcl.Exponent,
                    D = bcl.D,
                    P = bcl.P,
                    Q = bcl.Q,
                    DP = bcl.DP,
                    DQ = bcl.DQ,
                    InverseQ = bcl.InverseQ,
                };
            }

            public void ImportParameters(RSAParameters parameters) {
                var bcl = new BclRSAParameters {
                    Modulus = parameters.Modulus,
                    Exponent = parameters.Exponent,
                    D = parameters.D,
                    P = parameters.P,
                    Q = parameters.Q,
                    DP = parameters.DP,
                    DQ = parameters.DQ,
                    InverseQ = parameters.InverseQ,
                };
                this.inner.ImportParameters(bcl);
            }

            public byte[] Encrypt(byte[] data, int offset, int count, int mode) {
                var slice = SliceIfNeeded(data, offset, count);
                return this.inner.Encrypt(slice, BclRSAEncryptionPadding.Pkcs1);
            }

            public byte[] Decrypt(byte[] data, int offset, int count, int mode) {
                var slice = SliceIfNeeded(data, offset, count);
                return this.inner.Decrypt(slice, BclRSAEncryptionPadding.Pkcs1);
            }

            public byte[] SignData(byte[] data, int offset, int count, int mode, bool sha256) {
                var slice = SliceIfNeeded(data, offset, count);
                var hashName = sha256 ? BclHashAlgorithmName.SHA256 : BclHashAlgorithmName.SHA1;
                return this.inner.SignData(slice, hashName, BclRSASignaturePadding.Pkcs1);
            }

            public bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256) {
                var dataSlice = SliceIfNeeded(data, offset, count);
                var sigSlice = SliceIfNeeded(signedData, signedDataOffset, signedDataLength);
                var hashName = sha256 ? BclHashAlgorithmName.SHA256 : BclHashAlgorithmName.SHA1;
                return this.inner.VerifyData(dataSlice, sigSlice, hashName, BclRSASignaturePadding.Pkcs1);
            }

            private static byte[] SliceIfNeeded(byte[] data, int offset, int count) {
                if (offset == 0 && count == data.Length) return data;
                var slice = new byte[count];
                Array.Copy(data, offset, slice, 0, count);
                return slice;
            }
        }
    }
}
