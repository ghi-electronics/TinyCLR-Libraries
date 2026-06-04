using GHIElectronics.TinyCLR.Native;
using System;
using System.Reflection;

namespace System.Device.Pwm {
    public enum PwmPulsePolarity {
        ActiveHigh = 0,
        ActiveLow = 1,
    }

    public class PwmChannel : IDisposable {
        private readonly GHIElectronics.TinyCLR.Devices.Pwm.PwmController controller;
        private readonly GHIElectronics.TinyCLR.Devices.Pwm.PwmChannel channel;
        private bool disposed;

        public int Channel { get; }
        public int Controller {
            get {
                return 0;
            }
        }

        public double Frequency {
            get => this.controller.ActualFrequency;
            set {
                this.ThrowIfDisposed();
                // Controller-wide overload: the per-channel SetDesiredFrequency
                // throws CLR_E_INVALID_OPERATION on hardware that shares one
                // frequency across all of a timer's channels (e.g. STM32L4 / SC13048).
                this.controller.SetDesiredFrequency(value);
            }
        }

        public double DutyCycle {
            get => this.channel.GetActiveDutyCyclePercentage();
            set {
                this.ThrowIfDisposed();
                this.channel.SetActiveDutyCyclePercentage(value);
            }
        }

        public PwmPulsePolarity Polarity {
            get => this.channel.Polarity == GHIElectronics.TinyCLR.Devices.Pwm.PwmPulsePolarity.ActiveHigh
                ? PwmPulsePolarity.ActiveHigh
                : PwmPulsePolarity.ActiveLow;
            set {
                this.ThrowIfDisposed();
                this.channel.Polarity = value == PwmPulsePolarity.ActiveHigh
                    ? GHIElectronics.TinyCLR.Devices.Pwm.PwmPulsePolarity.ActiveHigh
                    : GHIElectronics.TinyCLR.Devices.Pwm.PwmPulsePolarity.ActiveLow;
            }
        }

        protected PwmChannel(int chip, int channel) : this(chip, channel, 400, 0.5) {
        }

        protected PwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage) {
            if (chip < 0)
                throw new ArgumentOutOfRangeException(nameof(chip));

            this.controller = ResolveController(chip);
            this.channel = this.controller.OpenChannel(channel);
            this.Channel = channel;

            // Controller-wide overload: the per-channel SetDesiredFrequency
            // throws CLR_E_INVALID_OPERATION on hardware that shares one
            // frequency across all of a timer's channels (e.g. STM32L4 / SC13048).
            this.controller.SetDesiredFrequency(frequency);
            this.channel.SetActiveDutyCyclePercentage(dutyCyclePercentage);
        }

        public static PwmChannel Create(int chip, int channel) => new PwmChannel(chip, channel);

        public static PwmChannel Create(int chip, int channel, int frequency = 400, double dutyCyclePercentage = 0.5) =>
            new PwmChannel(chip, channel, frequency, dutyCyclePercentage);

        public void Start() {
            this.ThrowIfDisposed();
            this.channel.Start();
        }

        public void Stop() {
            this.ThrowIfDisposed();
            this.channel.Stop();
        }

        public void Dispose() {
            if (this.disposed)
                return;

            this.channel.Dispose();
            this.controller.Dispose();
            this.disposed = true;
        }

        private static GHIElectronics.TinyCLR.Devices.Pwm.PwmController ResolveController(int chip) {
            if (chip < 0)
                throw new ArgumentOutOfRangeException(nameof(chip));

            var deviceName = DeviceInformation.DeviceName;
            if (string.IsNullOrEmpty(deviceName))
                throw new InvalidOperationException("DeviceInformation.DeviceName is not available.");

            var controllerName = $"GHIElectronics.TinyCLR.NativeApis.STM32H7.PwmController\\{chip}";

            if (deviceName.Length >=4 &&
                deviceName[0] == 'S' &&
                deviceName[1] == 'C' &&
                deviceName[2] == '1' &&
                deviceName[3] == '3' ) {
                controllerName = $"GHIElectronics.TinyCLR.NativeApis.STM32L4.PwmController\\{chip}";
            }

            return GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(controllerName);
        }

        private static string ResolveControllerNameFromPins(string deviceName, int oneBasedControllerIndex) {
            var fullTypeName =
                "GHIElectronics.TinyCLR.Pins." +
                deviceName +
                "+Timer+Pwm+Controller" +
                oneBasedControllerIndex.ToString() +
                ", GHIElectronics.TinyCLR.Pins";

            var controller = Type.GetType(fullTypeName);
            var idField = controller?.GetField("Id", BindingFlags.Public | BindingFlags.Static);

            if (idField == null)
                throw new NotSupportedException("PWM controller not found in pins map: " + deviceName + ".Timer.Pwm.Controller" + oneBasedControllerIndex.ToString());

            var idValue = idField.GetValue(null) as string;
            if (string.IsNullOrEmpty(idValue))
                throw new InvalidOperationException("PWM controller ID is empty in pins map.");

            return idValue;
        }

        private void ThrowIfDisposed() {
            if (this.disposed)
                throw new ObjectDisposedException(nameof(PwmChannel));
        }
    }
}
