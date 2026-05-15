using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    /// <summary>
    /// Represents a PWM peripheral. Set the frequency via
    /// <see cref="SetDesiredFrequency(double)"/>, open one or more channels with
    /// <see cref="OpenChannel(int)"/>, then drive each channel's duty cycle.
    /// </summary>
    public class PwmController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IPwmControllerProvider Provider { get; }

        private PwmController(IPwmControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default PWM controller for this device.</summary>
        public static PwmController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.PwmController) is PwmController c ? c : PwmController.FromName(NativeApi.GetDefaultName(NativeApiType.PwmController));
        /// <summary>Returns a PWM controller identified by its native API name.</summary>
        /// <param name="name">Native API name.</param>
        public static PwmController FromName(string name) => PwmController.FromProvider(new PwmControllerApiWrapper(NativeApi.Find(name, NativeApiType.PwmController)));
        /// <summary>Creates a controller from a custom <see cref="IPwmControllerProvider"/>.</summary>
        /// <param name="provider">Provider implementing the channel operations.</param>
        public static PwmController FromProvider(IPwmControllerProvider provider) => new PwmController(provider);

        /// <summary>
        /// The frequency the hardware actually applied, in Hz, after the last call
        /// to <see cref="SetDesiredFrequency(double)"/>. May differ from the requested
        /// value due to prescaler/timer rounding.
        /// </summary>
        public double ActualFrequency { get; private set; }

        /// <summary>Total number of channels on this controller.</summary>
        public int ChannelCount => this.Provider.ChannelCount;
        /// <summary>Minimum frequency in Hz this controller can generate.</summary>
        public double MinFrequency => this.Provider.MinFrequency;
        /// <summary>Maximum frequency in Hz this controller can generate.</summary>
        public double MaxFrequency => this.Provider.MaxFrequency;

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Sets the controller-wide PWM frequency.</summary>
        /// <param name="desiredFrequency">Target frequency in Hz.</param>
        /// <returns>The frequency actually applied (also published as <see cref="ActualFrequency"/>).</returns>
        public double SetDesiredFrequency(double desiredFrequency) => this.ActualFrequency = this.Provider.SetDesiredFrequency(desiredFrequency);

        /// <summary>Sets a per-channel frequency on hardware that supports it.</summary>
        /// <param name="channel">The channel to configure.</param>
        /// <param name="desiredFrequency">Target frequency in Hz.</param>
        /// <returns>The frequency actually applied to that channel.</returns>
        public double SetDesiredFrequency(PwmChannel channel, double desiredFrequency) => this.Provider.SetDesiredFrequency(channel.ChannelNumber, desiredFrequency);

        /// <summary>Opens a channel on this controller.</summary>
        /// <param name="channelNumber">Controller-relative channel index.</param>
        /// <returns>A <see cref="PwmChannel"/>; dispose it to release the channel.</returns>
        public PwmChannel OpenChannel(int channelNumber) => new PwmChannel(this, channelNumber);
    }

    /// <summary>
    /// A single PWM channel opened from a <see cref="PwmController"/>. Configure
    /// <see cref="Polarity"/> and duty cycle, then call <see cref="Start"/> to
    /// begin driving the output.
    /// </summary>
    public class PwmChannel : IDisposable {
        private PwmPulsePolarity polarity;
        private double dutyCycle;

        /// <summary>Controller-relative channel index this object represents.</summary>
        public int ChannelNumber { get; }
        /// <summary>The <see cref="PwmController"/> that owns this channel.</summary>
        public PwmController Controller { get; }
        /// <summary>True once <see cref="Start"/> has been called and <see cref="Stop"/> has not.</summary>
        public bool IsStarted { get; private set; }

        internal PwmChannel(PwmController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        /// <summary>Releases the channel so another caller can open it.</summary>
        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        /// <summary>Selects whether the active part of the pulse is high or low.</summary>
        public PwmPulsePolarity Polarity {
            get => this.polarity;
            set {
                this.polarity = value;

                this.Controller.Provider.SetPulseParameters(this.ChannelNumber, this.dutyCycle, this.polarity);
            }
        }

        /// <summary>Returns the most recently set duty cycle as a 0.0..1.0 fraction.</summary>
        public double GetActiveDutyCyclePercentage() => this.dutyCycle;

        /// <summary>Sets the duty cycle as a 0.0..1.0 fraction of the period.</summary>
        /// <param name="dutyCyclePercentage">0.0 = always inactive, 1.0 = always active.</param>
        /// <exception cref="ArgumentException">Thrown when the value is outside [0.0, 1.0].</exception>
        public void SetActiveDutyCyclePercentage(double dutyCyclePercentage) {
            if (dutyCyclePercentage > 1.0 || dutyCyclePercentage < 0.0)
                throw new ArgumentException("dutyCyclePercentage has to be in range 0.0 to 1.0");


            this.dutyCycle = dutyCyclePercentage;

            this.Controller.Provider.SetPulseParameters(this.ChannelNumber, this.dutyCycle, this.polarity);
        }

        /// <summary>Begins generating the configured waveform on the channel.</summary>
        public void Start() {
            if (!this.IsStarted) {
                this.Controller.Provider.EnableChannel(this.ChannelNumber);
                this.IsStarted = true;
            }
        }

        /// <summary>Stops the waveform; the pin is parked in its inactive state.</summary>
        public void Stop() {
            if (this.IsStarted) {
                this.Controller.Provider.DisableChannel(this.ChannelNumber);
                this.IsStarted = false;
            }
        }
    }

    /// <summary>Defines which level represents the "active" portion of the PWM pulse.</summary>
    public enum PwmPulsePolarity {
        /// <summary>The active part of the pulse drives high (idle low).</summary>
        ActiveHigh = 0,
        /// <summary>The active part of the pulse drives low (idle high).</summary>
        ActiveLow = 1,
    }

    namespace Provider {
        /// <summary>
        /// Provider contract for a PWM controller. Most users call
        /// <see cref="PwmController"/> / <see cref="PwmChannel"/> directly; implement
        /// this interface only when supplying a custom or virtual PWM.
        /// </summary>
        public interface IPwmControllerProvider : IDisposable {
            /// <summary>Total number of channels exposed by this controller.</summary>
            int ChannelCount { get; }
            /// <summary>Minimum frequency in Hz this controller can generate.</summary>
            double MinFrequency { get; }
            /// <summary>Maximum frequency in Hz this controller can generate.</summary>
            double MaxFrequency { get; }

            /// <summary>Acquires exclusive access to the specified channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void OpenChannel(int channel);
            /// <summary>Releases a previously opened channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void CloseChannel(int channel);

            /// <summary>Begins generating the configured waveform on the channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void EnableChannel(int channel);
            /// <summary>Stops the waveform; the pin is parked in its inactive state.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void DisableChannel(int channel);

            /// <summary>Sets the active duty cycle and polarity for the channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            /// <param name="dutyCycle">Active fraction of the period, 0.0..1.0.</param>
            /// <param name="polarity">Which level is "active".</param>
            void SetPulseParameters(int channel, double dutyCycle, PwmPulsePolarity polarity);

            /// <summary>Sets the controller-wide frequency.</summary>
            /// <param name="frequency">Target frequency in Hz.</param>
            /// <returns>The frequency actually applied.</returns>
            double SetDesiredFrequency(double frequency);

            /// <summary>Sets a per-channel frequency where the hardware supports it.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            /// <param name="frequency">Target frequency in Hz.</param>
            /// <returns>The frequency actually applied to that channel.</returns>
            double SetDesiredFrequency(int channel, double frequency);
        }

        /// <summary>
        /// Concrete <see cref="IPwmControllerProvider"/> backed by the native
        /// TinyCLR PWM HAL. Constructed internally by <see cref="PwmController"/>;
        /// you don't normally need to use this type directly.
        /// </summary>
        public sealed class PwmControllerApiWrapper : IPwmControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            /// <param name="api">The native PWM API to bind to.</param>
            public PwmControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            public extern int ChannelCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern double MinFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern double MaxFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(int channel);
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(int channel);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void EnableChannel(int channel);
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DisableChannel(int channel);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetPulseParameters(int channel, double dutyCycle, PwmPulsePolarity polarity);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern double SetDesiredFrequency(double frequency);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern double SetDesiredFrequency(int channel, double frequency);
        }
    }
}
