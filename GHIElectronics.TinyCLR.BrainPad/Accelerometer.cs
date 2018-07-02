using System;
using System.ComponentModel;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Accelerometer {


        //private I2C device;
        private I2cDevice device;
        private byte[] buffer1 = new byte[1];
        private byte[] buffer2 = new byte[2];
        public Accelerometer() {
            var settings = new I2cConnectionSettings(0x1C) {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared
            };
            this.device = I2cDevice.FromId(Board.BoardType == BoardType.BP2 ? FEZCLR.I2cBus.I2c1 : G30.I2cBus.I2c1, settings);
            WriteRegister(0x2A, 0x01);
        }
        private void WriteRegister(byte register, byte data) {
            this.buffer2[0] = register;
            this.buffer2[1] = data;

            this.device.Write(this.buffer2);
        }
        private void ReadRegisters(byte register, byte[] data) {
            this.buffer1[0] = register;

            this.device.WriteRead(this.buffer1, data);
        }

        private int ReadAxis(byte register) {
            this.ReadRegisters(register, this.buffer2);

            var value = (double)(this.buffer2[0] << 2 | this.buffer2[1] >> 6);

            if (value > 511.0)
                value -= 1024.0;

            var res = (int)((value / 256.0) * 100);

            if (this.EnableFullRange)
                return res;

            if (res > 100)
                return 100;

            if (res < -100)
                return -100;

            return res;
        }

        public bool EnableFullRange { get; set; } = false;

        /// <summary>
        /// Reads the acceleration on the y axis.
        /// </summary>
        /// <returns>The acceleration.</returns>
        public int ReadY() => -1 * ReadAxis(0x01);

        /// <summary>
        /// Reads the acceleration on the x axis.
        /// </summary>
        /// <returns>The acceleration.</returns>
        public int ReadX() => ReadAxis(0x03);

        /// <summary>
        /// Reads the acceleration on the z axis.
        /// </summary>
        /// <returns>The acceleration.</returns>
        public int ReadZ() => -1 * ReadAxis(0x05);

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
