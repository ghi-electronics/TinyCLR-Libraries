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

    internal class DefaultAdcControllerProvider : IAdcControllerProvider {
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
