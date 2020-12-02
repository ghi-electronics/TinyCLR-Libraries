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
            0x26, 0x00, 0x80,//Logical Maximum (32768)
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

        /// <summary>The maximum step amount for movement.</summary>
        public static int MaxStep => 32768;

        /// <summary>The minimum step amount for movement.</summary>
        public static int MinStep => -32768;

        /// <summary>The current wheel position.</summary>
        public int WheelPosition => this.wheel;

        /// <summary>The current x coordinate of the cursor.</summary>
        public int CursorX => this.x;

        /// <summary>The current y coordinate of the cursor.</summary>
        public int CursorY => this.y;

        /// <summary>Set true for absolute, false for relative mode.</summary>
        public bool AbsolutePosition {
            get => this.absolutePos;
            set {
                if (this.Initialized)
                    throw new Exception("Can not set after enabled.");

                this.absolutePos = value;

                if (this.absolutePos == true) {
                    reportDescriptorPayload[51] = 0x02;
                }
                else {
                    reportDescriptorPayload[51] = 0x06;
                }
            }
        }


        /// <summary>Creates a new mouse with default parameters.</summary>
        public Mouse(UsbClientController usbClientController)
            : this(usbClientController, new UsbClientSetting() {
                VendorId = RawDevice.GHI_VID,
                ProductId = (ushort)RawDevice.PID.Keyboard,
                Version = 0x100,
                MaxPower = RawDevice.MAX_POWER,
                ManufactureName = "GHI Electronics",
                ProductName = "Mouse",
                SerialNumber = "0",
                InterfaceName = "Mouse",
                Mode = UsbClientMode.Mouse
            }
            ) {
        }
        /// <summary>Creates a new mouse.</summary>
        /// <param name="usbClientController">UsbClient controller.</param>
        /// <param name="usbClientSetting">UsbClient setting</param>        
        public Mouse(UsbClientController usbClientController, UsbClientSetting usbClientSetting)
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
        }

        /// <summary>Whether or not the given button is pressed.</summary>
        /// <param name="button">The button to check.</param>
        /// <returns>If it is pressed or not.</returns>
        public bool IsPressed(Buttons button) => (this.buttonState & button) != 0;

        /// <summary>Sends mouse delta positions and button states to the host.</summary>
        /// <param name="deltaX">Change in the x direction from -127 to 127.</param>
        /// <param name="deltaY">Change in the y direction from -127 to 127.</param>
        /// <param name="deltaWheel">Change in the mouse wheel position from -127 to 127.</param>
        /// <param name="buttonsState">The currently pressed buttons.</param>
        public void SendRawData(int deltaX, int deltaY, int deltaWheel, Buttons buttonsState) {
            this.x += deltaX;
            this.y += deltaY;
            this.wheel += deltaWheel;
            this.buttonState = buttonsState;

            this.SendReport(deltaX, deltaY, deltaWheel, this.buttonState);
        }

        /// <summary>Presses the given button.</summary>
        /// <param name="button">The button to press.</param>
        public void PressButton(Buttons button) {
            this.buttonState |= button;

            this.SendReport(0, 0, 0, this.buttonState);
        }

        /// <summary>Releases the given button.</summary>
        /// <param name="button">The button to release.</param>
        public void ReleaseButton(Buttons button) {
            this.buttonState &= ~button;

            this.SendReport(0, 0, 0, this.buttonState);
        }

        /// <summary>Presses then releases the given button.</summary>
        /// <param name="button">The button to click.</param>
        public void Click(Buttons button) => this.Click(button, 10);

        /// <summary>Presses then releases the given button with the given delay between the actions.</summary>
        /// <param name="button">The button to click.</param>
        /// <param name="delay">The delay between the actions.</param>
        public void Click(Buttons button, int delay) {
            if (delay < 0) throw new ArgumentOutOfRangeException("delay", "delay must be non-negative.");

            this.PressButton(button);
            Thread.Sleep(delay);
            this.ReleaseButton(button);
        }

        /// <summary>Presses then releases the given button twice.</summary>
        /// <param name="button">The button to double click.</param>
        public void DoubleClick(Buttons button) => this.DoubleClick(button, 10);

        /// <summary>Presses then releases the given button twice with the given delay between the clicks.</summary>
        /// <param name="delay">The delay between the clicks.</param>
        /// <param name="button">The button to double click.</param>
        public void DoubleClick(Buttons button, int delay) => this.DoubleClick(button, delay, 50);

        /// <summary>Presses then releases the given button twice with the given delay between the clicks and the actions.</summary>
        /// <param name="delay">The delay between the clicks.</param>
        /// <param name="releaseDelay">The delay between the press and release of each click.</param>
        /// <param name="button">The button to double click.</param>
        public void DoubleClick(Buttons button, int delay, int releaseDelay) {
            if (delay < 0) throw new ArgumentOutOfRangeException("delay", "delay must be non-negative.");
            if (releaseDelay < 0) throw new ArgumentOutOfRangeException("releaseDelay", "releaseDelay must be non-negative.");

            this.Click(button, releaseDelay);
            Thread.Sleep(delay);
            this.Click(button, releaseDelay);
        }

        /// <summary>Moves the wheel to the given position.</summary>
        /// <param name="position">The new position.</param>
        public void MoveWheelTo(int position) => this.MoveWheelTo(position, position > this.wheel ? 100 : -100);

        /// <summary>Moves the wheel to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        public void MoveWheelTo(int position, int step) => this.MoveWheelTo(position, step, 1);

        /// <summary>Moves the wheel to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        /// <param name="stepDelay">How long to wait between each step.</param>
        public void MoveWheelTo(int position, int step, int stepDelay) {
            if (step < Mouse.MinStep || step > Mouse.MaxStep) throw new ArgumentOutOfRangeException("step", "step must be between MIN_STEP and MAX_STEP.");
            if (position > this.wheel && step < 0) throw new ArgumentOutOfRangeException("step", "step must be positive when position is greater than WheelPosition.");
            if (position < this.wheel && step > 0) throw new ArgumentOutOfRangeException("step", "step must be negative when position is less than WheelPosition.");
            if (step == 0) throw new ArgumentOutOfRangeException("step", "step must not be 0.");
            if (stepDelay < 0) throw new ArgumentOutOfRangeException("stepDelay", "stepDelay must be non-negative.");

            if (position == this.wheel)
                return;

            if (step > 0) {
                while (this.wheel + step < position) {
                    this.wheel += step;
                    this.SendReport(0, 0, step, this.buttonState);
                    Thread.Sleep(stepDelay);
                }
            }
            else {
                while (this.wheel + step > position) {
                    this.wheel += step;
                    this.SendReport(0, 0, step, this.buttonState);
                    Thread.Sleep(stepDelay);
                }
            }

            if (this.wheel != position) {
                this.SendReport(0, 0, position - this.wheel, this.buttonState);
                this.wheel = position;
            }
        }

        /// <summary>Moves the cursor's x coordinate to the given position.</summary>
        /// <param name="position">The new position.</param>
        public void MoveXTo(int position) => this.MoveXTo(position, position > this.x ? 100 : -100);

        /// <summary>Moves the cursor's x coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        public void MoveXTo(int position, int step) => this.MoveXTo(position, step, 1);

        /// <summary>Moves the cursor's x coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        /// <param name="stepDelay">How long to wait between each step.</param>
        public void MoveXTo(int position, int step, int stepDelay) => this.MoveCursorTo(position, this.y, step, 0, stepDelay);

        /// <summary>Moves the cursor's y coordinate to the given position.</summary>
        /// <param name="position">The new position.</param>
        public void MoveYTo(int position) => this.MoveYTo(position, position > this.y ? 100 : -100);

        /// <summary>Moves the cursor's y coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        public void MoveYTo(int position, int step) => this.MoveYTo(position, step, 1);

        /// <summary>Moves the cursor's y coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="position">The new position.</param>
        /// <param name="step">The amount by which to increment the position each request.</param>
        /// <param name="stepDelay">How long to wait between each step.</param>
        public void MoveYTo(int position, int step, int stepDelay) => this.MoveCursorTo(this.x, position, 0, step, stepDelay);

        /// <summary>Moves the cursor's x and y coordinate to the given position.</summary>
        /// <param name="x">The new x position.</param>
        /// <param name="y">The new p position.</param>
        public void Move(int x, int y) => this.MoveCursorTo(x, y, x > this.x ? 100 : -100, y > this.y ? 100 : -100);        

        /// <summary>Moves the cursor's x and y coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="x">The new x position.</param>
        /// <param name="y">The new p position.</param>
        /// <param name="stepX">The amount by which to increment the x position each request.</param>
        /// <param name="stepY">The amount by which to increment the y position each request.</param>
        private void MoveCursorTo(int x, int y, int stepX, int stepY) => this.MoveCursorTo(x, y, stepX, stepY, 1);

        /// <summary>Moves the cursor's x and y coordinate to the given position with the given step since each action can move at most 127 units.</summary>
        /// <param name="x">The new x position.</param>
        /// <param name="y">The new p position.</param>
        /// <param name="stepX">The amount by which to increment the x position each request.</param>
        /// <param name="stepY">The amount by which to increment the y position each request.</param>
        /// <param name="stepDelay">How long to wait between each step.</param>
        private void MoveCursorTo(int x, int y, int stepX, int stepY, int stepDelay) {
            if (this.AbsolutePosition == false) {
                if (stepX < Mouse.MinStep || stepX > Mouse.MaxStep) throw new ArgumentOutOfRangeException("stepX", "stepX must be between MIN_STEP and MAX_STEP.");
                if (stepY < Mouse.MinStep || stepY > Mouse.MaxStep) throw new ArgumentOutOfRangeException("stepY", "stepY must be between MIN_STEP and MAX_STEP.");
                if (x > this.x && stepX < 0) throw new ArgumentOutOfRangeException("x", "x must be positive when position is greater than CursorX.");
                if (x < this.x && stepX > 0) throw new ArgumentOutOfRangeException("x", "x must be negative when position is less than CursorX.");
                if (stepX == 0) throw new ArgumentOutOfRangeException("stepX", "stepX must not be 0.");
                if (y > this.y && stepY < 0) throw new ArgumentOutOfRangeException("y", "y must be positive when position is greater than CursorY.");
                if (y < this.y && stepY > 0) throw new ArgumentOutOfRangeException("y", "y must be negative when position is less than CursorY.");
                if (stepY == 0) throw new ArgumentOutOfRangeException("stepX", "stepX must not be 0.");
                if (stepDelay < 0) throw new ArgumentOutOfRangeException("stepDelay", "stepDelay must be non-negative.");

                if (y == this.y || x == this.x)
                    return;

                while (true) {
                    if (x == this.x && y == this.y)
                        break;

                    if (stepX > 0 && x - this.x < stepX) stepX = x - this.x;
                    if (stepY > 0 && y - this.y < stepY) stepY = y - this.y;

                    if (stepX < 0 && x - this.x > stepX) stepX = x - this.x;
                    if (stepY < 0 && y - this.y > stepY) stepY = y - this.y;

                    this.x += stepX;
                    this.y += stepY;

                    this.SendReport(stepX, stepY, 0, this.buttonState);

                    Thread.Sleep(stepDelay);
                }
            }
            else {

                this.x = x;
                this.y = y;

                this.SendReport(x, y, 0, this.buttonState);

                Thread.Sleep(stepDelay);
            }
        }

        private void SendReport(int deltaX, int deltaY, int deltaWheel, Buttons buttonsState) {
            this.report[Mouse.REPORT_FIELD_MOUSE_BUTTON] = (byte)buttonsState;
            this.report[Mouse.REPORT_FIELD_MOUSE_X] = (byte)(deltaX & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_X + 1] = (byte)((deltaX >> 8) & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_Y] = (byte)(deltaY & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_Y + 1] = (byte)((deltaY >> 8) & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_W] = (byte)(deltaWheel & 0xFF);
            this.report[Mouse.REPORT_FIELD_MOUSE_W + 1] = (byte)((deltaWheel >> 8) & 0xFF);

            this.stream.Write(this.report);
        }
    }
}
