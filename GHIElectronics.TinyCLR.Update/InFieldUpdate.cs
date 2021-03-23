using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.Update {
    public sealed class InFieldUpdate {
        private enum Mode {
            None = 0,
            Firmware = 1,
            Application = 2
        }

        private enum Cache {
            None = 0,
            InternalMemory = 1,
            ExternalFlash = 2,
            FileStream = 3 //only application use this
        }

        private Mode mode = Mode.None;

        private StorageController externalStorageController;
        private bool useExternalStorageController;

        private readonly byte[] applicationKey;
        private byte[] buffer;
        private byte[] applicationBuffer;
        private byte[] firmwareBuffer;

        private readonly uint bufferSize = 1024;

        private readonly Stream firmwareStream;
        private readonly Stream applicationStream;
        private GpioPin activityPin;
        private int activityPinId = -1;

        public TimeSpan ReadDataTimeOut { get; set; } = TimeSpan.FromSeconds(5);
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

        private uint applicationVersion = 0xFFFFFFFF;
        private uint firmwareVersion = 0xFFFFFFFF;

        public string ApplicaltionVersion => this.applicationVersion != 0xFFFFFFFF ? ((this.applicationVersion >> 24) & 0xFF).ToString() + "."
                                            + ((this.applicationVersion >> 16) & 0xFF).ToString() + "."
                                            + ((this.applicationVersion >> 8) & 0xFF).ToString() + "."
                                            + ((this.applicationVersion >> 0) & 0xFF).ToString() : "Invalid.";

        public string FirmwareVersion => this.firmwareVersion != 0xFFFFFFFF ? ((this.firmwareVersion >> 24) & 0xFF).ToString() + "."
                                            + ((this.firmwareVersion >> 16) & 0xFF).ToString() + "."
                                            + ((this.firmwareVersion >> 8) & 0xFF).ToString() + "."
                                            + ((this.firmwareVersion >> 0) & 0xFF).ToString() + "00" : "Invalid.";

        private Cache applicationCache = Cache.None;
        private Cache firmwareCache = Cache.None;

        private uint firmwareChunkIndex = 0;
        private uint applicationChunkIndex = 0;
        public InFieldUpdate(byte[] applicationKey) {
            this.applicationKey = applicationKey;
            this.buffer = new byte[this.bufferSize];
            this.useExternalStorageController = true;

            this.externalStorageController = StorageController.FromName(STM32H7.StorageController.QuadSpi);

            this.externalStorageController.Provider.Open();
        }

        public InFieldUpdate(Stream firmwareStream, Stream applicationStream, byte[] applicationKey, bool useExternalFlash) {
            if (firmwareStream != null) {
                this.mode |= Mode.Firmware;
            }

            if (applicationStream != null) {
                this.applicationKey = applicationKey ?? throw new ArgumentNullException("applicationKey null.");
                this.mode |= Mode.Application;
            }

            if (firmwareStream == null && applicationStream == null) {
                throw new ArgumentNullException();
            }

            this.activityPinId = -1;
            this.firmwareStream = firmwareStream;
            this.applicationStream = applicationStream;
            this.useExternalStorageController = useExternalFlash;

            if (useExternalFlash) {
                if (applicationStream != null) {
                    this.applicationCache = Cache.ExternalFlash;
                }

                if (firmwareStream != null) {
                    this.firmwareCache = Cache.ExternalFlash;
                }

                this.NativeInFieldUpdate(this.firmwareCache, null, this.applicationCache, null, null);
            }
            else {
                if (applicationStream != null) {
                    if (applicationStream is MemoryStream app) { // convert to memory directly
                        this.applicationBuffer = app.ToArray();

                        this.applicationCache = Cache.InternalMemory;
                    }
                    else {
                        try {
                            this.applicationBuffer = new byte[ApplicationMaxSize]; // allocate buffer

                            this.applicationCache = Cache.InternalMemory;
                        }
                        catch {
                            if (!(applicationStream is FileStream)) { // stop if not enough memoy for network stream
                                throw new OutOfMemoryException();
                            }
                        }

                        // If not enough memory and FileStream, use file stream directly
                        if (this.applicationBuffer == null && applicationStream is FileStream) {
                            this.applicationCache = Cache.FileStream;
                        }

                    }
                }

                if (firmwareStream != null) {

                    if (firmwareStream is MemoryStream fw) {
                        this.firmwareBuffer = fw.ToArray();// convert to memory directly

                        this.firmwareCache = Cache.InternalMemory;
                    }
                    else {
                        try {
                            this.firmwareBuffer = new byte[FirmwareMaxSize]; // allocate buffer

                            this.firmwareCache = Cache.InternalMemory;
                        }
                        catch {
                            throw new OutOfMemoryException();// stop if not enough memoy for both FS and NS stream
                        }
                    }
                }

                this.NativeInFieldUpdate(this.firmwareCache, this.firmwareBuffer, this.applicationCache, this.applicationBuffer, this.applicationCache == Cache.FileStream ? (FileStream)this.applicationStream : null);
            }

            if (useExternalFlash) {
                this.externalStorageController = StorageController.FromName(STM32H7.StorageController.QuadSpi);

                this.externalStorageController.Provider.Open();
            }

            this.buffer = new byte[this.bufferSize];
        }

        public void Build(bool firmware, bool application) {
            if (firmware && ((this.mode & Mode.Firmware) != Mode.Firmware)) {
                throw new ArgumentNullException("Invalid firmware or stream is null.");
            }

            if (application && ((this.mode & Mode.Application) != Mode.Application)) {
                throw new ArgumentNullException("Invalid application or stream is null.");
            }

            if (this.applicationStream == null && this.firmwareStream == null && this.useExternalStorageController) { // data mode 
                if (firmware && this.firmwareChunkIndex == 0) {
                    throw new ArgumentNullException("Invalid firmware.");
                }

                if (application && this.applicationChunkIndex == 0) {
                    throw new ArgumentNullException("Invalid application.");
                }

                this.NativeInFieldUpdate(this.firmwareCache, null, this.applicationCache, null, null);

                if (firmware && this.firmwareChunkIndex > 0) {
                    this.NativeSetFirmwareSize((uint)this.firmwareChunkIndex);

                    this.firmwareVersion = this.NativeAuthenticateFirmware(this.activityPinId);
                }

                if (application && this.applicationChunkIndex > 0) {
                    this.NativeSetApplicationSize((uint)this.applicationChunkIndex);

                    this.applicationVersion = this.NativeAuthenticateApplication(this.applicationKey, this.activityPinId);
                }
            }
            else { // stream mode
                if (application && this.applicationStream != null) {
                    if ((this.mode & Mode.Application) != Mode.Application)
                        throw new ArgumentNullException();

                    if (this.applicationCache != Cache.FileStream) {
                        var totalBufferred = this.useExternalStorageController ? this.BufferingToExternalFlash(this.applicationStream, ApplicationAddress, ApplicationMaxSize) : this.BufferingToMemory(this.applicationStream, ref this.applicationBuffer, ApplicationMaxSize);

                        if (totalBufferred > 0)
                            this.NativeSetApplicationSize((uint)totalBufferred);
                        else
                            throw new InvalidOperationException("Application data not available.");
                    }

                    this.applicationVersion = this.NativeAuthenticateApplication(this.applicationKey, this.activityPinId);
                }

                if (firmware && this.firmwareStream != null) {

                    if ((this.mode & Mode.Firmware) != Mode.Firmware)
                        throw new ArgumentNullException();

                    var totalBufferred = this.useExternalStorageController ? this.BufferingToExternalFlash(this.firmwareStream, FirmwareAddress, FirmwareMaxSize) : this.BufferingToMemory(this.firmwareStream, ref this.firmwareBuffer, FirmwareMaxSize);

                    if (totalBufferred > 0)
                        this.NativeSetFirmwareSize((uint)totalBufferred);
                    else
                        throw new InvalidOperationException("Firmware data not available.");

                    this.firmwareVersion = this.NativeAuthenticateFirmware(this.activityPinId);
                }
            }
        }

        public int LoadApplicationChunk(byte[] data, uint offset, int size, bool resetChunk) {
            if (resetChunk)
                this.applicationChunkIndex = 0;

            if (this.applicationChunkIndex >= ApplicationMaxSize)
                throw new ArgumentOutOfRangeException("Application too large.");

            if (this.applicationStream != null)
                throw new InvalidOperationException("Detected application stream.");

            var b = this.BufferingToExternalFlash(ApplicationAddress + this.applicationChunkIndex, data, offset, size);

            this.applicationChunkIndex += (uint)b;

            this.mode |= Mode.Application;
            this.applicationCache = Cache.ExternalFlash;

            this.ToggleActivityPin();

            return b;
        }

        public int LoadFirmwareChunk(byte[] data, uint offset, int size, bool resetChunk) {
            if (resetChunk)
                this.firmwareChunkIndex = 0;

            if (this.firmwareChunkIndex >= FirmwareMaxSize)
                throw new ArgumentOutOfRangeException("Firmware too large.");

            if (this.firmwareStream != null)
                throw new InvalidOperationException("Detected firmware stream.");

            var b = this.BufferingToExternalFlash(FirmwareAddress + this.firmwareChunkIndex, data, offset, size);

            this.firmwareChunkIndex += (uint)b;

            this.mode |= Mode.Firmware;
            this.firmwareCache = Cache.ExternalFlash;

            this.ToggleActivityPin();

            return b;
        }

        public void FlashAndReset() {
            if (this.mode != Mode.None) {
                if ((this.mode & Mode.Firmware) == Mode.Firmware) {
                    var v = this.NativeAuthenticateFirmware(this.activityPinId);

                    if (v != this.firmwareVersion || this.firmwareVersion == 0xFFFFFFFF) {
                        throw new InvalidOperationException("Detected corrupted data, need to call Build.");
                    }

                }

                if ((this.mode & Mode.Application) == Mode.Application) {
                    var v = this.NativeAuthenticateApplication(this.applicationKey, this.activityPinId);

                    if (v != this.applicationVersion || this.applicationVersion == 0xFFFFFFFF) {
                        throw new InvalidOperationException("Detected corrupted data, need to call Build.");
                    }
                }

                this.NativeFlashAndReset(this.activityPinId);
            }

            throw new ArgumentNullException();
        }

        private int BufferingToMemory(Stream stream, ref byte[] data, uint maxSize) {
            var totalRead = 0;
            var doRead = true;
            var address = 0;

            if (stream is FileStream || stream is MemoryStream) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            while (doRead) {
                var t = DateTime.Now.Ticks;

                var read = 0;

                while (read < (int)this.bufferSize) {
                    read += stream.Read(this.buffer, read, (int)this.bufferSize - read);
                    var delta = DateTime.Now.Ticks - t;

                    if (((stream is FileStream || stream is MemoryStream) && stream.Position == stream.Length && read == 0 && stream.Length > 0) || read < 0 || (delta > this.ReadDataTimeOut.Ticks)) {
                        doRead = false;

                        break;
                    }

                    // stream behavior:
                    // return 0 when no more data, no need to wait
                    // return < 0 when no more data of EOF, no need to wait
                    if (read <= 0) {
                        doRead = false;

                        break;
                    }
                }

                if (read > 0) {
                    totalRead += read;

                    if (totalRead > maxSize) {
                        throw new ArgumentOutOfRangeException("Data too large.");
                    }

#if DEBUG
                    Debug.WriteLine("Writting to memory: " + address + ", size " + read);
#endif                   

                    Array.Copy(this.buffer, 0, data, address, read);

                    address += read;
                }

                this.ToggleActivityPin();
            }

            return totalRead;
        }
        private int BufferingToExternalFlash(Stream stream, uint address, uint maxSize) {
            var totalRead = 0;
            var doRead = true;

            if (stream is FileStream || stream is MemoryStream) {
                stream.Seek(0, SeekOrigin.Begin);
            }

            while (doRead) {
                var t = DateTime.Now.Ticks;

                var read = 0;

                while (read < (int)this.bufferSize) {
                    read += stream.Read(this.buffer, read, (int)this.bufferSize - read);
                    var delta = DateTime.Now.Ticks - t;

                    if (((stream is FileStream || stream is MemoryStream) && stream.Position == stream.Length && read == 0 && stream.Length > 0) || read < 0 || (delta > this.ReadDataTimeOut.Ticks)) {
                        doRead = false;

                        break;
                    }

                    // stream behavior:
                    // return 0 when no more data, no need to wait
                    // return < 0 when no more data of EOF, no need to wait
                    if (read <= 0) {
                        doRead = false;

                        break;
                    }
                }

                if (read > 0) {
                    totalRead += read;

                    if (totalRead > maxSize) {
                        throw new ArgumentOutOfRangeException("Data too large.");
                    }

                    if (!this.externalStorageController.Provider.IsErased(address, read)) {
#if DEBUG
                        Debug.WriteLine("Erasing flash: 0x" + address.ToString("x8"));
#endif
                        this.externalStorageController.Provider.Erase(address, read, this.ReadDataTimeOut);
                    }
#if DEBUG
                    Debug.WriteLine("Writting to flash: 0x" + address.ToString("x8") + ", size 0x" + read.ToString("x8"));
#endif
                    this.externalStorageController.Provider.Write(address, read, this.buffer, 0, this.ReadDataTimeOut);

                    address += (uint)read;
                }

                this.ToggleActivityPin();
            }

            return totalRead;
        }

        private int BufferingToExternalFlash(uint address, byte[] data, uint offset, int size) {
            if (data == null)
                throw new ArgumentNullException("data null.");

            if (offset + size > data.Length)
                throw new ArgumentOutOfRangeException("Out of range.");


            if (!this.externalStorageController.Provider.IsErased(address, size)) {
#if DEBUG
                Debug.WriteLine("Erasing flash: 0x" + address.ToString("x8"));
#endif
                this.externalStorageController.Provider.Erase(address, size, this.ReadDataTimeOut);
            }
#if DEBUG
            Debug.WriteLine("Writting to flash: 0x" + address.ToString("x8") + ", size 0x" + size.ToString("x8"));
#endif
            if (this.externalStorageController.Provider.Write(address, size, data, 0, this.ReadDataTimeOut) != size) {
                throw new InvalidOperationException("Writting error: 0x" + address.ToString("x8"));
            }

            this.ToggleActivityPin();

            return size;
        }

        private void ToggleActivityPin() => this.ActivityPin?.Write(this.ActivityPin.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(Cache firmware, byte[] firmwareBuffer, Cache application, byte[] applicationBuffer, FileStream applicationFileStream);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateFirmware(int indicatorPinId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetFirmwareSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateApplication(byte[] key, int indicatorPinId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetApplicationSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeFlashAndReset(int indicatorPin);

        private extern static uint FirmwareAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint FirmwareMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
    }
}
