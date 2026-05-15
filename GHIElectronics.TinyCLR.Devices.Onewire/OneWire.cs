using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace GHIElectronics.TinyCLR.Devices.Onewire {
    /// <summary>
    /// Software 1-Wire bus master driven from a single GPIO pin. Devices like
    /// DS18B20 temperature sensors and DS24xx ROM chips can be enumerated with
    /// <see cref="FindAllDevices"/>, addressed by 8-byte serial number, and then
    /// transacted with via <see cref="TouchReset"/> / <see cref="WriteByte"/> /
    /// <see cref="ReadByte"/>.
    /// </summary>
    public class OneWireController : IDisposable {
        private GpioPin pin;

        /// <summary>Opens 1-Wire on a pin of the default <see cref="GpioController"/>.</summary>
        /// <param name="pinNumber">GPIO pin number connected to the 1-Wire data line.</param>
        public OneWireController(int pinNumber) : this(GpioController.GetDefault(), pinNumber) {

        }

        /// <summary>Opens 1-Wire on a pin of the supplied <see cref="GpioController"/>.</summary>
        /// <param name="gpioController">GPIO controller owning the data pin.</param>
        /// <param name="pinNumber">GPIO pin number connected to the 1-Wire data line.</param>
        public OneWireController(GpioController gpioController, int pinNumber) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            var gpioApi = p.Api.Implementation;

            this.pin = gpioController.OpenPin(pinNumber);

            this.pin.SetDriveMode(GpioPinDriveMode.Output);
            this.pin.Write(GpioPinValue.Low);

            this.NativeInitialize(gpioApi, this.pin.PinNumber);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInitialize(IntPtr gpioApi, int pinNumber);

        /// <summary>Issues a 1-Wire reset pulse and returns the presence-detect result.</summary>
        /// <returns>Non-zero when at least one slave responded with a presence pulse.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchReset();

        /// <summary>Reads/writes a single bit on the bus (write-then-sample within one slot).</summary>
        /// <param name="sendbit">Bit to drive (0 or 1).</param>
        /// <returns>The bit actually read back from the bus.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchBit(int sendbit);

        /// <summary>Reads/writes a single byte on the bus (LSB first).</summary>
        /// <param name="sendbyte">Byte to send.</param>
        /// <returns>The byte actually read back during the same slot.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchByte(int sendbyte);

        /// <summary>Writes a single byte; the read value is discarded.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteByte(int sendbyte);

        /// <summary>Reads a single byte (drives 0xFF on the bus to sample the slave).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadByte();

        /// <summary>Acquires exclusive access to the 1-Wire net. Returns a port number ≥ 0 on success.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int AcquireEx();

        /// <summary>Releases a previously acquired 1-Wire net.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Release();

        /// <summary>Starts a 1-Wire ROM search. Returns non-zero if a device was found.</summary>
        /// <param name="performResetBeforeSearch">When true, issues a reset pulse before searching.</param>
        /// <param name="searchWithAlarmCommand">When true, uses the alarm-search command (0xEC) instead of the regular search (0xF0).</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindFirstDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        /// <summary>Continues a ROM search started by <see cref="FindFirstDevice"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindNextDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        /// <summary>Reads or writes the 8-byte serial number of the last-discovered device.</summary>
        /// <param name="sNum">Serial-number buffer (8 bytes).</param>
        /// <param name="read">True to read the discovered serial number into <paramref name="sNum"/>; false to write <paramref name="sNum"/> as the active target.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int SerialNum(byte[] sNum, bool read);

        /// <summary>
        /// Enumerates every device on the bus and returns their 8-byte serial numbers.
        /// Acquires and releases the 1-Wire net internally. Returns null if the bus
        /// cannot be acquired.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of byte[8] serial numbers, or null on failure.</returns>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public ArrayList FindAllDevices() {
            int rslt;
            var portnum = 0;

            // attempt to acquire the 1-Wire Net
            if ((portnum = this.AcquireEx()) < 0) {
                //OWERROR_DUMP(stdout);

                // could not get access to 1-wire buss, return null
                return null;
            }

            var serialNumbers = new ArrayList();

            // find the first device (all devices not just alarming)
            rslt = this.FindFirstDevice(true, false);
            while (rslt != 0) {
                var sNum = new byte[8];

                // retrieve the serial number just found
                this.SerialNum(sNum, true);

                // save serial number
                serialNumbers.Add(sNum);

                // find the next device
                rslt = this.FindNextDevice(true, false);
            }

            // release the 1-Wire Net
            this.Release();

            return serialNumbers;
        }

        /// <summary>Closes the GPIO pin.</summary>
        public void Dispose() => this.pin.Dispose();
    }
}
