namespace System.Device.Gpio.Drivers {
    // Compatibility shim so existing .NET IoT code that new's LibGpiodDriver can compile on TinyCLR.
    public sealed class LibGpiodDriver : TinyClrGpioDriver {
        public int ChipNumber { get; }

        public LibGpiodDriver(int chipNumber) : base(checked(chipNumber * 16)) {
            this.ChipNumber = chipNumber;
        }
    }
}
