using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Cryptography.Provider;

namespace GHIElectronics.TinyCLR.Cryptography {
    public sealed class MD5 : IDisposable {

        public HashAlgorithmProvider Provider { get; }

        private MD5(HashAlgorithmProvider provider) => this.Provider = provider;

        public static MD5 Create() => new MD5(new HashAlgorithmApiWrapper());

        public void Dispose() => this.Provider.Dispose();

        public int HashSize => this.Provider.HashSize;

        public byte[] Hash => this.Provider.Hash;

        public void Clear() => this.Provider.Clear();

        public byte[] ComputeHash(Stream stream) => this.Provider.ComputeHash(stream);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) {
            if (buffer == null)
                throw new ArgumentNullException();

            if (offset + count > buffer.Length || offset < 0)
                throw new ArgumentOutOfRangeException();

            return this.Provider.ComputeHash(buffer, offset, count);
        }

        public byte[] ComputeHash(byte[] buffer) => this.Provider.ComputeHash(buffer, 0, buffer.Length);
    }

    namespace Provider {
        public interface HashAlgorithmProvider : IDisposable {
            int HashSize { get; }
            byte[] Hash { get; }

            void Clear();

            byte[] ComputeHash(Stream inputStream);
           
            byte[] ComputeHash(byte[] buffer, int offset, int count);
        }

        public sealed class HashAlgorithmApiWrapper : HashAlgorithmProvider {
            private readonly IntPtr impl;            
            private byte[] hashValue;

            public HashAlgorithmApiWrapper() => this.Acquire();

            public void Dispose() => this.Release();

            public int HashSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public byte[] Hash => this.hashValue;

            public void Clear() {
                this.NativeClear();

                this.hashValue = null;                
            }

            public byte[] ComputeHash(Stream inputStream) {
                var value = this.NativeComputeHash(inputStream);

                this.hashValue = value;

                return value;
            }

            public byte[] ComputeHash(byte[] buffer, int offset, int count) {
                var value = this.NativeComputeHash(buffer, offset, count);

                this.hashValue = value;

                return value;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeClear();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern byte[] NativeComputeHash(Stream inputStream);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern byte[] NativeComputeHash(byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();
        }
    }
}
