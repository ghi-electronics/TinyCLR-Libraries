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

        private Mode mode = Mode.None;

        public byte[] ApplicationKey { get; set; }

        private StorageController externalStorageController;
        private bool useExternalStorageController;

        private byte[] buffer;
        private readonly uint bufferSize = 1024;

        private Stream firmwareStream;
        private Stream applicationStream;

        public TimeSpan ReadDataTimeOut { get; set; } = TimeSpan.FromSeconds(5);


        public InFieldUpdate(Stream applicationStream, bool useExternalFlash = false) : this(applicationStream, null, useExternalFlash) {

        }

        public InFieldUpdate(Stream applicationStream, Stream firmwareStream, bool useExternalFlash = false) {
            if (firmwareStream != null) {
                this.mode |= Mode.Firmware;
            }

            if (applicationStream != null) {
                this.mode |= Mode.Application;
            }

            if (firmwareStream != null && (firmwareStream is FileStream || firmwareStream is NetworkStream) && !useExternalFlash) {
                throw new ArgumentException("Firmware updating requires MemoryStream or useExternalFlash equals true");
            }

            if (firmwareStream == null && applicationStream == null) {
                throw new ArgumentNullException();
            }

            this.firmwareStream = firmwareStream;
            this.applicationStream = applicationStream;
            this.useExternalStorageController = useExternalFlash;

            if (useExternalFlash) {
                this.NativeInFieldUpdate(applicationStream, firmwareStream, useExternalFlash);
            }
            else {
                if (firmwareStream != null && !(firmwareStream is MemoryStream))
                    throw new ArgumentException("Firmware has to be Memory stream or use external flash.");

                byte[] applicationBuffer = null;
                byte[] firmwareBuffer = null;

                if (applicationStream is MemoryStream app) {
                    applicationBuffer = app.ToArray();
                }

                if (firmwareStream is MemoryStream fw) {
                    firmwareBuffer = fw.ToArray();
                }

                if (applicationBuffer != null)
                    this.NativeInFieldUpdate(applicationBuffer, firmwareBuffer);
                else
                    this.NativeInFieldUpdate(applicationStream, firmwareStream, useExternalFlash);

            }

            if (useExternalFlash) {
                this.externalStorageController = StorageController.FromName(STM32H7.StorageController.QuadSpi);

                this.externalStorageController.Provider.Open();

                this.buffer = new byte[this.bufferSize];
            }
        }

        public void AuthenticateFirmware(out uint version) {
            if ((this.mode & Mode.Firmware) != Mode.Firmware)
                throw new ArgumentNullException();

            if (this.useExternalStorageController) {
                var totalBufferred = this.BufferingData(this.firmwareStream, FirmwareAddress, FirmwareMaxSize);
                if (totalBufferred > 0)
                    this.NativeSetFirmwareSize((uint)totalBufferred);
                else
                    throw new InvalidOperationException("Data not available.");
            }

            version = this.NativeAuthenticateFirmware();
        }

        public void AuthenticateApplication(out uint version) {
            if (this.ApplicationKey == null) throw new ArgumentNullException(nameof(this.ApplicationKey));

            if ((this.mode & Mode.Application) != Mode.Application)
                throw new ArgumentNullException();

            if (this.useExternalStorageController) {
                var totalBufferred = this.BufferingData(this.applicationStream, ApplicationAddress, ApplicationMaxSize);

                if (totalBufferred > 0)
                    this.NativeSetApplicationSize((uint)totalBufferred);
                else
                    throw new InvalidOperationException("Data not available.");
            }

            version = this.NativeAuthenticateApplication(this.ApplicationKey);
        }

        public void FlashAndReset() {
            if (this.mode != Mode.None) {
                if ((this.mode & Mode.Firmware) == Mode.Firmware) {
                    if (this.useExternalStorageController) {
                        var fwVer = this.NativeAuthenticateFirmware();
                    }
                    else {
                        this.AuthenticateFirmware(out var fwVer);
                    }
                }

                if ((this.mode & Mode.Application) == Mode.Application) {
                    if (this.useExternalStorageController) {
                        var appVer = this.NativeAuthenticateApplication(this.ApplicationKey);
                    }
                    else {
                        this.AuthenticateApplication(out var appVer);
                    }
                }

                this.NativeFlashAndReset();
            }

            throw new ArgumentNullException();
        }

        private int BufferingData(Stream stream, uint address, uint maxSize) {
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
                        Debug.WriteLine("Erasing address: " + address);
#endif
                        this.externalStorageController.Provider.Erase(address, read, this.ReadDataTimeOut);
                    }
#if DEBUG
                    Debug.WriteLine("Writting address: " + address + ", size " + read);
#endif
                    this.externalStorageController.Provider.Write(address, read, this.buffer, 0, this.ReadDataTimeOut);

                    address += (uint)read;
                }
            }

            return totalRead;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(byte[] applicationBuffer, byte[] firmwareBuffer);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(Stream applicationStream, Stream firmwareStream, bool useExternalFlash);

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
