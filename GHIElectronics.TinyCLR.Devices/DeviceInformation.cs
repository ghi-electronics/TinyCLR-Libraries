using System;

namespace GHIElectronics.TinyCLR.Devices.Enumeration {
    public sealed class DeviceInformation {
        private string m_id;
        private bool m_isDefault;

        /// <summary>
        /// Constructs a new DeviceInformation object.
        /// </summary>
        /// <param name="id">Unique identifier describing this device.</param>
        /// <param name="isDefault">Whether this is the default device for a given device class.</param>
        internal DeviceInformation(string id, bool isDefault) {
            this.m_id = id;
            this.m_isDefault = isDefault;
        }

        /// <summary>
        /// A string representing the identity of the device.
        /// </summary>
        /// <value>A string representing the identity of the device.</value>
        /// <remarks>This ID can be used to activate device functionality using the <c>CreateFromIdAsync</c> methods on
        ///     classes that implement device functionality.
        ///     <para>The DeviceInformation object that the Id property identifies is actually a device interface. For
        ///         simplicity in this documentation, the DeviceInformation object is called a device, and the
        ///         identifier in its Id property is called a DeviceInformation ID.</para></remarks>
        public string Id => this.m_id;

        /// <summary>
        /// Indicates whether this device is the default device for the class.
        /// </summary>
        /// <value>Indicates whether this device is the default device for the class.</value>
        public bool IsDefault => this.m_isDefault;

        /// <summary>
        /// Enumerates all DeviceInformation objects.
        /// </summary>
        /// <returns>List of all available DeviceInformation objects.</returns>
        public static DeviceInformation[] FindAll() =>
            // FUTURE: This should return DeviceInformationCollection
            FindAll(string.Empty);

        /// <summary>
        /// Enumerates DeviceInformation objects matching the specified Advanced Query Syntax (AQS) string.
        /// </summary>
        /// <param name="aqsFilter"></param>
        /// <returns>List of available DeviceInformation objects matching the given criteria..</returns>
        public static DeviceInformation[] FindAll(string aqsFilter) => throw new NotSupportedException();
    }
}
