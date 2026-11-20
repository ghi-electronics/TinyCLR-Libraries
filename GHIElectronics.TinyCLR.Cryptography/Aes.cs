using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Cryptography {
    // Low-level AES wrapper over the firmware's mbedTLS AES implementation
    // (mbedTLS AES is already linked in for TLS — this just exposes it).
    // The BCL-compatible surface the user sees (Aes, AesCryptoServiceProvider,
    // SymmetricAlgorithm, CipherMode, PaddingMode, ICryptoTransform) lives in
    // GHIElectronics.TinyCLR.System.Security.Cryptography and wraps this class.
    //
    // Bulk API (not block-by-block) — the native side loops with mbedtls_aes_crypt_cbc
    // / mbedtls_aes_crypt_ecb and handles padding in one call. Keeps managed code
    // simple and minimises interop round-trips.
    internal sealed class AesCryptoServiceProvider : IDisposable {

        public enum AesMode {
            Ecb = 0,
            Cbc = 1,
        }

        public enum AesPadding {
            None = 0,
            Pkcs7 = 1,
            Zeros = 2,
        }

        // Backing field initialized by native NativeAcquire.
        private IntPtr impl = IntPtr.Zero;

        public AesCryptoServiceProvider() {
            this.NativeAcquire();
            _ = this.impl;
        }

        public void Dispose() => this.NativeRelease();

        /// <summary>Encrypt the given slice. Key must be 16/24/32 bytes. IV must be 16 bytes (ignored for ECB).</summary>
        public byte[] Encrypt(byte[] key, byte[] iv, byte[] data, int offset, int count, AesMode mode, AesPadding padding) {
            ValidateArgs(key, iv, data, offset, count, mode);
            return this.NativeEncrypt(key, iv ?? EmptyIv, data, offset, count, (int)mode, (int)padding);
        }

        /// <summary>Decrypt the given slice. Returns plaintext with padding removed (if specified).</summary>
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] data, int offset, int count, AesMode mode, AesPadding padding) {
            ValidateArgs(key, iv, data, offset, count, mode);
            // Ciphertext length must be a whole number of blocks.
            if ((count & 0x0F) != 0) throw new ArgumentException();
            return this.NativeDecrypt(key, iv ?? EmptyIv, data, offset, count, (int)mode, (int)padding);
        }

        private static readonly byte[] EmptyIv = new byte[16];

        private static void ValidateArgs(byte[] key, byte[] iv, byte[] data, int offset, int count, AesMode mode) {
            if (key == null) throw new ArgumentNullException();
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) throw new ArgumentException();
            if (mode == AesMode.Cbc) {
                if (iv == null) throw new ArgumentNullException();
                if (iv.Length != 16) throw new ArgumentException();
            }
            if (data == null) throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset + count > data.Length) throw new ArgumentOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeEncrypt(byte[] key, byte[] iv, byte[] data, int offset, int count, int mode, int padding);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeDecrypt(byte[] key, byte[] iv, byte[] data, int offset, int count, int mode, int padding);
    }
}
