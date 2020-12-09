using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
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

        private StorageController cacheStorage;
        private bool useExternalFlash;

        public static uint FirmwareAddressOffet { get; } = 0;
        public static uint FirmwareMaxSize { get; } = 2 * 1024 * 1024;

        public static uint ApplicationAddressOffet { get; } = FirmwareAddressOffet + FirmwareMaxSize;
        public static uint ApplicationMaxSize { get; } = 6 * 1024 * 1024;

        private byte[] buffer;
        private Stream firmwareStream;
        private Stream applicationStream;
        private uint bufferSize;


        const uint BUFFER_SIZE = 4 * 1024;

        public InFieldUpdate(Stream applicationStream, bool useExternalFlash = false) : this(null, applicationStream, useExternalFlash) {

        }

        public InFieldUpdate(Stream firmwareStream, Stream applicationStream, bool useExternalFlash = false) {
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
            this.useExternalFlash = useExternalFlash;

            if (firmwareStream != null) {
                if (firmwareStream is FileStream firmwareFileStream) {
                    if ((applicationStream) == null)
                        this.NativeInFieldUpdate(firmwareFileStream, null, useExternalFlash);
                    else {
                        if (applicationStream is FileStream applicationFileStream) {
                            this.NativeInFieldUpdate(firmwareFileStream, applicationFileStream, useExternalFlash);
                        }
                        else {
                            throw new ArgumentException("");
                        }
                    }

                }
                else if (firmwareStream is MemoryStream firmwareMemoryStream) {

                    var firmwareBuffer = firmwareMemoryStream.ToArray();

                    if ((applicationStream) == null)
                        this.NativeInFieldUpdate(firmwareBuffer, null);
                    else {
                        if (applicationStream is MemoryStream applicationMemoryStream) {

                            var applicationBuffer = applicationMemoryStream.ToArray();

                            this.NativeInFieldUpdate(firmwareBuffer, applicationBuffer);
                        }
                        else {
                            throw new ArgumentException("");
                        }
                    }

                }
                else if (firmwareStream is NetworkStream firmwareNetworkStream) {
                    if ((applicationStream) == null) {
                        this.NativeInFieldUpdate(firmwareNetworkStream, null);
                    }
                    else {
                        if (applicationStream is NetworkStream applicationNetworkStream) {
                            this.NativeInFieldUpdate(firmwareNetworkStream, applicationNetworkStream);
                        }
                        else {
                            throw new ArgumentException("");
                        }
                    }
                }
            }
            else {
                if (applicationStream is FileStream applicationFileStream) {
                    this.NativeInFieldUpdate(null, applicationFileStream, useExternalFlash);
                }
                else if (applicationStream is MemoryStream applicationMemoryStream) {

                    var applicationBuffer = applicationMemoryStream.ToArray();

                    this.NativeInFieldUpdate(null, applicationBuffer);
                }
                else if (applicationStream is NetworkStream applicationNetworkStream) {
                    this.NativeInFieldUpdate(null, applicationNetworkStream);
                }
            }

            if (useExternalFlash) {
                this.cacheStorage = StorageController.FromName(STM32H7.StorageController.QuadSpi);

                this.cacheStorage.Provider.Open();

                if (this.cacheStorage.Descriptor != null && this.cacheStorage.Descriptor.RegionSizes != null)
                    this.bufferSize = (uint)this.cacheStorage.Descriptor.RegionSizes[0];
                else
                    this.bufferSize = BUFFER_SIZE;

                this.buffer = new byte[this.bufferSize];
            }



        }

        //public InFieldUpdate(byte[] firmwareBuffer, byte[] applicationBuffer) {
        //    if (firmwareBuffer == null && applicationBuffer == null)
        //        throw new ArgumentNullException();

        //    this.NativeInFieldUpdate(firmwareBuffer, applicationBuffer);

        //    if (firmwareBuffer != null)
        //        this.mode |= Mode.Firmware;

        //    if (applicationBuffer != null)
        //        this.mode |= Mode.Application;

        //    this.mode |= Mode.Firmware | Mode.Application;
        //}

        //public InFieldUpdate(FileStream stream) {
        //    if (stream == null) throw new ArgumentNullException(nameof(stream));

        //    this.NativeInFieldUpdate(stream);

        //    this.mode |= Mode.Application;
        //}

        public void AuthenticateFirmware(out uint version) {
            if ((this.mode & Mode.Firmware) != Mode.Firmware)
                throw new ArgumentNullException();

            if (this.useExternalFlash) {
                if (this.firmwareStream is NetworkStream) {
                    //Todo
                }
                else if (this.firmwareStream is MemoryStream) {
                    //No thing to do
                }
                else if (this.firmwareStream is FileStream) {
                    this.firmwareStream.Seek(0, SeekOrigin.Begin);

                    var block = this.firmwareStream.Length / this.bufferSize;
                    var remain = this.firmwareStream.Length % this.bufferSize;
                    var idx = 0U;
                    var address = FirmwareAddressOffet;

                    // erase
                    var b = remain > 0 ? (block + 1) : block;

                    while (b > 0) {
                        var size = (this.firmwareStream.Length - idx) > this.bufferSize ? this.bufferSize : (this.firmwareStream.Length - idx);

                        if (!this.cacheStorage.Provider.IsErased(address, (int)size)) {
                            Debug.WriteLine("Erasing..." + address);
                            this.cacheStorage.Provider.Erase(address, (int)size, TimeSpan.FromSeconds(1));
                        }

                        b--;
                        idx += (uint)size;
                        address += (uint)size;




                    }

                    idx = 0;
                    address = FirmwareAddressOffet;

                    b = remain > 0 ? (block + 1) : block;

                    while (b > 0) {
                        var r = (this.firmwareStream.Length - idx) > this.bufferSize ? this.bufferSize : (this.firmwareStream.Length - idx);
                        var size = this.firmwareStream.Read(this.buffer, 0, (int)r);

                        if (size > 0) {
                            this.cacheStorage.Provider.Write(address, size, this.buffer, 0, TimeSpan.FromSeconds(1));

                            Debug.WriteLine("Writting..." + idx + ", address: " + address);
                        }

                        b--;
                        idx += (uint)size;
                        address += (uint)size;


                        Thread.Sleep(1);
                    }

                    this.NativeSetFirmwareSize((uint)this.firmwareStream.Length);
                }
            }

            version = this.NativeAuthenticateFirmware();
        }

        public void AuthenticateApplication(out uint version) {
            if (this.ApplicationKey == null) throw new ArgumentNullException(nameof(this.ApplicationKey));

            if ((this.mode & Mode.Application) != Mode.Application)
                throw new ArgumentNullException();

            if (this.useExternalFlash) {
                if (this.applicationStream is NetworkStream) {
                    //Todo
                }
                else if (this.applicationStream is MemoryStream) {
                    //No thing to do
                }
                else if (this.applicationStream is FileStream) {
                    this.applicationStream.Seek(0, SeekOrigin.Begin);

                    var block = this.applicationStream.Length / this.bufferSize;
                    var remain = this.applicationStream.Length % this.bufferSize;
                    var idx = 0U;
                    var address = ApplicationAddressOffet;

                    // erase
                    var b = remain > 0 ? (block + 1) : block;

                    while (b > 0) {
                        var size = (this.applicationStream.Length - idx) > this.bufferSize ? this.bufferSize : (this.applicationStream.Length - idx);

                        if (!this.cacheStorage.Provider.IsErased(address, (int)size)) {
                            Debug.WriteLine("Erasing app..." + address);
                            this.cacheStorage.Provider.Erase(address, (int)size, TimeSpan.FromSeconds(1));
                        }

                        b--;
                        idx += (uint)size;
                        address += (uint)size;

                    }

                    idx = 0;
                    address = ApplicationAddressOffet;

                    b = remain > 0 ? (block + 1) : block;

                    while (b > 0) {
                        var r = (this.applicationStream.Length - idx) > this.bufferSize ? this.bufferSize : (this.applicationStream.Length - idx);
                        var size = this.applicationStream.Read(this.buffer, 0, (int)r);

                        if (size > 0) {
                            Debug.WriteLine("Writting app..." + address);
                            this.cacheStorage.Provider.Write(address, size, this.buffer, 0, TimeSpan.FromSeconds(1));
                        }

                        b--;
                        idx += (uint)size;
                        address += (uint)size;
                    }

                    this.NativeSetApplicationSize((uint)this.applicationStream.Length);
                }

            }

            version = this.NativeAuthenticateApplication(this.ApplicationKey);
        }

        public void FlashAndReset() {
            if (this.mode != Mode.None) {
                if ((this.mode & Mode.Firmware) == Mode.Firmware)
                    this.AuthenticateFirmware(out var fwVer);

                if ((this.mode & Mode.Application) == Mode.Application)
                    this.AuthenticateApplication(out var appVer);

                this.NativeFlashAndReset();
            }

            throw new ArgumentNullException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(byte[] firmwareBuffer, byte[] applicationBuffer);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(FileStream firmwareStream, FileStream applicationStream, bool useExternalFlash);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInFieldUpdate(NetworkStream firmwareStream, NetworkStream applicationStream);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //private extern void NativeInFieldUpdate(FileStream stream);

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
    }
}
