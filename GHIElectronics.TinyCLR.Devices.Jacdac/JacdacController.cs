using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Jacdac {

    public class JacdacSetting {        
        public TimeSpan StartPulseDuration { get; set; } = TimeSpan.FromTicks(140);
        public TimeSpan StartDataGapDuration { get; set; } = TimeSpan.FromTicks(600);
        public TimeSpan DataByteGapDuration { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan DataEndGapDuration { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan EndPulseDuration { get; set; } = TimeSpan.FromTicks(140);
        public TimeSpan FrameToFramDuration { get; set; } = TimeSpan.FromTicks(200);
    }

    
    public class JacdacController :IDisposable {

        int uartController;        
        private bool enabled;
        private bool swapTxRx;
        private JacdacSetting jacdacSetting;
        private bool disposed;

        private readonly NativeEventDispatcher nativeEventDispatcher;

        public delegate void PacketReceivedEvent(JacdacController sender, Packet packet);

        public event PacketReceivedEvent PacketReceived;

        public JacdacController(string uartPortName) : this(uartPortName, false) {

        }
        public JacdacController(string uartPortName, bool swapTxRx) : this(uartPortName, swapTxRx, null) {

        }
        public JacdacController(string uartPortName, bool swapTxRx, JacdacSetting jacdacSetting ) {
            this.jacdacSetting = jacdacSetting ?? new JacdacSetting();
            this.swapTxRx = swapTxRx;
            this.uartController = int.Parse(uartPortName.Substring(uartPortName.Length - 1, 1));

            this.Acquire();

            this.nativeEventDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.JacdacController.Event");

            this.nativeEventDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => {
                if (apiName.CompareTo("JacdacController") == 0 && d0 == this.uartController) {
                    var time = TimeSpan.FromTicks(d2);
                    var data = new byte[(int)d1];

                    this.NativeReadRawPacket(this.uartController, data, data.Length);

                    var packet = Packet.FromBinary(data);

                    packet.Timestamp = time;

                    this.PacketReceived?.Invoke(this, packet);
                }                    
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

            this.NativeSetActiveSetting(this.uartController, this.swapTxRx, startPulseDuration, startDataGapDuration, dataByteGapDuration, dataEndGapDuration, endPulseDuration, frameToFramDuration);
            this.NativeEnable(this.uartController);
        }

        public void Disable() => this.NativeDisable(this.uartController);

        public void SendRawPacket(byte[] data, int offset, int count) => this.NativeSendRawPacket(this.uartController, data, offset, count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetActiveSetting(int uartController, bool swapTxRx, long startPulseDuration, long startDataGapDuration, long dataByteGapDuration, long dataEndGapDuration, long endPulseDuration, long frameToFramDuration);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeEnable(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDisable(int uartController);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSendRawPacket(int uartController, byte[] data, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeReadRawPacket(int uartController, byte[] data, int count);
        public void Dispose() {
            if (!this.disposed) {
                this.disposed = true;

                this.Release();

            }
        }
    }


}
