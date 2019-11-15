using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Camera.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Camera {
    public sealed class CameraController : IDisposable {        

        public ICameraControllerProvider Provider { get; }

        private CameraController(ICameraControllerProvider provider) => this.Provider = provider;

        public static CameraController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DcmiController) is CameraController c ? c : CameraController.FromName(NativeApi.GetDefaultName(NativeApiType.DcmiController));
        public static CameraController FromName(string name) => CameraController.FromProvider(new CameraControllerApiWrapper(NativeApi.Find(name, NativeApiType.DcmiController)));
        public static CameraController FromProvider(ICameraControllerProvider provider) => new CameraController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock) => this.Provider.SetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode, sourceClock);

        public int Capture(byte[] data, int timeoutMillisecond) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return this.Capture(data, 0, data.Length, timeoutMillisecond);
        }

        public int Capture(byte[] data, int offset, int count, int timeoutMillisecond) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.Capture(data, offset, count, timeoutMillisecond);

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
        public interface ICameraControllerProvider : IDisposable {

            void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock);
            
            int Capture(byte[] data, int offset, int count, int timeoutMillisecond);

            void Enable();

            void Disable();            
        }

        public sealed class CameraControllerApiWrapper : ICameraControllerProvider {
            private readonly IntPtr impl;
            
            public NativeApi Api { get; }

            public CameraControllerApiWrapper(NativeApi api) {
                
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
               
            }            

            public void Dispose() => this.Release();

            public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock) => this.NativeSetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode, sourceClock);

            public int Capture(byte[] data, int offset, int count, int timeoutMillisecond) => this.NativeCapture(data, offset, count, timeoutMillisecond);

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
            private extern void NativeSetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int NativeCapture(byte[] data, int offset, int count, int timeoutMillisecond);
        }
    }
}
