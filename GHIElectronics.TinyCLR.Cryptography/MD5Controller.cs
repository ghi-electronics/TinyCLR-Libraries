using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Cryptography.Provider;

namespace GHIElectronics.TinyCLR.Cryptography {
    public sealed class MD5 : IDisposable {

        public IHashAlgorithmProvider Provider { get; }

        private MD5(IHashAlgorithmProvider provider) => this.Provider = provider;

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
        public interface IHashAlgorithmProvider : IDisposable {
            int HashSize { get; }
            byte[] Hash { get; }

            void Clear();

            byte[] ComputeHash(Stream inputStream);

            byte[] ComputeHash(byte[] buffer, int offset, int count);
        }

        public sealed class HashAlgorithmApiWrapper : IHashAlgorithmProvider {
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
                if (inputStream == null)
                    throw new ArgumentNullException();

                const int BLOCK_SIZE = 64;

                var streamLength = inputStream.Length;
                var block = streamLength / BLOCK_SIZE;
                var remain = streamLength % BLOCK_SIZE;

                if (!this.NativeComputeStart())
                    throw new InvalidOperationException();

                var buffer = new byte[BLOCK_SIZE];

                while (block-- > 0) {

                    inputStream.Read(buffer, 0, BLOCK_SIZE);

                    if (!this.NativeComputeUpdate(buffer, 0, BLOCK_SIZE))
                        throw new InvalidOperationException();
                }

                if (remain > 0) {
                    inputStream.Read(buffer, 0, (int)remain);

                    if (!this.NativeComputeUpdate(buffer, 0, (int)remain))
                        throw new InvalidOperationException();
                }

                return this.NativeComputeFinish();
            }

            public byte[] ComputeHash(byte[] buffer, int offset, int count) {
                var value = this.NativeComputeHash(buffer, offset, count);

                this.hashValue = value;

                return value;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeClear();
            
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern byte[] NativeComputeHash(byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern bool NativeComputeStart();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern bool NativeComputeUpdate(byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeComputeFinish();
        }
    }
}
