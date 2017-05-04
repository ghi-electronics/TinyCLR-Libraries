using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;

namespace GHIElectronics.TinyCLR.Devices {
    public sealed class LowLevelDevicesController {
        public static ILowLevelDevicesAggregateProvider DefaultProvider { get; set; }
        internal static LowLevelDevicesBuiltInProvider BuiltInProvider { get; } = new LowLevelDevicesBuiltInProvider();
    }

    public interface ILowLevelDevicesAggregateProvider {
        IAdcControllerProvider AdcControllerProvider { get; }
        IDacControllerProvider DacControllerProvider { get; }
        IDisplayControllerProvider DisplayControllerProvider { get; }
        IGpioControllerProvider GpioControllerProvider { get; }
        II2cControllerProvider I2cControllerProvider { get; }
        IPwmControllerProvider PwmControllerProvider { get; }
        ISpiControllerProvider SpiControllerProvider { get; }
    }

    public sealed class LowLevelDevicesAggregateProvider : ILowLevelDevicesAggregateProvider {
        public IAdcControllerProvider AdcControllerProvider { get; }
        public IDacControllerProvider DacControllerProvider { get; }
        public IDisplayControllerProvider DisplayControllerProvider { get; }
        public IGpioControllerProvider GpioControllerProvider { get; }
        public II2cControllerProvider I2cControllerProvider { get; }
        public IPwmControllerProvider PwmControllerProvider { get; }
        public ISpiControllerProvider SpiControllerProvider { get; }

        public LowLevelDevicesAggregateProvider(IAdcControllerProvider adc, IDacControllerProvider dac, IDisplayControllerProvider display, IPwmControllerProvider pwm, IGpioControllerProvider gpio, II2cControllerProvider i2c, ISpiControllerProvider spi) {
            this.AdcControllerProvider = adc;
            this.DacControllerProvider = dac;
            this.DisplayControllerProvider = display;
            this.PwmControllerProvider = pwm;
            this.GpioControllerProvider = gpio;
            this.I2cControllerProvider = i2c;
            this.SpiControllerProvider = spi;
        }
    }

    internal sealed class LowLevelDevicesBuiltInProvider {
        public IAdcProvider AdcProvider { get; } = new BuiltInAdcProvider();
        public IDacProvider DacProvider { get; } = new BuiltInDacProvider();
        public IDisplayProvider DisplayProvider { get; } = new BuiltInDisplayProvider();
        public IGpioProvider GpioProvider { get; } = new BuiltInGpioProvider();
        public II2cProvider I2cProvider { get; } = new BuiltInI2cProvider();
        public IPwmProvider PwmProvider { get; } = new BuiltInPwmProvider();
        public ISpiProvider SpiProvider { get; } = new BuiltInSpiProvider();

        internal LowLevelDevicesBuiltInProvider() { }

        private class BuiltInAdcProvider : IAdcProvider {
            public IAdcControllerProvider[] GetControllers() => new[] { DefaultAdcControllerProvider.Instance };
        }

        private class BuiltInDacProvider : IDacProvider {
            public IDacControllerProvider[] GetControllers() => new[] { DefaultDacControllerProvider.Instance };
        }

        private class BuiltInDisplayProvider : IDisplayProvider {
            public IDisplayControllerProvider[] GetControllers() => new[] { DefaultDisplayControllerProvider.Instance };
        }

        private class BuiltInGpioProvider : IGpioProvider {
            public IGpioControllerProvider[] GetControllers() => new[] { DefaultGpioControllerProvider.Instance };
        }

        private class BuiltInI2cProvider : II2cProvider {
            public II2cControllerProvider[] GetControllers() {
                var arr = new II2cControllerProvider[DefaultI2cControllerProvider.Instances.Length];

                for (var i = 0; i < arr.Length; i++)
                    arr[i] = DefaultI2cControllerProvider.Instances[i];

                return arr;
            }
        }

        private class BuiltInPwmProvider : IPwmProvider {
            public IPwmControllerProvider[] GetControllers() {
                var arr = new IPwmControllerProvider[DefaultPwmControllerProvider.Instances.Length];

                for (var i = 0; i < arr.Length; i++)
                    arr[i] = DefaultPwmControllerProvider.Instances[i];

                return arr;
            }
        }

        private class BuiltInSpiProvider : ISpiProvider {
            public ISpiControllerProvider[] GetControllers() {
                var arr = new ISpiControllerProvider[DefaultSpiControllerProvider.Instances.Length];

                for (var i = 0; i < arr.Length; i++)
                    arr[i] = DefaultSpiControllerProvider.Instances[i];

                return arr;
            }
        }
    }
}
