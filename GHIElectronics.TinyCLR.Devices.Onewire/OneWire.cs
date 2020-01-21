using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace GHIElectronics.TinyCLR.Devices.Onewire {
    public sealed class OneWireController :IDisposable {
        private GpioPin pin;

        public OneWireController(int pinNumber) : this(GpioController.GetDefault(), pinNumber) {

        }

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

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchReset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchBit(int sendbit);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchByte(int sendbyte);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteByte(int sendbyte);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadByte();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int AcquireEx();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Release();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindFirstDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindNextDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int SerialNum(byte[] sNum, bool read);

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

        public void Dispose() => this.pin.Dispose();
    }
}
