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
        private static bool enabled;
        private static IntPtr storage;

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
        /// <param name="storage">The storage device to attach.</param>
        /// <returns>The number associated with the new logical unit</returns>
        public void AttachLogicalUnit(IntPtr storage) {
            if (MassStorage.nextLogicalUnitNumber >= 1 || MassStorage.enabled) {
                throw new IndexOutOfRangeException("Support one Logical Unit only!");
            }

            MassStorage.nextLogicalUnitNumber++;
            MassStorage.storage = storage;
        }

        public void RemoveLogicalUnit(IntPtr storage) {
            if (MassStorage.storage != storage || MassStorage.nextLogicalUnitNumber == 0) {
                throw new InvalidOperationException("Hdc not found.");
            }

            if (MassStorage.enabled)
                throw new IndexOutOfRangeException("MassStorage is in used.");

            MassStorage.nextLogicalUnitNumber--;
        }
        public override void Enable() {
            if (MassStorage.enabled) {
                throw new InvalidOperationException("Already enabled.");
            }

            if (MassStorage.nextLogicalUnitNumber == 0) {
                throw new InvalidOperationException("No LogicalUnit found.");
            }

            this.EnableLogicalUnit(MassStorage.storage, MassStorage.nextLogicalUnitNumber - 1, " ", this.usbClientSetting.ProductName);

            base.Enable();

            MassStorage.enabled = true;
        }

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
        /// <param name="number">The logical unit number.</param>
        private void EnableLogicalUnit(IntPtr storage, int number, string vendor, string product) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeEnableLogicalUnit(storage, (byte)number, vendor, product);
        }

        /// <summary>Disables the logical unit associated with the given number.</summary>
        /// <param name="number">The logical unit number.</param>
        private void DisableLogicalUnit(IntPtr storage, int number) {
            if (number < 0 || number > 255) throw new ArgumentOutOfRangeException("number", "number must be non-negative and less than 256.");

            MassStorage.NativeDisableLogicalUnit(storage, (byte)number);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeEnableLogicalUnit(IntPtr storage, byte number, string vendor, string product);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeDisableLogicalUnit(IntPtr storage, byte number);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static byte NativeGetMaxLogicalUnits();
    }
}
