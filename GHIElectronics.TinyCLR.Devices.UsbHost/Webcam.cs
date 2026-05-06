using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Allows a USB webcam (UVC class, YUY2 uncompressed) to be used.</summary>
    /// <remarks>
    /// Streams raw video frames pre-converted from YUY2 to RGB565 on the device.
    /// The application chooses one of the camera's supported resolutions via
    /// <see cref="StartStreaming"/> and then either subscribes to the
    /// <see cref="FrameAvailable"/> event or polls <see cref="IsNewFrameAvailable"/>.
    /// Each frame is <see cref="FrameSize"/> bytes (= <c>Width * Height * 2</c>) of
    /// RGB565 pixel data, fetched with <see cref="GetFrame"/>.
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
        /// <remarks>The event handler should call <see cref="GetFrame"/> to retrieve the frame.</remarks>
        public event FrameAvailableEventHandler FrameAvailable;

        /// <summary>True while the camera is streaming.</summary>
        public bool IsStreaming => this.streaming;

        /// <summary>True if the active stream is MJPEG (caller must JPEG-decode); false for YUY2 (use <see cref="ConvertYuy2ToRgb565"/>).</summary>
        /// <remarks>Returns false until MJPEG support is implemented; YUY2 is the only currently-streamable format.</remarks>
        public bool IsMjpeg => this.activeFormat != null && this.activeFormat.Kind == FormatKind.Mjpeg;

        /// <summary>The negotiated frame width in pixels (valid while streaming).</summary>
        public int Width => this.activeFormat == null ? 0 : this.activeFormat.Width;

        /// <summary>The negotiated frame height in pixels (valid while streaming).</summary>
        public int Height => this.activeFormat == null ? 0 : this.activeFormat.Height;

        /// <summary>Bytes per frame, RGB565 (= Width * Height * 2). Valid while streaming.</summary>
        public int FrameSize {
            get {
                this.NativeGetFrameSize(out var size);
                return size;
            }
        }

        /// <summary>The camera's supported YUY2 formats (queried at construction).</summary>
        public Format[] SupportedFormats => this.supportedFormats;

        /// <summary>Constructs a webcam wrapper for a connected UVC camera.</summary>
        /// <param name="id">Device id from the connection event.</param>
        /// <param name="interfaceIndex">VideoControl interface index from the connection event.</param>
        public Webcam(uint id, byte interfaceIndex)
            : base(id, interfaceIndex, DeviceType.Webcam) {
            this.NativeConstructor(this.Id, this.InterfaceIndex);

            this.streaming = false;
            this.activeFormat = null;
            this.supportedFormats = this.QuerySupportedFormats();

            this.WorkerInterval = DefaultWorkerInterval;
        }

        /// <summary>Finalizer.</summary>
        ~Webcam() {
            this.Dispose(false);
        }

        /// <summary>Starts streaming the chosen format. Allocates the device-side double buffer.</summary>
        /// <param name="format">One of the entries from <see cref="SupportedFormats"/>.</param>
        /// <param name="fps">
        /// Requested frame rate. Default is <c>15</c> — the slowest rate that virtually every
        /// UVC camera supports, so it's the broadest-compatibility starting point. Pass <c>0</c>
        /// to use the camera's own default rate (typically 30 fps) from the frame descriptor.
        /// Any other value asks the camera for that rate; the camera rounds to its nearest
        /// supported value (visible in the PROBE GET_CUR exchange).
        ///
        /// <para>Use this to throttle a camera that produces faster than your application
        /// can consume: cheap UVC cameras often have an internal FIFO and produce at their
        /// max rate regardless. If you can render at 10 fps but the camera produces at 30,
        /// 20 fps of frames pile up in its FIFO per second, manifesting as a several-second
        /// "playback delayed behind reality" lag. Setting <paramref name="fps"/> to match
        /// your app's processing rate keeps producer and consumer aligned and eliminates
        /// the lag. When the camera refuses to go below its minimum supported rate, use
        /// <see cref="Resync"/> periodically to flush its internal FIFO instead.</para>
        /// </param>
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

        /// <summary>Flushes any frames the camera has queued in its internal sensor->encoder FIFO.</summary>
        /// <param name="soft">
        /// <c>false</c> (default) — hard flush via streaming alt-setting toggle. Definitive on
        /// every UVC camera but ~500-1000 ms blackout because the camera reboots its sensor
        /// pipeline.
        /// <para><c>true</c> — soft flush, replays the previously negotiated SET_CUR (Commit)
        /// only. ~20 ms blackout. Camera-dependent: some cameras flush their FIFO on
        /// commit-during-streaming, others ignore it and the call is effectively a no-op.
        /// Try once on your specific camera and fall back to <c>false</c> if the lag
        /// doesn't drop.</para>
        /// </param>
        /// <remarks>
        /// Some UVC cameras capture internally faster than they can ship over USB and accumulate
        /// the difference in an internal FIFO, producing a "screen lags real life" effect that
        /// grows over time. Calling <c>Resync</c> drops the camera's internal queue so the next
        /// frame the application receives reflects the moment of the call rather than several
        /// seconds ago. Application chooses the cadence. Throws if not currently streaming.
        /// </remarks>
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
        /// <remarks>
        /// Useful for polling rather than the <see cref="FrameAvailable"/> event. After this
        /// returns true, call <see cref="GetFrame"/> to retrieve the bytes.
        /// </remarks>
        public bool IsNewFrameAvailable() {
            this.CheckObjectState();
            return this.NativeIsNewFrameAvailable();
        }

        /// <summary>Copies the most recently completed frame into <paramref name="buffer"/> as raw bytes.</summary>
        /// <param name="buffer">Destination buffer. Must be at least <see cref="FrameSize"/> bytes (which is the upper bound; actual size returned).</param>
        /// <returns>Actual number of bytes written. For YUY2 always <c>Width*Height*2</c>. For MJPEG, varies per frame.</returns>
        /// <remarks>
        /// Returned bytes are the camera's raw stream:
        /// <list type="bullet">
        /// <item>For YUY2 (<see cref="IsMjpeg"/> == false): YUYV-packed pixels (Y1 U Y2 V tuples). Use <see cref="ConvertYuy2ToRgb565"/> to convert.</item>
        /// <item>For MJPEG (<see cref="IsMjpeg"/> == true): a complete JPEG-encoded frame. Decode with your JPEG decoder.</item>
        /// </list>
        /// The driver does not pre-convert pixel formats — application controls conversion timing/threading.
        /// </remarks>
        public int GetFrame(byte[] buffer) {
            this.CheckObjectState();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (!this.streaming) throw new InvalidOperationException("Not streaming.");

            return this.NativeGetFrame(buffer);
        }

        /// <summary>Converts a YUY2 (YUYV-packed) buffer to RGB565 in place into <paramref name="rgb565"/>.</summary>
        /// <param name="yuy2">Source buffer with YUY2-packed pixels (Y1 U Y2 V tuples).</param>
        /// <param name="rgb565">Destination buffer for RGB565 pixels (2 bytes per pixel).</param>
        /// <remarks>
        /// Output byte count equals input byte count (4 source bytes -> 2 RGB565 pixels = 4 dest bytes).
        /// Internally the byte count is rounded down to a multiple of 4. Uses BT.601 conversion math
        /// performed in native code.
        /// </remarks>
        public static void ConvertYuy2ToRgb565(byte[] yuy2, byte[] rgb565) {
            if (yuy2 == null) throw new ArgumentNullException(nameof(yuy2));
            if (rgb565 == null) throw new ArgumentNullException(nameof(rgb565));
            if (rgb565.Length < yuy2.Length) throw new ArgumentException("rgb565 buffer must be at least yuy2.Length bytes.", nameof(rgb565));

            NativeConvertYuy2ToRgb565(yuy2, rgb565, yuy2.Length);
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
        /// <remarks>
        /// The handler is invoked synchronously here, so a slow event handler (e.g. JPEG decode
        /// taking longer than the timer's polling interval) would otherwise let timer callbacks
        /// queue up — every cycle adds (handler_time − interval) of backlog, producing a
        /// multi-second offset between the camera's view and what's rendered. The
        /// <c>processingFrame</c> guard makes CheckEvents return immediately while a previous
        /// invocation is still running. The native double-buffer keeps only the most recent
        /// frame anyway, so dropping intermediates preserves real-time playback at the cost
        /// of an occasionally-skipped frame on slow handlers.
        /// </remarks>
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

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeConstructor(uint id, byte interfaceIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeFinalize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeGetFormatsCount(out int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeGetFormat(int index, out int width, out int height,
                                            out byte formatType, out byte bFormatIndex, out byte bFrameIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeStartStreaming(byte formatType, byte bFormatIndex, byte bFrameIndex,
                                                 int width, int height, int fps);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeStopStreaming();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeResync(bool soft);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private bool NativeIsNewFrameAvailable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeGetFrameSize(out int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private int NativeGetFrame(byte[] buffer);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private static void NativeConvertYuy2ToRgb565(byte[] yuy2, byte[] rgb565, int length);

        /// <summary>The on-the-wire pixel encoding for a UVC stream.</summary>
        public enum FormatKind : byte {
            /// <summary>Uncompressed 4:2:2 packed (YUYV). Streamable; converted to RGB565 on the device.</summary>
            Yuy2 = 0,
            /// <summary>Motion-JPEG. Currently enumerated only; streaming returns NotSupported until a JPEG decoder is wired in.</summary>
            Mjpeg = 1,
        }

        /// <summary>A camera-supported video format / resolution combination.</summary>
        /// <remarks>
        /// Obtain instances via <see cref="Webcam.SupportedFormats"/>; pass to <see cref="Webcam.StartStreaming"/>.
        /// Only <see cref="FormatKind.Yuy2"/> formats can currently be streamed.
        /// </remarks>
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
