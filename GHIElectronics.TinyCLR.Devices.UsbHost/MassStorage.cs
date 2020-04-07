using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
	/// <summary>Allows a usb device to be used as a mass storage device.</summary>
	public class MassStorage : BaseDevice {
		private byte logicalUnitIndex;
		private bool nativeConstructed;

        private StorageController storageController;


#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>The mass storage logical unit index.</summary>
        public int LogicalUnitIndex {
            get => this.logicalUnitIndex;

            set {
                if (value < 0 || value > 255) throw new ArgumentOutOfRangeException("logicalUnitIndex", "logicalUnitIndex must be between 0 and 255.");
                if (this.nativeConstructed) throw new InvalidOperationException("You cannot set LogicalUnitIndex after having mounted once already.");

                this.logicalUnitIndex = (byte)value;
            }
        }

        /// <summary>Not supported.</summary>
        public override int WorkerInterval {
            get => throw new NotSupportedException();

            set {
                if (value == 0 || value == Timeout.Infinite || this.disposed)
                    return;

                throw new NotSupportedException();
            }
        }

        /// <summary>Creates a new mass storage.</summary>
        /// <param name="id">The device id.</param>
        /// <param name="interfaceIndex">The device interface index.</param>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="portNumber">The device port number.</param>
        public MassStorage(uint id, byte interfaceIndex, ushort vendorId, ushort productId, byte portNumber)
			: base(id, interfaceIndex, DeviceType.MassStorage, vendorId, productId, portNumber) {
            this.logicalUnitIndex = 0;

            this.NativeConstructor(id, interfaceIndex, this.logicalUnitIndex);
            //this.nativeConstructed = false;
            //this.mounted = false;

            //this.Disconnected += (a, b) => {
            //	if (this.Mounted)
            //		this.Unmount();
            //};

            //this.storageController = StorageController.FromName(STM32H7.StorageController.UsbHostMassStorage);

        }

		/// <summary>The finalizer.</summary>
		~MassStorage() {
			this.Dispose(false);
		}

		
		/// <summary>Disposes the object.</summary>
		/// <param name="disposing">Whether or not this is called from Dispose.</param>
		protected override void Dispose(bool disposing) {
			if (this.disposed)
				return;			

			if (this.nativeConstructed)
				this.NativeFinalize();

			base.Dispose(disposing);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private void NativeConstructor(uint deviceId, byte interfaceIndex, byte massStorageLogicalUnitIndex);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern private void NativeFinalize();
		
	}
}
