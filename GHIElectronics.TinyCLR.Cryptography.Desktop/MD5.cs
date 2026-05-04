using System;
using System.IO;
using BclMD5 = System.Security.Cryptography.MD5;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class MD5 : IDisposable {
        public Provider.IHashAlgorithmProvider Provider { get; }

        private MD5(Provider.IHashAlgorithmProvider provider) => this.Provider = provider;

        public static MD5 Create() => new MD5(new Provider.HashAlgorithmApiWrapper());

        public void Dispose() => this.Provider.Dispose();

        public int HashSize => this.Provider.HashSize;

        public byte[] Hash => this.Provider.Hash;

        public void Clear() => this.Provider.Clear();

        public void Initialize() => this.Provider.Clear();

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

        internal sealed class HashAlgorithmApiWrapper : IHashAlgorithmProvider {
            private BclMD5 inner = BclMD5.Create();
            private byte[] hashValue;

            public void Dispose() => this.inner?.Dispose();

            public int HashSize => 128;

            public byte[] Hash => this.hashValue;

            public void Clear() {
                this.inner?.Dispose();
                this.inner = BclMD5.Create();
                this.hashValue = null;
            }

            public byte[] ComputeHash(Stream inputStream) {
                if (inputStream == null)
                    throw new ArgumentNullException();

                this.hashValue = this.inner.ComputeHash(inputStream);
                return this.hashValue;
            }

            public byte[] ComputeHash(byte[] buffer, int offset, int count) {
                this.hashValue = this.inner.ComputeHash(buffer, offset, count);
                return this.hashValue;
            }
        }
    }
}
