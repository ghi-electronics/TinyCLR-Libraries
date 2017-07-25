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
    }

    public class SpiProvider : ISpiProvider {
        private ISpiControllerProvider[] controllers;
        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public ISpiControllerProvider[] GetControllers() => this.controllers;

        private SpiProvider(string name) {
            var api = Api.Find(name, ApiType.SpiProvider);

            this.Name = name;
            this.controllers = new ISpiControllerProvider[api.Count];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultSpiControllerProvider(api.Implementation[i]);
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

        internal DefaultSpiControllerProvider(IntPtr nativeProvider) {
            this.nativeProvider = nativeProvider;
            this.created = 0;
            this.isExclusive = false;
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

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(byte[] buffer) {
            if (buffer == null) {
                throw new ArgumentException();
            }

            WriteInternal(buffer);
        }

        /// <summary>
        /// Reads from the connected device.
        /// </summary>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void Read(byte[] buffer) {
            if (buffer == null) {
                throw new ArgumentException();
            }

            ReadInternal(buffer);
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) {
            if ((writeBuffer == null) || (readBuffer == null)) {
                throw new ArgumentException();
            }

            TransferSequentialInternal(writeBuffer, readBuffer);
        }

        /// <summary>
        /// Transfer data using a full duplex communication system. Full duplex allows both the master and the slave to
        /// communicate simultaneously.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) {
            if ((writeBuffer == null) || (readBuffer == null)) {
                throw new ArgumentException();
            }

            TransferFullDuplexInternal(writeBuffer, readBuffer);
        }

        /// <summary>
        /// Closes the connection to the device.
        /// </summary>
        public void Dispose() {
            if (!this.m_disposed) {
                Dispose(true);
                GC.SuppressFinalize(this);
                this.m_disposed = true;
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void TransferFullDuplexInternal(byte[] writeBuffer, byte[] readBuffer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void TransferSequentialInternal(byte[] writeBuffer, byte[] readBuffer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void WriteInternal(byte[] buffer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void ReadInternal(byte[] buffer);

        /// <summary>
        /// Releases internal resources held by the device.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                this.parent.Release(this);
            }
        }
    }
}
