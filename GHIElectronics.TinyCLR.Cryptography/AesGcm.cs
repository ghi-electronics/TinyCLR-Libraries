using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Cryptography {
    // Low-level AES-GCM wrapper over the firmware's mbedTLS GCM implementation
    // (mbedtls_gcm_crypt_and_tag / mbedtls_gcm_auth_decrypt — already linked in for TLS).
    //
    // The BCL-compatible surface the user sees (System.Security.Cryptography.AesGcm)
    // lives in GHIElectronics.TinyCLR.System.Security.Cryptography and wraps this
    // class. Unlike AesCryptoServiceProvider (CBC/ECB), AesGcm has its own standalone
    // BCL class — no SymmetricAlgorithm / ICryptoTransform plumbing applies.
    //
    // Fixed tag size: 16 bytes. .NET BCL allows 12..16 but 16 covers TLS, JWE,
    // and essentially every real-world AEAD use. Dropping the configurability
    // simplifies the native + managed surface significantly.
    //
    // Fixed nonce size: 12 bytes. Also the de-facto standard (TLS / JWE / most
    // libraries). NIST allows other lengths but 12 is the recommended value.
    internal sealed class AesGcmController : IDisposable {

        public const int TagSize = 16;
        public const int NonceSize = 12;

        // Backing field initialized by native NativeAcquire.
        private IntPtr impl = IntPtr.Zero;

        public AesGcmController(byte[] key) {
            if (key == null) throw new ArgumentNullException();
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new ArgumentException();

            this.NativeAcquire(key);
            _ = this.impl;
        }

        public void Dispose() => this.NativeRelease();

        /// <summary>Encrypt and produce authentication tag.</summary>
        /// <param name="nonce">12 bytes.</param>
        /// <param name="plaintext">Raw data to encrypt (any length, including 0).</param>
        /// <param name="associatedData">Optional unencrypted-but-authenticated data (may be null).</param>
        /// <returns>Buffer of length = plaintext.Length + 16: ciphertext followed by 16-byte tag.</returns>
        public byte[] EncryptAndTag(byte[] nonce, byte[] plaintext, byte[] associatedData) {
            ValidateNonce(nonce);
            if (plaintext == null) throw new ArgumentNullException();

            return this.NativeEncryptAndTag(nonce, plaintext, associatedData ?? EmptyAad);
        }

        /// <summary>Verify tag and decrypt. Throws on tag mismatch (ciphertext was tampered).</summary>
        /// <param name="nonce">12 bytes.</param>
        /// <param name="ciphertextWithTag">Ciphertext followed by 16-byte tag. Must be ≥ 16 bytes.</param>
        /// <param name="associatedData">Optional unencrypted-but-authenticated data (may be null).</param>
        /// <returns>Plaintext (ciphertext length minus 16 tag bytes).</returns>
        public byte[] DecryptAndVerify(byte[] nonce, byte[] ciphertextWithTag, byte[] associatedData) {
            ValidateNonce(nonce);
            if (ciphertextWithTag == null) throw new ArgumentNullException();
            if (ciphertextWithTag.Length < TagSize) throw new ArgumentException();

            return this.NativeDecryptAndVerify(nonce, ciphertextWithTag, associatedData ?? EmptyAad);
        }

        private static readonly byte[] EmptyAad = new byte[0];

        private static void ValidateNonce(byte[] nonce) {
            if (nonce == null) throw new ArgumentNullException();
            if (nonce.Length != NonceSize) throw new ArgumentException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire(byte[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeEncryptAndTag(byte[] nonce, byte[] plaintext, byte[] associatedData);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeDecryptAndVerify(byte[] nonce, byte[] ciphertextWithTag, byte[] associatedData);
    }
}
