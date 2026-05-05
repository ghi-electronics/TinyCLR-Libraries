using System;
using System.Collections;
using GHIElectronics.TinyCLR.Devices.Gpio;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Onewire\OneWire.cs.
// Bodies on Desktop are safe no-ops; FindAllDevices returns an empty list.
namespace GHIElectronics.TinyCLR.Devices.Onewire {
    public class OneWireController : IDisposable {
        private GpioPin pin;

        public OneWireController(int pinNumber) : this(GpioController.GetDefault(), pinNumber) {
        }

        public OneWireController(GpioController gpioController, int pinNumber) {
            this.pin = gpioController.OpenPin(pinNumber);
            this.pin.SetDriveMode(GpioPinDriveMode.Output);
            this.pin.Write(GpioPinValue.Low);
        }

        public int TouchReset() => 0;
        public int TouchBit(int sendbit) => 0;
        public int TouchByte(int sendbyte) => 0;
        public int WriteByte(int sendbyte) => 0;
        public int ReadByte() => 0;
        public int AcquireEx() => 0;
        public int Release() => 0;
        public int FindFirstDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand) => 0;
        public int FindNextDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand) => 0;
        public int SerialNum(byte[] sNum, bool read) => 0;

        public ArrayList FindAllDevices() => new ArrayList();

        public void Dispose() => this.pin?.Dispose();
    }
}
