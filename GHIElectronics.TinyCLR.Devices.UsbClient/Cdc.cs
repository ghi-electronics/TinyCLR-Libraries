using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>This device emulates a CDC virtual COM port.</summary>
	/// <remarks>
	/// Your host operating system may need the driver located <a href="https://www.ghielectronics.com/downloads/src/CDC_Driver.zip">here</a>. If you
	/// build a custom or composite device, you must change the driver to reflect the VID, PID, and interface number that you are using. Search for
	/// the string USB\Vid_VVVV&amp;Pid_PPPP&amp;MI_II in the INF file and update VVVV to your VID, PPPP to your PID, and II to you interface index.
	/// </remarks>
	public class Cdc : RawDevice {
        private static byte[] payload1 = new byte[] { 0, 9, 1 };
        private static byte[] payload2 = new byte[] { 1, 3, 1 };
        private static byte[] payload3 = new byte[] { 2, 15 };
        private static byte[] payload4 = new byte[] { 6, 0, 0 };
        private CdcStream stream;

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

        /// <summary>The stream for the CDC connection.</summary>
        public CdcStream Stream => this.stream;

        /// <summary>Creates a new CDC interface with default parameters.</summary>
        public Cdc(UsbClientController usbClientController)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.CDC,
                BcdUsb = 0x210,
                BcdDevice = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "CDC VCOM",
                SerialNumber = "0",
                InterfaceName = "CDC VCOM",
                Mode = UsbClientMode.Cdc
            }) {
        }

        /// <summary>Creates a new CDC interface.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>        
        public Cdc(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {
            var readEndpoint = this.ReserveNewEndpoint();
            var writeEndpoint = this.ReserveNewEndpoint();
            var interruptEndpoint = this.ReserveNewEndpoint();

            usbClientSetting.Mode = UsbClientMode.Cdc;

            Configuration.Endpoint[] endpoints =
            {
                new Configuration.Endpoint((byte)(interruptEndpoint | Configuration.Endpoint.ATTRIB_Write), Configuration.Endpoint.ATTRIB_Interrupt) { wMaxPacketSize = 64, bInterval = 255 },
                new Configuration.Endpoint((byte)(writeEndpoint | Configuration.Endpoint.ATTRIB_Write), Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },
                new Configuration.Endpoint((byte)(readEndpoint | Configuration.Endpoint.ATTRIB_Read) ,   Configuration.Endpoint.ATTRIB_Bulk) { wMaxPacketSize = 64 },

            };

            var usbInterface = new Configuration.UsbInterface(0, endpoints) { bInterfaceClass = 0x02, bInterfaceSubClass = 0x02, bInterfaceProtocol = 0x01 };
            usbInterface.classDescriptors = new Configuration.ClassDescriptor[]
            {
                new Configuration.ClassDescriptor(0x24, Cdc.payload1),
                new Configuration.ClassDescriptor(0x24, Cdc.payload2),
                new Configuration.ClassDescriptor(0x24, Cdc.payload3),
                new Configuration.ClassDescriptor(0x24, Cdc.payload4),
            };

            this.stream = (CdcStream)this.CreateStream(writeEndpoint, readEndpoint);

            var interfaceIndex = this.AddInterface(usbInterface, usbClientSetting.InterfaceName);
            this.SetInterfaceMap(interfaceIndex, 0, 0, 0);

        }

        /// <summary>Creates a new instance of a CDC stream.</summary>
        /// <param name="index">The index of the stream</param>
        /// <param name="parent">The owning raw device.</param>
        /// <returns>The new stream.</returns>
        protected override RawStream CreateStream(int index, RawDevice parent) => new CdcStream(index, parent);
        /// <summary>Stream for reading and writing data over a CDC connection.</summary>
        public class CdcStream : RawDevice.RawStream {

            internal CdcStream(int streamIndex, RawDevice parent)
                : base(streamIndex, parent) {
            }

            /// <summary>Writes data to the stream.</summary>
            /// <param name="buffer">The buffer from which to write.</param>
            /// <param name="offset">The offset into the buffer at which to begin writing.</param>
            /// <param name="count">The number of bytes to write.</param>
            public override void Write(byte[] buffer, int offset, int count) {
                //base.Write(buffer, offset, count);

                //if (count % 64 == 0)
                //base.Write(buffer, 0, 0);

                const int BlockSize = 63;

                var block = count / BlockSize;
                var remain = count % BlockSize;
                var index = offset;

                while (block > 0) {
                    base.Write(buffer, index, BlockSize);
                    index += BlockSize;
                    block--;
                }

                if (remain > 0)
                    base.Write(buffer, index, remain);


            }

            public override bool DataAvailable => this.BytesToRead > 0;
        }
    }
}
