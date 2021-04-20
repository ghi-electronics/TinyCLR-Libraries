using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Update {
    public sealed class ApplicationUpdate {
        private Stream stream;
        private byte[] key;
        private int activityPinId = -1;
        private GpioPin activityPin;
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

        public ApplicationUpdate(Stream stream, byte[] key) {         
            this.stream = stream;
            this.key = key;

            if (this.stream == null || this.key == null)
                throw new ArgumentNullException();

            InFieldUpdate.NativeInitialize();
        }

        public string Verify() {
            InFieldUpdate.NativeSetApplicationSize((uint)this.stream.Length);
            var v = this.NativeAuthenticateApplication(this.stream, this.key, this.activityPinId);
            return InFieldUpdate.VersionConvertToString(v);
        }

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

    public sealed class InFieldUpdate:IDisposable {
        public enum CacheMode {
            Flash,
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

        public InFieldUpdate() {
            this.cacheMode = CacheMode.Ram;
            this.activityPinId = -1;

            this.firmwareBuffer = null;
            this.applicationBuffer = null;

            NativeInitialize();
        }

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

        public void LoadApplicationKey(byte[] key) => this.applicationKey = key;
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

        public string VerifyApplication() {
            NativeSetApplicationSize(this.applicationChunkIndex);

            var v = NativeAuthenticateApplication(this.applicationBuffer, this.applicationKey, this.activityPinId);

            return VersionConvertToString(v);

        }

        public string VerifyFirmware() {
            NativeSetFirmwareSize(this.firmwareChunkIndex);

            var v = NativeAuthenticateFirmware(this.firmwareBuffer, this.activityPinId);

            return VersionConvertToString(v);

        }

        public void ResetChunks() {
            this.firmwareChunkIndex = 0;
            this.applicationChunkIndex = 0;

        }

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
