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
            public string Id { get; } = Pins.BrainPad.PwmPin.Id;
            /// <summary>PWM pin.</summary>
            public int Pwm { get; } = Pins.BrainPad.PwmPin.Pwm;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public class SerialPortDef {
            public string Id { get; } = Pins.BrainPad.SerialPort.Id;

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
