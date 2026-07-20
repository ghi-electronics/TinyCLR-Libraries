using System;
using System.Reflection;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Represents the method that handles the text-changed event.</summary>
    public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs e);

    /// <summary>Provides data for the text-changed event.</summary>
    public class TextChangedEventArgs : RoutedEventArgs {
        /// <summary>Initializes a new instance of the <see cref="TextChangedEventArgs"/> class.</summary>
        public TextChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
    }

    /// <summary>An editable single-line text field that opens the on-screen keyboard when activated.</summary>
    public class TextBox : Control {
        // Cached once per AppDomain so every Text-change doesn't allocate a
        // fresh RoutedEvent object.
        private static readonly RoutedEvent TextChangedRoutedEvent =
            new RoutedEvent("TextChangedEvent", RoutingStrategy.Bubble, typeof(TextChangedEventHandler));

        private string text = string.Empty;
        private Color bordercolor = Colors.Black;
        private ushort borderthickness = 1, paddingx, paddingy;
        private int width, height;

        // Physical-keyboard editing: insertion point + blinking caret (shown only while focused).
        private int caretIndex;
        private bool caretVisible;
        private DispatcherTimer caretTimer;

        private object _bindSource;
        private string _bindPropertyName;
        private bool _bindTwoWay;
        private bool _suppressBindPush;

        /// <summary>Initializes a new instance of the <see cref="TextBox"/> class.</summary>
        public TextBox() {
            this.Background = Theme.TextBoxFillBrush;
            this.bordercolor = Theme.Border;
        }

        /// <summary>Raised when the text changes.</summary>
        public event TextChangedEventHandler TextChanged;

        /// <summary>The horizontal alignment of the displayed text.</summary>
        public TextAlignment TextAlign { get; set; } = TextAlignment.Left;

        /// <summary>When set, the character displayed in place of each typed character to mask input.</summary>
        public char PasswordChar { get; set; } = char.MinValue;

        /// <summary>The current text of the field.</summary>
        public string Text {
            get => this.text;
            set {
                this.text = value ?? string.Empty;

                if (this.caretIndex > this.text.Length) {
                    this.caretIndex = this.text.Length;
                }

                this.InvalidateMeasure();

                if (!this._suppressBindPush) {
                    this.PushTextToBinding();
                }

                var args = new TextChangedEventArgs(TextChangedRoutedEvent, this);

                this.TextChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Raised when a binding pull (source → TextBox) or push (TextBox → source)
        /// fails. Default behavior is silent (the framework can't sensibly recover);
        /// subscribe here to log or surface the error during development.
        /// </summary>
        public event BindingErrorEventHandler BindingError;

        /// <summary>
        /// One-way or two-way bind <see cref="Text"/> to a CLR property on <paramref name="source"/> using reflection.
        /// For change notifications implement <see cref="INotifyBindablePropertyChanged"/> on the source.
        /// </summary>
        public void SetTextBinding(object source, string propertyName, bool twoWay = true) {
            this.ClearTextBinding();
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            this._bindSource = source;
            this._bindPropertyName = propertyName;
            this._bindTwoWay = twoWay;
            this.PullTextFromBinding();
            if (source is INotifyBindablePropertyChanged n) {
                n.BindablePropertyChanged += this.OnBindablePropertyChanged;
            }
        }

        /// <summary>Removes any binding previously set with <see cref="SetTextBinding"/>.</summary>
        public void ClearTextBinding() {
            if (this._bindSource is INotifyBindablePropertyChanged n) {
                n.BindablePropertyChanged -= this.OnBindablePropertyChanged;
            }

            this._bindSource = null;
            this._bindPropertyName = null;
        }

        private void OnBindablePropertyChanged(object sender, string propertyName) {
            if (propertyName == null || propertyName.Length == 0 || propertyName == this._bindPropertyName) {
                this.PullTextFromBinding();
            }
        }

        private void PullTextFromBinding() {
            if (this._bindSource == null || this._bindPropertyName == null) {
                return;
            }

            try {
                var v = this._bindSource.GetType().InvokeMember(this._bindPropertyName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, this._bindSource, null);
                var s = v == null ? string.Empty : v.ToString();
                this._suppressBindPush = true;
                try {
                    this.text = s;
                    this.InvalidateMeasure();
                }
                finally {
                    this._suppressBindPush = false;
                }
            }
            catch (Exception ex) {
                // Reflection can throw a wide range of types here (missing
                // member, mismatched signature, user-getter throwing). Don't
                // tear down the paint pass — instead surface via BindingError
                // so a dev subscriber can log it.
                this.RaiseBindingError(BindingErrorDirection.Pull, ex);
            }
        }

        private void PushTextToBinding() {
            if (!this._bindTwoWay || this._bindSource == null || this._bindPropertyName == null) {
                return;
            }

            try {
                this._bindSource.GetType().InvokeMember(this._bindPropertyName, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance, null, this._bindSource, new object[] { this.text });
            }
            catch (Exception ex) {
                this.RaiseBindingError(BindingErrorDirection.Push, ex);
            }
        }

        private void RaiseBindingError(BindingErrorDirection direction, Exception ex) {
            var handler = this.BindingError;
            if (handler == null) return;
            handler(this, new BindingErrorEventArgs(direction, this._bindPropertyName, ex));
        }

        /// <summary>The color of the border drawn around the field.</summary>
        public Color BorderColor {
            get => this.bordercolor;
            set {
                this.bordercolor = value;

                this.InvalidateMeasure();
            }
        }

        /// <summary>The thickness in pixels of the border drawn around the field.</summary>
        public ushort BorderThickness {
            get => this.borderthickness;
            set {
                this.borderthickness = value;

                this.InvalidateMeasure();
            }
        }

        /// <summary>The horizontal padding in pixels between the border and the text.</summary>
        public ushort PaddingX {
            get => this.paddingx;
            set {
                this.paddingx = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>The vertical padding in pixels between the border and the text.</summary>
        public ushort PaddingY {
            get => this.paddingy;
            set {
                this.paddingy = value;
                this.InvalidateMeasure();
            }
        }

        internal bool ForOnScreenKeyboard { get; set; }

        /// <summary>Focuses the field on tap and, unless a physical keyboard has suppressed it, opens the on-screen
        /// keyboard.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled || this.ForOnScreenKeyboard) {
                return;
            }

            base.OnTouchUp(e); // raise the public TouchUp event for user/designer handlers

            // Focus so a physical keyboard (and the caret) target this field...
            Buttons.Focus(this);

            // ...and pop the on-screen keyboard only when auto-show is on (no physical keyboard is taking over).
            if (Application.Current.ShowOnScreenKeyboardAutomatically) {
                Application.Current.ShowOnScreenKeyboardFor(this);
            }
        }

        /// <summary>Hardware buttons: Select opens the on-screen keyboard (unless suppressed); Left/Right/Home move
        /// the caret. Arrow keys from a physical keyboard map here.</summary>
        protected override void OnButtonDown(ButtonEventArgs e) {
            if (!this.IsEnabled || this.ForOnScreenKeyboard) {
                return;
            }

            switch (e.Button) {
                case HardwareButton.Select:
                    if (Application.Current.ShowOnScreenKeyboardAutomatically) {
                        Application.Current.ShowOnScreenKeyboardFor(this);
                        e.Handled = true;
                    }
                    break;
                case HardwareButton.Left:
                    this.SetCaret(this.caretIndex - 1);
                    e.Handled = true;
                    break;
                case HardwareButton.Right:
                    this.SetCaret(this.caretIndex + 1);
                    e.Handled = true;
                    break;
                case HardwareButton.Home:
                    this.SetCaret(0);
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>Inserts or removes a character at the caret (physical-keyboard text entry). Backspace = '\b',
        /// delete = (char)127.</summary>
        protected override void OnCharacter(char c) {
            if (!this.IsEnabled || this.ForOnScreenKeyboard) {
                return;
            }

            var t = this.text;
            var i = this.caretIndex;

            if (c == '\b') {                          // backspace
                if (i <= 0) return;
                this.Text = t.Substring(0, i - 1) + t.Substring(i);
                this.caretIndex = i - 1;
            }
            else if (c == (char)127) {                // delete
                if (i >= t.Length) return;
                this.Text = t.Substring(0, i) + t.Substring(i + 1);
                this.caretIndex = i;
            }
            else if (c >= ' ') {                      // printable
                this.Text = t.Substring(0, i) + c + t.Substring(i);
                this.caretIndex = i + 1;
            }
            else {
                return;                               // other control characters ignored
            }

            this.caretVisible = true;
            this.Invalidate();
        }

        /// <summary>Starts the blinking caret and moves it to the end of the text when the field gains focus.</summary>
        protected override void OnGotFocus(FocusChangedEventArgs e) {
            base.OnGotFocus(e);

            this.caretIndex = this.text.Length;
            this.caretVisible = true;
            this.EnsureCaretTimer();
            this.caretTimer.Start();
            this.Invalidate();
        }

        /// <summary>Stops the blinking caret when the field loses focus.</summary>
        protected override void OnLostFocus(FocusChangedEventArgs e) {
            base.OnLostFocus(e);

            this.caretVisible = false;
            this.caretTimer?.Stop();
            this.Invalidate();
        }

        private void SetCaret(int index) {
            if (index < 0) index = 0;
            if (index > this.text.Length) index = this.text.Length;

            this.caretIndex = index;
            this.caretVisible = true;
            this.Invalidate();
        }

        private void EnsureCaretTimer() {
            if (this.caretTimer == null) {
                this.caretTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                this.caretTimer.Tick += (s, ev) => {
                    this.caretVisible = !this.caretVisible;
                    this.Invalidate();
                };
            }
        }

        /// <summary>Measures the size needed for the text plus padding and border.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            this._font.ComputeExtent(this.text, out desiredWidth, out desiredHeight);

            desiredWidth = this._font.MaxWidth + (this.PaddingX * 2) + (this.BorderThickness * 2);
            desiredHeight = this._font.Height + (this.PaddingY * 2) + (this.BorderThickness * 2);
        }

        /// <summary>Records the arranged size of the field.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            this.width = arrangeWidth;
            this.height = arrangeHeight;
        }

        /// <summary>Draws the field's border and text (masked when <see cref="PasswordChar"/> is set).</summary>
        public override void OnRender(DrawingContext dc) {
            if (this.Foreground is not SolidColorBrush b)
                throw new NotSupportedException("TextBox.Foreground must be a SolidColorBrush; gradient or image brushes are not supported.");

            base.OnRender(dc);

            var txt = string.Empty;

            for (var i = 0; i < this.text.Length; i++) {
                txt += this.PasswordChar == char.MinValue ? this.text[i] : this.PasswordChar;
            }
            //var diff = this._renderWidth - this.width;
            // Place the centerline of the font at the center of the textbox
            var y = (this.ActualHeight - this._font.Height) / 2;
            var x = this.BorderThickness + this.PaddingX;
            //var y = this.BorderThickness + this.PaddingY;
            var w = this.width - (this.BorderThickness * 2) - (this.PaddingX * 2);

            if (this.BorderThickness > 0) {
                dc.DrawRectangle(this.Background, new Pen(this.BorderColor, this.BorderThickness), 0, 0, this.width, this.height);
            }

            //if (diff > 0) {
            //    dc.DrawText(ref txt, this._font, b.Color, 0, y, this._renderWidth, this._font.Height, this.TextAlign, TextTrimming.CharacterEllipsis);
            //}
            //else {
            //    dc.DrawText(ref txt, this._font, b.Color, diff, y, this._renderWidth + this.width, this._font.Height, this.TextAlign, TextTrimming.CharacterEllipsis);
            //}

            if (txt != string.Empty)
                dc.DrawText(ref txt, this._font, b.Color, x, y, w, this._font.Height, this.TextAlign, TextTrimming.CharacterEllipsis);

            // Blinking caret at the insertion point, drawn only while the field is focused. Positioned after the
            // displayed text up to the caret (approximate for non-left alignment).
            if (this.IsFocused && this.caretVisible) {
                var idx = this.caretIndex;
                if (idx > txt.Length) idx = txt.Length;

                this._font.ComputeExtent(txt.Substring(0, idx), out var caretW, out var caretH);

                var caretX = x + caretW;
                var maxX = this.width - this.BorderThickness - 1;
                if (caretX > maxX) caretX = maxX;

                dc.DrawLine(new Pen(b.Color, 1), caretX, y, caretX, y + this._font.Height);
            }
        }
    }
}
