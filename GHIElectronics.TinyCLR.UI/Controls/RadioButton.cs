using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public static class RadioButtonManager {
        private static Hashtable groups = new Hashtable();

        // Adds a radio button to it's group.
        internal static void AddButton(RadioButton radioButton) {
            var name = radioButton.GroupName;

            if (!groups.Contains(name))
                groups.Add(name, new ArrayList());

            ((ArrayList)groups[name]).Add(radioButton);
        }

        public static string GetValue(string groupName) {
            if (!groups.Contains(groupName))
                throw new ArgumentOutOfRangeException("groupName", "No such radio button group exists.");

            // Find the selected button
            var group = (ArrayList)groups[groupName];
            RadioButton button;
            for (var i = 0; i < group.Count; i++) {
                button = (RadioButton)group[i];
                if (button.Checked)
                    return button.Value;
            }

            return string.Empty;
        }

        public static int GetCount(string groupName) {
            if (!groups.Contains(groupName))
                throw new ArgumentOutOfRangeException("groupName", "No such radio button group exists.");

            return ((ArrayList)groups[groupName]).Count;
        }

        internal static void TriggerTapEvent(object sender) {
            var tappedButton = (RadioButton)sender;

            if (!groups.Contains(tappedButton.GroupName))
                return;

            var group = (ArrayList)groups[tappedButton.GroupName];

            // Find the button that was previously selected and unselect it
            RadioButton button;
            for (var i = 0; i < group.Count; i++) {
                button = (RadioButton)group[i];
                if (button.Name == tappedButton.Name) continue;
                if (button.Checked) {
                    button.Checked = false;
                    break;
                }
            }
        }
    }

    public class RadioButton : ContentControl, IDisposable {
        // Cached once per AppDomain so every toggle doesn't allocate a fresh RoutedEvent.
        private static readonly RoutedEvent ClickRoutedEvent =
            new RoutedEvent("ClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        public event RoutedEventHandler Click;
        private BitmapImage bitmapImageRadioButton;

        private bool isChecked = false;
        private string value = string.Empty;

        public string Name { get; set; } = string.Empty;
        public ushort Alpha { get; set; } = Theme.DefaultAlpha;
        public int RadiusBorder { get; set; } = Theme.DefaultRadiusBorder;
        public bool ShowBackground { get; set; } = true;

        private void InitResource() => this.bitmapImageRadioButton = Resources.LoadBitmapImage(Resources.BitmapResources.RadioButton);

        public RadioButton() : this(string.Empty) {

        }
        public RadioButton(string groupName) : base() {
            this.InitResource();

            this.Width = this.bitmapImageRadioButton.Width;
            this.Height = this.bitmapImageRadioButton.Height;

            this.GroupName = groupName;

            RadioButtonManager.AddButton(this);
        }

        public override void OnRender(DrawingContext dc) {
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);

            if (ShowBackground)
                dc.Scale9Image(0, 0, w, h, this.bitmapImageRadioButton, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);

            var x = (w / 2) - 1;
            var y = (h / 2) - 1;

            var xRadius = w / 3;
            var yRadius = h / 3;

            var penSelected = new GHIElectronics.TinyCLR.UI.Media.Pen(this.SelectedColor, 1);
            var penSelectedOutline = new GHIElectronics.TinyCLR.UI.Media.Pen(this.SelectedOutlineColor, 1);
            var penUnselectOutline = new GHIElectronics.TinyCLR.UI.Media.Pen(this.OutlineUnselectColor, 1);

            var brush = new SolidColorBrush(this.SelectedColor);

            if (this.isChecked) {
                dc.DrawEllipse(null, penSelectedOutline, x, y, xRadius, yRadius);

                dc.DrawEllipse(brush, penSelected, x, y, w / 4, h / 4);

            }
            else {
                dc.DrawEllipse(null, penUnselectOutline, x, y, xRadius, yRadius);
            }
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        protected override void OnTouchDown(TouchEventArgs e) {
            // No-op. Click + Toggle now happen exactly once on TouchUp.
        }

        protected override void OnButtonUp(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        // Fires Click and toggles the radio group selection.
        // Returns the Click args.Handled flag so callers can forward it.
        // Exceptions from user handlers propagate — the framework should not
        // silently swallow them.
        private bool PerformClick() {
            var args = new RoutedEventArgs(ClickRoutedEvent, this);
            this.Click?.Invoke(this, args);

            this.Toggle();

            if (this.Parent != null)
                this.Invalidate();

            return args.Handled;
        }

        public string Value {
            get {
                if (this.isChecked)
                    return this.value;
                else
                    return string.Empty;
            }
            set => this.value = value;
        }


        public bool Checked {
            get => this.isChecked;
            set {
                if (this.isChecked != value) {
                    this.isChecked = value;

                    if (this.Parent != null)
                        this.Invalidate();
                }
            }
        }

        public string GroupName { get; set; } = string.Empty;

        public void Toggle() {
            var groupCount = RadioButtonManager.GetCount(this.GroupName);

            // Only allow change if this button is alone or in a group and not selected
            if (groupCount <= 1 || (groupCount > 1 && !this.Checked)) {
                this.Checked = !this.Checked;
                RadioButtonManager.TriggerTapEvent(this);
            }
        }

        public GHIElectronics.TinyCLR.UI.Media.Color OutlineUnselectColor { get; set; } = GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xb8, 0xb8, 0xb8);
        public GHIElectronics.TinyCLR.UI.Media.Color SelectedOutlineColor { get; set; } = GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0x00, 0x2d, 0xff);
        public GHIElectronics.TinyCLR.UI.Media.Color SelectedColor { get; set; } = GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0x35, 0x8b, 0xf6);

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                this.bitmapImageRadioButton?.graphics?.Dispose();
            }

            this.disposed = true;
        }

        ~RadioButton() {
            this.Dispose(false);
        }
    }
}
