using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Usb.Client.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Usb.Client {
    public sealed class CdcController : IDisposable {
        public ICdcControllerProvider Provider { get; }

        private CdcController(ICdcControllerProvider provider) => this.Provider = provider;

        public static CdcController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbClientController) is CdcController c ? c : CdcController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbClientController));
        public static CdcController FromName(string name) => CdcController.FromProvider(new CdcControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbClientController)));
        public static CdcController FromProvider(ICdcControllerProvider provider) => new CdcController(provider);

        public void Dispose() => this.Provider.Dispose();

        public int ByteToRead => this.Provider.ByteToRead;
        public bool Connected => this.Provider.Connected;

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public int Read(byte[] data, int offset, int count) => this.Provider.Read(data, offset, count);
        public int Write(byte[] data, int offset, int count) => this.Provider.Write(data, offset, count);

        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();
        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();

        public void Flush() => this.Provider.Flush();

    }

    namespace Provider {
        public interface ICdcControllerProvider : IDisposable {
            int ByteToRead { get; }
            int ByteToWrite { get; }

            bool Connected { get; }

            void Enable();
            void Disable();

            int Read(byte[] data, int offset, int count);
            int Write(byte[] data, int offset, int count);
            
            void ClearReadBuffer();
            void ClearWriteBuffer();

            void Flush();
        }

        public sealed class CdcControllerApiWrapper : ICdcControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public CdcControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            public extern int ByteToRead { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int ByteToWrite { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern bool Connected { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        }
    }
}
