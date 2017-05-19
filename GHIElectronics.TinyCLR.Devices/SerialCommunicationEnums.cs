namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public enum SerialError {
        TransmitFull = 0,
        ReceiveFull = 1,
        BufferOverrun = 2,
        ReceiveParity = 3,
        Frame = 4,
    }

    public enum SerialHandshake {
        None = 0,
        RequestToSend = 1,
        XOnXOff = 2,
        RequestToSendXOnXOff = 3,
    }

    public enum SerialParity {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4
    }

    public enum SerialPinChange {
        BreakSignal = 0,
        CarrierDetect = 1,
        ClearToSend = 2,
        DataSetReady = 3,
        RingIndicator = 4,
    }

    public enum SerialStopBitCount {
        One = 0,
        OnePointFive = 1,
        Two = 2,
    }

    public class ErrorReceivedEventArgs {
        public SerialError Error { get; }

        internal ErrorReceivedEventArgs(SerialError error) => this.Error = error;
    }

    public class PinChangedEventArgs {
        public SerialPinChange PinChange { get; }

        internal PinChangedEventArgs(SerialPinChange pinChange) => this.PinChange = pinChange;
    }
}
