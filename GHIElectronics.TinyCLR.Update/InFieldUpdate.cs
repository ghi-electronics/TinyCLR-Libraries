using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Update {
    /// <summary>
    /// Applies an over-the-air application update from a stream. Verifies the
    /// signature against <c>key</c>, then flashes and reboots into the new app.
    /// </summary>
    public class ApplicationUpdate {
        private Stream stream;
        private byte[] key;
        private int activityPinId = -1;
        private GpioPin activityPin;

        /// <summary>Optional pin to toggle during long-running update operations (visual progress LED).</summary>
        public GpioPin ActivityPin {
            get => this.activityPin;

            set {
                this.activityPin = value;

                if (this.activityPin == null) {
                    this.activityPinId = -1;
                }
                else {
                    this.activityPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.activityPinId = this.activityPin.PinNumber;
                }
            }
        }

        /// <summary>Creates an updater that reads the new application image from <paramref name="stream"/> and verifies its signature against <paramref name="key"/>.</summary>
        public ApplicationUpdate(Stream stream, byte[] key) {         
            this.stream = stream;
            this.key = key;

            if (this.stream == null || this.key == null)
                throw new ArgumentNullException();

            InFieldUpdate.NativeInitialize();
        }

        /// <summary>Verifies the signature of the streamed image without flashing it.</summary>
        /// <returns>Decoded version string (e.g. "3.0.0.1000") or "Invalid." if verification failed.</returns>
        public string Verify() {
            InFieldUpdate.NativeSetApplicationSize((uint)this.stream.Length);
            var v = this.NativeAuthenticateApplication(this.stream, this.key, this.activityPinId);
            return InFieldUpdate.VersionConvertToString(v);
        }

        /// <summary>
        /// Verifies the image, then writes it to the application region and resets
        /// the device. Does not return on success.
        /// </summary>
        public void FlashAndReset() {
            InFieldUpdate.NativeSetApplicationSize((uint)this.stream.Length);
            var v = this.NativeAuthenticateApplication(this.stream, this.key, this.activityPinId);
            if (v == 0xFFFFFFFF) {
                throw new Exception("Authenticate application failed.");
            }

            InFieldUpdate.NativeFlashAndReset(this.activityPinId);

            throw new InvalidOperationException("FlashAndReset failed.");
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateApplication(Stream stream, byte[] key, int indicatorPinId);
    }

    /// <summary>
    /// In-Field Update — feed firmware and/or application bytes in chunks, verify
    /// the signatures, then flash and reboot. Supports caching the chunks in RAM
    /// (faster) or to an external <see cref="StorageController"/> (handles images
    /// larger than free RAM).
    /// </summary>
    public class InFieldUpdate:IDisposable {
        /// <summary>Where the updater caches incoming chunks before flashing.</summary>
        public enum CacheMode {
            /// <summary>Buffer to external flash via a <see cref="StorageController"/>.</summary>
            Flash,
            /// <summary>Buffer in RAM.</summary>
            Ram,
        };

        private enum IfuMode {
            None = 0,
            Firmware = 1,
            Application = 2
        }

        private IfuMode mode = IfuMode.None;

        private StorageController storageController;
        private byte[] applicationKey;
        private byte[] applicationBuffer;
        private byte[] firmwareBuffer;

        private GpioPin activityPin;
        private int activityPinId = -1;

        private CacheMode cacheMode;

        private TimeSpan readDataTimeOut = TimeSpan.FromSeconds(5);
        /// <summary>Optional pin to toggle during long-running update operations (visual progress LED).</summary>
        public GpioPin ActivityPin {
            get => this.activityPin;

            set {
                this.activityPin = value;

                if (this.activityPin == null) {
                    this.activityPinId = -1;
                }
                else {
                    this.activityPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.activityPinId = this.activityPin.PinNumber;
                }

            }
        }


        private uint firmwareChunkIndex = 0;
        private uint applicationChunkIndex = 0;
        private UnmanagedBuffer uAppBuffer;
        private UnmanagedBuffer uFwBuffer;

        /// <summary>Creates a RAM-cached updater.</summary>
        public InFieldUpdate() {
            this.cacheMode = CacheMode.Ram;
            this.activityPinId = -1;

            this.firmwareBuffer = null;
            this.applicationBuffer = null;

            NativeInitialize();
        }

        /// <summary>Creates a flash-cached updater backed by an external storage device. Use for images that don't fit in RAM.</summary>
        public InFieldUpdate(StorageController storageController) {
            this.storageController = storageController;

            this.cacheMode = CacheMode.Flash;
            this.activityPinId = -1;
            this.firmwareBuffer = null;
            this.applicationBuffer = null;

            try {
                this.storageController.Provider.Open();
            }
            catch {
                throw new ArgumentException("Could not open the storage controller.");
            }

            NativeInitialize();
        }

        /// <summary>Loads the public key used to verify the application image.</summary>
        public void LoadApplicationKey(byte[] key) => this.applicationKey = key;

        /// <summary>Appends a chunk of bytes to the buffered application image.</summary>
        /// <returns>Number of bytes accepted.</returns>
        public int LoadApplicationChunk(byte[] data, int offset, int size) {
            if (this.cacheMode == CacheMode.Ram) {
                if (this.applicationChunkIndex == 0) {
                    if (this.applicationBuffer == null) {
                        if (Memory.UnmanagedMemory.FreeBytes > ApplicationMaxSize) {
                            this.uAppBuffer = new UnmanagedBuffer((int)ApplicationMaxSize);

                            this.applicationBuffer = this.uAppBuffer.Bytes;
                        }
                        else {
                            this.applicationBuffer = new byte[ApplicationMaxSize];
                        }
                    }
                }
            }
            if (this.applicationChunkIndex >= ApplicationMaxSize)
                throw new ArgumentOutOfRangeException("Application too large.");

            int b;

            if (this.cacheMode == CacheMode.Flash) {
                b = this.BufferingToExternalFlash(ApplicationAddress + this.applicationChunkIndex, data, offset, size);
            }
            else {
                b = this.BufferingToMemory(this.applicationChunkIndex, data, offset, size, false);
            }

            this.applicationChunkIndex += (uint)b;

            this.mode |= IfuMode.Application;

            this.ToggleActivityPin();

            return b;
        }

        /// <summary>Appends a chunk of bytes to the buffered firmware image.</summary>
        /// <returns>Number of bytes accepted.</returns>
        public int LoadFirmwareChunk(byte[] data, int offset, int size) {
            if (this.cacheMode == CacheMode.Ram) {
                if (this.firmwareChunkIndex == 0) {
                    if (this.firmwareBuffer == null) {
                        if (Memory.UnmanagedMemory.FreeBytes > FirmwareMaxSize) {
                            this.uFwBuffer = new UnmanagedBuffer((int)FirmwareMaxSize);

                            this.firmwareBuffer = this.uFwBuffer.Bytes;
                        }
                        else {
                            this.firmwareBuffer = new byte[FirmwareMaxSize];
                        }
                    }
                }
            }

            if (this.firmwareChunkIndex >= FirmwareMaxSize)
                throw new ArgumentOutOfRangeException("Firmware too large.");

            int b;

            if (this.cacheMode == CacheMode.Flash) {
                b = this.BufferingToExternalFlash(FirmwareAddress + this.firmwareChunkIndex, data, offset, size);
            }
            else {
                b = this.BufferingToMemory(this.firmwareChunkIndex, data, offset, size, true);
            }

            this.firmwareChunkIndex += (uint)b;

            this.mode |= IfuMode.Firmware;

            this.ToggleActivityPin();

            return b;
        }

        /// <summary>Verifies the application signature without flashing. Returns the embedded version string, or "Invalid." on failure.</summary>
        public string VerifyApplication() {
            NativeSetApplicationSize(this.applicationChunkIndex);

            var v = NativeAuthenticateApplication(this.applicationBuffer, this.applicationKey, this.activityPinId);

            return VersionConvertToString(v);

        }

        /// <summary>Verifies the firmware signature without flashing. Returns the embedded version string, or "Invalid." on failure.</summary>
        public string VerifyFirmware() {
            NativeSetFirmwareSize(this.firmwareChunkIndex);

            var v = NativeAuthenticateFirmware(this.firmwareBuffer, this.activityPinId);

            return VersionConvertToString(v);

        }

        /// <summary>Discards every buffered chunk and rewinds both write indices to zero.</summary>
        public void ResetChunks() {
            this.firmwareChunkIndex = 0;
            this.applicationChunkIndex = 0;

        }

        /// <summary>
        /// Verifies any buffered images, writes them to their destination regions,
        /// and resets the device. Does not return on success.
        /// </summary>
        public void FlashAndReset() {
            if (this.mode != IfuMode.None) {
                if ((this.mode & IfuMode.Firmware) == IfuMode.Firmware) {
                    var v = NativeAuthenticateFirmware(this.firmwareBuffer, this.activityPinId);

                    if (v == 0xFFFFFFFF) {
                        throw new Exception("Authenticate firmware failed.");
                    }
                }

                if ((this.mode & IfuMode.Application) == IfuMode.Application) {
                    var v = NativeAuthenticateApplication(this.applicationBuffer, this.applicationKey, this.activityPinId);

                    if (v == 0xFFFFFFFF) {
                        throw new Exception("Authenticate application failed.");
                    }
                }

                NativeFlashAndReset(this.activityPinId);
            }

            throw new InvalidOperationException("FlashAndReset failed.");
        }

        private int BufferingToMemory(uint address, byte[] data, int offset, int size, bool firmware) {
            if (data == null)
                throw new ArgumentNullException("Data null.");

            if (offset + size > data.Length)
                throw new ArgumentOutOfRangeException("Out of range.");

            if (firmware) {
                Array.Copy(data, offset, this.firmwareBuffer, (int)address, size);
            }
            else {
                Array.Copy(data, offset, this.applicationBuffer, (int)address, size);
            }
            this.ToggleActivityPin();

            return size;
        }

        private int BufferingToExternalFlash(uint address, byte[] data, int offset, int size) {
            if (data == null)
                throw new ArgumentNullException("Data null.");

            if (offset + size > data.Length)
                throw new ArgumentOutOfRangeException("Out of range.");

            var sectorSize = this.storageController.Provider.Descriptor.RegionSizes[0];

            var sectorId = address / sectorSize;

            if (sectorId * sectorSize == address) { // check and erase only once when start of sector

                if (!this.storageController.Provider.IsErased(sectorId * sectorSize, sectorSize > size ? sectorSize : size)) {
#if DEBUG
                    Debug.WriteLine("Erasing flash: 0x" + address.ToString("x8"));
#endif
                    this.storageController.Provider.Erase(sectorId * sectorSize, sectorSize > size ? sectorSize : size, this.readDataTimeOut);
                }
            }
#if DEBUG
            Debug.WriteLine("Writting to flash: 0x" + address.ToString("x8") + ", size 0x" + size.ToString("x8"));
#endif
            if (this.storageController.Provider.Write(address, size, data, 0, this.readDataTimeOut) != size) {
                throw new InvalidOperationException("Writting error: 0x" + address.ToString("x8"));
            }

            this.ToggleActivityPin();

            return size;
        }

        private void ToggleActivityPin() => this.ActivityPin?.Write(this.ActivityPin.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

        private bool disposed = false;
        /// <summary>Releases buffered memory (the unmanaged firmware/application buffers).</summary>
        public void Dispose() {
            if (this.disposed)
                return;

            if (this.uAppBuffer != null) {
                this.applicationBuffer = null;
                this.uAppBuffer.Dispose();
                this.uAppBuffer = null;
            }

            if (this.uFwBuffer != null) {
                this.firmwareBuffer = null;
                this.uFwBuffer.Dispose();
                this.uFwBuffer = null;
            }

            this.disposed = true;
        }

        /// <summary>Formats a packed 32-bit version (major.minor.build.revision) as a dotted string.</summary>
        public static string VersionConvertToString(uint version) {
            var v = version != 0xFFFFFFFF ? ((version >> 24) & 0xFF).ToString() + "."
                                            + ((version >> 16) & 0xFF).ToString() + "."
                                            + ((version >> 8) & 0xFF).ToString() + "."
                                            + ((version >> 0) & 0xFF).ToString() : "Invalid.";

            return v;

        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void NativeInitialize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeAuthenticateFirmware(byte[] buffer, int indicatorPinId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeSetFirmwareSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint NativeAuthenticateApplication(byte[] buffer, byte[] key, int indicatorPinId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void NativeSetApplicationSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void NativeFlashAndReset(int indicatorPin);
        
        private extern static uint FirmwareAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint FirmwareMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
    }
}
