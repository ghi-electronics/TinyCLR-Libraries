using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Spi.Provider {
    // warning CS0414: The field 'Windows.Devices.Spi.SpiDevice.xxx' is assigned but its value is never used
    //                 - These are all used in native code methods.
#pragma warning disable 0414

    public enum ProviderSpiMode {
        Mode0,
        Mode1,
        Mode2,
        Mode3
    }

    public enum ProviderSpiSharingMode {
        Exclusive,
        Shared
    }

    public sealed class ProviderSpiConnectionSettings {
        internal ProviderSpiConnectionSettings(SpiConnectionSettings source) {
            this.ChipSelectionLine = source.ChipSelectionLine;
            this.DataBitLength = source.DataBitLength;
            this.ClockFrequency = source.ClockFrequency;
            this.Mode = (ProviderSpiMode)source.Mode;
            this.SharingMode = (ProviderSpiSharingMode)source.SharingMode;
        }

        public int ChipSelectionLine { get; set; }
        public ProviderSpiMode Mode { get; set; }
        public int DataBitLength { get; set; }
        public int ClockFrequency { get; set; }
        public ProviderSpiSharingMode SharingMode { get; set; }
    }

    public interface ISpiProvider {
        ISpiControllerProvider[] GetControllers();
    }

    public interface ISpiControllerProvider {
        ISpiDeviceProvider GetDeviceProvider(ProviderSpiConnectionSettings settings);
    }

    public interface ISpiDeviceProvider : IDisposable {
        string DeviceId { get; }
        ProviderSpiConnectionSettings ConnectionSettings { get; }

        void Read(byte[] buffer);
        void Write(byte[] buffer);
        void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer);
        void TransferSequential(byte[] writeBuffer, byte[] readBuffer);

        void Read(byte[] buffer, int offset, int length);
        void Write(byte[] buffer, int offset, int length);
        void TransferFullDuplex(byte[] writeBuffer, int writeOffset, byte[] readBuffer, int readOffset, int length);
        void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength);
    }

    public class SpiProvider : ISpiProvider {
        private ISpiControllerProvider[] controllers;
        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public ISpiControllerProvider[] GetControllers() => this.controllers;

        private SpiProvider(string name) {
            this.Name = name;

            var api = Api.Find(this.Name, ApiType.SpiProvider);

            this.controllers = new ISpiControllerProvider[DefaultSpiControllerProvider.GetControllerCount(api.Implementation)];

            for (var i = 0; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultSpiControllerProvider(api.Implementation, i);
        }

        public static ISpiProvider FromId(string id) {
            if (SpiProvider.providers.Contains(id))
                return (ISpiProvider)SpiProvider.providers[id];

            var res = new SpiProvider(id);

            SpiProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultSpiControllerProvider : ISpiControllerProvider {
        private readonly IntPtr nativeProvider;
        private int created;
        private bool isExclusive;
        private int idx;

        internal DefaultSpiControllerProvider(IntPtr nativeProvider, int idx) {
            this.nativeProvider = nativeProvider;
            this.created = 0;
            this.isExclusive = false;
            this.idx = idx;
        }

        public void Release(DefaultSpiDeviceProvider provider) {
            if (--this.created == 0)
                this.ReleaseNative();
        }

        public ISpiDeviceProvider GetDeviceProvider(ProviderSpiConnectionSettings settings) {
            if (settings.SharingMode == ProviderSpiSharingMode.Exclusive && this.created > 0) throw new InvalidOperationException("Sharing conflict.");
            if (settings.SharingMode == ProviderSpiSharingMode.Shared && this.isExclusive) throw new InvalidOperationException("Sharing conflict.");

            this.isExclusive = settings.SharingMode == ProviderSpiSharingMode.Exclusive;

            if (this.created++ == 0)
                this.AcquireNative();

            return new DefaultSpiDeviceProvider(this, this.nativeProvider, settings);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void AcquireNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void ReleaseNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern static internal int GetControllerCount(IntPtr nativeProvider);
    }

    internal sealed class DefaultSpiDeviceProvider : ISpiDeviceProvider {
        private readonly IntPtr nativeProvider;
        private readonly DefaultSpiControllerProvider parent;

        private readonly SpiConnectionSettings m_settings;

        private bool m_disposed = false;
        private int m_mskPin = -1;
        private int m_misoPin = -1;
        private int m_mosiPin = -1;
        private int m_spiBus = -1;

        /// <summary>
        /// Initializes a new instance of SpiDevice.
        /// </summary>
        /// <param name="deviceId">The unique name of the device.</param>
        /// <param name="settings">Settings to open the device with.</param>
        internal DefaultSpiDeviceProvider(DefaultSpiControllerProvider parent, IntPtr nativeProvider, ProviderSpiConnectionSettings settings) {
            this.nativeProvider = nativeProvider;
            this.parent = parent;
            this.m_settings = new SpiConnectionSettings(settings);
        }

        ~DefaultSpiDeviceProvider() {
            Dispose(false);
        }

        /// <summary>
        /// Gets the unique ID associated with the device.
        /// </summary>
        /// <value>The ID.</value>
        public string DeviceId {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return "";
            }
        }

        /// <summary>
        /// Gets the connection settings for the device.
        /// </summary>
        /// <value>The connection settings.</value>
        public ProviderSpiConnectionSettings ConnectionSettings {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                // We must return a copy so the caller can't accidentally mutate our internal settings.
                return new ProviderSpiConnectionSettings(this.m_settings);
            }
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
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer != null ? buffer.Length : 0);
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, readBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0);
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);

        public void Read(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.ReadInternal(buffer, offset, length);
        }

        public void Write(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.WriteInternal(buffer, offset, length);
        }

        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, byte[] readBuffer, int readOffset, int length) {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeBuffer.Length < writeOffset + length) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (readBuffer.Length < readOffset + length) throw new ArgumentException(nameof(readBuffer));

            this.TransferFullDuplexInternal(writeBuffer, writeOffset, readBuffer, readOffset, length);
        }

        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeLength < 0) throw new ArgumentOutOfRangeException(nameof(writeLength));
            if (writeBuffer.Length < writeOffset + writeLength) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(readOffset));
            if (readLength < 0) throw new ArgumentOutOfRangeException(nameof(readLength));
            if (readBuffer.Length < readOffset + readLength) throw new ArgumentException(nameof(readBuffer));

            this.TransferSequentialInternal(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);
        }


        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void ReadInternal(byte[] buffer, int offset, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void WriteInternal(byte[] buffer, int offset, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void TransferFullDuplexInternal(byte[] writeBuffer, int writeOffset, byte[] readBuffer, int readOffset, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void TransferSequentialInternal(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength);
    }
}
