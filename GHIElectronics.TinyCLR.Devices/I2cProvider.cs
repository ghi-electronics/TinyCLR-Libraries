using GHIElectronics.TinyCLR.Devices.Internal;
using System;

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

    /// <summary>
    /// Represents a communications channel to a device on an inter-integrated circuit (I²C) bus.
    /// </summary>
    internal sealed class DefaultI2cDeviceProvider : II2cDeviceProvider {
        // We need to share a single device between all instances, since it reserves the pins.
        private static object s_deviceLock = new object();
        private static int s_deviceRefs = 0;
        private static I2CDevice s_device = null;
        internal static string I2cPrefix => "I2C";

        private readonly string m_deviceId;

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private I2CDevice.Configuration m_configuration;

        /// <summary>
        /// Constructs a new I2cDevice object.
        /// </summary>
        /// <param name="slaveAddress">The bus address of the I²C device. Only 7-bit addressing is supported, so the
        ///     range of valid values is from 8 to 119.</param>
        /// <param name="busSpeed"></param>
        internal DefaultI2cDeviceProvider(string deviceId, I2cConnectionSettings settings) {
            this.m_deviceId = deviceId.Substring(0, deviceId.Length);

#pragma warning disable CS0219 // Variable is assigned but its value is never used
            var clockRateKhz = 100;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            if (settings.BusSpeed == I2cBusSpeed.FastMode) {
                clockRateKhz = 400;
            }

            this.m_configuration = new I2CDevice.Configuration((ushort)settings.SlaveAddress, clockRateKhz);

            lock (s_deviceLock) {
                if (s_device == null) {
                    s_device = new I2CDevice(this.m_configuration);
                }

                ++s_deviceRefs;
            }
        }

        ~DefaultI2cDeviceProvider() {
            Dispose(false);
        }

        /// <summary>
        /// Gets the plug and play device identifier of the inter-integrated circuit (I2C) bus controller for the device.
        /// </summary>
        /// <value>The plug and play device identifier of the inter-integrated circuit (I²C) bus controller for the
        ///     device.</value>
        public string DeviceId => this.m_deviceId.Substring(0, this.m_deviceId.Length);

        /// <summary>
        /// Writes data to the inter-integrated circuit (I²C) bus on which the device is connected, based on the bus
        /// address specified in the I2cConnectionSettings object that you used to create the I2cDevice object.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        public void Write(byte[] writeBuffer) => WritePartial(writeBuffer);

        /// <summary>
        /// Writes data to the inter-integrated circuit (I²C) bus on which the device is connected, and returns
        /// information about the success of the operation that you can use for error handling.
        /// </summary>
        /// <param name="buffer">A buffer that contains the data that you want to write to the I²C device. This data
        ///     should not include the bus address.</param>
        /// <returns>A structure that contains information about the success of the write operation and the actual
        ///     number of bytes that the operation wrote into the buffer.</returns>
        public ProviderI2cTransferResult WritePartial(byte[] buffer) {
            lock (this.m_syncLock) {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(buffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Reads data from the inter-integrated circuit (I²C) bus on which the device is connected into the specified
        /// buffer.
        /// </summary>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        public void Read(byte[] readBuffer) => ReadPartial(readBuffer);

        /// <summary>
        /// Reads data from the inter-integrated circuit (I²C) bus on which the device is connected into the specified
        /// buffer, and returns information about the success of the operation that you can use for error handling.
        /// </summary>
        /// <param name="buffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        /// <returns>A structure that contains information about the success of the read operation and the actual number
        ///     of bytes that the operation read into the buffer.</returns>
        public ProviderI2cTransferResult ReadPartial(byte[] buffer) {
            lock (this.m_syncLock) {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(buffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I²C) bus
        /// on which the device is connected, and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => WriteReadPartial(writeBuffer, readBuffer);

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I²C) bus
        /// on which the device is connected, and returns information about the success of the operation that you can
        /// use for error handling.
        /// </summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I²C device. This
        ///     data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I²C bus. The length of the
        ///     buffer determines how much data to request from the device.</param>
        /// <returns>A structure that contains information about whether both the read and write parts of the operation
        ///     succeeded and the sum of the actual number of bytes that the operation wrote and the actual number of
        ///     bytes that the operation read.</returns>
        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) {
            lock (this.m_syncLock) {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                var transactions = new I2CDevice.I2CTransaction[] {
                I2CDevice.CreateWriteTransaction(writeBuffer),
                I2CDevice.CreateReadTransaction(readBuffer) };
                return ExecuteTransactions(transactions);
            }
        }

        /// <summary>
        /// Closes the connection to the inter-integrated circuit (I2C) device.
        /// </summary>
        public void Dispose() {
            lock (this.m_syncLock) {
                if (!this.m_disposed) {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                    this.m_disposed = true;
                }
            }
        }

        internal static string[] GetValidBusNames() => new string[] {
                DefaultI2cDeviceProvider.I2cPrefix + "1",
            };

        /// <summary>
        /// Executes an arbitrary transaction against the wrapped Microsoft.SPOT.Hardware.I2CDevice.
        /// </summary>
        /// <param name="transactions">List of transactions to execute. These may be any combination of read and write.</param>
        /// <returns>A structure that contains information about whether both the read and write parts of the operation
        ///     succeeded and the sum of the actual number of bytes that the operation wrote and the actual number of
        ///     bytes that the operation read.</returns>
        private ProviderI2cTransferResult ExecuteTransactions(I2CDevice.I2CTransaction[] transactions) {
            // FUTURE: Investigate how short we can make this timeout. UWP APIs should take no
            // longer than 15ms, but this is insufficient for micro-devices.

            const int transactionTimeoutMs = 1000;

            uint bytesRequested = 0;
            foreach (var transaction in transactions) {
                bytesRequested += (uint)transaction.Buffer.Length;
            }

            ProviderI2cTransferResult result;

            lock (s_deviceLock) {
                s_device.Config = this.m_configuration;
                result.BytesTransferred = (uint)s_device.Execute(transactions, transactionTimeoutMs);
            }

            if (result.BytesTransferred == bytesRequested) {
                result.Status = ProviderI2cTransferStatus.FullTransfer;
            }
            else if (result.BytesTransferred == 0) {
                result.Status = ProviderI2cTransferStatus.SlaveAddressNotAcknowledged;
            }
            else {
                result.Status = ProviderI2cTransferStatus.PartialTransfer;
            }

            return result;
        }

        /// <summary>
        /// Releases internal resources held by the device.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                lock (s_deviceLock) {
                    --s_deviceRefs;
                    if ((s_deviceRefs == 0) && (s_device != null)) {
                        s_device.Dispose();
                        s_device = null;
                    }
                }
            }
        }
    }
}
