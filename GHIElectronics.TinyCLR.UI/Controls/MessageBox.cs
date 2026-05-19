using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// WinForms-style modal message box.
    ///
    /// Usage:
    /// <code>
    /// MessageBox.DefaultFont = myFont;             // once at app start
    /// var r = MessageBox.Show("Erase all data?", "Confirm", MessageBoxButtons.YesNo);
    /// if (r == DialogResult.Yes) { ... }
    /// </code>
    ///
    /// Show() is synchronous: it nests a dispatcher frame so the UI keeps painting
    /// and dispatching input while the box is up, and returns when the user picks
    /// a button (or Esc cancels). Safe to call from any UI-thread event handler.
    /// </summary>
    public static class MessageBox {
        public enum MessageBoxButtons {
            OK = 0,
            Cancel = 1,
            OKCancel = 2,
            YesNo = 3,
        }

        public enum DialogResult {
            OK = 0,
            Cancel = 1,
            Yes = 2,
            No = 3,
        }

        /// <summary>
        /// Optional default font. Set this once at startup so callers don't have
        /// to pass a Font on every call. Each Show overload that omits a Font uses
        /// this value.
        /// </summary>
        public static Font DefaultFont { get; set; }

        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons)
            => Show(null, message, caption, buttons, DefaultFont);

        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons, Font font)
            => Show(null, message, caption, buttons, font);

        public static DialogResult Show(UIElement owner, string message, string caption, MessageBoxButtons buttons, Font font) {
            if (font == null) throw new ArgumentNullException(nameof(font), "Set MessageBox.DefaultFont or pass a Font.");

            // The dialog mounts under the owner if supplied and live, otherwise
            // under MainWindow.Child. Without either we have nowhere to draw.
            var host = ResolveHost(owner);
            if (host == null) throw new InvalidOperationException("No host: pass an owner or set Application.Current.MainWindow.");

            var dialog = new MessageBoxDialog(font, message ?? string.Empty, caption ?? string.Empty, buttons);
            return dialog.ShowModal(host);
        }

        private static UIElement ResolveHost(UIElement owner) {
            if (owner != null) return owner;

            var app = Application.Current;
            if (app == null) return null;

            var mw = app.MainWindow;
            if (mw == null) return null;

            return mw.Child ?? mw;
        }
    }

    /// <summary>
    /// The actual modal Canvas: fills the host, paints a scrim, hosts the dialog
    /// rectangle + buttons, and pumps a nested dispatcher frame until dismissed.
    /// Internal — public API is <see cref="MessageBox"/>.
    /// </summary>
    internal sealed class MessageBoxDialog : Canvas {
        // Layout constants (relative to font height so big-font configs scale).
        private const int OuterMarginRatioNum = 1;   // dialog inset from host edge: 1/8 of min(hostW,hostH)
        private const int OuterMarginRatioDen = 8;
        private const int InnerPad = 12;             // px padding inside dialog edges
        private const int LineSpacingNum = 5;        // line height multiplier: font.Height * 5/4
        private const int LineSpacingDen = 4;
        private const int ButtonGap = 12;            // px between two buttons
        private const ushort ScrimAlpha = 0x80;      // ~50% black scrim
        private const int MinDialogWidth = 160;

        private readonly Font _font;
        private readonly string _caption;
        private readonly MessageBox.MessageBoxButtons _kind;
        private readonly string[] _rawLines;
        private readonly DispatcherFrame _frame = new DispatcherFrame();

        // Static brush/pen cache, rebuilt only when Theme colors change.
        // Previously each dialog instance allocated its own set on every Show()
        // — needless garbage given that the visuals are theme-driven and shared.
        private static SolidColorBrush s_scrimBrush;
        private static SolidColorBrush s_captionBrush;
        private static SolidColorBrush s_bodyBrush;
        private static Media.Pen s_borderPen;
        private static Media.Color s_cachedCaptionColor;
        private static Media.Color s_cachedBodyColor;
        private static Media.Color s_cachedBorderColor;

        private static SolidColorBrush ScrimBrush() {
            if (s_scrimBrush == null) {
                s_scrimBrush = new SolidColorBrush(Colors.Black) { Opacity = ScrimAlpha };
            }
            return s_scrimBrush;
        }

        private static SolidColorBrush CaptionBrush() {
            var c = Theme.ControlSurface;
            if (s_captionBrush == null || !ColorEquals(s_cachedCaptionColor, c)) {
                s_captionBrush = new SolidColorBrush(c);
                s_cachedCaptionColor = c;
            }
            return s_captionBrush;
        }

        private static SolidColorBrush BodyBrush() {
            var c = Theme.TextBoxFill;
            if (s_bodyBrush == null || !ColorEquals(s_cachedBodyColor, c)) {
                s_bodyBrush = new SolidColorBrush(c);
                s_cachedBodyColor = c;
            }
            return s_bodyBrush;
        }

        private static Media.Pen BorderPen() {
            var c = Theme.Border;
            if (s_borderPen == null || !ColorEquals(s_cachedBorderColor, c)) {
                s_borderPen = new Media.Pen(c, 1);
                s_cachedBorderColor = c;
            }
            return s_borderPen;
        }

        private static bool ColorEquals(Media.Color a, Media.Color b) =>
            a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;

        private MessageBox.DialogResult _result = MessageBox.DialogResult.Cancel;

        private Button _primary;     // OK / Yes / center-only
        private Button _secondary;   // Cancel / No (only for OKCancel and YesNo)
        private MessageBox.DialogResult _primaryResult;
        private MessageBox.DialogResult _secondaryResult;

        // Geometry computed in BuildLayout once we know the host size.
        private string[] _wrappedLines;
        private int _dialogX, _dialogY, _dialogW, _dialogH;
        private int _captionBarHeight;
        private int _bodyTop, _bodyHeight;
        private int _lineSpacing;

        private UIElement _host;
        private UIElement _previousFocus;

        internal MessageBoxDialog(Font font, string message, string caption, MessageBox.MessageBoxButtons kind) {
            this._font = font;
            this._caption = caption;
            this._kind = kind;
            this._rawLines = SplitLines(message);
        }

        internal MessageBox.DialogResult ShowModal(UIElement host) {
            this._host = host;
            this._previousFocus = Buttons.FocusedElement;

            var hostW = host.ActualWidth > 0 ? host.ActualWidth : Application.Current.MainWindow.Width;
            var hostH = host.ActualHeight > 0 ? host.ActualHeight : Application.Current.MainWindow.Height;

            this.Width = hostW;
            this.Height = hostH;

            this.BuildLayout(hostW, hostH);
            this.BuildButtons();

            // Mount under the host. Logical-children list is the canvas's child
            // collection; appending puts us on top of everything else.
            host.LogicalChildren.Add(this);
            host.Invalidate();

            // Focus the primary button so Enter/Select activates it immediately.
            if (this._primary != null) {
                Buttons.Focus(this._primary);
            }

            try {
                // Nested message loop. Returns when a button click sets Continue=false.
                Dispatcher.PushFrame(this._frame);
            }
            finally {
                this.Detach();
            }

            return this._result;
        }

        private void Detach() {
            if (this._host != null) {
                if (this._host.LogicalChildren.Contains(this)) {
                    this._host.LogicalChildren.Remove(this);
                }
                this._host.Invalidate();
                this._host = null;
            }

            // Restore focus to whatever owned it before we opened.
            if (this._previousFocus != null) {
                Buttons.Focus(this._previousFocus);
                this._previousFocus = null;
            }
        }

        // --- layout -----------------------------------------------------------

        private void BuildLayout(int hostW, int hostH) {
            var inset = (hostW < hostH ? hostW : hostH) * OuterMarginRatioNum / OuterMarginRatioDen;
            var maxW = hostW - inset * 2;
            var maxH = hostH - inset * 2;
            if (maxW < MinDialogWidth) maxW = MinDialogWidth;

            this._lineSpacing = this._font.Height * LineSpacingNum / LineSpacingDen;
            this._captionBarHeight = (this._font.Height * 3 + 1) / 2;

            var bodyMaxW = maxW - InnerPad * 2;
            if (bodyMaxW < 1) bodyMaxW = 1;

            // Word-wrap each line to bodyMaxW. Empty lines stay empty (blank row).
            this._wrappedLines = WrapAllLines(this._rawLines, bodyMaxW);

            // Compute the desired body width: longest wrapped line, capped at bodyMaxW.
            // Also factor in caption width so the bar isn't clipped.
            var longest = 0;
            for (var i = 0; i < this._wrappedLines.Length; i++) {
                this._font.ComputeExtent(this._wrappedLines[i], out var w, out _);
                if (w > longest) longest = w;
            }
            if (this._caption.Length > 0) {
                this._font.ComputeExtent(this._caption, out var cw, out _);
                if (cw > longest) longest = cw;
            }

            var contentW = longest + InnerPad * 2;
            if (contentW > maxW) contentW = maxW;
            if (contentW < MinDialogWidth) contentW = MinDialogWidth;

            // Vertical layout: caption + body + button row, each with InnerPad above/below.
            var buttonRowH = this._font.Height * 2 + InnerPad;
            var bodyH = this._wrappedLines.Length * this._lineSpacing + InnerPad;
            var totalH = this._captionBarHeight + bodyH + buttonRowH + InnerPad;

            if (totalH > maxH) {
                // Trim body height; the renderer clips trailing lines.
                bodyH = maxH - this._captionBarHeight - buttonRowH - InnerPad;
                if (bodyH < this._lineSpacing) bodyH = this._lineSpacing;
                totalH = this._captionBarHeight + bodyH + buttonRowH + InnerPad;
            }

            this._dialogW = contentW;
            this._dialogH = totalH;
            this._dialogX = (hostW - contentW) / 2;
            this._dialogY = (hostH - totalH) / 2;
            this._bodyTop = this._dialogY + this._captionBarHeight + InnerPad / 2;
            this._bodyHeight = bodyH;
        }

        private string[] WrapAllLines(string[] lines, int maxWidth) {
            var bag = new ArrayList();
            for (var i = 0; i < lines.Length; i++) {
                this.WrapOne(lines[i], maxWidth, bag);
            }
            var arr = new string[bag.Count];
            for (var i = 0; i < arr.Length; i++) arr[i] = (string)bag[i];
            return arr;
        }

        private void WrapOne(string line, int maxWidth, ArrayList output) {
            if (line.Length == 0) {
                output.Add(string.Empty);
                return;
            }

            var start = 0;
            while (start < line.Length) {
                // Fast path: does the rest fit?
                var rest = line.Substring(start);
                this._font.ComputeExtent(rest, out var fullW, out _);
                if (fullW <= maxWidth) {
                    output.Add(rest);
                    return;
                }

                // Walk forward until we exceed maxWidth, remember the last
                // boundary that still fit.
                var fitChars = 1;
                for (var i = start + 1; i <= line.Length; i++) {
                    this._font.ComputeExtent(line.Substring(start, i - start), out var w, out _);
                    if (w > maxWidth) break;
                    fitChars = i - start;
                }

                // Prefer to break at the last whitespace inside the fit window.
                var breakLen = fitChars;
                for (var i = fitChars; i > 0; i--) {
                    var c = line[start + i - 1];
                    if (c == ' ' || c == '\t') {
                        breakLen = i - 1;          // drop the space itself
                        break;
                    }
                }
                if (breakLen == 0) breakLen = fitChars; // no whitespace → hard break

                output.Add(line.Substring(start, breakLen));
                start += breakLen;
                // Skip leading whitespace on the next line.
                while (start < line.Length && (line[start] == ' ' || line[start] == '\t')) start++;
            }
        }

        // --- buttons ----------------------------------------------------------

        private void BuildButtons() {
            string primaryLabel, secondaryLabel;
            switch (this._kind) {
                case MessageBox.MessageBoxButtons.OK:
                    primaryLabel = "OK"; secondaryLabel = null;
                    this._primaryResult = MessageBox.DialogResult.OK;
                    break;
                case MessageBox.MessageBoxButtons.Cancel:
                    primaryLabel = "Cancel"; secondaryLabel = null;
                    this._primaryResult = MessageBox.DialogResult.Cancel;
                    break;
                case MessageBox.MessageBoxButtons.OKCancel:
                    primaryLabel = "OK"; secondaryLabel = "Cancel";
                    this._primaryResult = MessageBox.DialogResult.OK;
                    this._secondaryResult = MessageBox.DialogResult.Cancel;
                    break;
                case MessageBox.MessageBoxButtons.YesNo:
                    primaryLabel = "Yes"; secondaryLabel = "No";
                    this._primaryResult = MessageBox.DialogResult.Yes;
                    this._secondaryResult = MessageBox.DialogResult.No;
                    break;
                default:
                    primaryLabel = "OK"; secondaryLabel = null;
                    this._primaryResult = MessageBox.DialogResult.OK;
                    break;
            }

            // Size each button to fit its label with horizontal padding equal
            // to one font height on each side (works for any string length and
            // any proportional font).
            this._primary = MakeButton(primaryLabel);
            this._primary.Click += this.OnPrimaryClick;

            if (secondaryLabel != null) {
                this._secondary = MakeButton(secondaryLabel);
                this._secondary.Click += this.OnSecondaryClick;
            }

            this.LayoutButtons();

            this.LogicalChildren.Add(this._primary);
            if (this._secondary != null) this.LogicalChildren.Add(this._secondary);
        }

        private Button MakeButton(string label) {
            var t = new Text(this._font, label) {
                ForeColor = Theme.TextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            this._font.ComputeExtent(label, out var w, out var h);
            return new Button {
                Child = t,
                Width = w + this._font.Height * 2,
                Height = h + this._font.Height,
            };
        }

        private void LayoutButtons() {
            var rowY = this._dialogY + this._dialogH - this._primary.Height - InnerPad / 2;

            if (this._secondary == null) {
                // Single button centered.
                var x = this._dialogX + (this._dialogW - this._primary.Width) / 2;
                Canvas.SetLeft(this._primary, x);
                Canvas.SetTop(this._primary, rowY);
                return;
            }

            // Two buttons: equal widths (use the larger of the two), centered as a pair.
            var btnW = this._primary.Width > this._secondary.Width ? this._primary.Width : this._secondary.Width;
            this._primary.Width = btnW;
            this._secondary.Width = btnW;

            var totalW = btnW * 2 + ButtonGap;
            var startX = this._dialogX + (this._dialogW - totalW) / 2;

            Canvas.SetLeft(this._primary, startX);
            Canvas.SetTop(this._primary, rowY);
            Canvas.SetLeft(this._secondary, startX + btnW + ButtonGap);
            Canvas.SetTop(this._secondary, rowY);
        }

        private void OnPrimaryClick(object sender, RoutedEventArgs e) => this.Dismiss(this._primaryResult);
        private void OnSecondaryClick(object sender, RoutedEventArgs e) => this.Dismiss(this._secondaryResult);

        private void Dismiss(MessageBox.DialogResult result) {
            this._result = result;
            this._frame.Continue = false;
        }

        // --- input ------------------------------------------------------------

        // Swallow touches that fall on the scrim (anywhere outside the dialog),
        // so the underlying UI doesn't react while we're modal.
        protected override void OnTouchDown(TouchEventArgs e) {
            e.Handled = true;
            base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            e.Handled = true;
            base.OnTouchUp(e);
        }

        protected override void OnButtonDown(ButtonEventArgs e) {
            // Esc / Back dismisses with the secondary result if there is one,
            // otherwise with Cancel. Matches WinForms semantics.
            if (e.Button == HardwareButton.Back) {
                this.Dismiss(this._secondary != null ? this._secondaryResult : MessageBox.DialogResult.Cancel);
                e.Handled = true;
                return;
            }

            // Tab moves focus between primary/secondary.
            if (e.Button == HardwareButton.Right || e.Button == HardwareButton.Down) {
                if (this._secondary != null) {
                    Buttons.Focus(this._secondary);
                    e.Handled = true;
                }
                return;
            }
            if (e.Button == HardwareButton.Left || e.Button == HardwareButton.Up) {
                if (this._primary != null) {
                    Buttons.Focus(this._primary);
                    e.Handled = true;
                }
                return;
            }

            base.OnButtonDown(e);
        }

        // --- paint ------------------------------------------------------------

        public override void OnRender(DrawingContext dc) {
            // Scrim over the whole host area first (we sized this.Width/Height
            // to host dimensions in ShowModal).
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            dc.DrawRectangle(ScrimBrush(), null, 0, 0, w, h);

            // Caption bar.
            dc.DrawRectangle(CaptionBrush(), BorderPen(),
                this._dialogX, this._dialogY, this._dialogW, this._captionBarHeight);

            if (this._caption.Length > 0) {
                var captionY = this._dialogY + (this._captionBarHeight - this._font.Height) / 2;
                var caption = this._caption;
                dc.DrawText(ref caption, this._font, Theme.TextPrimary,
                    this._dialogX + InnerPad, captionY,
                    this._dialogW - InnerPad * 2, this._font.Height,
                    TextAlignment.Left, TextTrimming.WordEllipsis);
            }

            // Body background — sits between caption bar and button row.
            var bodyVisibleH = this._dialogH - this._captionBarHeight - (this._primary != null ? this._primary.Height + InnerPad : 0);
            if (bodyVisibleH < 1) bodyVisibleH = 1;
            dc.DrawRectangle(BodyBrush(), BorderPen(),
                this._dialogX, this._dialogY + this._captionBarHeight, this._dialogW, bodyVisibleH);

            // Body text. Clip lines that overflow the visible body region.
            var maxBodyBottom = this._dialogY + this._captionBarHeight + bodyVisibleH - InnerPad / 2;
            var y = this._bodyTop;
            for (var i = 0; i < this._wrappedLines.Length; i++) {
                if (y + this._font.Height > maxBodyBottom) break;
                var line = this._wrappedLines[i];
                dc.DrawText(ref line, this._font, Theme.TextPrimary,
                    this._dialogX + InnerPad, y,
                    this._dialogW - InnerPad * 2, this._font.Height,
                    TextAlignment.Left, TextTrimming.WordEllipsis);
                y += this._lineSpacing;
            }
        }

        // --- helpers ----------------------------------------------------------

        // Splits on \n, dropping a trailing \r from each segment so Windows-style
        // line endings produce one row per line, not blank rows in between.
        private static string[] SplitLines(string message) {
            if (message.Length == 0) return new string[] { string.Empty };

            var raw = message.Split('\n');
            for (var i = 0; i < raw.Length; i++) {
                var s = raw[i];
                if (s.Length > 0 && s[s.Length - 1] == '\r') {
                    raw[i] = s.Substring(0, s.Length - 1);
                }
            }
            return raw;
        }
    }
}
