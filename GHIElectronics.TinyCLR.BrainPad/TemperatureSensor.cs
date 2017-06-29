﻿using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.ComponentModel;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class TemperatureSensor {
        //private AnalogInput input;
        private AdcChannel input = AdcController.GetDefault().OpenChannel(Board.BoardType == BoardType.BP2 ? FEZChip.AdcChannel.PB0 : G30.AdcChannel.PB0);

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