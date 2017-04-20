using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    /// <summary>
    /// Provides access to the light sensor on the BrainPad.
    /// </summary>
    public class LightSensor {
        //private AnalogInput input;
        private AdcChannel input = AdcController.GetDefault().OpenChannel(G30.AdcChannel.PB1);
        public LightSensor() {
            //input = new AnalogInput(Peripherals.LightSensor);
        }

        /// <summary>
        /// Reads the light level.
        /// </summary>
        /// <returns>The light level.</returns>
        public int ReadLightLevel() => (int)(this.input.ReadRatio() * 100);
    }
}
