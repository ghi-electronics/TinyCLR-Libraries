using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.SecureStorage
{
    /// <summary>Selects which on-chip non-volatile region the controller targets.</summary>
    public enum SecureStorage {
        /// <summary>Re-writable configuration region (erasable, can be updated freely).</summary>
        Configuration = 0,
        /// <summary>One-Time-Programmable region. Writes are permanent; <see cref="SecureStorageController.Erase"/> is not supported.</summary>
        Otp = 1
    }

    /// <summary>
    /// Reads and writes blocks in one of the chip's secure non-volatile regions.
    /// Pick the region in the constructor; subsequent calls operate on that region.
    /// </summary>
    public class SecureStorageController
    {
        private SecureStorage secureStorage;

        /// <summary>Opens a controller bound to the given region.</summary>
        /// <param name="secureStorage">Which region to operate on.</param>
        public SecureStorageController(SecureStorage secureStorage) => this.secureStorage = secureStorage;

        /// <summary>Block (write) granularity of the region, in bytes.</summary>
        public uint BlockSize => this.NativeGetBlockSize(this.secureStorage);
        /// <summary>Total size of the region, in bytes.</summary>
        public uint TotalSize => this.NativeGetTotalSize(this.secureStorage);

        /// <summary>Writes one block at the given block index.</summary>
        /// <param name="blockIndex">Index of the block to write (0-based).</param>
        /// <param name="data">Block contents. Length must equal <see cref="BlockSize"/>.</param>
        /// <returns>Bytes actually written.</returns>
        public int Write(uint blockIndex, byte[] data) {
            if (data == null) {
                throw new ArgumentNullException();
            }

            if (data.Length != this.BlockSize) {
                throw new ArgumentException(string.Format("Array size must be {0} bytes.", this.BlockSize));
            }

            if (this.BlockSize * blockIndex + data.Length > this.TotalSize) {
                throw new IndexOutOfRangeException();
            }

            return this.NativeWrite(this.secureStorage, blockIndex, data);
        }

        /// <summary>Reads one block at the given block index.</summary>
        /// <param name="blockIndex">Index of the block to read (0-based).</param>
        /// <param name="data">Destination buffer. Length must equal <see cref="BlockSize"/>.</param>
        /// <returns>Bytes actually read.</returns>
        public int Read(uint blockIndex, byte[] data) {
            if (data == null) {
                throw new ArgumentNullException();
            }

            if (data.Length != this.BlockSize) {
                throw new ArgumentException(string.Format("Array size must be {0} bytes.", this.BlockSize));
            }

            if (this.BlockSize * blockIndex + data.Length > this.TotalSize) {
                throw new IndexOutOfRangeException();
            }

            return this.NativeRead(this.secureStorage, blockIndex, data);
        }

        /// <summary>Erases the entire region. Not supported for OTP storage.</summary>
        public void Erase() {
            if (this.secureStorage == SecureStorage.Otp)
                throw new ArgumentException("Otp does not support erase.");

            this.NativeErase(this.secureStorage);
        }

        /// <summary>Returns true if the addressed block is in its erased (all-0xFF) state.</summary>
        public bool IsBlank(uint blockIndex) {

            var data = new byte[this.BlockSize];

            var read = this.NativeRead(this.secureStorage, blockIndex, data);

            for (var i = 0; i < data.Length; i++) {
                if (data[i] != 0xFF)
                    return false;
            }

            return read == this.BlockSize;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeWrite(SecureStorage type, uint blockIndex, byte[] data);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeRead(SecureStorage type, uint blockIndex, byte[] data);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeErase(SecureStorage type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeGetBlockSize(SecureStorage type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeGetTotalSize(SecureStorage type);
    }
}
