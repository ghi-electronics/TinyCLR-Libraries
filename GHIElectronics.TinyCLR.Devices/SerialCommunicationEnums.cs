using System;

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

namespace GHIElectronics.TinyCLR.Storage.Streams {
    public enum InputStreamOptions {
        None = 0,
        Partial = 1,
        ReadAhead = 2
    }

    public interface IOutputStream : IDisposable {
        bool Flush();
        uint Write(IBuffer buffer);
    }

    public interface IInputStream : IDisposable {
        uint Read(IBuffer buffer, uint count, InputStreamOptions options);
    }

    public interface IBuffer {
        uint Capacity { get; }
        uint Length { get; set; }
    }

    public class Buffer : IBuffer {
        internal byte[] data;

        private uint length;

        public uint Capacity { get; }

        public uint Length {
            get => this.length;
            set {
                if (value > this.Capacity) throw new ArgumentOutOfRangeException(nameof(value));

                this.length = value;
            }
        }

        public Buffer(uint capacity) {
            this.data = new byte[capacity];
            this.length = 0;
        }
    }
}