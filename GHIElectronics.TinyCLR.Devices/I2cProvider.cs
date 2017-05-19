using System;
using System.Runtime.CompilerServices;

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
        II2cControllerProvider[] GetControllers();
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
    }

    public class I2cProvider : II2cProvider {
        private II2cControllerProvider[] controllers;

        public string Name { get; }

        public II2cControllerProvider[] GetControllers() => this.controllers;

        private I2cProvider(string name) {
            this.Name = name;
            this.controllers = new II2cControllerProvider[DefaultI2cControllerProvider.GetControllerCount(name)];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultI2cControllerProvider(name, i);
        }

        public static II2cProvider FromId(string id) => new I2cProvider(id);
    }

    internal class DefaultI2cControllerProvider : II2cControllerProvider {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private IntPtr nativeProvider;
#pragma warning restore CS0649
#pragma warning restore CS0169

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint GetControllerCount(string providerName);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern DefaultI2cControllerProvider(string name, uint index);

        public II2cDeviceProvider GetDeviceProvider(ProviderI2cConnectionSettings settings) => new DefaultI2cDeviceProvider(this.nativeProvider, settings);
    }

    internal sealed class DefaultI2cDeviceProvider : II2cDeviceProvider {
#pragma warning disable CS0169
        private IntPtr nativeProvider;
#pragma warning restore CS0169

        private bool m_disposed = false;
        private I2cConnectionSettings m_settings;

        public string DeviceId => "";

        internal DefaultI2cDeviceProvider(IntPtr nativeProvider, ProviderI2cConnectionSettings settings) {
            this.nativeProvider = nativeProvider;
            this.m_settings = new I2cConnectionSettings(settings);

            InitNative();
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
                DisposeNative();
            }
        }


        public void Read(byte[] buffer) => this.ReadPartial(buffer);
        public void Write(byte[] buffer) => this.WritePartial(buffer);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, readBuffer);

        public ProviderI2cTransferResult ReadPartial(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.ReadInternal(buffer, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        public ProviderI2cTransferResult WritePartial(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.WriteInternal(buffer, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            this.WriteReadInternal(writeBuffer, readBuffer, out var transferred, out var status);

            return new ProviderI2cTransferResult { BytesTransferred = transferred, Status = status };
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void InitNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void DisposeNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ReadInternal(byte[] buffer, out uint transferred, out ProviderI2cTransferStatus status);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void WriteInternal(byte[] buffer, out uint transferred, out ProviderI2cTransferStatus status);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void WriteReadInternal(byte[] writeBuffer, byte[] readBuffer, out uint transferred, out ProviderI2cTransferStatus status);
    }
}
