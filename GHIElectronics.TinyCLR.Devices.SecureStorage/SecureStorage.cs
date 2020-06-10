using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.SecureStorage
{
    public enum SecureStorage {
        Configuration = 0,
        Otp = 1
    }

    public class SecureStorageController
    {
        private SecureStorage secureStorage;

        public SecureStorageController(SecureStorage secureStorage) => this.secureStorage = secureStorage;

        public uint BlockSize => this.NativeGetBlockSize(this.secureStorage);
        public uint TotalSize => this.NativeGetTotalSize(this.secureStorage);

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

        public void Erase() {
            if (this.secureStorage == SecureStorage.Otp)
                throw new ArgumentException("Otp does not support erase.");

            this.NativeErase(this.secureStorage);
        }

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
