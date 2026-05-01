using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Usb;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>This device emulates a keyboard. You can set the state of any key, but only seven keys can be pressed at once.</summary>
	public class Keyboard : RawDevice {
        private const byte MAX_KEYS = 7;
        private const byte HID_INTERFACE_CLASS = 0x03;
        private const byte HID_DESCRIPTOR_TYPE = 0x21;

        private static byte[] REPORT_DESCRIPTOR_PAYLOAD =
        {
            0x05, 0x01, 0x09, 0x06, 0xA1, 0x01, 0x05, 0x07, 0x19, 0xE0, 0x29, 0xE7, 0x15, 0x00, 0x25, 0x01, 0x75, 0x01, 0x95, 0x08, 0x81, 0x02,
            0x95, Keyboard.MAX_KEYS, 0x75, 0x08, 0x15, 0x00, 0x26, 0xFF, 0x00, 0x05, 0x07, 0x19, 0x00, 0x2A, 0xFF, 0x00, 0x81, 0x00, 0xC0
        };

        private static byte[] CLASS_DESCRIPTOR_PAYLOAD =
        {
            0x01, 0x01, //bcdHID
            0x00, //bCountryCode
            0x01, //bNumDescriptors
            0x22, //bDescriptorType
            (byte)Keyboard.REPORT_DESCRIPTOR_PAYLOAD.Length, 0x00 //wItemLength
        };
        private RawDevice.RawStream stream;
        private byte[] report;

        /// <summary>Creates a new keyboard with default parameters.</summary>
        public Keyboard(UsbClientController usbClientController)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.Keyboard,
                BcdUsb = 0x210,
                BcdDevice = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "Keyboard",
                SerialNumber = "0",
                InterfaceName = "Keyboard",
                Mode = UsbClientMode.Keyboard
            }
            ) {
        }

        /// <summary>Creates a new keyboard.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>        
        public Keyboard(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {
            var endpoint = this.ReserveNewEndpoint();
            usbClientSetting.Mode = UsbClientMode.Keyboard;
            Configuration.Endpoint[] endpoints =
            {
                new Configuration.Endpoint((byte)endpoint, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Interrupt) { bInterval = 10, wMaxPacketSize = 8},
            };

            var usbInterface = new Configuration.UsbInterface(0, endpoints) { bInterfaceClass = Keyboard.HID_INTERFACE_CLASS, bInterfaceSubClass = 1, bInterfaceProtocol = 1 };
            usbInterface.classDescriptors = new Configuration.ClassDescriptor[] { new Configuration.ClassDescriptor(Keyboard.HID_DESCRIPTOR_TYPE, Keyboard.CLASS_DESCRIPTOR_PAYLOAD) };

            var interfaceIndex = this.AddInterface(usbInterface, usbClientSetting.InterfaceName);

            var reportDescriptor = new Configuration.GenericDescriptor(Configuration.GenericDescriptor.REQUEST_Standard | Configuration.GenericDescriptor.REQUEST_Interface | Configuration.GenericDescriptor.REQUEST_IN, 0x2200, Keyboard.REPORT_DESCRIPTOR_PAYLOAD);
            this.AddDescriptor(reportDescriptor);

            reportDescriptor.wIndex = interfaceIndex;

            this.stream = this.CreateStream(endpoint, RawDevice.RawStream.NullEndpoint);
            this.stream.WriteTimeout = 20;

            this.report = new byte[Keyboard.MAX_KEYS + 1];

            this.usbClientSetting = usbClientSetting;            
        }

        /// <summary>Presses and then releases the key.</summary>
        /// <param name="key">The key to press and release.</param>
        public void Stroke(Key key) => this.Stroke(key, 10);

        /// <summary>Presses and then releases the key.</summary>
        /// <param name="key">The key to press and release.</param>
        /// <param name="delay">How long to wait after the press before releasing.</param>
        public void Stroke(Key key, int delay) {
            if (delay < 0) throw new ArgumentOutOfRangeException("delay", "delay must be non-negative.");

            this.Press(key);
            Thread.Sleep(delay);
            this.Release(key);
        }

        /// <summary>Releases the given key.</summary>
        /// <param name="key">The key to release.</param>
        public void Release(Key key) {
            if (key >= Key.LeftCtrl && key <= Key.RightGUI) {
                this.report[0] &= (byte)~(1 << (key - Key.LeftCtrl));
            }
            else {
                for (var i = 0; i < Keyboard.MAX_KEYS; i++) {
                    if (this.report[i + 1] == (byte)key) {
                        this.report[i + 1] = 0;

                        break;
                    }
                }
            }

            this.stream.Write(this.report);
        }

        /// <summary>Presses the given key.</summary>
        /// <param name="key">The key to press.</param>
        public void Press(Key key) {
            if (key >= Key.LeftCtrl && key <= Key.RightGUI) {
                this.report[0] |= (byte)(1 << (key - Key.LeftCtrl));
            }
            else {
                var empty = 0;

                for (var i = 0; i < Keyboard.MAX_KEYS; i++) {
                    if (this.report[i + 1] == (byte)key)
                        break;

                    if (this.report[i + 1] == 0)
                        empty = i;
                }

                if (empty < Keyboard.MAX_KEYS)
                    this.report[empty + 1] = (byte)key;
            }

            this.stream.Write(this.report);
        }
    }
}
