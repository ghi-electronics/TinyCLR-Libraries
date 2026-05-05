using System;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.SecureStorage\SecureStorage.cs.
// Backed by an in-memory byte[] on Desktop so Read/Write round-trip during a
// single process; persistence isn't simulated. Erase fills with 0xFF.
namespace GHIElectronics.TinyCLR.Devices.SecureStorage {
    public enum SecureStorage {
        Configuration = 0,
        Otp = 1
    }

    public class SecureStorageController {
        private const int DefaultBlockSize = 4096;
        private const int DefaultTotalSize = 64 * 1024;

        private readonly SecureStorage secureStorage;
        private readonly byte[] backing;

        public SecureStorageController(SecureStorage secureStorage) {
            this.secureStorage = secureStorage;
            this.backing = new byte[DefaultTotalSize];
            for (var i = 0; i < this.backing.Length; i++) this.backing[i] = 0xFF;
        }

        public uint BlockSize => DefaultBlockSize;
        public uint TotalSize => DefaultTotalSize;

        public int Write(uint blockIndex, byte[] data) {
            if (data == null) throw new ArgumentNullException();
            if (data.Length != this.BlockSize) throw new ArgumentException(string.Format("Array size must be {0} bytes.", this.BlockSize));
            if (this.BlockSize * blockIndex + data.Length > this.TotalSize) throw new IndexOutOfRangeException();

            Array.Copy(data, 0, this.backing, (int)(blockIndex * this.BlockSize), data.Length);
            return data.Length;
        }

        public int Read(uint blockIndex, byte[] data) {
            if (data == null) throw new ArgumentNullException();
            if (data.Length != this.BlockSize) throw new ArgumentException(string.Format("Array size must be {0} bytes.", this.BlockSize));
            if (this.BlockSize * blockIndex + data.Length > this.TotalSize) throw new IndexOutOfRangeException();

            Array.Copy(this.backing, (int)(blockIndex * this.BlockSize), data, 0, data.Length);
            return data.Length;
        }

        public void Erase() {
            if (this.secureStorage == SecureStorage.Otp)
                throw new ArgumentException("Otp does not support erase.");

            for (var i = 0; i < this.backing.Length; i++) this.backing[i] = 0xFF;
        }

        public bool IsBlank(uint blockIndex) {
            var data = new byte[this.BlockSize];
            this.Read(blockIndex, data);

            for (var i = 0; i < data.Length; i++) {
                if (data[i] != 0xFF) return false;
            }

            return true;
        }
    }
}
