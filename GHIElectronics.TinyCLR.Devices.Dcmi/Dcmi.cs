using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Dcmi.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Dcmi {
    public sealed class DcmiController : IDisposable {        

        public IDcmiControllerProvider Provider { get; }

        private DcmiController(IDcmiControllerProvider provider) => this.Provider = provider;

        public static DcmiController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DcmiController) is DcmiController c ? c : DcmiController.FromName(NativeApi.GetDefaultName(NativeApiType.DcmiController));
        public static DcmiController FromName(string name) => DcmiController.FromProvider(new DcmiControllerApiWrapper(NativeApi.Find(name, NativeApiType.DcmiController)));
        public static DcmiController FromProvider(IDcmiControllerProvider provider) => new DcmiController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode) => this.Provider.SetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode);


        public int Capture(byte[] data) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return this.Capture(data, 0, data.Length);
        }

        public int Capture(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.Capture(data, offset, count);

        }

        public void Enable() => this.Provider.Enable();

        public void Disable() => this.Provider.Disable();
    }
    
    public enum CaptureRate
    {
        AllFrame = 0,
        AlternateTwoFrame = 1,
        AlternateFourFrame = 2
    }

    public enum ExtendedDataMode
    {
        Extended8bit = 0,
        Extended10bit = 1,
        Extended12bit = 2,
        Extended14bit = 3
    }

    public enum SynchronizationMode
    {
        Hardware = 0,
        Embedded = 1
    }   
   
    namespace Provider {
        public interface IDcmiControllerProvider : IDisposable {

            void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode);
            
            int Capture(byte[] data, int offset, int count);

            void Enable();

            void Disable();            
        }

        public sealed class DcmiControllerApiWrapper : IDcmiControllerProvider {
            private readonly IntPtr impl;
            
            public NativeApi Api { get; }

            public DcmiControllerApiWrapper(NativeApi api) {
                
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
               
            }            

            public void Dispose() => this.Release();

            public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode) => this.NativeSetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode);

            public int Capture(byte[] data, int offset, int count) => this.NativeCapture(data, offset, count);

            public void Enable() => this.NativeEnable();

            public void Disable() => this.NativeDisable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeEnable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeDisable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeSetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int NativeCapture(byte[] data, int offset, int count);
        }
    }
}
