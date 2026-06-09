using GHIElectronics.TinyCLR.Native;
using System;
using System.Reflection;

namespace System.Device.Pwm {
    /// <summary>Polarity of the active part of a PWM pulse.</summary>
    public enum PwmPulsePolarity {
        /// <summary>The pulse is high during the duty cycle.</summary>
        ActiveHigh = 0,
        /// <summary>The pulse is low during the duty cycle.</summary>
        ActiveLow = 1,
    }

    /// <summary>
    /// .NET-style PWM channel. Same surface as <c>System.Device.Pwm.PwmChannel</c>;
    /// internally routes through TinyCLR's PWM driver.
    /// </summary>
    public class PwmChannel : IDisposable {
        private readonly GHIElectronics.TinyCLR.Devices.Pwm.PwmController controller;
        private readonly GHIElectronics.TinyCLR.Devices.Pwm.PwmChannel channel;
        private bool disposed;

        /// <summary>The channel number on the controller.</summary>
        public int Channel { get; }
        /// <summary>The controller (chip) index.</summary>
        public int Controller {
            get {
                return 0;
            }
        }

        /// <summary>The output frequency in Hz. Setting it changes the whole controller's frequency.</summary>
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

        /// <summary>The duty cycle from 0.0 to 1.0.</summary>
        public double DutyCycle {
            get => this.channel.GetActiveDutyCyclePercentage();
            set {
                this.ThrowIfDisposed();
                this.channel.SetActiveDutyCyclePercentage(value);
            }
        }

        /// <summary>The pulse polarity.</summary>
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

        /// <summary>Opens a channel on the given chip at 400 Hz and 50% duty cycle.</summary>
        protected PwmChannel(int chip, int channel) : this(chip, channel, 400, 0.5) {
        }

        /// <summary>Opens a channel on the given chip with the given frequency and duty cycle.</summary>
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

        /// <summary>Opens a channel on the given chip at 400 Hz and 50% duty cycle.</summary>
        public static PwmChannel Create(int chip, int channel) => new PwmChannel(chip, channel);

        /// <summary>Opens a channel on the given chip with the given frequency and duty cycle.</summary>
        public static PwmChannel Create(int chip, int channel, int frequency = 400, double dutyCyclePercentage = 0.5) =>
            new PwmChannel(chip, channel, frequency, dutyCyclePercentage);

        /// <summary>Starts the PWM output.</summary>
        public void Start() {
            this.ThrowIfDisposed();
            this.channel.Start();
        }

        /// <summary>Stops the PWM output.</summary>
        public void Stop() {
            this.ThrowIfDisposed();
            this.channel.Stop();
        }

        /// <summary>Stops the output and releases the channel.</summary>
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
