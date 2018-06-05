using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.I2c.Provider {
    public sealed class ProviderI2cConnectionSettings {
        internal ProviderI2cConnectionSettings(I2cConnectionSettings settings) {
            this.BusSpeed = (ProviderI2cBusSpeed)settings.BusSpeed;
            this.SharingMode = (ProviderI2cSharingMode)settings.SharingMode;
            this.SlaveAddress = settings.SlaveAddress;
        }

        public ProviderI2cBusSpeed BusSpeed { get; set; }
        public ProviderI2cSharingMode SharingMode { get; set; }
        public int SlaveAddress { get; set; }
    }

    public enum ProviderI2cBusSpeed {
        StandardMode,
        FastMode
    }

    public enum ProviderI2cSharingMode {
        Exclusive,
        Shared
    }

    public enum ProviderI2cTransferStatus {
        FullTransfer,
        PartialTransfer,
        SlaveAddressNotAcknowledged,
    }

    public struct ProviderI2cTransferResult {
        public ProviderI2cTransferStatus Status;
        public uint BytesTransferred;
    }

    public interface II2cProvider {
        II2cControllerProvider GetController(int idx);
    }

    public interface II2cControllerProvider {
        II2cDeviceProvider GetDeviceProvider(ProviderI2cConnectionSettings settings);
    }

    public interface II2cDeviceProvider : IDisposable {
        string DeviceId { get; }

        void Read(byte[] buffer);
        ProviderI2cTransferResult ReadPartial(byte[] buffer);
        void Write(byte[] buffer);
        ProviderI2cTransferResult WritePartial(byte[] buffer);
        void WriteRead(byte[] writeBuffer, byte[] readBuffer);
        ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);

        void Read(byte[] buffer, int offset, int length);
        ProviderI2cTransferResult ReadPartial(byte[] buffer, int offset, int length);
        void Write(byte[] buffer, int offset, int length);
        ProviderI2cTransferResult WritePartial(byte[] buffer, int offset, int length);
        void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength);
        ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength);
    }

    public class I2cProvider : II2cProvider {
        private II2cControllerProvider controller;
        private static Hashtable providers = new Hashtable();
        private int idx;

        public string Name { get; }

        public II2cControllerProvider GetController(int idx) {
            var api = Api.Find(this.Name, ApiType.I2cProvider);

            this.idx = idx;

            this.controller = new DefaultI2cControllerProvider(api.Implementation, idx);

            return this.controller;
        }

        private I2cProvider(string name) => this.Name = name;

        public static II2cProvider FromId(string id) {
            if (I2cProvider.providers.Contains(id))
                return (II2cProvider)I2cProvider.providers[id];

            var res = new I2cProvider(id);

            I2cProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultI2cControllerProvider : II2cControllerProvider {
        private readonly IntPtr nativeProvider;
        private int created;
        private bool isExclusive;
        private int idx;

        internal DefaultI2cControllerProvider(IntPtr nativeProvider, int idx) {
            this.nativeProvider = nativeProvider;
            this.created = 0;
            this.isExclusive = false;
            this.idx = idx;
        }

        public void Release(DefaultI2cDeviceProvider provider) {
            if (--this.created == 0)
                this.ReleaseNative();
        }

        public II2cDeviceProvider GetDeviceProvider(ProviderI2cConnectionSettings settings) {
            if (settings.SharingMode == ProviderI2cSharingMode.Exclusive && this.created > 0) throw new InvalidOperationException("Sharing conflict.");
            if (settings.SharingMode == ProviderI2cSharingMode.Shared && this.isExclusive) throw new InvalidOperationException("Sharing conflict.");

            this.isExclusive = settings.SharingMode == ProviderI2cSharingMode.Exclusive;

            if (this.created++ == 0)
                this.AcquireNative();

            return new DefaultI2cDeviceProvider(this, this.nativeProvider, settings);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void AcquireNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void ReleaseNative();
    }

    internal sealed class DefaultI2cDeviceProvider : II2cDeviceProvider {
        private readonly IntPtr nativeProvider;
        private readonly DefaultI2cControllerProvider parent;

        private bool m_disposed = false;
        private I2cConnectionSettings m_settings;

        public string DeviceId => "";

        internal DefaultI2cDeviceProvider(DefaultI2cControllerProvider parent, IntPtr nativeProvider, ProviderI2cConnectionSettings settings) {
            this.nativeProvider = nativeProvider;
            this.parent = parent;
            this.m_settings = new I2cConnectionSettings(settings);
        }

        ~DefaultI2cDeviceProvider() {
            Dispose(false);
        }

        public void Dispose() {
            if (!this.m_disposed) {
                Dispose(true);
                GC.SuppressFinalize(this);
                this.m_disposed = true;
            }
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                this.parent.Release(this);
            }
        }

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult ReadPartial(byte[] buffer) => this.ReadPartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult WritePartial(byte[] buffer) => this.WritePartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);
        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);

        public void Read(byte[] buffer, int offset, int length) => this.ReadPartial(buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WritePartial(buffer, offset, length);
        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteReadPartial(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public ProviderI2cTransferResult ReadPartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.ReadInternal(buffer, offset, length, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        public ProviderI2cTransferResult WritePartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.WriteInternal(buffer, offset, length, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeLength < 0) throw new ArgumentOutOfRangeException(nameof(writeLength));
            if (writeBuffer.Length < writeOffset + writeLength) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(readOffset));
            if (readLength < 0) throw new ArgumentOutOfRangeException(nameof(readLength));
            if (readBuffer.Length < readOffset + readLength) throw new ArgumentException(nameof(readBuffer));

            this.WriteReadInternal(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ReadInternal(byte[] buffer, int offset, int length, out uint transferred, out ProviderI2cTransferStatus status);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void WriteInternal(byte[] buffer, int offset, int length, out uint transferred, out ProviderI2cTransferStatus status);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void WriteReadInternal(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out uint transferred, out ProviderI2cTransferStatus status);
    }
}
