using System;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public delegate void SPWF04SxIndicationReceivedEventHandler(SPWF04SxInterface sender, SPWF04SxIndicationReceivedEventArgs e);
    public delegate void SPWF04SxErrorReceivedEventHandler(SPWF04SxInterface sender, SPWF04SxErrorReceivedEventArgs e);

    public class SPWF04SxIndicationReceivedEventArgs : EventArgs {
        public SPWF04SxIndication Indication { get; }
        public string Message { get; }

        public SPWF04SxIndicationReceivedEventArgs(SPWF04SxIndication indication, string message) {
            this.Indication = indication;
            this.Message = message;
        }
    }

    public class SPWF04SxErrorReceivedEventArgs : EventArgs {
        public int Error { get; }
        public string Message { get; }

        public SPWF04SxErrorReceivedEventArgs(int error, string message) {
            this.Error = error;
            this.Message = message;
        }
    }
}
