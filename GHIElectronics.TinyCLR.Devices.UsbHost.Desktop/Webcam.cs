using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Allows a USB webcam (UVC class) to be used.</summary>
    /// <remarks>
    /// Streams raw video frames (YUY2 or MJPEG). The application chooses one of the camera's
    /// supported formats via <see cref="StartStreaming"/> and either subscribes to the
    /// <see cref="FrameAvailable"/> event or polls <see cref="IsNewFrameAvailable"/>.
    /// </remarks>
    public class Webcam : BaseDevice {
        // Default polling cadence for the FrameAvailable event (~30 fps).
        private const int DefaultWorkerInterval = 33;

#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        private Format[] supportedFormats;
        private Format activeFormat;
        private bool streaming;
        private bool processingFrame;  // re-entrancy guard for CheckEvents

        /// <summary>Delegate fired when a new frame is ready.</summary>
        public delegate void FrameAvailableEventHandler(Webcam sender, EventArgs e);

        /// <summary>Raised on the BaseDevice worker thread when a new frame is available.</summary>
        public event FrameAvailableEventHandler FrameAvailable;

        /// <summary>True while the camera is streaming.</summary>
        public bool IsStreaming => this.streaming;

        /// <summary>True if the active stream is MJPEG; false for YUY2.</summary>
        public bool IsMjpeg => this.activeFormat != null && this.activeFormat.Kind == FormatKind.Mjpeg;

        /// <summary>The negotiated frame width in pixels (valid while streaming).</summary>
        public int Width => this.activeFormat == null ? 0 : this.activeFormat.Width;

        /// <summary>The negotiated frame height in pixels (valid while streaming).</summary>
        public int Height => this.activeFormat == null ? 0 : this.activeFormat.Height;

        /// <summary>Bytes per frame (= Width * Height * 2 for YUY2). Valid while streaming.</summary>
        public int FrameSize {
            get {
                this.NativeGetFrameSize(out var size);
                return size;
            }
        }

        /// <summary>The camera's supported formats.</summary>
        public Format[] SupportedFormats => this.supportedFormats;

        /// <summary>Constructs a webcam wrapper for a connected UVC camera.</summary>
        /// <param name="id">Device id from the connection event.</param>
        /// <param name="interfaceIndex">VideoControl interface index from the connection event.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown when no standard UVC VideoStreaming interface is found on the device.
        /// </exception>
        public Webcam(uint id, byte interfaceIndex)
            : base(id, interfaceIndex, DeviceType.Webcam) {
            try {
                this.NativeConstructor(this.Id, this.InterfaceIndex);
            }
            catch (NotSupportedException) {
                throw new NotSupportedException(
                    "No standard streaming interface found. It cannot be streamed via UVC.");
            }

            this.streaming = false;
            this.activeFormat = null;
            this.supportedFormats = this.QuerySupportedFormats();

            this.WorkerInterval = DefaultWorkerInterval;
        }

        /// <summary>Finalizer.</summary>
        ~Webcam() {
            this.Dispose(false);
        }

        /// <summary>Starts streaming the chosen format.</summary>
        /// <param name="format">One of the entries from <see cref="SupportedFormats"/>.</param>
        /// <param name="fps">Requested frame rate. Default 15.</param>
        public void StartStreaming(Format format, int fps = 15) {
            this.CheckObjectState();

            if (format == null) throw new ArgumentNullException(nameof(format));
            if (fps < 0) throw new ArgumentOutOfRangeException(nameof(fps));
            if (this.streaming) throw new InvalidOperationException("Already streaming.");

            this.NativeStartStreaming(format.FormatType, format.BFormatIndex, format.BFrameIndex,
                                       format.Width, format.Height, fps);

            this.activeFormat = format;
            this.streaming = true;
        }

        /// <summary>Flushes any frames queued in the camera's internal FIFO.</summary>
        /// <param name="soft">false = hard alt-toggle flush; true = soft SET_CUR(Commit) replay.</param>
        public void Resync(bool soft = false) {
            this.CheckObjectState();
            if (!this.streaming) throw new InvalidOperationException("Not streaming.");

            this.NativeResync(soft);
        }

        /// <summary>Stops streaming and frees device-side buffers.</summary>
        public void StopStreaming() {
            if (!this.streaming) return;

            this.NativeStopStreaming();
            this.streaming = false;
            this.activeFormat = null;
        }

        /// <summary>Returns true and clears the flag if a new frame is ready since the last call.</summary>
        public bool IsNewFrameAvailable() {
            this.CheckObjectState();
            return this.NativeIsNewFrameAvailable();
        }

        /// <summary>Copies the most recently completed frame into <paramref name="buffer"/>.</summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <returns>Actual number of bytes written.</returns>
        public int GetFrame(byte[] buffer) {
            this.CheckObjectState();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (!this.streaming) throw new InvalidOperationException("Not streaming.");

            return this.NativeGetFrame(buffer);
        }

        /// <summary>Converts a YUY2 (YUYV-packed) buffer to RGB565 into <paramref name="rgb565"/>.</summary>
        /// <param name="yuy2">Source buffer with YUY2-packed pixels (Y1 U Y2 V tuples).</param>
        /// <param name="rgb565">Destination buffer for RGB565 pixels (2 bytes per pixel).</param>
        /// <remarks>
        /// Pure managed implementation on Desktop so PC test code matches device behavior.
        /// BT.601 conversion math; identical formula to the native YUV2.cpp helper. Output byte
        /// count equals input byte count (4 source bytes -> 2 RGB565 pixels = 4 dest bytes).
        /// Internally rounded down to a multiple of 4.
        /// </remarks>
        public static void ConvertYuy2ToRgb565(byte[] yuy2, byte[] rgb565) {
            if (yuy2 == null) throw new ArgumentNullException(nameof(yuy2));
            if (rgb565 == null) throw new ArgumentNullException(nameof(rgb565));
            if (rgb565.Length < yuy2.Length) throw new ArgumentException("rgb565 buffer must be at least yuy2.Length bytes.", nameof(rgb565));

            var size = yuy2.Length - (yuy2.Length % 4);
            var srcIdx = 0;
            var dstIdx = 0;

            while (srcIdx < size) {
                int y1 = yuy2[srcIdx + 0] - 16;
                int u  = yuy2[srcIdx + 1] - 128;
                int y2 = yuy2[srcIdx + 2] - 16;
                int v  = yuy2[srcIdx + 3] - 128;

                int r = (298 * y1 + 409 * v + 128) >> 8;
                int g = (298 * y1 - 100 * u - 208 * v + 128) >> 8;
                int b = (298 * y1 + 516 * u + 128) >> 8;
                if (r < 0) r = 0; else if (r > 255) r = 255;
                if (g < 0) g = 0; else if (g > 255) g = 255;
                if (b < 0) b = 0; else if (b > 255) b = 255;
                var pix1 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3);
                rgb565[dstIdx++] = (byte)(pix1 & 0xFF);
                rgb565[dstIdx++] = (byte)((pix1 >> 8) & 0xFF);

                r = (298 * y2 + 409 * v + 128) >> 8;
                g = (298 * y2 - 100 * u - 208 * v + 128) >> 8;
                b = (298 * y2 + 516 * u + 128) >> 8;
                if (r < 0) r = 0; else if (r > 255) r = 255;
                if (g < 0) g = 0; else if (g > 255) g = 255;
                if (b < 0) b = 0; else if (b > 255) b = 255;
                var pix2 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3);
                rgb565[dstIdx++] = (byte)(pix2 & 0xFF);
                rgb565[dstIdx++] = (byte)((pix2 >> 8) & 0xFF);

                srcIdx += 4;
            }
        }

        /// <summary>Disposes the webcam, stopping any active stream.</summary>
        protected override void Dispose(bool disposing) {
            if (this.disposed) return;

            if (this.streaming) {
                try { this.NativeStopStreaming(); } catch { }
                this.streaming = false;
            }

            this.NativeFinalize();

            base.Dispose(disposing);
        }

        /// <summary>Polled by BaseDevice's worker; raises FrameAvailable when a new frame lands.</summary>
        protected override void CheckEvents(object sender) {
            if (!this.CheckObjectState(false)) return;
            if (!this.streaming) return;

            if (this.processingFrame) return;
            this.processingFrame = true;
            try {
                bool ready;
                try { ready = this.NativeIsNewFrameAvailable(); }
                catch { return; }

                if (ready)
                    this.FrameAvailable?.Invoke(this, EventArgs.Empty);
            }
            finally {
                this.processingFrame = false;
            }
        }

        private Format[] QuerySupportedFormats() {
            this.NativeGetFormatsCount(out var count);
            if (count <= 0) return new Format[0];

            var arr = new Format[count];
            for (var i = 0; i < count; i++) {
                this.NativeGetFormat(i, out var w, out var h, out var ft, out var bfi, out var bfri);
                arr[i] = new Format {
                    Width = w,
                    Height = h,
                    FormatType = ft,
                    BFormatIndex = bfi,
                    BFrameIndex = bfri
                };
            }
            return arr;
        }

        // Desktop: no real USB Host webcam. No-op stubs so the public surface compiles and
        // callers that branch on IsStreaming / IsNewFrameAvailable just see "no device, no frames".
        // Test rigs that want frames can mock at the application boundary (e.g. inject bytes
        // via a test-only setter or use ConvertYuy2ToRgb565 directly with synthetic buffers).
        private void NativeConstructor(uint id, byte interfaceIndex) { }
        private void NativeFinalize() { }
        private void NativeGetFormatsCount(out int count) { count = 0; }
        private void NativeGetFormat(int index, out int width, out int height,
                                     out byte formatType, out byte bFormatIndex, out byte bFrameIndex) {
            width = 0; height = 0; formatType = 0; bFormatIndex = 0; bFrameIndex = 0;
        }
        private void NativeStartStreaming(byte formatType, byte bFormatIndex, byte bFrameIndex,
                                          int width, int height, int fps) { }
        private void NativeStopStreaming() { }
        private void NativeResync(bool soft) { }
        private bool NativeIsNewFrameAvailable() => false;
        private void NativeGetFrameSize(out int size) { size = 0; }
        private int NativeGetFrame(byte[] buffer) => 0;

        /// <summary>The on-the-wire pixel encoding for a UVC stream.</summary>
        public enum FormatKind : byte {
            /// <summary>Uncompressed 4:2:2 packed (YUYV).</summary>
            Yuy2 = 0,
            /// <summary>Motion-JPEG.</summary>
            Mjpeg = 1,
        }

        /// <summary>A camera-supported video format / resolution combination.</summary>
        public class Format {
            /// <summary>Frame width in pixels.</summary>
            public int Width { get; internal set; }
            /// <summary>Frame height in pixels.</summary>
            public int Height { get; internal set; }
            /// <summary>The pixel encoding (YUY2 or MJPEG).</summary>
            public FormatKind Kind => (FormatKind)this.FormatType;

            // Internal wire-format identifiers. 0 = YUY2, 1 = MJPEG.
            internal byte FormatType { get; set; }
            internal byte BFormatIndex { get; set; }
            internal byte BFrameIndex { get; set; }
        }
    }
}
