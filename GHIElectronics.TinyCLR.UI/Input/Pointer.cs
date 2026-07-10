using System;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    /// An on-screen mouse cursor for pointer-driven UIs (e.g. an HDMI monitor with a USB mouse, where there is no
    /// touch panel). It draws an arrow on top of the whole UI and turns pointer motion + the left button into the
    /// framework's normal touch events, so every existing control works unchanged.
    /// </summary>
    /// <remarks>
    /// The device-specific part (reading a USB-host mouse, a trackball, etc.) stays in the application; it just
    /// feeds this class:
    /// <code>
    ///   var pointer = new Pointer(Application.Current);
    ///   // when the mouse reports movement / a button:
    ///   pointer.MoveTo(x, y);            // or pointer.MoveBy(dx, dy)
    ///   pointer.SetLeftButton(pressed);  // press = touch-down, release = touch-up, move-while-pressed = drag
    ///   pointer.Visible = false;         // e.g. when the mouse is unplugged
    /// </code>
    /// The cursor is drawn via <see cref="WindowManager.PostRender"/> and only invalidates its own small rectangle
    /// as it moves, so it costs almost nothing to render and never triggers a full-screen repaint. All members are
    /// safe to call from any thread (the mouse driver's worker thread included); they marshal onto the UI dispatcher
    /// internally.
    /// </remarks>
    public sealed class Pointer {
        // Arrow, tip at (0,0), classic up-left pointer. Flat [x0,y0,x1,y1,...] relative to the hotspot.
        private static readonly int[] ArrowRelative = { 0, 0, 0, 16, 4, 12, 7, 18, 9, 17, 6, 11, 11, 11 };
        private const int CursorWidth = 14;   // invalidate box (arrow + outline slack)
        private const int CursorHeight = 21;

        private readonly Application application;
        private readonly DispatcherOperationCallback applyMove;
        private readonly PostRenderEventHandler postRender;
        private readonly Brush fill;
        private readonly Pen outline;
        private readonly int[] absolutePoints = new int[ArrowRelative.Length];

        private int screenWidth;
        private int screenHeight;
        private int x;
        private int y;
        private int pendingX;
        private int pendingY;
        private bool movePending;
        private bool hooked;
        private bool visible = true;
        private bool leftDown;

        /// <summary>Creates a cursor for the given application and shows it centered once the UI is running.</summary>
        public Pointer(Application application) {
            this.application = application ?? throw new ArgumentNullException();
            this.applyMove = new DispatcherOperationCallback(this.ApplyMove);
            this.postRender = new PostRenderEventHandler(this.OnPostRender);
            this.fill = new SolidColorBrush(Colors.White);
            this.outline = new Pen(Colors.Black);

            // Hook PostRender + center once the dispatcher is pumping (WindowManager.Instance exists by then).
            this.application.Dispatcher.BeginInvoke(new DispatcherOperationCallback(this.EnsureAttached), null);
        }

        /// <summary>The current cursor x position, in screen pixels.</summary>
        public int X => this.x;

        /// <summary>The current cursor y position, in screen pixels.</summary>
        public int Y => this.y;

        /// <summary>Whether the cursor is drawn. Set to <c>false</c> to hide it (e.g. when the mouse is unplugged).</summary>
        public bool Visible {
            get => this.visible;
            set {
                if (this.visible == value) {
                    return;
                }

                this.visible = value;
                this.application.Dispatcher.BeginInvoke(new DispatcherOperationCallback(_ => { this.Invalidate(this.x, this.y); return null; }), null);
            }
        }

        /// <summary>Moves the cursor to an absolute screen position (clamped to the display).</summary>
        public void MoveTo(int x, int y) {
            this.pendingX = x;
            this.pendingY = y;

            if (this.movePending) {
                return;   // coalesce - at most one queued update in flight
            }

            this.movePending = true;
            this.application.Dispatcher.BeginInvoke(this.applyMove, null);
        }

        /// <summary>Moves the cursor by a relative delta.</summary>
        public void MoveBy(int deltaX, int deltaY) => this.MoveTo(this.x + deltaX, this.y + deltaY);

        /// <summary>Reports the left button state: <c>true</c> = press (touch-down), <c>false</c> = release
        /// (touch-up). While pressed, subsequent moves are reported as drags.</summary>
        public void SetLeftButton(bool pressed) {
            this.application.Dispatcher.BeginInvoke(new DispatcherOperationCallback(_ => {
                this.leftDown = pressed;
                this.RaiseTouch(pressed ? TouchMessages.Down : TouchMessages.Up);
                return null;
            }), null);
        }

        private object EnsureAttached(object unused) {
            if (this.hooked) {
                return null;
            }

            var wm = WindowManager.Instance;
            if (wm == null) {
                return null;   // not running yet; retried on the first MoveTo
            }

            this.screenWidth = (int)wm.DisplayController.ActiveConfiguration.Width;
            this.screenHeight = (int)wm.DisplayController.ActiveConfiguration.Height;

            if (this.x == 0 && this.y == 0) {
                this.x = this.pendingX = this.screenWidth / 2;
                this.y = this.pendingY = this.screenHeight / 2;
            }

            wm.PostRender += this.postRender;
            this.hooked = true;
            this.Invalidate(this.x, this.y);
            return null;
        }

        private object ApplyMove(object unused) {
            this.movePending = false;
            this.EnsureAttached(null);

            var nx = this.pendingX;
            var ny = this.pendingY;
            if (this.screenWidth > 0) {
                nx = nx < 0 ? 0 : (nx >= this.screenWidth ? this.screenWidth - 1 : nx);
            }

            if (this.screenHeight > 0) {
                ny = ny < 0 ? 0 : (ny >= this.screenHeight ? this.screenHeight - 1 : ny);
            }

            this.Invalidate(this.x, this.y);   // erase old
            this.x = nx;
            this.y = ny;
            this.Invalidate(this.x, this.y);   // draw new

            if (this.leftDown) {
                this.RaiseTouch(TouchMessages.Move);   // dragging
            }

            return null;
        }

        private void RaiseTouch(TouchMessages which) =>
            this.application.InputProvider.RaiseTouch(this.x, this.y, which, DateTime.Now);

        private void Invalidate(int px, int py) {
            var wm = WindowManager.Instance;
            if (wm != null) {
                wm.InvalidateRect(px, py, CursorWidth, CursorHeight);
            }
        }

        // Drawn after the whole window tree renders (clipped to the current dirty rect), so the arrow lands on top.
        private void OnPostRender(DrawingContext dc) {
            if (!this.visible) {
                return;
            }

            for (var i = 0; i < ArrowRelative.Length; i += 2) {
                this.absolutePoints[i] = ArrowRelative[i] + this.x;
                this.absolutePoints[i + 1] = ArrowRelative[i + 1] + this.y;
            }

            dc.DrawPolygon(this.fill, this.outline, this.absolutePoints);
        }
    }
}
