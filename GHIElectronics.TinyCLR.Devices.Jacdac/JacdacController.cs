using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;

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


        public JacdacController(string uartPortName) : this(uartPortName, false) {

        }
        public JacdacController(string uartPortName, bool swapTxRx) : this(uartPortName, swapTxRx, null) {

        }
        public JacdacController(string uartPortName, bool swapTxRx, JacdacSetting jacdacSetting ) {
            this.jacdacSetting = jacdacSetting ?? new JacdacSetting();
            this.swapTxRx = swapTxRx;
            this.uartController = int.Parse(uartPortName.Substring(uartPortName.Length - 1, 1));

            this.Acquire();
             
        }

        private void Acquire() => this.NativeAcquire(this.uartController);

        private void Release() {
            this.Disable();
            this.NativeRelease(this.uartController);
        }

        public void Enable() {
            var startPulseDuration = this.jacdacSetting.StartDataGapDuration.Ticks;
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
        public void Dispose() {
            if (!this.disposed) {
                this.disposed = true;

                this.Release();

            }
        }
    }


}
