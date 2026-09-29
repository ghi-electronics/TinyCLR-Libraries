using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>An on-screen software keyboard window used to enter text into a text box.</summary>
    public class OnScreenKeyboard : Window {
        private Hashtable views;
        private TextBox source;
        private TextBox input;
        private HighlightImage image;
        private KeyboardView active;
        private double scaleX;
        private double scaleY;
        private int offsetX;
        private int offsetY;

        /// <summary>Gets or sets the font used for the keyboard's text input field.</summary>
        public static new Font Font { get; set; }

        /// <summary>Whether the keyboard darkens a key while it is being pressed to give
        /// visual feedback. Enabled by default. Set to false to fall back to the flat
        /// touch-up-only behavior that shipped prior to this option being added.</summary>
        public static bool ShowPressFeedback { get; set; } = true;

        /// <summary>Whether holding the Backspace key auto-repeats the delete (like a
        /// physical keyboard). After an initial ~500 ms delay characters are removed
        /// every ~100 ms until the finger is released. Enabled by default. Set to
        /// false to require a fresh tap per delete.</summary>
        public static bool RepeatBackspaceOnHold { get; set; } = true;

        // Timing for RepeatBackspaceOnHold. Kept internal — tuning these on the
        // fly isn't a user story we're supporting yet.
        private const int InitialHoldDelayMs = 500;
        private const int HoldRepeatIntervalMs = 100;

        private DispatcherTimer holdTimer;
        // holdActive: touch is still down on backspace. Cleared on TouchUp so a
        // Tick that was already queued to the dispatcher can no-op instead of
        // firing after release.
        private bool holdActive;
        // holdRepeatFired: at least one repeat tick has fired for the current hold.
        // Used to suppress the normal single-Backspace action in OnTouchUp — we
        // already deleted via the timer, don't delete one more on release.
        private bool holdRepeatFired;

        internal OnScreenKeyboard() {
            this.views = new Hashtable();

            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;
            this.Background = new SolidColorBrush(Colors.Red);

            var holder = new StackPanel();

            this.input = new TextBox {
                ForOnScreenKeyboard = true,
                Font = OnScreenKeyboard.Font,
                Height = 2 * Font.Height
            };

            holder.Children.Add(this.input);

            this.image = new HighlightImage {
                Source = null,
                Width = WindowManager.Instance.ActualWidth,
                Height = WindowManager.Instance.ActualHeight - this.input.Height,
                Stretch = Stretch.Fill
            };

            this.image.TouchDown += this.OnTouchDown;
            this.image.TouchUp += this.OnTouchUp;

            // Single reusable timer; interval is reprogrammed each hold.
            this.holdTimer = new DispatcherTimer();
            this.holdTimer.Tick += this.OnHoldTimerTick;

            holder.Children.Add(this.image);

            this.Child = holder;
        }

        internal void ShowFor(TextBox textBox) {
            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;

            this.source = textBox;
            this.input.Text = this.source.Text;

            // Reset any leftover highlight from a previous session — the keyboard
            // instance is cached and reused across Show/Hide by Application.
            this.image.PressActive = false;

            // Same reason: cancel any auto-repeat that was somehow still armed
            // from the previous session before we present the keyboard again.
            this.holdActive = false;
            this.holdRepeatFired = false;
            this.holdTimer.Stop();

            this.ShowView(KeyboardViewId.Lowercase);
        }

        // Backspace is always the rightmost cell of row 2 in every view
        // (Lowercase/Uppercase/Numbers/Symbols). This assumption is what
        // CreateView produces and is the only key we auto-repeat.
        private bool IsBackspace(int row, int col) {
            if (row != 2 || this.active == null) return false;
            var specialRow = this.active.SpecialKeys?[row];
            return specialRow != null && col == specialRow.Length - 1 && specialRow[col] != null;
        }

        private void OnHoldTimerTick(object sender, EventArgs e) {
            // The tick may have been queued to the dispatcher before OnTouchUp ran;
            // guard so a released hold doesn't produce one final delete.
            if (!this.holdActive) return;

            this.Backspace();
            this.holdRepeatFired = true;

            // After the initial delay, switch to the faster repeat rate.
            if (this.holdTimer.Interval.TotalMilliseconds > HoldRepeatIntervalMs) {
                this.holdTimer.Interval = TimeSpan.FromMilliseconds(HoldRepeatIntervalMs);
            }
        }

        // Locate the key that a touch point lands on, using the same math the
        // original OnTouchUp used so the semantics (including boundary behavior)
        // match byte-for-byte — press feedback should never dim a different key
        // than the one that will fire on release. Returns false if the point is
        // outside any key.
        private bool HitTestKey(TouchEventArgs e, out int row, out int column, out int srcX, out int srcY, out int srcW, out int srcH) {
            row = column = srcX = srcY = srcW = srcH = 0;

            if (this.active == null) return false;

            var x = (int)((e.Touches[0].X - this.offsetX) * this.scaleX);
            var y = (int)((e.Touches[0].Y - this.offsetY) * this.scaleY);

            var r = y / this.active.RowHeight;
            if (r < 0 || r >= this.active.ColumnWidth.Length) return false;

            var cw = this.active.ColumnWidth[r];
            var total = this.active.RowColumnOffset[r];
            var col = 0;

            while (col < cw.Length && total < x)
                total += cw[col++];

            if (--col < 0 || total < x)
                return false;

            row = r;
            column = col;
            srcX = total - cw[col];
            srcY = r * this.active.RowHeight;
            srcW = cw[col];
            srcH = this.active.RowHeight;
            return true;
        }

        private void OnTouchDown(object sender, TouchEventArgs e) {
            if (!HitTestKey(e, out var row, out var column, out var sx, out var sy, out var sw, out var sh))
                return;

            if (ShowPressFeedback) {
                this.image.PressSrcX = sx;
                this.image.PressSrcY = sy;
                this.image.PressSrcW = sw;
                this.image.PressSrcH = sh;
                this.image.PressActive = true;
                this.image.Invalidate();
            }

            // Arm the auto-repeat only when the initial press is on backspace.
            // Dragging onto backspace mid-touch will NOT start repeat — matches
            // Glide's behavior.
            if (RepeatBackspaceOnHold && this.IsBackspace(row, column)) {
                this.holdActive = true;
                this.holdRepeatFired = false;
                this.holdTimer.Interval = TimeSpan.FromMilliseconds(InitialHoldDelayMs);
                this.holdTimer.Start();
            }
        }

        private void OnTouchUp(object sender, TouchEventArgs e) {
            // Always clear the highlight on release, even if the release
            // landed outside a key (finger dragged off) — otherwise the
            // last-touched key would stay dimmed until the next press.
            if (this.image.PressActive) {
                this.image.PressActive = false;
                this.image.Invalidate();
            }

            // Stop the auto-repeat (safe even if never started). Capture the
            // "did it fire?" flag before clearing so we can decide below whether
            // to suppress the touch-up action.
            var wasRepeating = this.holdRepeatFired;
            this.holdActive = false;
            this.holdRepeatFired = false;
            this.holdTimer.Stop();

            if (!HitTestKey(e, out var row, out var column, out _, out _, out _, out _))
                return;

            // If auto-repeat already deleted characters AND the finger came up
            // on backspace, suppress the extra single delete that OnTouchUp
            // would normally fire. If the finger came up on a different key
            // (user dragged off backspace), fire that key's action normally.
            if (wasRepeating && this.IsBackspace(row, column))
                return;

            if (this.active.SpecialKeys?[row]?[column] is Action a) {
                a();
            }
            else {
                this.Append(this.active.Keys[row][column]);
            }
        }

        private void Backspace() { if (this.input.Text.Length > 0) this.input.Text = this.input.Text.Substring(0, this.input.Text.Length - 1); }
        private void Append(char c) => this.input.Text += c;

        private new void Close() {
            this.source.Text = this.input.Text;

            Application.Current.CloseOnScreenKeyboard();
        }
        private void Cancel()
        {
            Application.Current.CloseOnScreenKeyboard();
        }

        private void ShowView(KeyboardViewId id) {
            if (!this.views.Contains(id))
                this.CreateView(id);

            this.active = (KeyboardView)this.views[id];

            this.image.Source = this.active.Image;

            this.image.InvalidateMeasure();

            this.scaleX = this.active.Image.Width / (double)this.image.Width;
            this.scaleY = this.active.Image.Height / (double)this.image.Height;
            this.offsetX = 0;
            this.offsetY = this.input.Height;
        }

        private void CreateView(KeyboardViewId id) {
            var hf = 40;
            var sz = 80;
            var szh = 120;
            var full = new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz, sz };
            var image = default(System.Drawing.Bitmap);
            var view = new KeyboardView { RowHeight = sz };

            switch (id) {
                case KeyboardViewId.Lowercase:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Lowercase);
                    view.RowColumnOffset = new[] { 0, hf, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz },
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 4, sz, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' },
                        new[] { 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l' },
                        new[] { '\0', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0', '\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Uppercase), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, () => this.Cancel(), () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Uppercase:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Uppercase);
                    view.RowColumnOffset = new[] { 0, hf, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz },
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 4, sz, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P' },
                        new[] { 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L' },
                        new[] { '\0', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0','\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, () => this.Cancel(), () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Numbers:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Numbers);
                    view.RowColumnOffset = new[] { 0, 0, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        full,
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 4, sz, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' },
                        new[] { '@', '#', '$', '%', '&', '*', '-', '+', '(', ')' },
                        new[] { '\0', '!', '"', '\'', ':', ';', '/', '?', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0','\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Symbols), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, () => this.Cancel(), () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Symbols:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Symbols);
                    view.RowColumnOffset = new[] { 0, 0, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        full,
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 4, sz, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { '~', '`', '|', '•', '√', 'π', '÷', '×', '{', '}' },
                        new[] { '\t', '£', '¢', '€', 'º', '^', '_', '=', '[', ']' },
                        new[] { '\0', '™', '®', '©', '¶', '\\', '<', '>', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0','\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, () => this.Cancel(),() => this.Close() }
                    };

                    break;
            }

            view.Image = BitmapImage.FromGraphics(Graphics.FromImage(image));

            this.views.Add(id, view);
        }

        private enum KeyboardViewId {
            Lowercase,
            Uppercase,
            Numbers,
            Symbols
        }

        private class KeyboardView {
            public BitmapImage Image { get; set; }
            public int RowHeight { get; set; }
            public int[] RowColumnOffset { get; set; }
            public int[][] ColumnWidth { get; set; }
            public char[][] Keys { get; set; }
            public Action[][] SpecialKeys { get; set; }
        }

        // Image subclass that draws the base bitmap and, when PressActive is set,
        // overlays a translucent black rectangle over the pressed key so the user
        // gets visual confirmation their touch registered. The rectangle is stored
        // in bitmap-source coordinates and mapped to render coordinates locally,
        // so it stays aligned regardless of how the parent scales this control.
        private class HighlightImage : Controls.Image {
            public int PressSrcX;
            public int PressSrcY;
            public int PressSrcW;
            public int PressSrcH;
            public bool PressActive;

            public override void OnRender(DrawingContext dc) {
                base.OnRender(dc);

                if (!this.PressActive || this.Source == null) return;

                var sw = this.Source.Width;
                var sh = this.Source.Height;
                if (sw <= 0 || sh <= 0) return;

                this.GetRenderSize(out var renderW, out var renderH);

                var rx = this.PressSrcX * renderW / sw;
                var ry = this.PressSrcY * renderH / sh;
                var rw = this.PressSrcW * renderW / sw;
                var rh = this.PressSrcH * renderH / sh;

                var brush = new SolidColorBrush(Colors.Black) { Opacity = 128 };
                dc.DrawRectangle(brush, null, rx, ry, rw, rh);
            }
        }
    }
}
