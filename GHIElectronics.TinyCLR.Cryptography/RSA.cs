using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Cryptography.CryptoServiceProvider;

namespace GHIElectronics.TinyCLR.Cryptography {
    internal class RSACryptoServiceProvider : IDisposable {

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
        public ICryptoServiceProvider Provider { get; }

        public RSACryptoServiceProvider() => this.Provider = new CryptoServiceApiWrapper(1024);
        public RSACryptoServiceProvider(int dwKeySize) => this.Provider = new CryptoServiceApiWrapper(dwKeySize);

        public void Dispose() => this.Provider.Dispose();

        public int KeySize => this.Provider.KeySize;

        public string KeyExchangeAlgorithm => this.Provider.KeyExchangeAlgorithm;

        public RSAParameters ExportParameters(bool includePrivateParameters) => this.Provider.ExportParameters(includePrivateParameters);

        public void ImportParameters(RSAParameters parameters) => this.Provider.ImportParameters(parameters);

        public byte[] Encrypt(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Encrypt(data, 0, data.Length, RSAMode.Public);
        }

        public byte[] Encrypt(byte[] data, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Public) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Encrypt(data, 0, data.Length, padding, mode);
        }

        public byte[] Decrypt(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Decrypt(data, 0, data.Length, RSAMode.Private);
        }

        public byte[] Decrypt(byte[] data, RSAEncryptionPaddingMode padding, RSAMode mode = RSAMode.Private) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Decrypt(data, 0, data.Length, padding, mode);
        }

        public byte[] SignData(byte[] data, bool sha256 = false) {
            if (data == null)
                throw new ArgumentNullException();

            return this.SignData(data, 0, data.Length, sha256);
        }

        public byte[] SignHash(byte[] hash, RSAHashAlgorithm hashAlgorithm, RSASignaturePaddingMode padding = RSASignaturePaddingMode.Pkcs1, RSAMode mode = RSAMode.Private) {
            ValidateHash(hash, hashAlgorithm);

            if (padding != RSASignaturePaddingMode.Pkcs1)
                throw CreateTodoNotSupportedException("RSA signature padding " + padding.ToString() + ".");

            return this.Provider.SignData(hash, 0, hash.Length, (int)mode, hashAlgorithm == RSAHashAlgorithm.Sha256);
        }

        public bool VerifyData(byte[] data, byte[] signedData, bool sha256 = false) {
            if (data == null)
                throw new ArgumentNullException();
            if (signedData == null)
                throw new ArgumentNullException();

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

        public byte[] SignData(byte[] data, int offset, int count, bool sha256 = false, RSAMode mode = RSAMode.Private ) {
            ValidateBuffer(data, offset, count);
            return this.Provider.SignData(data, offset, count, (int)mode, sha256);
        }

        public bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, bool sha256 = false, RSAMode mode = RSAMode.Public) {
            ValidateBuffer(data, offset, count);
            ValidateBuffer(signedData, signedDataOffset, signedDataLength);
            return this.Provider.VerifyData(data, offset, count, signedData, signedDataOffset, signedDataLength, (int)mode, sha256);
        }

        private static void ValidateBuffer(byte[] data, int offset, int count) {
            if (data == null)
                throw new ArgumentNullException();

            if (offset < 0 || count < 0 || offset + count > data.Length)
                throw new ArgumentOutOfRangeException();
        }

        private static void ValidateHash(byte[] hash, RSAHashAlgorithm hashAlgorithm) {
            ValidateBuffer(hash, 0, hash.Length);

            switch (hashAlgorithm) {
                case RSAHashAlgorithm.Sha1:
                    if (hash.Length != 20)
                        throw new ArgumentException("SHA1 hash must be 20 bytes.");
                    break;

                case RSAHashAlgorithm.Sha256:
                    if (hash.Length != 32)
                        throw new ArgumentException("SHA256 hash must be 32 bytes.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(hashAlgorithm));
            }
        }

        private static NotSupportedException CreateTodoNotSupportedException(string feature) =>
            new NotSupportedException("TODO-Not supported: " + feature);
    }

    namespace CryptoServiceProvider {
        internal interface ICryptoServiceProvider : IDisposable {
            int KeySize { get;  }

            string KeyExchangeAlgorithm { get; }

            RSAParameters ExportParameters(bool includePrivateParameters);

            void ImportParameters(RSAParameters parameters);

            byte[] Encrypt(byte[] data, int offset, int count, int mode);

            byte[] Decrypt(byte[] data, int offset, int count, int mode);

            byte[] SignData(byte[] data, int offset, int count, int mode, bool sha256);

            bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256);

        }

        internal sealed class CryptoServiceApiWrapper : ICryptoServiceProvider {

            private IntPtr impl = IntPtr.Zero;

            public int KeySize  { get;  }

            public string KeyExchangeAlgorithm => this.NativeKeyExchangeAlgorithm;

            public CryptoServiceApiWrapper(int dwKeySize) {
                this.KeySize = dwKeySize;

                this.NativeAcquire(dwKeySize);
                _ = this.impl; // Backing field is initialized by native side.
            }

            public void Dispose() => this.NativeRelase();            

            public RSAParameters ExportParameters(bool includePrivateParameters) => this.NativeExportParameters(includePrivateParameters);

            public void ImportParameters(RSAParameters parameters) => this.NativeImportParameters(parameters);

            public byte[] Encrypt(byte[] data, int offset, int count, int mode) => this.NativeEncrypt(data, offset, count, mode );

            public byte[] Decrypt(byte[] data,int offset, int count, int mode) => this.NativeDecrypt(data, offset, count, mode);

            public byte[] SignData(byte[] data, int offset, int count, int mode, bool sha256) => this.NativeSignData(data, offset, count, mode, sha256);

            public bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256) => this.NativeVerifyData(data, offset, count, signedData, signedDataOffset, signedDataLength, mode, sha256);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAcquire(int dwKeySize);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeRelase();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeEncrypt(byte[] data, int offset, int count, int mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeDecrypt(byte[] data, int offset, int count, int mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeSignData(byte[] data, int offset, int count, int mode, bool sha256);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern bool NativeVerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern RSAParameters NativeExportParameters(bool includePrivateParameters);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeImportParameters(RSAParameters parameters);

            public string NativeKeyExchangeAlgorithm { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        } 
		
    }
}
