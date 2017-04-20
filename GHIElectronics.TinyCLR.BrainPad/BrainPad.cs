using GHIElectronics.TinyCLR.BrainPad.Internal;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;

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

        private static I2cDevice i2cDisplayDevice;

        public static Accelerometer Accelerometer { get; }
        public static Buttons Buttons { get; }
        public static Buzzer Buzzer { get; }
        public static Display Display { get; }
        public static ImageBuffer ImageBuffer { get; }
        public static LightBulb LightBulb { get; }
        public static LightSensor LightSensor { get; }
        public static ServoMotor ServoMotor { get; }
        public static TemperatureSensor TemperatureSensor { get; }
        public static Wait Wait { get; }

        static BrainPad() {
            BrainPad.Accelerometer = new Accelerometer();
            BrainPad.Buttons = new Buttons();
            BrainPad.Buzzer = new Buzzer();
            BrainPad.Display = new Display();
            BrainPad.ImageBuffer = BrainPad.Display.imageBuffer;
            BrainPad.LightBulb = new LightBulb();
            BrainPad.LightSensor = new LightSensor();
            BrainPad.ServoMotor = new ServoMotor();
            BrainPad.TemperatureSensor = new TemperatureSensor();
            BrainPad.Wait = new Wait();
        }

        /// <summary>
        /// Provides names for the expansion the pins.
        /// </summary>
        public class Pins {
            public class GpioPin {
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
            public class AdcChannel {
                public const int An = G30.AdcChannel.PA7;
                public const int Rst = G30.AdcChannel.PA6;
                public const int Cs = G30.AdcChannel.PC3;
                public const int Int = G30.AdcChannel.PA2;
            }
            public class PwmPin {
                public const string Id = G30.PwmPin.Controller1.Id;
                public const int Pwm = G30.PwmPin.Controller1.PA8;
            }
        }
    }
}
