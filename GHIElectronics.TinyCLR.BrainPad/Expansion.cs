using System;
using System.ComponentModel;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Expansion {
        public class GpioPinDef {
            /// <summary>GPIO pin.</summary>
            public int Mosi { get; } = Pins.BrainPad.GpioPin.Mosi;
            /// <summary>GPIO pin.</summary>
            public int Miso { get; } = Pins.BrainPad.GpioPin.Miso;
            /// <summary>GPIO pin.</summary>
            public int Sck { get; } = Pins.BrainPad.GpioPin.Sck;
            /// <summary>GPIO pin.</summary>
            public int Cs { get; } = Pins.BrainPad.GpioPin.Cs;
            /// <summary>GPIO pin.</summary>
            public int Rst { get; } = Pins.BrainPad.GpioPin.Rst;
            /// <summary>GPIO pin.</summary>
            public int An { get; } = Pins.BrainPad.GpioPin.An;
            /// <summary>GPIO pin.</summary>
            public int Pwm { get; } = Pins.BrainPad.GpioPin.Pwm;
            /// <summary>GPIO pin.</summary>
            public int Int { get; } = Pins.BrainPad.GpioPin.Int;
            /// <summary>GPIO pin.</summary>
            public int Rx { get; } = Pins.BrainPad.GpioPin.Rx;
            /// <summary>GPIO pin.</summary>
            public int Tx { get; } = Pins.BrainPad.GpioPin.Tx;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public class AdcChannelDef {
            /// <summary>ADC channel.</summary>
            public int An { get; } = Pins.BrainPad.AdcChannel.An;
            /// <summary>ADC channel.</summary>
            public int Rst { get; } = Pins.BrainPad.AdcChannel.Rst;
            /// <summary>ADC channel.</summary>
            public int Cs { get; } = Pins.BrainPad.AdcChannel.Cs;
            /// <summary>ADC channel.</summary>
            public int Int { get; } = Pins.BrainPad.AdcChannel.Int;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public class PwmPinDef {
            /// <summary>PWM controller.</summary>
            public Controller1Def Controller1 { get; } = new Controller1Def();
            /// <summary>PWM controller.</summary>
            public Controller2Def Controller2 { get; } = new Controller2Def();

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();

            public class Controller1Def {
                public string Id { get; } = Pins.BrainPad.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public int Pwm { get; } = Pins.BrainPad.PwmPin.Controller1.Pwm;
                /// <summary>PWM pin.</summary>
                public int Rx { get; } = Pins.BrainPad.PwmPin.Controller1.Rx;
                /// <summary>PWM pin.</summary>
                public int Tx { get; } = Pins.BrainPad.PwmPin.Controller1.Tx;

                [EditorBrowsable(EditorBrowsableState.Never)]
                public override bool Equals(object obj) => base.Equals(obj);
                [EditorBrowsable(EditorBrowsableState.Never)]
                public override int GetHashCode() => base.GetHashCode();
                [EditorBrowsable(EditorBrowsableState.Never)]
                public override string ToString() => base.ToString();
                [EditorBrowsable(EditorBrowsableState.Never)]
                public new Type GetType() => base.GetType();
            }

            public class Controller2Def {
                public string Id { get; } = Pins.BrainPad.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public int Int { get; } = Pins.BrainPad.PwmPin.Controller2.Int;

                [EditorBrowsable(EditorBrowsableState.Never)]
                public override bool Equals(object obj) => base.Equals(obj);
                [EditorBrowsable(EditorBrowsableState.Never)]
                public override int GetHashCode() => base.GetHashCode();
                [EditorBrowsable(EditorBrowsableState.Never)]
                public override string ToString() => base.ToString();
                [EditorBrowsable(EditorBrowsableState.Never)]
                public new Type GetType() => base.GetType();
            }
        }

        public class SerialPortDef {
            public string Id { get; } = Pins.BrainPad.SerialPort.Com1;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public class I2cBusDef {
            public string Id { get; } = Pins.BrainPad.I2cBus.I2c1;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public class SpiBusDef {
            public string Id { get; } = Pins.BrainPad.SpiBus.Spi1;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public GpioPinDef GpioPin { get; } = new GpioPinDef();
        public AdcChannelDef AdcChannel { get; } = new AdcChannelDef();
        public PwmPinDef PwmPin { get; } = new PwmPinDef();
        public SerialPortDef SerialPort { get; } = new SerialPortDef();
        public I2cBusDef I2cBus { get; } = new I2cBusDef();
        public SpiBusDef SpiBus { get; } = new SpiBusDef();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();
    }
}
