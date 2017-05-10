namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int Mosi = G30.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int Miso = G30.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int Sck = G30.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Cs = G30.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Rst = G30.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int An = G30.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int Pwm = G30.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int Int = G30.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int Rx = G30.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int Tx = G30.GpioPin.PA9;
        }

        /// <summary>ADc channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>ADC channel.</summary>
            public const int An = G30.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int Rst = G30.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int Cs = G30.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int Int = G30.AdcChannel.PA2;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            public const string Id = G30.PwmPin.Controller1.Id;
            /// <summary>PWM pin.</summary>
            public const int Pwm = G30.PwmPin.Controller1.PA8;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            public const string Id = G30.SerialPort.Com1;
        }
    }
}
