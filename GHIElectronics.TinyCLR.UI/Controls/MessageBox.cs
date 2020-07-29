using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class MessageBox : StackPanel, IDisposable {
        private BitmapImage backImage;
        private Button buttonLeft;
        private Button buttonRight;

        public event RoutedEventHandler ButtonLeftClick;
        public event RoutedEventHandler ButtonRightClick;

        private void InitResource() => this.backImage = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Modal)));

        public MessageBox(int width, int height, string buttonLeftText, string buttonRightText, Font font) {
            this.InitResource();

            this.Orientation = Orientation.Horizontal;

            this.Width = width;
            this.Height = height;
            this.TitlebarHeight = this.Height / 3;
            this.Font = font;

            var textButtonLeft = new Text(font, buttonLeftText) {
                ForeColor = Colors.Black,
                HorizontalAlignment = GHIElectronics.TinyCLR.UI.HorizontalAlignment.Center,
                VerticalAlignment = GHIElectronics.TinyCLR.UI.VerticalAlignment.Center,

            };

            this.buttonLeft = new Button() {
                Child = textButtonLeft,
                Width = width / 4,
                Height = font.Height * 2,
            };

            if (buttonRightText != null) {
                this.buttonLeft.SetMargin((width / 4 - this.buttonLeft.Width / 2), this.TitlebarHeight + font.Height * 2, 0, 0);
            }
            else {
                this.buttonLeft.SetMargin((width / 2 - this.buttonLeft.Width / 2), this.TitlebarHeight + font.Height * 2, 0, 0);
            }

            this.Children.Add(this.buttonLeft);
            this.buttonLeft.Click += this.ButtonLeft_Click;

            if (buttonRightText != null) {
                var textButtonRight = new Text(font, buttonRightText) {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = GHIElectronics.TinyCLR.UI.HorizontalAlignment.Center,
                    VerticalAlignment = GHIElectronics.TinyCLR.UI.VerticalAlignment.Center,

                };

                this.buttonRight = new Button() {
                    Child = textButtonRight,
                    Width = width / 4,
                    Height = font.Height * 2,
                };

                this.buttonRight.SetMargin(width / 2 - this.buttonRight.Width, this.TitlebarHeight + font.Height * 2, 0, 0);
                this.Children.Add(this.buttonRight);
                this.buttonRight.Click += this.ButtonRight_Click;
            }
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e) => ButtonRightClick?.Invoke(sender, e);

        private void ButtonLeft_Click(object sender, RoutedEventArgs e) => ButtonLeftClick?.Invoke(sender, e);

        public override void OnRender(DrawingContext dc) {
            var offsetX = 10;

            base.OnRender(dc);

            var tile = this.Title;
            var message = this.Message;

            dc.Scale9Image(0, 0, this.Width, this.Height, this.backImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);

            dc.DrawText(ref tile, this.Font, this.TitleColor, offsetX, (this.TitlebarHeight - this.Font.Height) / 2, this.Width, this.Font.Height, TextAlignment.Left, TextTrimming.None);

            dc.DrawText(ref message, this.Font, this.MessageColor, offsetX, this.TitlebarHeight, this.Width, this.Font.Height, TextAlignment.Left, TextTrimming.None);
        }

        public string Message { get; set; } = string.Empty;
        public Media.Color MessageColor { get; set; } = Colors.Black;
        public int TitlebarHeight { get; set; } = 34;
        public string Title { get; set; } = string.Empty;
        public Font Font { get; set; }
        public Media.Color TitleColor { get; set; } = Colors.Black;
        public int RadiusBorder { get; set; } = 5;
        public ushort Alpha { get; set; } = 0xFF;

        private bool disposed;

        public void Dispose() {
            if (this.buttonLeft != null) {
                this.buttonLeft.Click -= this.ButtonLeft_Click;
            }

            if (this.buttonRight != null) {
                this.buttonRight.Click -= this.ButtonRight_Click;
            }

            if (this.Children != null)
                this.Children.Clear();

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.backImage.graphics.Dispose();

                this.disposed = true;
            }
        }

        ~MessageBox() {
            this.Dispose(false);
        }
    }
}
