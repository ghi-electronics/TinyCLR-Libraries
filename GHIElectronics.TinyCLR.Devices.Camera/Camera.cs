using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Camera.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Camera {
    /// <summary>
    /// Represents a DCMI / parallel camera interface. Configure timing via
    /// <see cref="SetActiveSettings"/>, <see cref="Enable"/> the capture engine,
    /// then call <see cref="Capture(byte[], int)"/> to read a frame into a buffer.
    /// </summary>
    public class CameraController : IDisposable {

        /// <summary>The low-level provider backing this controller.</summary>
        public ICameraControllerProvider Provider { get; }

        private CameraController(ICameraControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default camera controller for this device.</summary>
        public static CameraController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DcmiController) is CameraController c ? c : CameraController.FromName(NativeApi.GetDefaultName(NativeApiType.DcmiController));
        /// <summary>Returns a camera controller identified by its native API name.</summary>
        public static CameraController FromName(string name) => CameraController.FromProvider(new CameraControllerApiWrapper(NativeApi.Find(name, NativeApiType.DcmiController)));
        /// <summary>Creates a controller from a custom <see cref="ICameraControllerProvider"/>.</summary>
        public static CameraController FromProvider(ICameraControllerProvider provider) => new CameraController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Applies a complete set of DCMI timing/protocol settings.</summary>
        /// <param name="captureRate">Per-frame capture rate.</param>
        /// <param name="horizontalSyncPolarity">HSYNC polarity (false = active low).</param>
        /// <param name="verticalSyncPolarity">VSYNC polarity (false = active low).</param>
        /// <param name="pixelClockPolarity">Pixel-clock polarity (false = falling-edge sample).</param>
        /// <param name="synchronizationMode">Hardware vs. embedded synchronization.</param>
        /// <param name="extendedDataMode">Data bus width.</param>
        /// <param name="sourceClock">Source clock feeding the camera, in Hz.</param>
        public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock) => this.Provider.SetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode, sourceClock);

        /// <summary>Captures one frame into the supplied buffer.</summary>
        /// <param name="data">Destination buffer. Must be large enough for one frame at the configured resolution/format.</param>
        /// <param name="timeoutMillisecond">Maximum time to wait for the frame, in milliseconds.</param>
        /// <returns>Number of bytes actually captured.</returns>
        public int Capture(byte[] data, int timeoutMillisecond) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return this.Capture(data, 0, data.Length, timeoutMillisecond);
        }

        /// <summary>Captures one frame into a slice of the buffer.</summary>
        public int Capture(byte[] data, int offset, int count, int timeoutMillisecond) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.Capture(data, offset, count, timeoutMillisecond);

        }

        /// <summary>Powers on the capture engine.</summary>
        public void Enable() => this.Provider.Enable();

        /// <summary>Powers off the capture engine.</summary>
        public void Disable() => this.Provider.Disable();
    }

    /// <summary>Frame-dropping policy for the capture engine.</summary>
    public enum CaptureRate
    {
        /// <summary>Capture every frame.</summary>
        AllFrame = 0,
        /// <summary>Capture every other frame.</summary>
        AlternateTwoFrame = 1,
        /// <summary>Capture every fourth frame.</summary>
        AlternateFourFrame = 2
    }

    /// <summary>Camera-to-SoC parallel data bus width.</summary>
    public enum ExtendedDataMode
    {
        /// <summary>8-bit data bus.</summary>
        Extended8bit = 0,
        /// <summary>10-bit data bus.</summary>
        Extended10bit = 1,
        /// <summary>12-bit data bus.</summary>
        Extended12bit = 2,
        /// <summary>14-bit data bus.</summary>
        Extended14bit = 3
    }

    /// <summary>How the camera frames its data.</summary>
    public enum SynchronizationMode
    {
        /// <summary>Discrete HSYNC/VSYNC signals.</summary>
        Hardware = 0,
        /// <summary>Synchronization codes embedded in the data stream (ITU-R BT.656 style).</summary>
        Embedded = 1
    }

    namespace Provider {
        /// <summary>Provider contract for a camera controller.</summary>
        public interface ICameraControllerProvider : IDisposable {

            /// <summary>Applies a complete set of camera timing/protocol settings.</summary>
            void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock);

            /// <summary>Captures one frame.</summary>
            int Capture(byte[] data, int offset, int count, int timeoutMillisecond);

            /// <summary>Powers on the capture engine.</summary>
            void Enable();

            /// <summary>Powers off the capture engine.</summary>
            void Disable();
        }

        /// <summary>Concrete <see cref="ICameraControllerProvider"/> backed by the native TinyCLR camera HAL.</summary>
        public sealed class CameraControllerApiWrapper : ICameraControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public CameraControllerApiWrapper(NativeApi api) {

                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            /// <inheritdoc/>
            public void SetActiveSettings(CaptureRate captureRate, bool horizontalSyncPolarity, bool verticalSyncPolarity, bool pixelClockPolarity, SynchronizationMode synchronizationMode, ExtendedDataMode extendedDataMode, uint sourceClock) => this.NativeSetActiveSettings(captureRate, horizontalSyncPolarity, verticalSyncPolarity, pixelClockPolarity, synchronizationMode, extendedDataMode, sourceClock);

            /// <inheritdoc/>
            public int Capture(byte[] data, int offset, int count, int timeoutMillisecond) => this.NativeCapture(data, offset, count, timeoutMillisecond);

            /// <inheritdoc/>
            public void Enable() => this.NativeEnable();

            /// <inheritdoc/>
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
