using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    public class DacProvider : IDacProvider {
        private IDacControllerProvider[] controllers;

        public string Name { get; }

        public IDacControllerProvider[] GetControllers() => this.controllers;

        private DacProvider(string name) {
            var api = Api.Find(name, ApiType.DacProvider);

            this.Name = name;
            this.controllers = new IDacControllerProvider[api.Count];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultDacControllerProvider(api.Implementation[i]);
        }

        public static IDacProvider FromId(string id) => new DacProvider(id);
    }

    internal class DefaultDacControllerProvider : IDacControllerProvider {
#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
#pragma warning restore CS0169

        internal DefaultDacControllerProvider(IntPtr nativeProvider) => this.nativeProvider = nativeProvider;

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
