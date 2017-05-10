namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public enum SerialError {
        Frame = 4,
        BufferOverrun = 2,
        ReceiveFull = 1,
        ReceiveParity = 3,
        TransmitFull = 0
    }

    public enum SerialHandshake {
        None,
        RequestToSend = 0x06,
        XOnXOff = 0x18,
        RequestToSendXOnXOff = RequestToSend | XOnXOff
    }

    public enum SerialParity {
        None,
        Odd,
        Even,
        Mark,
        Space
    }

    public enum SerialPinChange {
        BreakSignal,
        CarrierDetect,
        ClearToSend,
        DataSetReady,
        RingIndicator
    }

    public enum SerialStopBitCount {
        One = 1,
        OnePointFive = 3,
        Two = 2
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
