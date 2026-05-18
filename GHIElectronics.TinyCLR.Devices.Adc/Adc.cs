using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Adc {
    /// <summary>How an ADC channel sees its input.</summary>
    public enum AdcChannelMode {
        /// <summary>Channel input is referenced to ground.</summary>
        SingleEnded = 0,
        /// <summary>Channel input is the difference between two pins.</summary>
        Differential = 1
    }

    /// <summary>
    /// Represents an ADC peripheral. Open a channel via <see cref="OpenChannel(int)"/>
    /// to take samples; use <see cref="MinValue"/>/<see cref="MaxValue"/> to know
    /// the raw-code range for the configured resolution.
    /// </summary>
    public class AdcController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IAdcControllerProvider Provider { get; }

        private AdcController(IAdcControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default ADC controller for this device.</summary>
        public static AdcController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.AdcController) is AdcController c ? c : AdcController.FromName(NativeApi.GetDefaultName(NativeApiType.AdcController));

        /// <summary>Returns an ADC controller identified by its native API name.</summary>
        /// <param name="name">Native API name.</param>
        public static AdcController FromName(string name) => AdcController.FromProvider(new AdcControllerApiWrapper(NativeApi.Find(name, NativeApiType.AdcController)));

        /// <summary>Creates a controller from a custom <see cref="IAdcControllerProvider"/>.</summary>
        /// <param name="provider">Provider implementing the channel operations.</param>
        public static AdcController FromProvider(IAdcControllerProvider provider) => new AdcController(provider);

        /// <summary>Number of channels exposed by this controller.</summary>
        public int ChannelCount => this.Provider.ChannelCount;

        /// <summary>Sample width in bits. Determines the spread between <see cref="MinValue"/> and <see cref="MaxValue"/>.</summary>
        public int ResolutionInBits => this.Provider.ResolutionInBits;

        /// <summary>Smallest raw value <see cref="AdcChannel.ReadValue"/> can return.</summary>
        public int MinValue => this.Provider.MinValue;

        /// <summary>Largest raw value <see cref="AdcChannel.ReadValue"/> can return.</summary>
        public int MaxValue => this.Provider.MaxValue;

        /// <summary>
        /// Controller-wide channel mode (single-ended or differential). Not every
        /// mode is supported on every controller — check with <see cref="IsChannelModeSupported(AdcChannelMode)"/>.
        /// </summary>
        public AdcChannelMode ChannelMode {
            get => this.Provider.GetChannelMode();
            set => this.Provider.SetChannelMode(value);
        }

        /// <summary>Tests whether the controller supports a given channel mode.</summary>
        /// <param name="mode">The mode to test.</param>
        public bool IsChannelModeSupported(AdcChannelMode mode) => this.Provider.IsChannelModeSupported(mode);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Opens a channel for sampling.</summary>
        /// <param name="channelNumber">Controller-relative channel index.</param>
        /// <returns>An <see cref="AdcChannel"/>; dispose it to release the channel.</returns>
        public AdcChannel OpenChannel(int channelNumber) => new AdcChannel(this, channelNumber);
    }

    /// <summary>
    /// A single ADC channel opened from an <see cref="AdcController"/>. Call
    /// <see cref="ReadValue"/> for raw codes or <see cref="ReadRatio"/> for a
    /// 0.0–1.0 normalized reading.
    /// </summary>
    public class AdcChannel : IDisposable {
        /// <summary>Controller-relative channel index this object represents.</summary>
        public int ChannelNumber { get; }

        /// <summary>The <see cref="AdcController"/> that owns this channel.</summary>
        public AdcController Controller { get; }

        /// <summary>
        /// Sample-and-hold time the controller is asked to use. Longer times
        /// reduce noise on high-impedance sources but slow conversion.
        /// </summary>
        public TimeSpan SamplingTime { get; set; } = TimeSpan.FromTicks(1);

        internal AdcChannel(AdcController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        /// <summary>Releases the channel so another caller can open it.</summary>
        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        /// <summary>Performs a conversion and returns the raw integer code.</summary>
        /// <returns>An integer in the range [<see cref="AdcController.MinValue"/>, <see cref="AdcController.MaxValue"/>].</returns>
        public int ReadValue() => this.Controller.Provider.Read(this.ChannelNumber, this.SamplingTime);

        /// <summary>Performs a conversion and returns a 0.0..1.0 normalized reading.</summary>
        /// <returns>The raw code mapped to [0.0, 1.0] using the controller's <see cref="AdcController.MinValue"/>/<see cref="AdcController.MaxValue"/>.</returns>
        public double ReadRatio() => (this.ReadValue() - this.Controller.MinValue) / (double)(this.Controller.MaxValue - this.Controller.MinValue);
    }

    namespace Provider {
        /// <summary>
        /// Provider contract for an ADC controller. Most users call
        /// <see cref="AdcController"/> / <see cref="AdcChannel"/> directly; implement
        /// this interface only when supplying a custom or virtual ADC.
        /// </summary>
        public interface IAdcControllerProvider : IDisposable {
            /// <summary>Total number of channels exposed by this controller.</summary>
            int ChannelCount { get; }
            /// <summary>Sample width in bits.</summary>
            int ResolutionInBits { get; }
            /// <summary>Smallest raw value <see cref="Read(int, TimeSpan)"/> can return.</summary>
            int MinValue { get; }
            /// <summary>Largest raw value <see cref="Read(int, TimeSpan)"/> can return.</summary>
            int MaxValue { get; }

            /// <summary>Tests whether the controller supports the given channel mode.</summary>
            /// <param name="mode">The mode to test.</param>
            bool IsChannelModeSupported(AdcChannelMode mode);

            /// <summary>Returns the controller's current channel mode.</summary>
            AdcChannelMode GetChannelMode();

            /// <summary>Sets the controller's channel mode.</summary>
            /// <param name="value">New channel mode.</param>
            void SetChannelMode(AdcChannelMode value);

            /// <summary>Acquires exclusive access to the specified channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void OpenChannel(int channel);

            /// <summary>Releases a previously opened channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void CloseChannel(int channel);

            /// <summary>Performs a conversion on the channel and returns the raw code.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            /// <param name="samplingTime">Requested sample-and-hold time.</param>
            int Read(int channel, TimeSpan samplingTime);
        }

        /// <summary>
        /// Concrete <see cref="IAdcControllerProvider"/> backed by the native
        /// TinyCLR ADC HAL. Constructed internally by <see cref="AdcController"/>;
        /// you don't normally need to use this type directly.
        /// </summary>
        public sealed class AdcControllerApiWrapper : IAdcControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            /// <param name="api">The native ADC API to bind to.</param>
            public AdcControllerApiWrapper(NativeApi api) {
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
            public extern int ResolutionInBits { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int MinValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int MaxValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsChannelModeSupported(AdcChannelMode mode);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern AdcChannelMode GetChannelMode();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetChannelMode(AdcChannelMode value);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(int channel);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(int channel);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(int channel, TimeSpan samplingTime);
        }
    }
}
