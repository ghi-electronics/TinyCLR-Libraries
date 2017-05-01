using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Adc.Provider {
    public interface IAdcControllerProvider {
        int ChannelCount {
            get;
        }

        int ResolutionInBits {
            get;
        }

        int MinValue {
            get;
        }

        int MaxValue {
            get;
        }

        ProviderAdcChannelMode ChannelMode {
            get;
            set;
        }

        bool IsChannelModeSupported(ProviderAdcChannelMode channelMode);
        void AcquireChannel(int channel);
        void ReleaseChannel(int channel);
        int ReadValue(int channelNumber);
    }

    public interface IAdcProvider {
        IAdcControllerProvider[] GetControllers();
    }

    public class AdcProvider : IAdcProvider {
        private IAdcControllerProvider[] controllers;

        public string Name { get; }

        public IAdcControllerProvider[] GetControllers() => this.controllers;

        private AdcProvider(string name) {
            this.Name = name;
            this.controllers = new IAdcControllerProvider[DefaultAdcControllerProvider.GetControllerCount(name)];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultAdcControllerProvider(name, i);
        }

        public static IAdcProvider FromId(string id) => new AdcProvider(id);
    }

    internal class DefaultAdcControllerProvider : IAdcControllerProvider {
        private IntPtr nativeProvider;

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint GetControllerCount(string providerName);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern DefaultAdcControllerProvider(string name, uint index);

        public extern ProviderAdcChannelMode ChannelMode {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }

        public extern int ChannelCount {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int MaxValue {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int MinValue {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int ResolutionInBits {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern bool IsChannelModeSupported(ProviderAdcChannelMode channelMode);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void AcquireChannel(int channel);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ReleaseChannel(int channel);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadValue(int channelNumber);
    }
}
