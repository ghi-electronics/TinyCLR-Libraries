using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Cryptography.CryptoServiceProvider;

namespace GHIElectronics.TinyCLR.Cryptography {
    public sealed class RSACryptoServiceProvider : IDisposable {
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

            return this.Encrypt(data, 0, data.Length);
        }
        public byte[] Decrypt(byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            return this.Decrypt(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data, int offset, int count) => this.Provider.Encrypt(data, offset, count);

        public byte[] Decrypt(byte[] data, int offset, int count) => this.Provider.Decrypt(data, offset, count);
    }

    namespace CryptoServiceProvider {
        public interface ICryptoServiceProvider : IDisposable {
            int KeySize { get;  }

            string KeyExchangeAlgorithm { get; }

            RSAParameters ExportParameters(bool includePrivateParameters);

            void ImportParameters(RSAParameters parameters);

            byte[] Encrypt(byte[] data, int offset, int count);

            byte[] Decrypt(byte[] data, int offset, int count);

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

            public byte[] Encrypt(byte[] data, int offset, int count) => this.NativeEncrypt(data, offset, count);

            public byte[] Decrypt(byte[] data,int offset, int count) => this.NativeDecrypt(data, offset, count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAcquire(int dwKeySize);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeRelase();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeEncrypt(byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeDecrypt(byte[] data, int offset, int count);
            
            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern RSAParameters NativeExportParameters(bool includePrivateParameters);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeImportParameters(RSAParameters parameters);

            public string NativeKeyExchangeAlgorithm { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        }        
    }
}
