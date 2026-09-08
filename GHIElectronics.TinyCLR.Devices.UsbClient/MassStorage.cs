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
	/// Only one mass storage interface can be used, and one logical unit is supported. 
	/// </remarks>
	public class MassStorage : RawDevice {
        private static int maxSupportLogicaUnits;
        private static int nextLogicalUnitNumber;
        private RawDevice.RawStream stream;
        private int logicalUnitCount;
        private static bool enabled;
        private static IntPtr storage;
        private static string vendor;
        private static string product;
        private static string revision;

        /// <summary>The maximum number of logical units that any mass storage can support.</summary>
        public static int MaximumSupportedLogicalUnits => MassStorage.maxSupportLogicaUnits;

        /// <summary>The number of logical units assocaited with this mass storage.</summary>
        public int LogicalUnitCount => this.logicalUnitCount;

        static MassStorage() {
            MassStorage.nextLogicalUnitNumber = 0;
            MassStorage.vendor = null;
            MassStorage.product = null;
            MassStorage.revision = null;
            MassStorage.maxSupportLogicaUnits = 1;
        }

        /// <summary>Creates a new mass storage with default parameters.</summary>
        public MassStorage(UsbClientController usbClientController)
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
        /// <param name="usbClientController">USBClient controller.</param>
        /// <param name="usbClientSetting">USBClient setting.</param>
        public MassStorage(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {

            var interfaceName = usbClientSetting.InterfaceName;
            var logicalUnitCount = 1;

            this.usbClientSetting = usbClientSetting;


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
        /// <param name="storage">The storage device (hdc )to attach.</param>
        public void AttachLogicalUnit(IntPtr storage) => this.AttachLogicalUnit(storage, null, null, null);

        /// <summary>Attaches a removable storage device, giving it SCSI identification strings.</summary>
        /// <param name="storage">The storage device (hdc )to attach.</param>
        /// <param name="vendor">
        /// SCSI vendor identification, up to 8 characters. Reported by INQUIRY and shown by
        /// Windows as the <c>Ven_</c> part of the device id. Pass <c>null</c> to keep the
        /// default. Longer strings are truncated by the device; shorter ones are space padded.
        /// </param>
        /// <param name="product">
        /// SCSI product identification, up to 16 characters, shown as the <c>Prod_</c> part.
        /// Pass <c>null</c> to use <see cref="UsbClientSetting.ProductName"/>, which is what
        /// earlier releases always did.
        /// </param>
        public void AttachLogicalUnit(IntPtr storage, string vendor, string product) =>
            this.AttachLogicalUnit(storage, vendor, product, null);

        /// <summary>Attaches a removable storage device, giving it SCSI identification strings.</summary>
        /// <param name="storage">The storage device (hdc )to attach.</param>
        /// <param name="vendor">SCSI vendor identification, up to 8 characters. <c>null</c> keeps the default.</param>
        /// <param name="product">SCSI product identification, up to 16 characters. <c>null</c> uses <see cref="UsbClientSetting.ProductName"/>.</param>
        /// <param name="revision">SCSI product revision level, up to 4 characters. <c>null</c> keeps "1.00".</param>
        public void AttachLogicalUnit(IntPtr storage, string vendor, string product, string revision) {
            if (MassStorage.nextLogicalUnitNumber >= 1 || MassStorage.enabled) {
                throw new IndexOutOfRangeException("Support one Logical Unit only!");
            }

            MassStorage.nextLogicalUnitNumber++;
            MassStorage.storage = storage;
            MassStorage.vendor = vendor;
            MassStorage.product = product;
            MassStorage.revision = revision;
        }

        /// <summary>Remove a removable storage device.</summary>
        /// <param name="storage">The storage device (hdc )to attach.</param>    
        public void RemoveLogicalUnit(IntPtr storage) {
            if (MassStorage.storage != storage || MassStorage.nextLogicalUnitNumber == 0) {
                throw new InvalidOperationException("Hdc not found.");
            }

            if (MassStorage.enabled)
                throw new IndexOutOfRangeException("MassStorage is in used.");

            MassStorage.nextLogicalUnitNumber--;
        }

        /// <summary>Enable a removable storage device.</summary>          
        public override void Enable() {
            if (MassStorage.enabled) {
                throw new InvalidOperationException("Already enabled.");
            }

            if (MassStorage.nextLogicalUnitNumber == 0) {
                throw new InvalidOperationException("No LogicalUnit found.");
            }

            // Vendor used to be hardcoded to a single space, which Windows shows as an
            // empty Ven_ field and SCSI INQUIRY does not really allow. Callers can now
            // supply both strings via AttachLogicalUnit; the old defaults are kept for
            // anyone who does not.
            this.EnableLogicalUnit(MassStorage.storage, MassStorage.nextLogicalUnitNumber - 1,
                MassStorage.vendor ?? " ",
                MassStorage.product ?? this.usbClientSetting.ProductName,
                MassStorage.revision ?? "1.00");

            base.Enable();

            MassStorage.enabled = true;
        }

        /// <summary>Disable a removable storage device.</summary>         
        public override void Disable() {
            if (!MassStorage.enabled) {
                throw new InvalidOperationException("Already disabled.");
            }

            if (MassStorage.nextLogicalUnitNumber == 0) {
                throw new InvalidOperationException("No LogicalUnit found.");
            }

            this.DisableLogicalUnit(MassStorage.storage, MassStorage.nextLogicalUnitNumber - 1);

            base.Disable();

            MassStorage.enabled = false;
        }


        /// <summary>Enables the logical unit associated with the given number.</summary>
        /// <param name="storage">Storage Hdc of storage controller.</param>
        /// <param name="number">The logical unit number.</param>
        /// <param name="vendor">vendor.</param>
        /// <param name="product">product.</param>
        private void EnableLogicalUnit(IntPtr storage, int number, string vendor, string product, string revision) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeEnableLogicalUnit(storage, (byte)number, vendor, product, revision);
        }

        /// <summary>Disables the logical unit associated with the given number.</summary>
        /// <param name="storage">Storage Hdc of storage controller.</param>
        /// <param name="number">The logical unit number.</param>
        private void DisableLogicalUnit(IntPtr storage, int number) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeDisableLogicalUnit(storage, (byte)number);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeEnableLogicalUnit(IntPtr storage, byte number, string vendor, string product, string revision);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeDisableLogicalUnit(IntPtr storage, byte number);

    }
}
