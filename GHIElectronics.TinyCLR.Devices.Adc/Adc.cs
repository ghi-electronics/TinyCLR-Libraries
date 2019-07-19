using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Adc {
    public enum AdcChannelMode {
        SingleEnded = 0,
        Differential = 1
    }

    public sealed class AdcController : IDisposable {
        public IAdcControllerProvider Provider { get; }

        private AdcController(IAdcControllerProvider provider) => this.Provider = provider;

        public static AdcController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.AdcController) is AdcController c ? c : AdcController.FromName(NativeApi.GetDefaultName(NativeApiType.AdcController));
        public static AdcController FromName(string name) => AdcController.FromProvider(new AdcControllerApiWrapper(NativeApi.Find(name, NativeApiType.AdcController)));
        public static AdcController FromProvider(IAdcControllerProvider provider) => new AdcController(provider);

        public int ChannelCount => this.Provider.ChannelCount;
        public int ResolutionInBits => this.Provider.ResolutionInBits;
        public int MinValue => this.Provider.MinValue;
        public int MaxValue => this.Provider.MaxValue;

        public AdcChannelMode ChannelMode {
            get => this.Provider.GetChannelMode();
            set => this.Provider.SetChannelMode(value);
        }

        public bool IsChannelModeSupported(AdcChannelMode mode) => this.Provider.IsChannelModeSupported(mode);

        public void Dispose() => this.Provider.Dispose();

        public AdcChannel OpenChannel(int channelNumber) => new AdcChannel(this, channelNumber);
    }

    public sealed class AdcChannel : IDisposable {
        public int ChannelNumber { get; }
        public AdcController Controller { get; }

        internal AdcChannel(AdcController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        public int ReadValue() => this.Controller.Provider.Read(this.ChannelNumber);
        public double ReadRatio() => (this.ReadValue() - this.Controller.MinValue) / (double)(this.Controller.MaxValue - this.Controller.MinValue);
    }

    namespace Provider {
        public interface IAdcControllerProvider : IDisposable {
            int ChannelCount { get; }
            int ResolutionInBits { get; }
            int MinValue { get; }
            int MaxValue { get; }

            bool IsChannelModeSupported(AdcChannelMode mode);
            AdcChannelMode GetChannelMode();
            void SetChannelMode(AdcChannelMode value);

            void OpenChannel(int channel);
            void CloseChannel(int channel);

            int Read(int channel);
        }

        public sealed class AdcControllerApiWrapper : IAdcControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public AdcControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int ChannelCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int ResolutionInBits { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MinValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MaxValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsChannelModeSupported(AdcChannelMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern AdcChannelMode GetChannelMode();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetChannelMode(AdcChannelMode value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(int channel);
        }
    }
}
