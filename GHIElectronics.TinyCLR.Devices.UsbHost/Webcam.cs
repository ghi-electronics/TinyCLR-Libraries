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

        /// <summary>Delegate fired when a new frame is ready.</summary>
        public delegate void FrameAvailableEventHandler(Webcam sender, EventArgs e);

        /// <summary>Raised on the BaseDevice worker thread when a new frame is available.</summary>
        /// <remarks>The event handler should call <see cref="GetFrame"/> to retrieve the frame.</remarks>
        public event FrameAvailableEventHandler FrameAvailable;

        /// <summary>True while the camera is streaming.</summary>
        public bool IsStreaming => this.streaming;

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
        public void StartStreaming(Format format) {
            this.CheckObjectState();

            if (format == null) throw new ArgumentNullException(nameof(format));
            if (this.streaming) throw new InvalidOperationException("Already streaming.");

            this.NativeStartStreaming(format.FormatType, format.BFormatIndex, format.BFrameIndex,
                                       format.Width, format.Height);

            this.activeFormat = format;
            this.streaming = true;
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

        /// <summary>Copies the most recently completed frame into <paramref name="buffer"/> as RGB565 bytes.</summary>
        /// <param name="buffer">Destination buffer. Must be at least <see cref="FrameSize"/> bytes.</param>
        public void GetFrame(byte[] buffer) {
            this.CheckObjectState();

            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (!this.streaming) throw new InvalidOperationException("Not streaming.");

            this.NativeGetFrame(buffer);
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

            bool ready;
            try { ready = this.NativeIsNewFrameAvailable(); }
            catch { return; }

            if (ready)
                this.FrameAvailable?.Invoke(this, EventArgs.Empty);
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
                                                 int width, int height);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeStopStreaming();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private bool NativeIsNewFrameAvailable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeGetFrameSize(out int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeGetFrame(byte[] buffer);

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
