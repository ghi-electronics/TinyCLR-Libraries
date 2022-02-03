using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>
	/// This device emulates a mass storage. Your Micro Framework device will appear as a virtual mass storage device similar to a USB Drive. This
	/// works by exposing the storage connected to this device, such as SD card or USB sticks, to the host.
	/// </summary>
	/// <remarks>
	/// Only one mass storage interface can be used, but multiple logical units are supported. Each logical unit will appear as a separate device on
	/// your host PC.
	/// </remarks>
	public class MassStorage : RawDevice {
        private static int maxSupportLogicaUnits;
        private static int nextLogicalUnitNumber;
        private RawDevice.RawStream stream;
        private int logicalUnitCount;

        /// <summary>The maximum number of logical units that any mass storage can support.</summary>
        public static int MaximumSupportedLogicalUnits => MassStorage.maxSupportLogicaUnits;

        /// <summary>The number of logical units assocaited with this mass storage.</summary>
        public int LogicalUnitCount => this.logicalUnitCount;

        static MassStorage() {
            MassStorage.nextLogicalUnitNumber = 0;
            MassStorage.maxSupportLogicaUnits = MassStorage.NativeGetMaxLogicalUnits();
        }

        /// <summary>Creates a new mass storage with default parameters.</summary>
        public MassStorage(UsbClientController usbClientController)
            //: this(RawDevice.GHI_VID, (ushort)RawDevice.PID.MassStorage, 0x100, RawDevice.MAX_POWER, "GHI Electronics", "Mass Storage", "0", "Mass Storage", 1) {
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.MassStorage,
                BcdUsb = 0x210,
                BcdDevice = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "Mass Storage",
                SerialNumber = "0",
                InterfaceName = "Mass Storage",
                Mode = UsbClientMode.MassStorage
            }) {
        }

        /// <summary>Creates a new mass storage.</summary>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="version">The device version.</param>
        /// <param name="maxPower">The maximum power required from bus in milliamps.</param>
        /// <param name="manufacturer">The manufacturer name.</param>
        /// <param name="product">The product name.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="interfaceName">The name of the interface.</param>
        /// <param name="logicalUnitCount">The number of logical units in this device.</param>
        //public MassStorage(ushort vendorId, ushort productId, ushort version, ushort maxPower, string manufacturer, string product, string serialNumber, string interfaceName, int logicalUnitCount)
        //    : base(vendorId, productId, version, maxPower, manufacturer, product, serialNumber) {
        public MassStorage(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {

            var vendorId = usbClientSetting.VendorId;
            var productId = usbClientSetting.ProductId;
            var version = usbClientSetting.BcdDevice;
            var maxPower = usbClientSetting.MaxPower;
            var manufacturer = usbClientSetting.ManufactureName;
            var product = usbClientSetting.ProductName;
            var serialNumber = usbClientSetting.SerialNumber;
            var interfaceName = usbClientSetting.InterfaceName;
            var logicalUnitCount = 1;


            if (logicalUnitCount < 0 || logicalUnitCount > MassStorage.MaximumSupportedLogicalUnits) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than MassStorage.MaximumSupportedLogicalUnits.");

            var readEndpoint = this.ReserveNewEndpoint();
            var writeEndpoint = this.ReserveNewEndpoint();

            Configuration.Endpoint[] endpoints =
            {
                new Configuration.Endpoint((byte)writeEndpoint, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },
                new Configuration.Endpoint((byte)readEndpoint, Configuration.Endpoint.ATTRIB_Read | Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },
            };

            var usbInterface = new Configuration.UsbInterface(0, endpoints) { bInterfaceClass = 0x08, bInterfaceSubClass = 0x06, bInterfaceProtocol = 0x50 };
            var interfaceIndex = this.AddInterface(usbInterface, interfaceName);

            this.logicalUnitCount = logicalUnitCount;
            this.stream = this.CreateStream(writeEndpoint, readEndpoint);

            this.SetInterfaceMap(interfaceIndex, /*RawDevice.InterfaceMapType.MassStorage,*/ (byte)this.logicalUnitCount, (byte)this.stream.StreamIndex, 0);
        }

        /// <summary>Attaches a removable storage device.</summary>
        /// <param name="storage">The storage device to attach.</param>
        /// <returns>The number associated with the new logical unit</returns>
        public int AttachLogicalUnit(IntPtr storage) {
            var number = MassStorage.nextLogicalUnitNumber++;

            this.AttachLogicalUnit(storage, number);

            return number;
        }

        /// <summary>Attaches a removable storage device to a logical unit.</summary>
        /// <param name="storage">The storage device to attach.</param>
        /// <param name="number">The logical unit number.</param>
        public void AttachLogicalUnit(IntPtr storage, int number) => this.AttachLogicalUnit(storage, number, " ", " ");

        /// <summary>Attaches a removable storage device to a logical unit.</summary>
        /// <param name="storage">The storage device to attach.</param>
        /// <param name="number">The logical unit number.</param>
        /// <param name="vendor">The vendor name.</param>
        /// <param name="product">The product name.</param>
        public void AttachLogicalUnit(IntPtr storage, int number, string vendor, string product) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");
            if (number >= this.logicalUnitCount) throw new ArgumentOutOfRangeException("number", "number must be less than LogicalUnitCount.");
            //if (storage == null) throw new ArgumentNullException("storage");
            if (vendor == null) throw new ArgumentNullException("vendor");
            if (product == null) throw new ArgumentNullException("product");

            //storage.ForceInitialization();

            MassStorage.NativeAttachLogicalUnit((byte)number, storage, vendor, product);
        }

        /// <summary>Enables the logical unit associated with the given number.</summary>
        /// <param name="number">The logical unit number.</param>
        public void EnableLogicalUnit(int number) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeEnableLogicalUnit((byte)number);
        }

        /// <summary>Disables the logical unit associated with the given number.</summary>
        /// <param name="number">The logical unit number.</param>
        public void DisableLogicalUnit(int number) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeDisableLogicalUnit((byte)number);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeEnableLogicalUnit(byte number);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeDisableLogicalUnit(byte number);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeAttachLogicalUnit(byte number, IntPtr storageId, string vendor, string product);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static byte NativeGetMaxLogicalUnits();
    }
}
