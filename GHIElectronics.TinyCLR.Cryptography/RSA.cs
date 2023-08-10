using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Cryptography.CryptoServiceProvider;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class RSACryptoServiceProvider : IDisposable {

        public enum RSAMode {
            Public = 0,
            Private = 1,
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
        public byte[] Decrypt(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Decrypt(data, 0, data.Length, RSAMode.Private);
        }

        public byte[] SignData(byte[] data, bool sha256 = false) {
            if (data == null)
                throw new ArgumentNullException();

            return this.SignData(data, 0, data.Length, sha256);
        }
        public bool VerifyData(byte[] data, byte[] signedData, bool sha256 = false) {
            if (data == null)
                throw new ArgumentNullException();

            return this.VerifyData(data, 0, data.Length, signedData, 0, signedData.Length, sha256);
        }

        public byte[] Encrypt(byte[] data, int offset, int count, RSAMode mode = RSAMode.Public) => this.Provider.Encrypt(data, offset, count, (int)mode);

        public byte[] Decrypt(byte[] data, int offset, int count, RSAMode mode = RSAMode.Private) => this.Provider.Decrypt(data, offset, count, (int)mode);

        public byte[] SignData(byte[] data, int offset, int count, bool sha256 = false, RSAMode mode = RSAMode.Private ) => this.Provider.SignData(data, offset, count, (int)mode, sha256);

        public bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, bool sha256 = false, RSAMode mode = RSAMode.Public) => this.Provider.VerifyData(data, offset, count, signedData, signedDataOffset, signedDataLength, (int)mode, sha256);
    }

    namespace CryptoServiceProvider {
        public interface ICryptoServiceProvider : IDisposable {
            int KeySize { get;  }

            string KeyExchangeAlgorithm { get; }

            RSAParameters ExportParameters(bool includePrivateParameters);

            void ImportParameters(RSAParameters parameters);

            byte[] Encrypt(byte[] data, int offset, int count, int mode);

            byte[] Decrypt(byte[] data, int offset, int count, int mode);

            byte[] SignData(byte[] data, int offset, int count, int mode, bool sha256);

            bool VerifyData(byte[] data, int offset, int count, byte[] signedData, int signedDataOffset, int signedDataLength, int mode, bool sha256);

        }

        public sealed class CryptoServiceApiWrapper : ICryptoServiceProvider {

            private IntPtr impl;

            public int KeySize  { get;  }

            public string KeyExchangeAlgorithm => this.NativeKeyExchangeAlgorithm;

            public CryptoServiceApiWrapper(int dwKeySize) {
                this.KeySize = dwKeySize;

                this.NativeAcquire(dwKeySize);
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
