using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.ComponentModel;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class LightSensor {
        //private AnalogInput input;
        private AdcChannel input = AdcController.GetDefault().OpenChannel(Board.BoardType == BoardType.BP1 ? FEZChip.AdcChannel.PB1 : G30.AdcChannel.PB1);
        public LightSensor() {
            //input = new AnalogInput(Peripherals.LightSensor);
        }

        /// <summary>
        /// Reads the light level.
        /// </summary>
        /// <returns>The light level.</returns>
        public int ReadLightLevel() => (int)(this.input.ReadRatio() * 100);

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
