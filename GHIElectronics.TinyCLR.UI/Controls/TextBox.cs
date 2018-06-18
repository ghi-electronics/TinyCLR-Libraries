﻿using System;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs e);

    public class TextChangedEventArgs : RoutedEventArgs {
        public TextChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
    }

    public class TextBox : Control {
        private string text = string.Empty;

        public TextBox() => this.Background = new SolidColorBrush(Colors.White);

        public event TextChangedEventHandler TextChanged;

        public string Text {
            get => this.text;
            set {
                this.text = value;

                var evt = new RoutedEvent("TextChangedEvent", RoutingStrategy.Bubble, typeof(TextChangedEventHandler));
                var args = new TextChangedEventArgs(evt, this);

                this.TextChanged?.Invoke(this, args);
            }
        }

        protected override void OnTouchUp(TouchEventArgs e) {

        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) => this._font.ComputeExtent(this.text, out desiredWidth, out desiredHeight);

        public override void OnRender(DrawingContext dc) {
            if (!(this.Foreground is SolidColorBrush b)) throw new NotSupportedException();

            base.OnRender(dc);

            var txt = this.text;

            dc.DrawText(ref txt, this._font, b.Color, 0, 0, this._renderWidth, this._font.Height, TextAlignment.Left, TextTrimming.CharacterEllipsis);
        }
    }
}
