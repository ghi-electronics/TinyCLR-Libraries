using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Usb;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    /// <summary>
	/// This device emulates a joystick. You can set the x, y, and z axis values, the z axis rotation, the state of 32 buttons, and 8 hat swtich directions.
	/// </summary>
	public class Joystick : RawDevice {
        private const int REPORT_FIELD_BUTTON = 0;
        private const int REPORT_FIELD_HATSWITCH = 4;
        private const int REPORT_FIELD_X = 5;
        private const int REPORT_FIELD_Y = 7;
        private const int REPORT_FIELD_Z = 9;
        private const int REPORT_FIELD_Z_ROTATION = 11;
        private const byte HID_INTERFACE_CLASS = 0x03;
        private const byte HID_DESCRIPTOR_TYPE = 0x21;

        private static byte[] REPORT_DESCRIPTOR_PAYLOAD =
        {
            0x05, 0x01, 0x09, 0x04, 0xA1, 0x01, 0x05, 0x01, 0x09, 0x01, 0xA1, 0x00, 0x05, 0x09,
            0x19, 0x01, 0x29, 0x20, 0x15, 0x00, 0x25, 0x01, 0x75, 0x01, 0x95, 0x20, 0x81, 0x02,
            0x05, 0x01, 0x09, 0x39, 0x25, 0x07, 0x35, 0x00,
            0x46, 0x0E, 0x01, 0x66, 0x40, 0x00, 0x75, 0x08, 0x95, 0x01, 0x81, 0x42,
            0x09, 0x30, 0x09, 0x31, 0x09, 0x32, 0x09, 0x35,
            0x16, 0x00, 0x80, 0x26, 0xFF, 0x7F, 0x47, 0xFF, 0xFF, 0x00, 0x00, 0x66, 0x00, 0x00, 0x75, 0x10, 0x95, 0x04,
            0x81, 0x02, 0xC0, 0xC0
        };

        private static byte[] CLASS_DESCRIPTOR_PAYLOAD =
        {
            0x01, 0x01, //bcdHID
            0x00, //bCountryCode
            0x01, //bNumDescriptors
            0x22, //bDescriptorType
            (byte)REPORT_DESCRIPTOR_PAYLOAD.Length, 0x00 //wItemLength
        };
        private RawDevice.RawStream stream;
        private byte[] report;
        private uint buttons;
        private int x;
        private int y;
        private int z;
        private int zRotation;
        private HatSwitchDirection hatSwitch;

        /// <summary>Sets the x axis position. It must be between –32,768 to +32,767.</summary>
        public int X { get => this.x; set => this.SendRawData(value, this.y, this.z, this.zRotation, this.buttons, this.hatSwitch); }

        /// <summary>Sets the y axis position. It must be between –32,768 to +32,767.</summary>
        public int Y { get => this.y; set => this.SendRawData(this.x, value, this.z, this.zRotation, this.buttons, this.hatSwitch); }

        /// <summary>Sets the z axis position. It must be between –32,768 to +32,767.</summary>
        public int Z { get => this.x; set => this.SendRawData(this.x, this.y, value, this.zRotation, this.buttons, this.hatSwitch); }

        /// <summary>Sets the z rotation. It must be between –32,768 to +32,767.</summary>
        public int ZRotation { get => this.x; set => this.SendRawData(this.x, this.y, this.z, value, this.buttons, this.hatSwitch); }

        /// <summary>Sets the hat switch direction.</summary>
        public HatSwitchDirection HatSwitch { get => this.hatSwitch; set => this.SendRawData(this.x, this.y, this.z, this.zRotation, this.buttons, value); }

        /// <summary>Creates a new joystick with default parameters.</summary>
        public Joystick(UsbClientController usbClientController)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.Joystick,
                BcdUsb = 0x210,
                BcdDevice = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "Joystick",
                SerialNumber = "0",
                InterfaceName = "Joystick",
                Mode = UsbClientMode.Joystick
            }) {
        }

        /// <summary>Creates a new joystick.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>        
        public Joystick(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
            : base(usbClientController, usbClientSetting) {
            usbClientSetting.Mode = UsbClientMode.Joystick;
            this.report = new byte[13];
            this.buttons = 0;
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.zRotation = 0;
            this.hatSwitch = HatSwitchDirection.None;

            var endpointNumber = this.ReserveNewEndpoint();

            Configuration.Endpoint[] endpointDescriptor =
            {
                new Configuration.Endpoint((byte)endpointNumber, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Interrupt) { wMaxPacketSize = 16, bInterval = 10 },
            };

            var usbInterface = new Configuration.UsbInterface(0, endpointDescriptor) { bInterfaceClass = Joystick.HID_INTERFACE_CLASS, bInterfaceSubClass = 0, bInterfaceProtocol = 0 };
            usbInterface.classDescriptors = new Configuration.ClassDescriptor[] { new Configuration.ClassDescriptor(Joystick.HID_DESCRIPTOR_TYPE, Joystick.CLASS_DESCRIPTOR_PAYLOAD) };

            var reportDescriptor = new Configuration.GenericDescriptor(Configuration.GenericDescriptor.REQUEST_Standard | Configuration.GenericDescriptor.REQUEST_Interface | Configuration.GenericDescriptor.REQUEST_IN, 0x2200, Joystick.REPORT_DESCRIPTOR_PAYLOAD);
            var interfaceIndex = this.AddInterface(usbInterface, usbClientSetting.InterfaceName);

            this.AddDescriptor(reportDescriptor);

            reportDescriptor.wIndex = interfaceIndex;

            this.stream = this.CreateStream(endpointNumber, RawDevice.RawStream.NullEndpoint);
            this.stream.WriteTimeout = 20;

            this.report[Joystick.REPORT_FIELD_HATSWITCH] = (byte)HatSwitchDirection.None;
        }

        /// <summary>Sends the given raw data directly to the host..</summary>
        /// <param name="x">The new x position from -32,768 to 32,767.</param>
        /// <param name="y">The new y position from -32,768 to 32,767.</param>
        /// <param name="z">The new z position from -32,768 to 32,767.</param>
        /// <param name="zRotation">The new z rotation from -32,768 to 32,767.</param>
        /// <param name="buttons">
        /// Sends the given uint as a bitfield where each 1 represents a button press for the button whose number is the bit number.
        /// </param>
        /// <param name="hatSwitch">The new direction of the hat switch.</param>
        public void SendRawData(int x, int y, int z, int zRotation, uint buttons, HatSwitchDirection hatSwitch) {
            if (x < short.MinValue || x > short.MaxValue) throw new ArgumentOutOfRangeException("x", "x must be between short.MinValue and short.MaxValue");
            if (y < short.MinValue || y > short.MaxValue) throw new ArgumentOutOfRangeException("y", "y must be between short.MinValue and short.MaxValue");
            if (z < short.MinValue || z > short.MaxValue) throw new ArgumentOutOfRangeException("z", "z must be between short.MinValue and short.MaxValue");
            if (zRotation < short.MinValue || zRotation > short.MaxValue) throw new ArgumentOutOfRangeException("zRotation", "zRotation must be between short.MinValue and short.MaxValue");

            this.x = x;
            this.y = y;
            this.z = z;
            this.zRotation = zRotation;
            this.buttons = buttons;
            this.hatSwitch = hatSwitch;

            this.SendReport();
        }

        /// <summary>Whether or not the given button has been pressed.</summary>
        /// <param name="index">The button to query.</param>
        /// <returns>The button state.</returns>
        public bool IsButtonPressed(int index) => (this.buttons & (1 << index)) != 0;

        /// <summary>Presses the given button.</summary>
        /// <param name="index">The button to press.</param>
        public void PressButton(int index) {
            if (index < 0 || index > 31) throw new ArgumentOutOfRangeException("index", "index must be between 0 and 31.");

            this.buttons |= (1U << index);

            this.SendRawData(this.x, this.y, this.z, this.zRotation, this.buttons, this.hatSwitch);
        }

        /// <summary>Releases the given button.</summary>
        /// <param name="index">The button to release.</param>
        public void ReleaseButton(int index) {
            if (index < 0 || index > 31) throw new ArgumentOutOfRangeException("index", "index must be between 0 and 31.");

            this.buttons &= ~(1U << index);

            this.SendRawData(this.x, this.y, this.z, this.zRotation, this.buttons, this.hatSwitch);
        }

        /// <summary>Presses then releases the given button.</summary>
        /// <param name="index">The button to click.</param>
        public void ClickButton(int index) => this.ClickButton(index, 10);

        /// <summary>Presses then releases the given button with the given delay between the actions.</summary>
        /// <param name="index">The button to click.</param>
        /// <param name="delay">The delay between the actions.</param>
        public void ClickButton(int index, int delay) {
            if (delay < 0) throw new ArgumentOutOfRangeException("delay", "delay must be non-negative.");

            this.PressButton(index);
            Thread.Sleep(delay);
            this.ReleaseButton(index);
        }

        private void SendReport() {
            var x = BitConverter.GetBytes((ushort)this.x);
            var y = BitConverter.GetBytes((ushort)this.y);
            var z = BitConverter.GetBytes((ushort)this.z);
            var zRot = BitConverter.GetBytes((ushort)this.zRotation);
            var bt = BitConverter.GetBytes(this.buttons);

            Array.Copy(x, 0, this.report, Joystick.REPORT_FIELD_X, 2);
            Array.Copy(y, 0, this.report, Joystick.REPORT_FIELD_Y, 2);
            Array.Copy(z, 0, this.report, Joystick.REPORT_FIELD_Z, 2);
            Array.Copy(zRot, 0, this.report, Joystick.REPORT_FIELD_Z_ROTATION, 2);
            Array.Copy(bt, 0, this.report, Joystick.REPORT_FIELD_BUTTON, 4);

            this.report[Joystick.REPORT_FIELD_HATSWITCH] = (byte)this.hatSwitch;

            this.stream.Write(this.report);
        }
    }
}
