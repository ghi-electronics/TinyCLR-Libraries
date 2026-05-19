using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>
    /// Central palette for TinyCLR.UI. Brushes are shared instances; change <see cref="WindowBackground"/> etc.
    /// then replace the corresponding brush field if you need live updates.
    /// </summary>
    public static class Theme {
        public static Color WindowBackground { get; set; } = Colors.White;
        public static Color ControlSurface { get; set; } = Colors.LightGray;
        public static Color TextBoxFill { get; set; } = Colors.White;
        public static Color TextPrimary { get; set; } = Colors.Black;
        public static Color Border { get; set; } = Colors.Black;
        public static Color FocusRing { get; set; } = Colors.CornflowerBlue;
        public static Color SelectionHighlight { get; set; } = Colors.Teal;

        /// <summary>Default Scale9Image alpha for surface-rendered controls (Button, CheckBox, RadioButton, ProgressBar, Dropdown, Slider). ~78% opacity.</summary>
        public static ushort DefaultAlpha { get; set; } = 0xC8;

        /// <summary>Default corner radius (in pixels) for Scale9Image-rendered surfaces.</summary>
        public static int DefaultRadiusBorder { get; set; } = 5;

        public static readonly SolidColorBrush WindowBackgroundBrush = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush ControlSurfaceBrush = new SolidColorBrush(Colors.LightGray);
        public static readonly SolidColorBrush TextBoxFillBrush = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush TextPrimaryBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush SelectionBrush = new SolidColorBrush(Colors.Teal);

        static Theme() => RefreshBrushesFromColors();

        /// <summary>Call after mutating color fields if you need brushes to match.</summary>
        public static void RefreshBrushesFromColors() {
            WindowBackgroundBrush.Color = WindowBackground;
            ControlSurfaceBrush.Color = ControlSurface;
            TextBoxFillBrush.Color = TextBoxFill;
            TextPrimaryBrush.Color = TextPrimary;
            SelectionBrush.Color = SelectionHighlight;
        }
    }
}
