using System;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs e);

    public class TextChangedEventArgs : RoutedEventArgs {
        public TextChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
    }

    public class TextBox : Control {
        private string text = string.Empty;
        private Color bordercolor = Colors.Black;
        private ushort borderthickness = 1, paddingx, paddingy;
        private int width, height;

        public TextBox() => this.Background = new SolidColorBrush(Colors.White);

        public event TextChangedEventHandler TextChanged;

        public TextAlignment TextAlign { get; set; } = TextAlignment.Left;

        public string Text {
            get => this.text;
            set {
                this.text = value;

                this.InvalidateMeasure();

                var evt = new RoutedEvent("TextChangedEvent", RoutingStrategy.Bubble, typeof(TextChangedEventHandler));
                var args = new TextChangedEventArgs(evt, this);

                this.TextChanged?.Invoke(this, args);
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
            if (!(this.Foreground is SolidColorBrush b)) throw new NotSupportedException();

            base.OnRender(dc);

            var txt = this.text;
            //var diff = this._renderWidth - this.width;
            // Place the centerline of the font at the center of the textbox
            //var y = (this.ActualHeight - this._font.Height) / 2;
            var x = this.BorderThickness + this.PaddingX;
            var y = this.BorderThickness + this.PaddingY;
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
            dc.DrawText(ref txt, this._font, b.Color, x, y, w, this._font.Height, this.TextAlign, TextTrimming.CharacterEllipsis);
        }
    }
}
