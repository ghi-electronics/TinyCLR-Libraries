using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Update {
    public sealed class InFieldUpdate {
        private enum Mode {
            None = 0,
            Firmware = 1,
            Application = 2
        }

        private Mode mode = Mode.None;

        public byte[] ApplicationKey { get; set; }

        public InFieldUpdate(byte[] firmwareBuffer, byte[] applicationBuffer) {
            if (firmwareBuffer == null && applicationBuffer == null)
                throw new ArgumentNullException();

            this.NativeInFieldUpdate(firmwareBuffer, applicationBuffer);

            if (firmwareBuffer != null)
                this.mode |= Mode.Firmware;

            if (applicationBuffer != null)
                this.mode |= Mode.Application;
        }

        public InFieldUpdate(FileStream stream) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            this.NativeInFieldUpdate(stream);

            this.mode |= Mode.Application;
        }

        public void AuthenticateFirmware(out uint version) {
            if ((this.mode & Mode.Firmware) != Mode.Firmware)
                throw new ArgumentNullException();

            version = this.NativeAuthenticateFirmware();
        }

        public void AuthenticateApplication(out uint version) {
            if (this.ApplicationKey == null ) throw new ArgumentNullException(nameof(this.ApplicationKey));

            if ((this.mode & Mode.Application) != Mode.Application)
                throw new ArgumentNullException();

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
        private extern void NativeInFieldUpdate(FileStream stream);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateFirmware();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint NativeAuthenticateApplication(byte[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeFlashAndReset();
    }
}
