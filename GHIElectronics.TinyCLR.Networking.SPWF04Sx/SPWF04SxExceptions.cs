using System;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public class SPWF04SxException : Exception {
        public SPWF04SxException(string message) : base(message) { }
    }

    public class SPWF04SxBufferOverflowException : SPWF04SxException {
        public SPWF04SxBufferOverflowException(string message) : base(message) { }
    }
}
