using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class MessageBox : StackPanel {        
        private BitmapImage backImage;
        private Button buttonLeft;
        private Button buttonRight;

        public event RoutedEventHandler ButtonLeftClick;
        public event RoutedEventHandler ButtonRightClick;

        private void InitResource() => this.backImage = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Modal)));

        public MessageBox(int width, int height, int buttonWidth, int buttonHeight, string buttonLeftText, int buttonLeftMarginLeft, int buttonLeftMarginTop, string buttonRightText, int buttonRightMarginLeft, int buttonRightMarginTop, Font buttonFont) {
            this.InitResource();

            this.Orientation = Orientation.Horizontal;

            this.Width = width;
            this.Height = height;            

            var textButtonLeft = new Text(buttonFont, buttonLeftText) {
                ForeColor = Colors.Black,
                HorizontalAlignment = GHIElectronics.TinyCLR.UI.HorizontalAlignment.Center,
                VerticalAlignment = GHIElectronics.TinyCLR.UI.VerticalAlignment.Center,

            };

            this.buttonLeft = new Button() {
                Child = textButtonLeft,
                Width = buttonWidth,
                Height = buttonHeight,
            };

            this.buttonLeft.SetMargin(buttonLeftMarginLeft, buttonLeftMarginTop, 0, 0);

            var textButtonRight = new Text(buttonFont, buttonRightText) {
                ForeColor = Colors.Black,
                HorizontalAlignment = GHIElectronics.TinyCLR.UI.HorizontalAlignment.Center,
                VerticalAlignment = GHIElectronics.TinyCLR.UI.VerticalAlignment.Center,

            };

            this.buttonRight = new Button() {
                Child = textButtonRight,
                Width = buttonWidth,
                Height = buttonHeight,
            };

            this.buttonRight.SetMargin(buttonRightMarginLeft, buttonRightMarginTop, 0, 0);

            this.Children.Add(this.buttonLeft);
            this.Children.Add(this.buttonRight);

            this.buttonLeft.Click += this.ButtonLeft_Click;
            this.buttonRight.Click += this.ButtonRight_Click;

            this.TitlebarHeight = this.Height / 3;
            this.MessageFont = buttonFont;
            this.TitleFont = buttonFont;
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e) => ButtonRightClick?.Invoke(sender, e);

        private void ButtonLeft_Click(object sender, RoutedEventArgs e) => ButtonLeftClick?.Invoke(sender, e);

        public override void OnRender(DrawingContext dc) {
            var x = 0;
            var y = this.TitlebarHeight;

            base.OnRender(dc);

            var tile = this.Title;
            var message = this.Message;

            dc.Scale9Image(0, 0, this.Width, this.Height, this.backImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
            dc.DrawText(ref tile, this.TitleFont, this.TitleColor, x + 10, 0 + ((this.TitlebarHeight - this.TitleFont.Height) / 2), this.Width - 20, this.TitleFont.Height, TextAlignment.Left, TextTrimming.None);

            var messageHeight = this.Height - this.TitlebarHeight;

            dc.DrawText(ref message, this.MessageFont, this.MessageColor, x + 10, y + 5, this.Width - 20, messageHeight - 10, TextAlignment.Left, TextTrimming.None);
        }

        public void Dispose() => this.backImage.graphics.Dispose();

        public string Message { get; set; } = string.Empty;
        public Font MessageFont { get; set; }
        public Media.Color MessageColor { get; set; } = Colors.Black;
        public int TitlebarHeight  { get; set; } = 34;
        public string Title { get; set; } = string.Empty;
        public Font TitleFont { get; set; }
        public Media.Color TitleColor { get; set; } = Colors.Black;
        public int RadiusBorder { get; set; } = 5;
        public ushort Alpha { get; set; }  = 0xFF;
    }
}
