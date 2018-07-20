using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;

namespace GHIElectronics.TinyCLR.Devices.Dac {
    public sealed class DacController : IDisposable {
        public IDacControllerProvider Provider { get; }

        private DacController(IDacControllerProvider provider) => this.Provider = provider;

        public static DacController GetDefault() => Api.GetDefaultFromCreator(ApiType.DacController) is DacController c ? c : DacController.FromName(Api.GetDefaultName(ApiType.DacController));
        public static DacController FromName(string name) => DacController.FromProvider(new DacControllerApiWrapper(Api.Find(name, ApiType.DacController)));
        public static DacController FromProvider(IDacControllerProvider provider) => new DacController(provider);

        public uint ChannelCount => this.Provider.ChannelCount;
        public uint ResolutionInBits => this.Provider.ResolutionInBits;
        public int MinValue => this.Provider.MinValue;
        public int MaxValue => this.Provider.MaxValue;

        public void Dispose() => this.Provider.Dispose();

        public DacChannel OpenChannel(int channelNumber) => this.OpenChannel((uint)channelNumber);
        public DacChannel OpenChannel(uint channelNumber) => new DacChannel(this, channelNumber);
    }

    public sealed class DacChannel : IDisposable {
        public uint ChannelNumber { get; }
        public DacController Controller { get; }

        public int LastWrittenValue { get; private set; }

        internal DacChannel(DacController controller, uint channelNumber) {
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
            uint ChannelCount { get; }
            uint ResolutionInBits { get; }
            int MinValue { get; }
            int MaxValue { get; }

            void OpenChannel(uint channel);
            void CloseChannel(uint channel);

            void Write(uint channel, int value);
        }

        public sealed class DacControllerApiWrapper : IDacControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public DacControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern uint ChannelCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern uint ResolutionInBits { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MinValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MaxValue { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Write(uint channel, int value);
        }
    }
}
