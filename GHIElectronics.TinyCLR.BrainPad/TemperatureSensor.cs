using System;
using System.ComponentModel;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class TemperatureSensor {
        //private AnalogInput input;
        private AdcChannel input = AdcController.GetDefault().OpenChannel(Board.BoardType == BoardType.BP2 ? FEZCLR.AdcChannel.PB0 : G30.AdcChannel.PB0);
        private int voltage = Board.BoardType == BoardType.BP2 ? 3080 : 3300;   //Compensate for voltage drop from diode D2 on BP2.

        public TemperatureSensor() {
            //input = new AnalogInput(Peripherals.TemperatureSensor);
        }

        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <returns>The temperature in celsius.</returns>
        public double ReadTemperatureInCelsius() {
            double sum = 0;

            //average over 10
            for (var i = 0; i < 10; i++)
                sum += this.input.ReadRatio();// input.Read();

            sum /= 10.0;

            return (sum * this.voltage - 400.0) / 19.5;
        }

        public double ReadTemperatureInFahrenheit() => (9.0 / 5.0) * this.ReadTemperatureInCelsius() + 32.0;

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
