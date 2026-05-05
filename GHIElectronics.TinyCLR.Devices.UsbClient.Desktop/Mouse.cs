using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Usb;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    public class Mouse : RawDevice {
        private const byte HID_INTERFACE_CLASS = 0x03;
        private const byte HID_DESCRIPTOR_TYPE = 0x21;
        private const int REPORT_FIELD_MOUSE_BUTTON = 0;
        private const int REPORT_FIELD_MOUSE_X = 1;
        private const int REPORT_FIELD_MOUSE_Y = 3;
        private const int REPORT_FIELD_MOUSE_W = 5;

        private static byte[] CLASS_DESCRIPTOR_PAYLOAD =
        {
            0x01, 0x01, //bcdHID
            0x00, //bCountryCode
            0x01, //bNumDescriptors
            0x22, //bDescriptorType
            0x36, 0x00 //wItemLength
        };

        private static byte[] reportDescriptorPayload =
        {
            0x05, 0x01, //Usage Page (Generic Desktop)
            0x09, 0x02, //Usage (Mouse)
            0xa1, 0x01, //Collection (Application)
            0x09, 0x01, //Usage (Pointer)
            0xa1, 0x00, //Collection (Physical)
            0x05, 0x09, //Usage Page (Buttons)
            0x19, 0x01, //Usage Minimum (01) first button
            0x29, 0x05, //Usage Maximun (03) fifth button
            0x15, 0x00, //Logical Minimum (0)
            0x25, 0x01, //Logical Maximum (1)
            0x95, 0x05, //Report Count (5)
            0x75, 0x01, //Report Size (1)
            0x81, 0x02, //Input (Data, Variable, Absolute)
            0x95, 0x01, //Report Count (1)
            0x75, 0x03, //Report Size (3)
            0x81, 0x01, //Input (Constant)    // pad for the byte
            0x05, 0x01, //Usage Page (Generic Desktop)
            0x09, 0x30, //Usage (X)
            0x09, 0x31, //Usage (Y)
            0x09, 0x38, //Usage (W)

            0x16, 0x00, 0x80, //Logical Minimum (-32768)
            0x26, 0xFF, 0x7F,//Logical Maximum (32767)
            0x75, 0x10, //Report Size (16)
            0x95, 0x03, //Report Count (3)
            0x81, 0x06, //Input (Data, Variable, Relative)

            0xc0, // End Collection
            0xc0 // End Collection
        };
        private RawDevice.RawStream stream;
        private byte[] report;

        private Buttons buttonState;
        private int x;
        private int y;
        private int wheel;
        private bool absolutePos;
        private int clickDelay = 10;

        /// <summary>The maximum range for movement.</summary>
        public static int MaxRange => 32767;

        /// <summary>The minimum range for movement.</summary>
        public static int MinRange => -32768;

        /// <summary>The mouse click delay in millisecond.</summary>
        public int ClickButtonDelay {
            get => this.clickDelay;
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("delay", "delay must be non-negative.");
                }
                this.clickDelay = value;
            }

        }

        /// <summary>Return true for absolute, false for relative mode.</summary>
        public bool AbsolutePosition {
            get => this.absolutePos;

            private set {
                this.absolutePos = value;

                if (this.absolutePos == true) {
                    reportDescriptorPayload[51] = 0x02;
                }
                else {
                    reportDescriptorPayload[51] = 0x06;
                }
            }
        }

        /// <summary>Creates a new mouse.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="absolutePositon">true for absolute position, false for relative </param>   
        public Mouse(UsbClientController usbClientController, bool absolutePositon = false)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.Keyboard,
                BcdUsb = 0x210,
                BcdDevice = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "Mouse",
                SerialNumber = "0",
                InterfaceName = "Mouse",
                Mode = UsbClientMode.Mouse
            }, absolutePositon
            ) {
        }

        /// <summary>Creates a new mouse.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>
        /// <param name="absolutePositon">true for absolute position, false for relative </param>   
        public Mouse(UsbClientController usbClientController, UsbClientSetting usbClientSetting, bool absolutePositon = false)
            : base(usbClientController, usbClientSetting) {
            this.report = new byte[7]; // 1 button state + 2 byteX + 2 byteY + 2 byteWheel
            this.x = 0;
            this.y = 0;
            this.wheel = 0;
            this.buttonState = Buttons.None;

            var endpointNumber = this.ReserveNewEndpoint();

            Configuration.Endpoint[] endpointDescriptor =
            {
                new Configuration.Endpoint((byte)endpointNumber, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Interrupt) { wMaxPacketSize = 8, bInterval = 10 },
            };

            var usbInterface = new Configuration.UsbInterface(0, endpointDescriptor) { bInterfaceClass = Mouse.HID_INTERFACE_CLASS, bInterfaceSubClass = 0, bInterfaceProtocol = 0 };
            usbInterface.classDescriptors = new Configuration.ClassDescriptor[] { new Configuration.ClassDescriptor(Mouse.HID_DESCRIPTOR_TYPE, Mouse.CLASS_DESCRIPTOR_PAYLOAD) };

            var reportDescriptor = new Configuration.GenericDescriptor(Configuration.GenericDescriptor.REQUEST_Standard | Configuration.GenericDescriptor.REQUEST_Interface | Configuration.GenericDescriptor.REQUEST_IN, 0x2200, Mouse.reportDescriptorPayload);
            var interfaceIndex = this.AddInterface(usbInterface, usbClientSetting.InterfaceName);

            this.AddDescriptor(reportDescriptor);

            reportDescriptor.wIndex = interfaceIndex;

            this.stream = this.CreateStream(endpointNumber, RawDevice.RawStream.NullEndpoint);
            this.stream.WriteTimeout = 20;
            this.AbsolutePosition = absolutePositon;
        }

        /// <summary>Whether or not the given button is pressed.</summary>
        /// <param name="button">The button to check.</param>
        /// <returns>If it is pressed or not.</returns>
        public bool IsPressedButton(Buttons button) => (this.buttonState & button) != 0;


        /// <summary>Presses the given button.</summary>
        /// <param name="button">The button to press.</param>
        public void PressButton(Buttons button) {
            this.buttonState |= button;

            this.SendReport(this.x, this.y, this.wheel, this.buttonState);
        }

        /// <summary>Releases the given button.</summary>
        /// <param name="button">The button to release.</param>
        public void ReleaseButton(Buttons button) {
            this.buttonState &= ~button;

            this.SendReport(this.x, this.y, this.wheel, this.buttonState);
        }


        /// <summary>Presses then releases the given button with the given delay between the actions.</summary>
        /// <param name="button">The button to click.</param>       
        public void Click(Buttons button) {
            this.PressButton(button);
            Thread.Sleep(this.clickDelay);
            this.ReleaseButton(button);
        }

        /// <summary>Moves the wheel to the given position.</summary>
        /// <param name="position">The new position.</param>
        public void MoveWheel(int position) {
            if (position == this.wheel)
                return;

            this.SendReport(this.x, this.y, position, this.buttonState);
        }

        /// <summary>Moves the cursor's x and y coordinate to the given position.</summary>
        /// <param name="x">The new x position.</param>
        /// <param name="y">The new p position.</param>
        public void MoveCursor(int x, int y) => this.SendReport(x, y, this.wheel, this.buttonState);

        /// <summary>Moves the cursor's x and y coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="x">The new x position.</param>
        /// <param name="y">The new p position.</param>
        /// <param name="wheel">The new wheel position.</param>
        /// <param name="buttonsState">The new buttonsState state.</param>          
        private void SendReport(int x, int y, int wheel, Buttons buttonsState) {
            if (x < Mouse.MinRange || x > Mouse.MaxRange) throw new ArgumentOutOfRangeException("X", "X must be between MinRange and MaxRange.");
            if (y < Mouse.MinRange || y > Mouse.MaxRange) throw new ArgumentOutOfRangeException("Y", "Y must be between MinRange and MaxRange.");
            if (wheel < Mouse.MinRange || wheel > Mouse.MaxRange) throw new ArgumentOutOfRangeException("Wheel", "Wheel must be between MinRange and MaxRange.");

            if (this.AbsolutePosition) {
                this.x = x;
                this.y = y;
                this.wheel = wheel;
            }

            this.report[Mouse.REPORT_FIELD_MOUSE_BUTTON] = (byte)buttonsState;
            this.report[Mouse.REPORT_FIELD_MOUSE_X] = (byte)(x & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_X + 1] = (byte)((x >> 8) & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_Y] = (byte)(y & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_Y + 1] = (byte)((y >> 8) & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_W] = (byte)(wheel & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_W + 1] = (byte)((wheel >> 8) & 0xFF);

            this.stream.Write(this.report);
        }
    }
}
