namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int Mosi = FEZ.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int Miso = FEZ.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int Sck = FEZ.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Cs = FEZ.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Rst = FEZ.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int An = FEZ.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int Pwm = FEZ.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int Int = FEZ.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int Rx = FEZ.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int Tx = FEZ.GpioPin.PA9;
        }

        /// <summary>ADc channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>ADC channel.</summary>
            public const int An = FEZ.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int Rst = FEZ.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int Cs = FEZ.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int Int = FEZ.AdcChannel.PA2;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            public const string Id = FEZ.PwmPin.Controller1.Id;
            /// <summary>PWM pin.</summary>
            public const int Pwm = FEZ.PwmPin.Controller1.PA8;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            public const string Id = FEZ.SerialPort.Com1;
        }
    }
}
