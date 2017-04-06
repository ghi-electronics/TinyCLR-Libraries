/*

The display code is part GHI Electronics Part from
https://github.com/adafruit/Adafruit_SSD1306/blob/master/Adafruit_SSD1306.cpp
with thie following license...

Software License Agreement (BSD License)
Copyright (c) 2012, Adafruit Industries
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holders nor the
names of its contributors may be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Threading;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The BrainPad class used with GHI Electronics's BrainPad.
    /// </summary>
    public static class BrainPad {
        /// <summary>
        /// A constant value that is always true for endless looping.
        /// </summary>
        public const bool Looping = true;

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(string message) => System.Diagnostics.Debug.WriteLine(message);

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(int message) => WriteDebugMessage(message.ToString());

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(double message) => WriteDebugMessage(message.ToString());

        /// <summary>
        /// Provides access to the servo motor on the BrainPad.
        /// </summary>
        public static class ServoMotor {
            private static PwmPin[] servos;
            //private static bool started;
            // min and max pulse width in milliseconds
            static private double _MinPulseCalibration = 1.0;
            static private double _MaxPulseCalibration = 2.0;
            // invert servos
            static private bool[] invertServo;

            public enum Servo {
                One = 0,
                Two = 1
            }
            static ServoMotor() {
                var PWM = PwmController.FromId(G30.PwmPin.Controller2.Id);
                invertServo = new bool[2]
                {
                false,
                false
                };
                servos = new PwmPin[2]
                {
                PWM.OpenPin(G30.PwmPin.Controller2.PA3),
                PWM.OpenPin(G30.PwmPin.Controller2.PA0)
                };
                PWM.SetDesiredFrequency(1 / 0.020);
                //output = new PWM(Peripherals.ServoMotor, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
                //started = false;
            }
            // Use to calibrate the servo in use. Set it to the minimum pulse width the servo needs, in milliseconds.
            static public double MinPulseCalibration {
                set {
                    if (value > 1.5 || value < 0.1)
                        throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms");
                    _MinPulseCalibration = value;
                }
            }
            // Use to calibrate the servo in use. Set it to the maximum pulse width the servo needs, in milliseconds.
            static public double MaxPulseCalibration {
                set {
                    if (value > 3 || value < 1.6)
                        throw new ArgumentOutOfRangeException("Must be between 1.6 and 3ms");
                    _MaxPulseCalibration = value;
                }
            }
            /// <summary>
            /// Inverts a servo's behavior.
            /// </summary>
            /// <param name="servo">The servo to be inverted.</param>
            static public void Invert(Servo servo, bool invert) => invertServo[(int)servo] = invert;

            /// <summary>
            /// Sets the position of a fixed-type Servo Motor.
            /// </summary>
            /// <param name="position">The position of the servo between 0 and 180 degrees.</param>
            public static void FixedSetPosition(Servo servo, double position) {
                if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");


                PwmController.GetDefault().SetDesiredFrequency(1 / 0.020);// in case we used the other stuff. remove when we fix PWM controllers

                if (invertServo[(int)servo] == true)
                    position = 180 - position;

                // Typically, with 50 hz, 0 degree is 0.05 and 180 degrees is 0.10
                //double duty = ((position / 180.0) * (0.10 - 0.05)) + 0.05;
                var duty = ((position / 180.0) * (_MaxPulseCalibration / 20 - _MinPulseCalibration / 20)) + _MinPulseCalibration / 20;


                servos[(int)servo].SetActiveDutyCyclePercentage(duty);
                servos[(int)servo].Start();
            }

            /// <summary>
            /// Sets the position of a continous-type Servo Motor.
            /// </summary>
            /// <param name="speed">The speed of the servo between -100 and 100 percent.</param>
            public static void ContiniousSetSpeed(Servo servo, int speed) {
                if (speed < -100 || speed > 100) throw new ArgumentOutOfRangeException("speed", "degrees must be between -100 and 100.");

                speed += 100;
                var d = speed / 200.0 * 180;
                FixedSetPosition(servo, (int)d);
            }


            /// <summary>
            /// Stops the servo motor.
            /// </summary>
            public static void Stop(Servo servo) => servos[(int)servo].Stop();
        }

        /// <summary>
        /// Provides access to the light bulb on the BrainPad.
        /// </summary>
        public static class LightBulb {
            private static PwmPin red;
            private static PwmPin green;
            private static PwmPin blue;
            //private static bool started;

            static LightBulb() {
                //started = false;

                var PWM = PwmController.FromId(G30.PwmPin.Controller3.Id);
                PWM.SetDesiredFrequency(10000);
                red = PWM.OpenPin(G30.PwmPin.Controller3.PC9);
                green = PWM.OpenPin(G30.PwmPin.Controller3.PC8);
                blue = PWM.OpenPin(G30.PwmPin.Controller3.PC7);
                // red = new PWM(Peripherals.LightBulb.Red, 10000, 1, false);
                // green = new PWM(Peripherals.LightBulb.Green, 10000, 1, false);
                //blue = new PWM(Peripherals.LightBulb.Blue, 10000, 1, false);
                red.Start();
                green.Start();
                blue.Start();

                SetRgbColor(0, 0, 0);
            }

            /// <summary>
            /// Sets the color of the light bulb.
            /// </summary>
            /// <param name="r">The red value of the color between 0 (fully off) and 100 (fully on).</param>
            /// <param name="g">The green value of the color between 0 (fully off) and 100 (fully on).</param>
            /// <param name="blue">The blue value of the color between 0 (fully off) and 100 (fully on).</param>
            public static void SetRgbColor(double r, double g, double b) {
                if (r < 0 || r > 100) throw new ArgumentOutOfRangeException("red", "red must be between zero and one hundred.");
                if (g < 0 || g > 100) throw new ArgumentOutOfRangeException("green", "green must be between zero and one hundred.");
                if (b < 0 || b > 100) throw new ArgumentOutOfRangeException("blue", "blue must be between zero and one hundred.");

                red.SetActiveDutyCyclePercentage(r / 100);
                green.SetActiveDutyCyclePercentage(g / 100);
                blue.SetActiveDutyCyclePercentage(b / 100);
            }

            /// <summary>
            /// Turns off the light bulb.
            /// </summary>
            public static void TurnOff() => SetRgbColor(0, 0, 0);

            /// <summary>
            /// Turns the light bulb on.
            /// </summary>
            public static void TurnOn() => SetRgbColor(100, 100, 100);

            /// <summary>
            /// Turns the light bulb Red.
            /// </summary>
            public static void TurnRed() => SetRgbColor(100, 0, 0);

            /// <summary>
            /// Turns the light bulb Green.
            /// </summary>
            public static void TurnGreen() => SetRgbColor(0, 100, 0);

            /// <summary>
            /// Turns the light bulb Blue.
            /// </summary>
            public static void TurnBlue() => SetRgbColor(0, 0, 100);
        }

        /// <summary>
        /// Provides access to the buzzer on the BrainPad.
        /// </summary>
        public static class Buzzer {
            private static PwmController controller;
            private static PwmPin buzz;

            static Buzzer() {
                Buzzer.controller = PwmController.FromId(G30.PwmPin.Controller4.Id);
                Buzzer.buzz = Buzzer.controller.OpenPin(G30.PwmPin.Controller4.PB8);
            }

            /// <summary>
            /// Starts a given frequency.
            /// </summary>
            /// <param name="frequency">The frequency to play.</param>
            public static void Start(double frequency) {
                Stop();
                if (frequency > 0) {
                    controller.SetDesiredFrequency(frequency);
                    buzz.Start();
                    buzz.SetActiveDutyCyclePercentage(0.5);
                }
            }

            /// <summary>
            /// Makes a short beep sound.
            /// </summary>
            public static void Beep() {
                Start(2000);
                Wait.Milliseconds(5);
                Stop();
            }

            /// <summary>
            /// Stops any note or frequency currently playing.
            /// </summary>
            public static void Stop() => buzz.Stop();
        }

        /// <summary>
        /// Provices access to the buttons on the BrainPad.
        /// </summary>
        public static class Buttons {
            //private static InterruptPort[] ports;
            private static GpioPin[] buttons;
            /// <summary>
            /// The avilable buttons.
            /// </summary>
            public enum Button {
                Left = 0,
                Select = 1,
                Right = 2
            }

            static Buttons() {
                var GPIO = GpioController.GetDefault();
                buttons = new GpioPin[]
                {
#if true
                // new BrainPad
                GPIO.OpenPin(G30.GpioPin.PA15),
                GPIO.OpenPin(G30.GpioPin.PB10),
                GPIO.OpenPin(G30.GpioPin.PC13)
#else
                // old
                GPIO.OpenPin(G30.GpioPin.PB10),
                GPIO.OpenPin(G30.GpioPin.PC13),
                GPIO.OpenPin(G30.GpioPin.PA5)
#endif
                };
                foreach (var button in buttons) {
                    button.SetDriveMode(GpioPinDriveMode.InputPullUp);
                    button.ValueChanged += Button_ValueChanged;
                }
            }
            /// <summary>
            /// Returns a friendly name (string) for a specific button.
            /// </summary>
            /// <param name="button">The button needed.</param>
            /// <returns>The name of that button.</returns>
            static public string GetFriendlyName(Button button) {
                string str;
                switch (button) {
                    case BrainPad.Buttons.Button.Left:
                        str = "Left";
                        break;
                    case BrainPad.Buttons.Button.Right:
                        str = "Right";
                        break;
                    case BrainPad.Buttons.Button.Select:
                        str = "Select";
                        break;
                    default:
                        str = "Unknown!";
                        break;
                }
                return str;
            }
            /// <summary>
            /// The signature of all button events.
            /// </summary>
            /// <param name="button">The button in question.</param>
            public delegate void ButtonEventHandler(Button button);

            /// <summary>
            /// The event raised when a button is released.
            /// </summary>
            public static event ButtonEventHandler ButtonReleased;
            public static event ButtonEventHandler ButtonPressed;
            private static void Button_ValueChanged(object sender, GpioPinValueChangedEventArgs e) {

                for (var i = 0; i < 3; i++) {
                    if (((GpioPin)sender).PinNumber == buttons[i].PinNumber) {
                        if (e.Edge == GpioPinEdge.FallingEdge)
                            ButtonPressed?.Invoke((Button)i);
                        else
                            ButtonReleased?.Invoke((Button)i);
                    }
                }
            }

            /// <summary>
            /// Is the select button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsUpPressed() => IsPressed(Button.Select);

            /// <summary>
            /// Is the left button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsLeftPressed() => IsPressed(Button.Left);

            /// <summary>
            /// Is the right button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsRightPressed() => IsPressed(Button.Right);

            /// <summary>
            /// Checks if a button is pressed.
            /// </summary>
            /// <param name="button">The button to check.</param>
            /// <returns>Whether or not it was pressed.</returns>
            public static bool IsPressed(Button button) => buttons[(int)button].Read() == GpioPinValue.Low;// it is low when it is pressed
        }

        /// <summary>
        /// Provides access to the traffic light on the BrainPad.
        /// </summary>
        public static class TrafficLight {

            private static GpioPin red = GpioController.GetDefault().OpenPin(G30.GpioPin.PA1);
            private static GpioPin yellow = GpioController.GetDefault().OpenPin(G30.GpioPin.PC6);
            private static GpioPin green = GpioController.GetDefault().OpenPin(G30.GpioPin.PB9);

            static TrafficLight() {
                red.SetDriveMode(GpioPinDriveMode.Output);
                yellow.SetDriveMode(GpioPinDriveMode.Output);
                green.SetDriveMode(GpioPinDriveMode.Output);
            }

            /// <summary>
            /// Turns the red light on.
            /// </summary>
            public static void TurnRedLightOn() =>
                //TurnColorOn(Color.Red);
                red.Write(GpioPinValue.High);

            /// <summary>
            /// Turns the red light off.
            /// </summary>
            public static void TurnRedLightOff() =>
                //TurnColorOff(Color.Red);
                red.Write(GpioPinValue.Low);

            /// <summary>
            /// Turns the yellow light on.
            /// </summary>
            public static void TurnYellowLightOn() =>
                //TurnColorOn(Color.Yellow);
                yellow.Write(GpioPinValue.High);

            /// <summary>
            /// Turns the yellow light off.
            /// </summary>
            public static void TurnYellowLightOff() =>
                //TurnColorOff(Color.Yellow);
                yellow.Write(GpioPinValue.Low);

            /// <summary>
            /// Turns the green light on.
            /// </summary>
            public static void TurnGreenLightOn() =>
                //TurnColorOn(Color.Green);
                green.Write(GpioPinValue.High);

            /// <summary>
            /// Turns the green light off.
            /// </summary>
            public static void TurnGreenLightOff() =>
                //TurnColorOff(Color.Green);
                green.Write(GpioPinValue.Low);
        }

        /// <summary>
        /// Provides access to the light sensor on the BrainPad.
        /// </summary>
        public static class LightSensor {
            //private static AnalogInput input;
            private static AdcChannel input = AdcController.GetDefault().OpenChannel(G30.AdcChannel.PB1);
            static LightSensor() {
                //input = new AnalogInput(Peripherals.LightSensor);
            }

            /// <summary>
            /// Reads the light level.
            /// </summary>
            /// <returns>The light level.</returns>
            public static int ReadLightLevel() => (int)(input.ReadRatio() * 100);
        }

        /// <summary>
        /// Provides access to the temperature sensor on the BrainPad.
        /// </summary>
        public static class TemperatureSensor {
            //private static AnalogInput input;
            private static AdcChannel input = AdcController.GetDefault().OpenChannel(G30.AdcChannel.PB0);

            static TemperatureSensor() {
                //input = new AnalogInput(Peripherals.TemperatureSensor);
            }

            /// <summary>
            /// Reads the temperature.
            /// </summary>
            /// <returns>The temperature in celsius.</returns>
            public static double ReadTemperature() {
                double sum = 0;

                //average over 10
                for (var i = 0; i < 10; i++)
                    sum += input.ReadRatio();// input.Read();

                sum /= 10.0;

                return (sum * 3300.0 - 450.0) / 19.5;
            }
        }

        /// <summary>
        /// Provices access to the accelerometer on the BrainPad.
        /// </summary>
        public static class Accelerometer {


            //private static I2C device;
            private static I2cDevice device;
            private static byte[] buffer1 = new byte[1];
            private static byte[] buffer2 = new byte[2];
            static Accelerometer() {
                var settings = new I2cConnectionSettings(0x1C) {
                    BusSpeed = I2cBusSpeed.FastMode
                };
                var aqs = I2cDevice.GetDeviceSelector("I2C1");
                device = I2cDevice.FromId(aqs, settings);


                // device.WriteRegister(0x2A, 0x01);
                WriteRegister(0x2A, 0x01);
            }
            private static void WriteRegister(byte register, byte data) {
                buffer2[0] = register;
                buffer2[1] = data;

                device.Write(buffer2);
            }
            private static void ReadRegisters(byte register, byte[] data) {
                buffer1[0] = register;

                device.WriteRead(buffer1, data);
            }
            private static double ReadAxis(byte register) {
                // device.ReadRegisters(register, buffer);
                ReadRegisters(register, buffer2);
                var value = (double)(buffer2[0] << 2 | buffer2[1] >> 6);

                if (value > 511.0)
                    value -= 1024.0;

                return value / 256.0 * 100;
            }

            /// <summary>
            /// Reads the acceleration on the x axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadX() => ReadAxis(0x01);

            /// <summary>
            /// Reads the acceleration on the y axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadY() => ReadAxis(0x03);

            /// <summary>
            /// Reads the acceleration on the z axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadZ() => ReadAxis(0x05);
        }
        private static I2cDevice i2cDisplayDevice;
        public static class Display {
            static private void Ssd1306_command(int cmd) {

                var buff = new byte[2];
                buff[1] = (byte)cmd;
                i2cDisplayDevice.Write(buff);


            }

            static Display() {
                //buffer1 = new byte[1];
                //buffer2 = new byte[2];
                //buffer4 = new byte[4];
                //clearBuffer = new byte[160 * 2 * 16];
                //characterBuffer1 = new byte[80];
                //characterBuffer2 = new byte[320];
                //characterBuffer4 = new byte[1280];
                /*
                            controlPin = new OutputPort(Peripherals.Display.Control, false);
                            resetPin = new OutputPort(Peripherals.Display.Reset, false);
                            backlightPin = new OutputPort(Peripherals.Display.Backlight, true);
                            spi = new SPI(new SPI.Configuration(Peripherals.Display.ChipSelect, false, 0, 0, false, true, 12000, Peripherals.Display.SpiModule));
                            */

                var settings = new I2cConnectionSettings(0x3C) {
                    BusSpeed = I2cBusSpeed.FastMode
                };
                var aqs = I2cDevice.GetDeviceSelector("I2C1");
                i2cDisplayDevice = I2cDevice.FromId(aqs, settings);

                // Init sequence
                Ssd1306_command(0xae);// SSD1306_DISPLAYOFF);                    // 0xAE
                Ssd1306_command(0xd5);// SSD1306_SETDISPLAYCLOCKDIV);            // 0xD5
                Ssd1306_command(0x80);                                  // the suggested ratio 0x80

                Ssd1306_command(0xa8);// SSD1306_SETMULTIPLEX);                  // 0xA8
                Ssd1306_command(64 - 1);

                Ssd1306_command(0xd3);// SSD1306_SETDISPLAYOFFSET);              // 0xD3
                Ssd1306_command(0x0);                                   // no offset
                Ssd1306_command(0x40);// SSD1306_SETSTARTLINE | 0x0);            // line #0
                Ssd1306_command(0x8d);// SSD1306_CHARGEPUMP);                    // 0x8D

                //if (false)//vccstate == SSD1306_EXTERNALVCC)
                //
                //{ Ssd1306_command(0x10); }
                //else {
                Ssd1306_command(0x14);
                //}

                Ssd1306_command(0x20);// SSD1306_MEMORYMODE);                    // 0x20
                Ssd1306_command(0x00);                                  // 0x0 act like ks0108
                Ssd1306_command(0xa1);// SSD1306_SEGREMAP | 0x1);
                Ssd1306_command(0xc8);// SSD1306_COMSCANDEC);


                Ssd1306_command(0xda);// SSD1306_SETCOMPINS);                    // 0xDA
                Ssd1306_command(0x12);
                Ssd1306_command(0x81);// SSD1306_SETCONTRAST);                   // 0x81

                //if (false)//vccstate == SSD1306_EXTERNALVCC)
                //{ Ssd1306_command(0x9F); }
                //else {
                Ssd1306_command(0xCF);
                //}

                Ssd1306_command(0xd9);// SSD1306_SETPRECHARGE);                  // 0xd9

                //if (false)//vccstate == SSD1306_EXTERNALVCC)
                //{ Ssd1306_command(0x22); }
                //else {
                Ssd1306_command(0xF1);
                //}

                Ssd1306_command(0xd8);// SSD1306_SETVCOMDETECT);                 // 0xDB
                Ssd1306_command(0x40);
                Ssd1306_command(0xa4);//SSD1306_DISPLAYALLON_RESUME);           // 0xA4
                Ssd1306_command(0xa6);// SSD1306_NORMALDISPLAY);                 // 0xA6

                Ssd1306_command(0x2e);// SSD1306_DEACTIVATE_SCROLL);

                Ssd1306_command(0xaf);// SSD1306_DISPLAYON);//--turn on oled panel


                Ssd1306_command(0x21);// SSD1306_COLUMNADDR);
                Ssd1306_command(0);   // Column start address (0 = reset)
                Ssd1306_command(128 - 1); // Column end address (127 = reset)
                Ssd1306_command(0x22);// SSD1306_PAGEADDR);
                Ssd1306_command(0); // Page start address (0 = reset)
                Ssd1306_command(7); // Page end address


                ImageBuffer.Clear();
                ImageBuffer.ShowOnScreen();
            }


            /// <summary>
            /// Draws text at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            public static void DrawText(int x, int y, string text) {
                ImageBuffer.Clear();
                ImageBuffer.DrawText(x, y, text, 2, 2);
                ImageBuffer.ShowOnScreen();
            }
            public static void DrawSmallText(int x, int y, string text) {
                ImageBuffer.Clear();
                ImageBuffer.DrawText(x, y, text, 1, 1);
                ImageBuffer.ShowOnScreen();
            }
            public static void DrawText(int x, int y, double number) => DrawText(x, y, number.ToString("N2"));
            public static void DrawSmallText(int x, int y, double number) => DrawSmallText(x, y, number.ToString("N2"));

        }
        /// <summary>
        /// Controls the display on the BrainPad.
        /// </summary>
        public static class ImageBuffer {

            /// <summary>
            /// The width of the display in pixels.
            /// </summary>
            public const int Width = 128;

            /// <summary>
            /// The height of the display in pixels.
            /// </summary>
            public const int Height = 64;

            static private byte[] vram = new byte[(128 * 64 / 8) + 1];


            //private static byte[] buffer1;
            //private static byte[] buffer2;
            //private static byte[] buffer4;
            public enum Color {
                Black,
                White
            }



            static public void ShowOnScreen() =>

                //Display.Render(vram);
                i2cDisplayDevice.Write(vram);

            /// <summary>
            /// Draws a pixel.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="color">The color to draw.</param>
            static void DrawPoint(int x, int y, Color color = Color.White) {
                if (x < 0 || x > 127)
                    return;
                if (y < 0 || y > 63)
                    return;
                var index = (x + (y / 8) * 128) + 1;

                if (color == Color.White)
                    vram[index] |= (byte)(1 << (y % 8));
                else
                    vram[index] &= (byte)(~(1 << (y % 8)));
            }

            /// <summary>
            /// Clears the Display.
            /// </summary>
            public static void Clear() {
                Array.Clear(vram, 0, vram.Length);
                vram[0] = 0x40;
            }

            /// <summary>
            /// Draws an image.
            /// </summary>
            /// <param name="data">The image as a byte array.</param>
            /*public static void DrawImage(byte[] data)
            {
                if (data == null) throw new ArgumentNullException("data");
                if (data.Length == 0) throw new ArgumentException("data.Length must not be zero.", "data");

                WriteCommand(0x2C);
                WriteData(data);
            }*/

            /// <summary>
            /// Draws an image at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="image">The image to draw.</param>
            /*public static void DrawImage(int x, int y, Image image)
            {
                if (image == null) throw new ArgumentNullException("image");
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                SetClip(x, y, image.Width, image.Height);
                DrawImage(image.Pixels);
            }*/


            /// <summary>
            /// Draws a line.
            /// </summary>
            /// <param name="x">The x coordinate to start drawing at.</param>
            /// <param name="y">The y coordinate to start drawing at.</param>
            /// <param name="x1">The ending x coordinate.</param>
            /// <param name="y1">The ending y coordinate.</param>
            /// <param name="color">The color to draw.</param>
            public static void DrawLine(int x0, int y0, int x1, int y1, Color color = Color.White) {
                if (x0 < 0) throw new ArgumentOutOfRangeException("x0", "x0 must not be negative.");
                if (y0 < 0) throw new ArgumentOutOfRangeException("y0", "y0 must not be negative.");
                if (x1 < 0) throw new ArgumentOutOfRangeException("x1", "x1 must not be negative.");
                if (y1 < 0) throw new ArgumentOutOfRangeException("y1", "y1 must not be negative.");

                var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
                int t, dX, dY, yStep, error;

                if (steep) {
                    t = x0;
                    x0 = y0;
                    y0 = t;
                    t = x1;
                    x1 = y1;
                    y1 = t;
                }

                if (x0 > x1) {
                    t = x0;
                    x0 = x1;
                    x1 = t;

                    t = y0;
                    y0 = y1;
                    y1 = t;
                }

                dX = x1 - x0;
                dY = System.Math.Abs(y1 - y0);

                error = (dX / 2);

                if (y0 < y1) {
                    yStep = 1;
                }
                else {
                    yStep = -1;
                }

                for (; x0 < x1; x0++) {
                    if (steep) {
                        DrawPoint(y0, x0, color);
                    }
                    else {
                        DrawPoint(x0, y0, color);
                    }

                    error -= dY;

                    if (error < 0) {
                        y0 += (byte)yStep;
                        error += dX;
                    }
                }
            }

            /// <summary>
            /// Draws a circle.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="r">The radius of the circle.</param>
            /// <param name="color">The color to draw.</param>
            public static void DrawCircle(int x, int y, int r, Color color = Color.White) {
                //if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                //if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (r <= 0) return;// throw new ArgumentOutOfRangeException("radius", "radius must be positive.");

                var f = 1 - r;
                var ddFX = 1;
                var ddFY = -2 * r;
                var dX = 0;
                var dY = r;

                DrawPoint(x, y + r, color);
                DrawPoint(x, y - r, color);
                DrawPoint(x + r, y, color);
                DrawPoint(x - r, y, color);

                while (dX < dY) {
                    if (f >= 0) {
                        dY--;
                        ddFY += 2;
                        f += ddFY;
                    }

                    dX++;
                    ddFX += 2;
                    f += ddFX;

                    DrawPoint(x + dX, y + dY, color);
                    DrawPoint(x - dX, y + dY, color);
                    DrawPoint(x + dX, y - dY, color);
                    DrawPoint(x - dX, y - dY, color);

                    DrawPoint(x + dY, y + dX, color);
                    DrawPoint(x - dY, y + dX, color);
                    DrawPoint(x + dY, y - dX, color);
                    DrawPoint(x - dY, y - dX, color);
                }
            }

            /// <summary>
            /// Draws a rectangle.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="width">The width of the rectangle.</param>
            /// <param name="height">The height of the rectangle.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawRectangle(int x, int y, int width, int height, Color color = Color.White) {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
                if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

                for (var i = x; i < x + width; i++) {
                    DrawPoint(i, y, color);
                    DrawPoint(i, y + height - 1, color);
                }

                for (var i = y; i < y + height; i++) {
                    DrawPoint(x, i, color);
                    DrawPoint(x + width - 1, i, color);
                }
            }

            static byte[] font = new byte[95 * 5] {
            0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
            0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
            0x00, 0x07, 0x00, 0x07, 0x00, /* " */
            0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
            0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
            0x23, 0x13, 0x08, 0x64, 0x62, /* % */
            0x36, 0x49, 0x55, 0x22, 0x20, /* & */
            0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
            0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
            0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
            0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
            0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
            0x50, 0x30, 0x00, 0x00, 0x00, /* , */
            0x08, 0x08, 0x08, 0x08, 0x08, /* - */
            0x00, 0x60, 0x60, 0x00, 0x00, /* . */
            0x20, 0x10, 0x08, 0x04, 0x02, /* / */
            0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
            0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
            0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
            0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
            0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
            0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
            0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
            0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
            0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
            0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
            0x00, 0x36, 0x36, 0x00, 0x00, /* : */
            0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
            0x08, 0x14, 0x22, 0x41, 0x00, /* < */
            0x14, 0x14, 0x14, 0x14, 0x14, /* = */
            0x00, 0x41, 0x22, 0x14, 0x08, /* > */
            0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
            0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
            0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
            0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
            0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
            0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
            0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
            0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
            0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
            0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
            0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
            0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
            0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
            0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
            0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
            0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
            0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
            0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
            0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
            0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
            0x26, 0x49, 0x49, 0x49, 0x32, /* S */
            0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
            0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
            0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
            0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
            0x63, 0x14, 0x08, 0x14, 0x63, /* X */
            0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
            0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
            0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
            0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
            0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
            0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
            0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
            0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
            0x20, 0x54, 0x54, 0x54, 0x78, /* a */
            0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
            0x38, 0x44, 0x44, 0x44, 0x44, /* c */
            0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
            0x38, 0x54, 0x54, 0x54, 0x18, /* e */
            0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
            0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
            0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
            0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
            0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
            0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
            0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
            0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
            0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
            0x38, 0x44, 0x44, 0x44, 0x38, /* o */
            0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
            0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
            0x7c, 0x08, 0x04, 0x04, 0x00, /* r */
            0x48, 0x54, 0x54, 0x54, 0x24, /* s */
            0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
            0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
            0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
            0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
            0x44, 0x28, 0x10, 0x28, 0x44, /* x */
            0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
            0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
            0x08, 0x36, 0x41, 0x41, 0x00, /* { */
            0x00, 0x00, 0x77, 0x00, 0x00, /* | */
            0x00, 0x41, 0x41, 0x36, 0x08, /* } */
            0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

            private static void DrawText(int x, int y, char letter, int HscaleFactor, int VscaleFactor) {
                var index = 5 * (letter - 32);

                for (var h = 0; h < 5; h++) {
                    for (var hs = 0; hs < HscaleFactor; hs++) {
                        for (var v = 0; v < 8; v++) {
                            var show = (font[index + h] & (1 << v)) != 0;
                            for (var vs = 0; vs < VscaleFactor; vs++) {
                                DrawPoint(x + (h * HscaleFactor) + hs, y + (v * VscaleFactor) + vs, show ? Color.White : Color.Black);
                            }
                        }

                    }
                }

            }

            /// <summary>
            /// Draws text at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="text">The string to draw.</param>
            public static void DrawText(int x, int y, string text, int HScale, int VScale) {
                var originalX = x;

                if (text == null) throw new ArgumentNullException("data");

                for (var i = 0; i < text.Length; i++) {
                    if (text[i] >= 32) {
                        DrawText(x, y, text[i], HScale, VScale);
                        x += (6 * HScale);
                    }
                    else {
                        if (text[i] == '\n') {
                            y += (9 * VScale);
                        }
                        if (text[i] == '\r') {
                            x = originalX;
                        }
                    }
                }
            }



        }

        /// <summary>
        /// Tells the BrainPad to wait.
        /// </summary>
        public static class Wait {
            /// <summary>
            /// Tells the BrainPad to wait for the given number of seconds.
            /// </summary>
            /// <param name="seconds">The number of seconds to wait.</param>
            public static void Seconds(double seconds) {
                if (seconds < 0.0) throw new ArgumentOutOfRangeException("seconds", "seconds must not be negative.");

                Thread.Sleep((int)(seconds * 1000));
            }

            /// <summary>
            /// Tells the BrainPad to wait for the given number of milliseconds.
            /// </summary>
            /// <param name="milliseconds">The number of milliseconds to wait.</param>
            public static void Milliseconds(double milliseconds) {
                if (milliseconds < 0) throw new ArgumentOutOfRangeException("milliseconds", "milliseconds must not be negative.");

                Thread.Sleep((int)milliseconds);
            }
        }

        /// <summary>
        /// Provides names for the expansion the pins.
        /// </summary>
        public static class Pins {
            public static class GpioPin {
                public const int Mosi = G30.GpioPin.PB5;
                public const int Miso = G30.GpioPin.PB4;
                public const int Sck = G30.GpioPin.PB3;
                public const int Cs = G30.GpioPin.PC3;
                public const int Rst = G30.GpioPin.PA6;
                public const int An = G30.GpioPin.PA7;
                public const int Pwm = G30.GpioPin.PA8;
                public const int Int = G30.GpioPin.PA2;
                public const int Rx = G30.GpioPin.PA10;
                public const int Tx = G30.GpioPin.PA9;
            }
            public static class AdcChannel {
                public const int An = G30.AdcChannel.PA7;
                public const int Rst = G30.AdcChannel.PA6;
                public const int Cs = G30.AdcChannel.PC3;
                public const int Int = G30.AdcChannel.PA2;
            }
            public static class PwmPin {
                public const string Id = G30.PwmPin.Controller1.Id;
                public const int Pwm = G30.PwmPin.Controller1.PA8;
            }
        }
    }
}
