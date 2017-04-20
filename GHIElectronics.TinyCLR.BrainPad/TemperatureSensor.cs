using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    /// <summary>
    /// Provides access to the temperature sensor on the BrainPad.
    /// </summary>
    public class TemperatureSensor {
        //private AnalogInput input;
        private AdcChannel input = AdcController.GetDefault().OpenChannel(G30.AdcChannel.PB0);

        public TemperatureSensor() {
            //input = new AnalogInput(Peripherals.TemperatureSensor);
        }

        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <returns>The temperature in celsius.</returns>
        public double ReadTemperature() {
            double sum = 0;

            //average over 10
            for (var i = 0; i < 10; i++)
                sum += this.input.ReadRatio();// input.Read();

            sum /= 10.0;

            return (sum * 3300.0 - 450.0) / 19.5;
        }
    }
}
