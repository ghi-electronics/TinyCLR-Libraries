using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Jacdac.Transport {

    public class JacdacSerialWireSetting {
        public TimeSpan StartPulseDuration { get; set; } = TimeSpan.FromTicks(140);
        public TimeSpan StartDataGapDuration { get; set; } = TimeSpan.FromTicks(600);
        public TimeSpan DataByteGapDuration { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan DataEndGapDuration { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan EndPulseDuration { get; set; } = TimeSpan.FromTicks(140);
        public TimeSpan FrameToFramDuration { get; set; } = TimeSpan.FromTicks(200);

        public int WriteBufferSize => 1;
        public int ReadBufferSize { get; set; } = 32;
    }

    public enum JacdacSerialWireError {
        Frame = 0,
        Overrun = 1,
        BufferFull = 2,
    }

    public class ErrorReceivedEventArgs {
        public JacdacSerialWireError Error { get; }
        public DateTime Timestamp { get; }

        public byte[] Data { get; }

        internal ErrorReceivedEventArgs(JacdacSerialWireError error, DateTime timestamp, byte[] data) {
            this.Error = error;
            this.Timestamp = timestamp;
            this.Data = data;
        }
    }

    public class PacketReceivedEventArgs {
        public DateTime Timestamp { get; }

        public byte[] Data { get; }

        internal PacketReceivedEventArgs(byte[] data, DateTime timestamp) {
            this.Timestamp = timestamp;
            this.Data = data;
        }
    }


    public class JacdacSerialWireController : IDisposable {

        private int uartController;
        private bool swapTxRx;
        private JacdacSerialWireSetting jacdacSetting;
        private UartSetting uartSetting;
        private bool disposed;

        private readonly NativeEventDispatcher nativeReceivedEventDispatcher;
        private readonly NativeEventDispatcher nativeErrorEventDispatcher;

        public delegate void PacketReceivedEvent(JacdacSerialWireController sender, PacketReceivedEventArgs packet);
        public delegate void ErrorReceivedEvent(JacdacSerialWireController sender, ErrorReceivedEventArgs args);

        private PacketReceivedEvent packetReceived;
        private ErrorReceivedEvent errorReceived;

        public JacdacSerialWireController(string uartPortName) : this(uartPortName, null) {

        }
        public JacdacSerialWireController(string uartPortName, UartSetting uartSetting) : this(uartPortName, uartSetting, null) {

        }
        public JacdacSerialWireController(string uartPortName, UartSetting uartSetting, JacdacSerialWireSetting jacdacSetting) {

            this.uartSetting = uartSetting ?? new UartSetting() {
                BaudRate = 1000000,
            };

            this.jacdacSetting = jacdacSetting ?? new JacdacSerialWireSetting();

            if (this.uartSetting.BaudRate == 0)
                this.uartSetting.BaudRate = 1000000;

            if (this.uartSetting.DataBits == 0)
                this.uartSetting.DataBits = 8;

            if (this.uartSetting.BaudRate != 1000000
                || this.uartSetting.DataBits != 8
                || this.uartSetting.Parity != UartParity.None
                || this.uartSetting.StopBits != UartStopBitCount.One
                || this.uartSetting.Handshaking != UartHandshake.None
                  )

                throw new Exception("Not support.");

            if (this.jacdacSetting.ReadBufferSize < 1)
                throw new Exception("Not support.");


            this.swapTxRx = this.uartSetting.SwapTxRxPin;

            this.uartController = int.Parse(uartPortName.Substring(uartPortName.Length - 1, 1));

            this.Acquire();

            this.nativeReceivedEventDispatcher = NativeEventDispatcher.GetDispatcher("JacdacSerialWireController.ReceivedEvent");
            this.nativeErrorEventDispatcher = NativeEventDispatcher.GetDispatcher("JacdacSerialWireController.ErrorEvent");

            this.nativeReceivedEventDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => {
                if (apiName.CompareTo("JacdacSerialWireController") == 0 && d0 == this.uartController) {
                    var data = new byte[(int)d1];

                    this.NativeRead(this.uartController, data, data.Length);

                    var args = new PacketReceivedEventArgs(data, ts);

                    this.packetReceived?.Invoke(this, args);
                }
            };

            this.nativeErrorEventDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => {
                if (apiName.CompareTo("JacdacSerialWireController") == 0 && d0 == this.uartController) {
                    byte[] data = null;

                    var error = (JacdacSerialWireError)d2;


                    if (d1 > 0) {
                        data = new byte[(int)d1];

                        this.NativeReadErrorPacket(this.uartController, data, data.Length);
                    }

                    var args = new ErrorReceivedEventArgs(error, ts, data);

                    this.errorReceived?.Invoke(this, args);
                };

            };
        }

        private void Acquire() => this.NativeAcquire(this.uartController);

        private void Release() {
            this.Disable();
            this.NativeRelease(this.uartController);
        }

        public void Enable() {
            var startPulseDuration = this.jacdacSetting.StartPulseDuration.Ticks;
            var startDataGapDuration = this.jacdacSetting.StartDataGapDuration.Ticks;
            var dataByteGapDuration = this.jacdacSetting.DataByteGapDuration.Ticks;
            var dataEndGapDuration = this.jacdacSetting.DataEndGapDuration.Ticks;
            var endPulseDuration = this.jacdacSetting.EndPulseDuration.Ticks;
            var frameToFramDuration = this.jacdacSetting.FrameToFramDuration.Ticks;
            var readBufferSize = this.jacdacSetting.ReadBufferSize;

            this.NativeSetActiveSetting(this.uartController, this.swapTxRx, startPulseDuration, startDataGapDuration, dataByteGapDuration, dataEndGapDuration, endPulseDuration, frameToFramDuration, readBufferSize);
            this.NativeEnable(this.uartController);
        }

        public void Disable() => this.NativeDisable(this.uartController);

        public void Write(byte[] data) => this.Write(data, 0, data.Length);
        public void Write(byte[] data, int offset, int count) {
            if (data == null
                || offset < 0
                || offset + count > data.Length) {
                throw new ArgumentException("Invalid argument.");
            }
            this.NativeWrite(this.uartController, data, offset, count);
        }

        public event PacketReceivedEvent PacketReceived {
            add {
                if (this.packetReceived == null) {
                    this.NativeEnableReceivedPacketEvent(this.uartController, true);
                }
                this.packetReceived += value;
            }

            remove {
                if (this.packetReceived != null) {
                    this.packetReceived -= value;
                }

                if (this.packetReceived == null) {
                    this.NativeEnableReceivedPacketEvent(this.uartController, false);
                }
            }
        }

        public event ErrorReceivedEvent ErrorReceived {
            add {
                if (this.errorReceived == null) {
                    this.NativeEnableErrorReceivedEvent(this.uartController, true);
                }
                this.errorReceived += value;
            }

            remove {
                if (this.errorReceived != null) {
                    this.errorReceived -= value;
                }

                if (this.errorReceived == null) {
                    this.NativeEnableErrorReceivedEvent(this.uartController, false);
                }
            }
        }

        public static ushort Crc(byte[] data, int index, int size) => NativeCrc(data, index, size);

        public void ClearReadBuffer() => this.NativeClearReadBuffer(this.uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetActiveSetting(int uartController, bool swapTxRx, long startPulseDuration, long startDataGapDuration, long dataByteGapDuration, long dataEndGapDuration, long endPulseDuration, long frameToFramDuration, int readBufferSize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeEnable(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDisable(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeWrite(int uartController, byte[] data, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeRead(int uartController, byte[] data, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeEnableReceivedPacketEvent(int uartController, bool value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeEnableErrorReceivedEvent(int uartController, bool value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeClearReadBuffer(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeReadErrorPacket(int uartController, byte[] data, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static ushort NativeCrc(byte[] data, int offset, int count);

        public void Dispose() {
            if (!this.disposed) {
                this.disposed = true;

                this.Release();

            }
        }
    }


}
