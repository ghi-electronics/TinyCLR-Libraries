using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Update {
    public class InFieldUpdate {

        public InFieldUpdate(byte[] firmwareBuffer, byte[] applicationBuffer) => this.NativeInFieldUpdate(firmwareBuffer, applicationBuffer);

        public InFieldUpdate(FileStream stream) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            this.NativeInFieldUpdate(stream);
        }

        public void AuthenticateFirmware(out uint version) => version = this.NativeAuthenticateFirmware();

        public void AuthenticateApplication(byte[] key, out uint version) {
            if (key == null) throw new ArgumentNullException(nameof(key));

            version = this.NativeAuthenticateApplication(key);
        }

        public void FlashAndReset() => this.NativeFlashAndReset();

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
