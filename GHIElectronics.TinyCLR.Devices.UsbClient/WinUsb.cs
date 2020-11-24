using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    public class WinUsb : RawDevice {
        private WinUsbStream stream;

        public WinUsbStream Stream => this.stream;

        private DataReceivedEventHandler dataReceivedCallbacks;
        private void OnDataReceived(RawDevice sender, uint count) => this.dataReceivedCallbacks?.Invoke(this, count);

        public event DataReceivedEventHandler DataReceived {
            add {
                if (this.dataReceivedCallbacks == null)
                    this.usbClientController.Provider.DataReceived += this.OnDataReceived;

                this.dataReceivedCallbacks += value;
            }
            remove {
                this.dataReceivedCallbacks -= value;

                if (this.dataReceivedCallbacks == null)
                    this.usbClientController.Provider.DataReceived -= this.OnDataReceived;
            }
        }


        /// <summary>Creates a new WinUsb interface with default parameters.</summary>
        public WinUsb(UsbClientController usbClientController)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.WinUsb,
                Version = 0x200,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "WinUsb",
                SerialNumber = "0",
                InterfaceName = "WinUsb",
                Mode = UsbClientMode.WinUsb
            }) {
        }

        /// <summary>Creates a new WinUsb interface.</summary>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="version">The device version.</param>
        /// <param name="maxPower">The maximum power required from bus in milliamps.</param>
        /// <param name="manufacturer">The manufacturer name.</param>
        /// <param name="product">The product name.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="interfaceName">The name of the interface.</param>
        public WinUsb(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {

            if (usbClientSetting.Guid == null || usbClientSetting.Guid.Length == 0)
                throw new ArgumentException("Invalid Guid.");

            if (usbClientSetting.Mode != UsbClientMode.WinUsb)
                throw new ArgumentException("Invalid Mode.");

            var readEndpoint = this.ReserveNewEndpoint();
            var writeEndpoint = this.ReserveNewEndpoint();            

            Configuration.Endpoint[] endpoints =
            {
                new Configuration.Endpoint((byte)writeEndpoint, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },
                new Configuration.Endpoint((byte)readEndpoint, Configuration.Endpoint.ATTRIB_Read | Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },
            };

            var usbInterface = new Configuration.UsbInterface(0, endpoints) { bInterfaceClass = 0xFF, bInterfaceSubClass = 0x01, bInterfaceProtocol = 0x01 };

            this.stream = (WinUsbStream)this.CreateStream(writeEndpoint, readEndpoint);

            var interfaceIndex = this.AddInterface(usbInterface, usbClientSetting.InterfaceName);
            this.SetInterfaceMap(interfaceIndex, RawDevice.InterfaceMapType.CDC, 0, 0, 0);
        }

        /// <summary>Creates a new instance of a CDC stream.</summary>
        /// <param name="index">The index of the stream</param>
        /// <param name="parent">The owning raw device.</param>
        /// <returns>The new stream.</returns>
        protected override RawStream CreateStream(int index, RawDevice parent) => new WinUsbStream(index, parent);
        /// <summary>Stream for reading and writing data over a CDC connection.</summary>
        public class WinUsbStream : RawDevice.RawStream {

            internal WinUsbStream(int streamIndex, RawDevice parent)
                : base(streamIndex, parent) {
            }

            /// <summary>Writes data to the stream.</summary>
            /// <param name="buffer">The buffer from which to write.</param>
            /// <param name="offset">The offset into the buffer at which to begin writing.</param>
            /// <param name="count">The number of bytes to write.</param>
            public override void Write(byte[] buffer, int offset, int count) {
                base.Write(buffer, offset, count);

                if (count % 64 == 0)
                    base.Write(buffer, 0, 0);
            }
        }
    }
}
