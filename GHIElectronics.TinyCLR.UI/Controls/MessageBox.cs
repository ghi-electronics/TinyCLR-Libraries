using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {


    public class MessageBox : Canvas, IDisposable {

        public Font Font { get; set; }

        public delegate void MessageBoxRoutedEventHandler(object sender, MessageBoxRoutedEventArgs e);
        public event MessageBoxRoutedEventHandler ButtonClick;

        private Button buttonCenter;
        private Button buttonLeft;
        private Button buttonRight;
        private MessageBoxButtons messageBoxButton;
        private int messageLines;
        public UIElement Owner { get; set; }

        public class MessageBoxRoutedEventArgs {
            public MessageBoxRoutedEventArgs() {

            }

            public RoutedEventArgs RoutedEventArgs { get; internal set; }
            public DialogResult DialogResult { get; internal set; }
        }

        private bool isAtived = false;
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

        public MessageBox(Font font) => this.Font = font;
        public void Show(string message, string caption, MessageBoxButtons messageBoxButton) => this.Show(null, message, caption, messageBoxButton);
        public void Show(UIElement owner, string message, string caption, MessageBoxButtons messageBoxButton) {

            if (this.isAtived)
                return;

            if (this.Font == null)
                throw new ArgumentNullException(nameof(message));

            if (message == null || message.Length == 0)
                throw new ArgumentNullException(nameof(message));

            this.Owner = owner;

            var windowWidth = 0;
            var windowHeight = 0;

            var alignToOwner = false;

            if (owner != null) {
                try {

                    windowWidth = this.Owner.ActualWidth;
                    windowHeight = this.Owner.ActualHeight;

                    alignToOwner = windowWidth > 0 && windowHeight > 0;
                }
                catch {
                }
            }

            windowWidth = windowWidth == 0 ? Application.Current.MainWindow.Width : windowWidth;
            windowHeight = windowHeight == 0 ? Application.Current.MainWindow.Height : windowHeight;

            this.captionBarHeight = (this.Font.Height * 3 / 2);
            this.Width = (windowWidth * 2) / 3;

            this.messageLines = 0;

            this.messageList = new ArrayList();

            for (var i = 0; i < message.Length; i++) {
                if (message[i] == '\n' || message[i] == '\r') {
                    this.messageLines++;

                    var str = message.Substring(0, i);

                    this.messageList.Add(str);

                    message = message.Substring(i + 1, message.Length - (i + 1));
                    i = 0;
                }
            }

            this.messageList.Add(message);
            this.messageLines++;
            var maxWith = 0;
            var maxAverageWith = 0;

            for (var i = 0; i < this.messageList.Count; i++) {
                var str = this.messageList[i] as string;

                this.Font.ComputeExtent(str, out var w, out var h);

                maxWith = Math.Max(maxWith, w);
                maxAverageWith = Math.Max(maxAverageWith, w / str.Length);
            }

            this.Width = maxWith + maxAverageWith * 2;
            this.Width = Math.Min(this.Width, windowWidth);


            var buttonCenterMessage = string.Empty;
            var buttonLeftMessage = string.Empty;
            var buttonRightMessage = string.Empty;


            this.caption = caption ?? string.Empty;

            this.messageBoxButton = messageBoxButton;

            switch (this.messageBoxButton) {
                case MessageBoxButtons.OK:
                    buttonCenterMessage = "OK";
                    break;
                case MessageBoxButtons.Cancel:
                    buttonCenterMessage = "Cancel";
                    break;
                case MessageBoxButtons.OKCancel:
                    buttonLeftMessage = "OK";
                    buttonRightMessage = "Cancel";
                    break;
                case MessageBoxButtons.YesNo:
                    buttonLeftMessage = "Yes";
                    buttonRightMessage = "No";
                    break;
            }


            if (buttonCenterMessage != string.Empty) {

                var textButtonCenter = new Text(this.Font, buttonCenterMessage) {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                this.Font.ComputeExtent(buttonCenterMessage, out var w, out var h);
                var averageChar = w / buttonCenterMessage.Length;

                this.buttonCenter = new Button() {
                    Child = textButtonCenter,
                    Width = w + averageChar * 2,
                    Height = h * 2,

                };

                this.Height = this.captionBarHeight + (this.messageLines + 2) * this.Font.Height + this.buttonCenter.Height + this.Font.Height;
                this.Width = Math.Max(this.Width, this.buttonCenter.Width * 4);

                this.buttonCenter.SetMargin(this.Width / 2 - this.buttonCenter.Width / 2, this.Height - (this.buttonCenter.Height + this.Font.Height), 0, 0);
                this.buttonCenter.Click += this.Button_Click;

                this.Children.Add(this.buttonCenter);
            }
            else {


                var textButtonLeft = new Text(this.Font, buttonLeftMessage) {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var textButtonRight = new Text(this.Font, buttonRightMessage) {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                this.Font.ComputeExtent(buttonLeftMessage, out var w1, out var h1);
                var averageChar1 = w1 / buttonLeftMessage.Length;

                this.Font.ComputeExtent(buttonRightMessage, out var w2, out var h2);
                var averageChar2 = w1 / buttonRightMessage.Length;

                var averageChar = Math.Max(averageChar1, averageChar2);
                var w = Math.Max(w1, w2);
                var h = Math.Max(h1, h2);

                this.buttonLeft = new Button() {
                    Child = textButtonLeft,
                    Width = w + averageChar * 2,
                    Height = h * 2,
                };

                this.Height = this.captionBarHeight + (this.messageLines + 2) * this.Font.Height + this.buttonLeft.Height + this.Font.Height;
                this.Width = Math.Max(this.Width, this.buttonLeft.Width * 4);

                this.buttonLeft.SetMargin(this.Width / 2 - this.buttonLeft.Width * 3 / 2, this.Height - (this.buttonLeft.Height + this.Font.Height), 0, 0);

                this.buttonLeft.Click += this.Button_Click;

                this.Children.Add(this.buttonLeft);

                this.buttonRight = new Button() {
                    Child = textButtonRight,
                    Width = Math.Max(this.buttonLeft.Width, w + averageChar * 2),
                    Height = h * 2,
                };

                this.buttonRight.SetMargin(this.Width / 2 + this.buttonRight.Width / 2, this.Height - (this.buttonLeft.Height + this.Font.Height), 0, 0);

                this.Width = Math.Max(this.Width, this.buttonRight.Width * 4);


                this.buttonRight.Click += this.Button_Click;
                this.Children.Add(this.buttonRight);
            }

            this.Width = Math.Min(this.Width, windowWidth);
            this.Height = Math.Min(this.Height, windowHeight);

            if (alignToOwner)
                this.SetMargin(windowWidth / 2 - this.Width / 2, windowHeight / 2 - this.Height / 2, 0, 0);
            else {
                SetLeft(this, windowWidth / 2 - this.Width / 2);
                SetTop(this, windowHeight / 2 - this.Height / 2);
            }


            if (this.Owner != null && !this.Owner._logicalChildren.Contains(this))
                this.Owner._logicalChildren.Add(this);
            else if (!Application.Current.MainWindow.Child._logicalChildren.Contains(this))
                Application.Current.MainWindow.Child._logicalChildren.Add(this);

            this.isAtived = true;
        }

        public void Close() {
            if (this.Owner != null && this.Owner._logicalChildren.Contains(this)) {
                this.Owner._logicalChildren.Remove(this);
            }
            else {
                if (Application.Current.MainWindow.Child._logicalChildren.Contains(this))
                    Application.Current.MainWindow.Child._logicalChildren.Remove(this);
            }

            if (this.buttonLeft != null) {

                this.buttonLeft.Click -= this.Button_Click;
                this.Children.Remove(this.buttonLeft);
                this.buttonLeft.Dispose();
                this.buttonLeft = null;
            }

            if (this.buttonRight != null) {

                this.buttonRight.Click -= this.Button_Click;
                this.Children.Remove(this.buttonRight);
                this.buttonRight.Dispose();
                this.buttonRight = null;
            }

            if (this.buttonCenter != null) {

                this.buttonCenter.Click -= this.Button_Click;
                this.Children.Remove(this.buttonCenter);
                this.buttonCenter.Dispose();
                this.buttonCenter = null;
            }

            this.isAtived = false;


        }
        private void Button_Click(object sender, RoutedEventArgs e) {

            //if (this.Owner != null && this.Owner._logicalChildren.Contains(this)) {
            //    this.Owner._logicalChildren.Remove(this);
            //}
            //else {
            //    if (Application.Current.MainWindow.Child._logicalChildren.Contains(this))
            //        Application.Current.MainWindow.Child._logicalChildren.Remove(this);
            //}

            this.isAtived = false;

            var e1 = new MessageBoxRoutedEventArgs() {
                RoutedEventArgs = e
            };

            var s = sender as Button;

            if (s == this.buttonCenter) {
                if (this.messageBoxButton == MessageBoxButtons.OK)
                    e1.DialogResult = DialogResult.OK;

                if (this.messageBoxButton == MessageBoxButtons.Cancel)
                    e1.DialogResult = DialogResult.Cancel;
            }

            if (s == this.buttonLeft) {
                if (this.messageBoxButton == MessageBoxButtons.OKCancel)
                    e1.DialogResult = DialogResult.OK;

                if (this.messageBoxButton == MessageBoxButtons.YesNo)
                    e1.DialogResult = DialogResult.Yes;
            }

            if (s == this.buttonRight) {
                if (this.messageBoxButton == MessageBoxButtons.OKCancel)
                    e1.DialogResult = DialogResult.Cancel;

                if (this.messageBoxButton == MessageBoxButtons.YesNo)
                    e1.DialogResult = DialogResult.No;
            }

            ButtonClick?.Invoke(sender, e1);

            this.Close();

        }

        public override void OnRender(DrawingContext dc) {
            var offsetX = 10;
            var offsetY = this.captionBarHeight;

            base.OnRender(dc);

            if (this.caption != null && this.caption.Length > 0)
                dc.DrawRectangle(this.brushCaption, this.penCaption, 0, 0, this.Width, this.captionBarHeight);

            dc.DrawRectangle(this.brushMessage, this.penMessage, 0, this.captionBarHeight, this.Width, this.Width - this.Font.Height * 2);

            if (this.caption != null && this.caption.Length > 0)
                dc.DrawText(ref this.caption, this.Font, Colors.Black, offsetX, (this.captionBarHeight - this.Font.Height) / 2, this.Width, this.Font.Height, TextAlignment.Left, TextTrimming.None);

            for (var i = 0; i < this.messageList.Count; i++) {
                var msg = this.messageList[i] as string;

                dc.DrawText(ref msg, this.Font, Colors.Black, offsetX, offsetY, this.Width, this.Font.Height, TextAlignment.Left, TextTrimming.WordEllipsis);
                offsetY += (this.Font.Height * 3) / 2;
            }
        }

        private Media.Pen penCaption = new Media.Pen(Media.Color.FromRgb(240, 240, 240));
        private SolidColorBrush brushCaption = new SolidColorBrush(Media.Color.FromRgb(240, 240, 240));

        private Media.Pen penMessage = new Media.Pen(Colors.White);
        private SolidColorBrush brushMessage = new SolidColorBrush(Colors.White);

        private ArrayList messageList;
        private string caption = string.Empty;

        private int captionBarHeight = 0;

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            this.isAtived = false;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;

                if (disposing) {

                    this.Close();
                }
            }
        }

        ~MessageBox() {
            this.Dispose(false);
        }
    }
}
