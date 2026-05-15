using System;
using System.Reflection;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs e);

    public class TextChangedEventArgs : RoutedEventArgs {
        public TextChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
    }

    public class TextBox : Control {
        // Cached once per AppDomain so every Text-change doesn't allocate a
        // fresh RoutedEvent object.
        private static readonly RoutedEvent TextChangedRoutedEvent =
            new RoutedEvent("TextChangedEvent", RoutingStrategy.Bubble, typeof(TextChangedEventHandler));

        private string text = string.Empty;
        private Color bordercolor = Colors.Black;
        private ushort borderthickness = 1, paddingx, paddingy;
        private int width, height;

        private object _bindSource;
        private string _bindPropertyName;
        private bool _bindTwoWay;
        private bool _suppressBindPush;

        public TextBox() {
            this.Background = Theme.TextBoxFillBrush;
            this.bordercolor = Theme.Border;
        }

        public event TextChangedEventHandler TextChanged;

        public TextAlignment TextAlign { get; set; } = TextAlignment.Left;

        public char PasswordChar { get; set; } = char.MinValue;

        public string Text {
            get => this.text;
            set {
                this.text = value;

                this.InvalidateMeasure();

                if (!this._suppressBindPush) {
                    this.PushTextToBinding();
                }

                var args = new TextChangedEventArgs(TextChangedRoutedEvent, this);

                this.TextChanged?.Invoke(this, args);
            }
        }

        /// <summary>
        /// One-way or two-way bind <see cref="Text"/> to a CLR property on <paramref name="source"/> using reflection.
        /// For change notifications implement <see cref="INotifyBindablePropertyChanged"/> on the source.
        /// </summary>
        public void SetTextBinding(object source, string propertyName, bool twoWay = true) {
            this.ClearTextBinding();
            if (source == null || propertyName == null) {
                throw new ArgumentNullException();
            }

            this._bindSource = source;
            this._bindPropertyName = propertyName;
            this._bindTwoWay = twoWay;
            this.PullTextFromBinding();
            if (source is INotifyBindablePropertyChanged n) {
                n.BindablePropertyChanged += this.OnBindablePropertyChanged;
            }
        }

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
            catch {
            }
        }

        private void PushTextToBinding() {
            if (!this._bindTwoWay || this._bindSource == null || this._bindPropertyName == null) {
                return;
            }

            try {
                this._bindSource.GetType().InvokeMember(this._bindPropertyName, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance, null, this._bindSource, new object[] { this.text });
            }
            catch {
            }
        }

        public Color BorderColor {
            get => this.bordercolor;
            set {
                this.bordercolor = value;

                this.InvalidateMeasure();
            }
        }

        public ushort BorderThickness {
            get => this.borderthickness;
            set {
                this.borderthickness = value;

                this.InvalidateMeasure();
            }
        }

        public ushort PaddingX {
            get => this.paddingx;
            set => this.paddingx = value;
        }

        public ushort PaddingY {
            get => this.paddingy;
            set => this.paddingy = value;
        }

        internal bool ForOnScreenKeyboard { get; set; }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            if (!this.ForOnScreenKeyboard)
                Application.Current.ShowOnScreenKeyboardFor(this);
        }

        /// <summary>
        /// Hardware button support: <see cref="HardwareButton.Select"/> opens the
        /// on-screen keyboard, mirroring tap-to-edit behavior.
        /// </summary>
        protected override void OnButtonDown(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            if (!this.ForOnScreenKeyboard) {
                Application.Current.ShowOnScreenKeyboardFor(this);
                e.Handled = true;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            this._font.ComputeExtent(this.text, out desiredWidth, out desiredHeight);

            desiredWidth = this._font.MaxWidth + (this.PaddingX * 2) + (this.BorderThickness * 2);
            desiredHeight = this._font.Height + (this.PaddingY * 2) + (this.BorderThickness * 2);
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            this.width = arrangeWidth;
            this.height = arrangeHeight;
        }

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
        }
    }
}
