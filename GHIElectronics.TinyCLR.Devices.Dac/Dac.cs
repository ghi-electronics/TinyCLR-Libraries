using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Dac {
    public sealed class DacController : IDisposable {
        public IDacControllerProvider Provider { get; }

        private DacController(IDacControllerProvider provider) => this.Provider = provider;

        public static DacController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DacController) is DacController c ? c : DacController.FromName(NativeApi.GetDefaultName(NativeApiType.DacController));
        public static DacController FromName(string name) => DacController.FromProvider(new DacControllerApiWrapper(NativeApi.Find(name, NativeApiType.DacController)));
        public static DacController FromProvider(IDacControllerProvider provider) => new DacController(provider);

        public int ChannelCount => this.Provider.ChannelCount;
        public int ResolutionInBits => this.Provider.ResolutionInBits;
        public int MinValue => this.Provider.MinValue;
        public int MaxValue => this.Provider.MaxValue;

        public void Dispose() => this.Provider.Dispose();

        public DacChannel OpenChannel(int channelNumber) => new DacChannel(this, channelNumber);
    }

    public sealed class DacChannel : IDisposable {
        public int ChannelNumber { get; }
        public DacController Controller { get; }

        public int LastWrittenValue { get; private set; }

        internal DacChannel(DacController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        public void WriteValue(int value) => this.Controller.Provider.Write(this.ChannelNumber, this.LastWrittenValue = value);
        public void WriteValue(double ratio) => this.WriteValue((int)(ratio * (this.Controller.MaxValue - this.Controller.MinValue) + this.Controller.MinValue));
    }

    namespace Provider {
        public interface IDacControllerProvider : IDisposable {
            int ChannelCount { get; }
            int ResolutionInBits { get; }
            int MinValue { get; }
            int MaxValue { get; }

            void OpenChannel(int channel);
            void CloseChannel(int channel);

            void Write(int channel, int value);
        }

        public sealed class DacControllerApiWrapper : IDacControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public DacControllerApiWrapper(NativeApi api) {
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
            public extern void OpenChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Write(int channel, int value);
        }
    }
}
