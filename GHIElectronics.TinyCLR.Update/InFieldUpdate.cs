using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private bool applicationBuilt;
        private bool firmwareBuilt;

        private readonly Mode mode = Mode.None;

        private StorageController externalStorageController;
        private bool useExternalStorageController;

        private readonly byte[] applicationKey;
        private byte[] buffer;
        private byte[] applicationBuffer;
        private byte[] firmwareBuffer;

        private readonly uint bufferSize = 1024;

        private readonly Stream firmwareStream;
        private readonly Stream applicationStream;

        public TimeSpan ReadDataTimeOut { get; set; } = TimeSpan.FromSeconds(5);

        public uint ApplicationVersion { get; private set; }
        public uint FirmwareVersion { get; private set; }

        private Cache applicationCache = Cache.None;
        private Cache firmwareCache = Cache.None;


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

            this.firmwareStream = firmwareStream;
            this.applicationStream = applicationStream;
            this.useExternalStorageController = useExternalFlash;

            if (useExternalFlash) {
                this.NativeInFieldUpdate(Cache.ExternalFlash, null, Cache.ExternalFlash, null, null);
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
                            if (applicationStream is NetworkStream) { // stop if not enough memoy for network stream
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

                this.buffer = new byte[this.bufferSize];
            }
        }

        public void Build(bool firmware, bool application) {
            if (application) {
                this.applicationBuilt = false;

                if ((this.mode & Mode.Application) != Mode.Application)
                    throw new ArgumentNullException();

                if (this.applicationCache != Cache.FileStream) {
                    var totalBufferred = this.useExternalStorageController ? this.BufferingToExternalFlash(this.applicationStream, ApplicationAddress, ApplicationMaxSize) : this.BufferingToMemory(this.applicationStream, ref this.applicationBuffer, ApplicationMaxSize);

                    if (totalBufferred > 0)
                        this.NativeSetApplicationSize((uint)totalBufferred);
                    else
                        throw new InvalidOperationException("Application data not available.");
                }

                this.ApplicationVersion = this.NativeAuthenticateApplication(this.applicationKey);

                this.applicationBuilt = true;
            }

            if (firmware) {
                this.firmwareBuilt = false;

                if ((this.mode & Mode.Firmware) != Mode.Firmware)
                    throw new ArgumentNullException();

                var totalBufferred = this.useExternalStorageController ? this.BufferingToExternalFlash(this.firmwareStream, FirmwareAddress, FirmwareMaxSize) : this.BufferingToMemory(this.firmwareStream, ref this.firmwareBuffer, FirmwareMaxSize);

                if (totalBufferred > 0)
                    this.NativeSetFirmwareSize((uint)totalBufferred);
                else
                    throw new InvalidOperationException("Firmware data not available.");

                this.FirmwareVersion = this.NativeAuthenticateFirmware();

                this.firmwareBuilt = true;
            }
        }


        public void FlashAndReset() {
            if (this.mode != Mode.None) {
                if ((this.mode & Mode.Firmware) == Mode.Firmware) {
                    if (this.firmwareBuilt) {
                        this.NativeAuthenticateFirmware();
                    }
                    else {
                        throw new InvalidOperationException("Require call Build firmware.");
                    }
                }

                if ((this.mode & Mode.Application) == Mode.Application) {
                    if (this.applicationBuilt) {
                        this.NativeAuthenticateApplication(this.applicationKey);
                    }
                    else {
                        throw new InvalidOperationException("Require call Build application.");
                    }
                }

                this.NativeFlashAndReset();
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

                    if (read < 0 || (delta > this.ReadDataTimeOut.Ticks)) {
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

                    if (read < 0 || (delta > this.ReadDataTimeOut.Ticks)) {
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
                        Debug.WriteLine("Erasing flash: " + address);
#endif
                        this.externalStorageController.Provider.Erase(address, read, this.ReadDataTimeOut);
                    }
#if DEBUG
                    Debug.WriteLine("Writting to flash: " + address + ", size " + read);
#endif
                    this.externalStorageController.Provider.Write(address, read, this.buffer, 0, this.ReadDataTimeOut);

                    address += (uint)read;
                }
            }

            return totalRead;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(Cache firmware, byte[] firmwareBuffer, Cache application, byte[] applicationBuffer, FileStream applicationFileStream);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateFirmware();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetFirmwareSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateApplication(byte[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetApplicationSize(uint size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeFlashAndReset();

        private extern static uint FirmwareAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint FirmwareMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        private extern static uint ApplicationMaxSize { [MethodImpl(MethodImplOptions.InternalCall)] get; }
    }
}
