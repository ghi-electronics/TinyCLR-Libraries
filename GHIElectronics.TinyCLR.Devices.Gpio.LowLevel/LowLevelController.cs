using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Gpio.LowLevel {
    /// <summary>How a GPIO pin's output driver is wired.</summary>
    public enum OutputType {
        /// <summary>Driver actively drives both high and low.</summary>
        PushPull = 0,
        /// <summary>Driver actively pulls low and floats when set high (use with a pull-up).</summary>
        OpenDrain = 1,
    }

    /// <summary>Internal pull resistor selection.</summary>
    public enum PullDirection {
        /// <summary>No internal pull resistor.</summary>
        None = 0,
        /// <summary>Engage the internal pull-up resistor.</summary>
        PullUp = 1,
        /// <summary>Engage the internal pull-down resistor.</summary>
        PullDown = 2,
    }

    /// <summary>What role the pin plays in the chip's pin-mux fabric.</summary>
    public enum PortMode {
        /// <summary>Plain GPIO input.</summary>
        GpioInput = 0,
        /// <summary>Plain GPIO output.</summary>
        GpioOutput = 1,
        /// <summary>Routed to a peripheral (UART/SPI/I2C/...) selected by <see cref="AlternateFunction"/>.</summary>
        AlternateFunction = 2,
        /// <summary>Pin disconnected from digital logic, used by ADC/DAC.</summary>
        Analog = 3
    }

    /// <summary>Slew-rate / drive-strength setting for output pins.</summary>
    public enum OutputSpeed {
        /// <summary>Slowest slew rate, lowest EMI.</summary>
        Low = 0,
        /// <summary>Moderate slew rate.</summary>
        Medium = 1,
        /// <summary>Fast slew rate.</summary>
        High = 2,
        /// <summary>Maximum slew rate (highest EMI).</summary>
        VeryHigh = 3
    }

    /// <summary>Alternate-function index selecting which peripheral the pin connects to (platform-specific meaning).</summary>
    public enum AlternateFunction {
        /// <summary>Alternate function 0.</summary>
        AF0 = 0,
        /// <summary>Alternate function 1.</summary>
        AF1 = 1,
        /// <summary>Alternate function 2.</summary>
        AF2 = 2,
        /// <summary>Alternate function 3.</summary>
        AF3 = 3,
        /// <summary>Alternate function 4.</summary>
        AF4 = 4,
        /// <summary>Alternate function 5.</summary>
        AF5 = 5,
        /// <summary>Alternate function 6.</summary>
        AF6 = 6,
        /// <summary>Alternate function 7.</summary>
        AF7 = 7,
        /// <summary>Alternate function 8.</summary>
        AF8 = 8,
        /// <summary>Alternate function 9.</summary>
        AF9 = 9,
        /// <summary>Alternate function 10.</summary>
        AF10 = 10,
        /// <summary>Alternate function 11.</summary>
        AF11 = 11,
        /// <summary>Alternate function 12.</summary>
        AF12 = 12,
        /// <summary>Alternate function 13.</summary>
        AF13 = 13,
        /// <summary>Alternate function 14.</summary>
        AF14 = 14,
        /// <summary>Alternate function 15.</summary>
        AF15 = 15
    }

    /// <summary>Bundle of low-level pin settings passed to <see cref="LowLevelController.TransferFeature"/>.</summary>
    public class Settings {
        /// <summary>Pin role (GPIO input/output, alternate function, analog).</summary>
        public PortMode mode;
        /// <summary>Output driver type.</summary>
        public OutputType type;
        /// <summary>Pull-resistor selection.</summary>
        public PullDirection driveDirection;
        /// <summary>Output speed / drive strength.</summary>
        public OutputSpeed speed;
        /// <summary>Alternate-function index when <see cref="mode"/> is <see cref="PortMode.AlternateFunction"/>.</summary>
        public AlternateFunction alternate;
    }

    /// <summary>
    /// Low-level pin-mux helper. Lets a driver re-route or reconfigure pins at a
    /// level finer than the regular <see cref="GpioPin"/> API (slew rate, alternate
    /// function, etc.). Mostly used by peripheral library authors; ordinary apps
    /// don't need this.
    /// </summary>
    static public class LowLevelController {
        static IGpioControllerProvider provider = new GpioControllerApiWrapper(NativeApi.Find(NativeApi.GetDefaultName(NativeApiType.GpioController), NativeApiType.GpioController));
        /// <summary>Applies a low-level pin-mux configuration moving <paramref name="pinSource"/>'s signal to <paramref name="pinDestination"/>.</summary>
        /// <param name="pinSource">Source pin index.</param>
        /// <param name="pinDestination">Destination pin index.</param>
        /// <param name="settings">Mode, type, pulls, speed, and alternate function.</param>
        public static void TransferFeature(int pinSource, int pinDestination, Settings settings) => TransferFeature(pinSource, pinDestination, (uint)settings.mode, (uint)settings.type, (uint)settings.driveDirection, (uint)settings.speed, (uint)settings.alternate);
        private static void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate) => provider.TransferFeature(pinSource, pinDestination, mode, type, direction, speed, alternate);
    }
}
