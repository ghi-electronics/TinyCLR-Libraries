using System;

namespace GHIElectronics.TinyCLR.Devices.SerialCommunication {
    public enum SerialError {
        Frame,
        BufferOverrun,
        ReceiveFull,
        ReceiveParity,
        TransmitFull
    }

    public enum SerialHandshake {
        None,
        RequestToSend,
        XOnXOff,
        RequestToSendXOnXOff
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
        One,
        OnePointFive,
        Two
    }

    public class ErrorReceivedEventArgs {
        public SerialError Error { get; }

        internal ErrorReceivedEventArgs(SerialError error) {
            this.Error = error;
        }
    }

    public class PinChangedEventArgs {
        public SerialPinChange PinChange { get; }

        internal PinChangedEventArgs(SerialPinChange pinChange) {
            this.PinChange = pinChange;
        }
    }
}

namespace GHIElectronics.TinyCLR.Storage.Streams {
    public enum InputStreamOptions {
        None = 0,
        Partial = 1,
        ReadAhead = 2
    }

    public interface IOutputStream : IDisposable {
        bool FlushAsync();
        uint WriteAsync(IBuffer buffer);
    }

    public interface IInputStream : IDisposable {
        uint ReadAsync(IBuffer buffer, uint count, InputStreamOptions options);
    }

    public interface IBuffer {
        uint Capacity { get; }
        uint Length { get; set; }
    }

    public class Buffer : IBuffer {
        private uint length;
        private byte[] data;

        internal byte[] Data => this.data;
        public uint Capacity => this.length;

        public uint Length {
            get {
                return this.length;
            }
            set {
                this.length = value;

                var newBuffer = new byte[this.length];

                Array.Copy(this.data, newBuffer, (int)this.length);

                this.data = newBuffer;
            }
        }

        public Buffer(byte[] data) {
            if (data == null) throw new ArgumentNullException(nameof(data));

            this.data = data;
            this.length = (uint)data.Length;
        }

        public Buffer(uint length) {
            this.data = new byte[length];
            this.length = length;
        }
    }
}