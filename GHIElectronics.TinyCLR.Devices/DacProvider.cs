using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Dac.Provider {
    public interface IDacControllerProvider {
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

        void AcquireChannel(int channel);
        void ReleaseChannel(int channel);
        void WriteValue(int channel, int value);
    }

    public interface IDacProvider {
        IDacControllerProvider[] GetControllers();
    }

    internal class NativeDacControllerProvider : IDacControllerProvider {
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
        public extern void AcquireChannel(int channel);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ReleaseChannel(int channel);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void WriteValue(int channel, int value);
    }
}
