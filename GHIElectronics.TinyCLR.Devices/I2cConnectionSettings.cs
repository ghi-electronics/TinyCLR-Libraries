using GHIElectronics.TinyCLR.Devices.I2c.Provider;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    /// <summary>
    /// Represents the connection settings you want to use for an inter-integrated circuit (I²C) device.
    /// </summary>
    public sealed class I2cConnectionSettings {
        private int m_slaveAddress = 0;
        private I2cBusSpeed m_busSpeed = I2cBusSpeed.StandardMode;
        private I2cSharingMode m_sharingMode = I2cSharingMode.Exclusive;

        /// <summary>
        /// Creates and initializes a new instance of the I2cConnectionSettings class for inter-integrated
        /// circuit (I2C) device with specified bus address, using the default settings of the standard mode for the bus
        /// speed and exclusive sharing mode.
        /// </summary>
        /// <param name="slaveAddress">Initial address of the device.</param>
        public I2cConnectionSettings(int slaveAddress) => this.m_slaveAddress = slaveAddress;

        internal I2cConnectionSettings(ProviderI2cConnectionSettings settings) {
            this.BusSpeed = (I2cBusSpeed)settings.BusSpeed;
            this.SharingMode = (I2cSharingMode)settings.SharingMode;
            this.SlaveAddress = settings.SlaveAddress;
        }

        /// <summary>
        /// Construct a copy of an I2cConnectionSettings object.
        /// </summary>
        /// <param name="source">Source object to copy from.</param>
        internal I2cConnectionSettings(I2cConnectionSettings source) {
            this.m_slaveAddress = source.m_slaveAddress;
            this.m_busSpeed = source.m_busSpeed;
            this.m_sharingMode = source.m_sharingMode;
        }

        /// <summary>
        /// Gets or sets the bus address of the inter-integrated circuit (I²C) device.
        /// </summary>
        /// <value>The bus address of the I²C device. Only 7-bit addressing is supported, so the range of valid values
        ///     is from 8 to 119.</value>
        public int SlaveAddress {
            get => this.m_slaveAddress;

            set => this.m_slaveAddress = value;
        }

        /// <summary>
        /// Gets or sets the bus speed to use for connecting to an inter-integrated circuit (I²C) device. The bus speed
        /// is the frequency at which to clock the I²C bus when accessing the device.
        /// </summary>
        /// <value>The bus speed to use for connecting to an I²C device.</value>
        public I2cBusSpeed BusSpeed {
            get => this.m_busSpeed;

            set => this.m_busSpeed = value;
        }

        /// <summary>
        /// Gets or sets the sharing mode to use to connect to the inter-integrated circuit (I²C) bus address. This mode
        /// determines whether other connections to the I²C bus address can be opened while you are connect to the I²C
        /// bus address.
        /// </summary>
        /// <value>The sharing mode to use to connect to the I²C bus address.</value>
        public I2cSharingMode SharingMode {
            get => this.m_sharingMode;

            set => this.m_sharingMode = value;
        }
    }
}
