using System;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Dac\Dac.cs.
// Bodies on Desktop are safe no-ops:
//   * WriteValue(int)/(double) -> stored in LastWrittenValue, otherwise empty
//   * Open/CloseChannel        -> empty
// Defaults match a generic 12-bit DAC: 2 channels, 0..4095.
namespace GHIElectronics.TinyCLR.Devices.Dac {
    /// <summary>
    /// Represents a DAC peripheral. Open a channel via <see cref="OpenChannel(int)"/>
    /// to drive analog output; use <see cref="MinValue"/>/<see cref="MaxValue"/>
    /// to know the raw-code range for the configured resolution.
    /// </summary>
    public class DacController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IDacControllerProvider Provider { get; }

        private DacController(IDacControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default DAC controller for this device.</summary>
        public static DacController GetDefault() => DacController.FromName("Simulator");
        /// <summary>Returns a DAC controller identified by its native API name.</summary>
        /// <param name="name">Native API name.</param>
        public static DacController FromName(string name) => DacController.FromProvider(new DacControllerApiWrapper(NativeApi.Find(name, NativeApiType.DacController)));
        /// <summary>Creates a controller from a custom <see cref="IDacControllerProvider"/>.</summary>
        /// <param name="provider">Provider implementing the channel operations.</param>
        public static DacController FromProvider(IDacControllerProvider provider) => new DacController(provider);

        /// <summary>Number of channels exposed by this controller.</summary>
        public int ChannelCount => this.Provider.ChannelCount;
        /// <summary>Sample width in bits.</summary>
        public int ResolutionInBits => this.Provider.ResolutionInBits;
        /// <summary>Smallest raw value accepted by <see cref="DacChannel.WriteValue(int)"/>.</summary>
        public int MinValue => this.Provider.MinValue;
        /// <summary>Largest raw value accepted by <see cref="DacChannel.WriteValue(int)"/>.</summary>
        public int MaxValue => this.Provider.MaxValue;

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Opens a channel for output.</summary>
        /// <param name="channelNumber">Controller-relative channel index.</param>
        /// <returns>A <see cref="DacChannel"/>; dispose it to release the channel.</returns>
        public DacChannel OpenChannel(int channelNumber) => new DacChannel(this, channelNumber);
    }

    /// <summary>
    /// A single DAC channel opened from a <see cref="DacController"/>. Call
    /// <see cref="WriteValue(int)"/> for raw codes or <see cref="WriteValue(double)"/>
    /// for a 0.0–1.0 ratio of the output range.
    /// </summary>
    public class DacChannel : IDisposable {
        /// <summary>Controller-relative channel index this object represents.</summary>
        public int ChannelNumber { get; }
        /// <summary>The <see cref="DacController"/> that owns this channel.</summary>
        public DacController Controller { get; }

        /// <summary>The most recent raw value written through <see cref="WriteValue(int)"/>.</summary>
        public int LastWrittenValue { get; private set; }

        internal DacChannel(DacController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        /// <summary>Releases the channel so another caller can open it.</summary>
        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        /// <summary>Drives the channel to a raw output code.</summary>
        /// <param name="value">Code in [<see cref="DacController.MinValue"/>, <see cref="DacController.MaxValue"/>].</param>
        public void WriteValue(int value) => this.Controller.Provider.Write(this.ChannelNumber, this.LastWrittenValue = value);
        /// <summary>Drives the channel using a 0.0..1.0 fraction of the full output range.</summary>
        /// <param name="ratio">Normalized output level; 0.0 maps to <see cref="DacController.MinValue"/> and 1.0 to <see cref="DacController.MaxValue"/>.</param>
        public void WriteValue(double ratio) => this.WriteValue((int)(ratio * (this.Controller.MaxValue - this.Controller.MinValue) + this.Controller.MinValue));
    }

    namespace Provider {
        /// <summary>
        /// Provider contract for a DAC controller. Most users call
        /// <see cref="DacController"/> / <see cref="DacChannel"/> directly; implement
        /// this interface only when supplying a custom or virtual DAC.
        /// </summary>
        public interface IDacControllerProvider : IDisposable {
            /// <summary>Total number of channels exposed by this controller.</summary>
            int ChannelCount { get; }
            /// <summary>Sample width in bits.</summary>
            int ResolutionInBits { get; }
            /// <summary>Smallest raw value accepted by <see cref="Write(int, int)"/>.</summary>
            int MinValue { get; }
            /// <summary>Largest raw value accepted by <see cref="Write(int, int)"/>.</summary>
            int MaxValue { get; }

            /// <summary>Acquires exclusive access to the specified channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void OpenChannel(int channel);
            /// <summary>Releases a previously opened channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            void CloseChannel(int channel);

            /// <summary>Writes a raw code to the channel.</summary>
            /// <param name="channel">Controller-relative channel index.</param>
            /// <param name="value">Output code.</param>
            void Write(int channel, int value);
        }

        /// <summary>
        /// Concrete <see cref="IDacControllerProvider"/> backed by the native
        /// TinyCLR DAC HAL. Constructed internally by <see cref="DacController"/>;
        /// you don't normally need to use this type directly.
        /// </summary>
        public sealed class DacControllerApiWrapper : IDacControllerProvider {
            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            /// <param name="api">The native DAC API to bind to.</param>
            public DacControllerApiWrapper(NativeApi api) => this.Api = api;

            /// <summary>Releases the native controller.</summary>
            public void Dispose() { }

            /// <inheritdoc/>
            public int ChannelCount => 2;
            /// <inheritdoc/>
            public int ResolutionInBits => 12;
            /// <inheritdoc/>
            public int MinValue => 0;
            /// <inheritdoc/>
            public int MaxValue => 4095;

            /// <inheritdoc/>
            public void OpenChannel(int channel) { }
            /// <inheritdoc/>
            public void CloseChannel(int channel) { }

            /// <inheritdoc/>
            public void Write(int channel, int value) { }
        }
    }
}
