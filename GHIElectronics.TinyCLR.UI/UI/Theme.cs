using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>
    /// Central palette for TinyCLR.UI. Brushes are shared instances; change <see cref="WindowBackground"/> etc.
    /// then replace the corresponding brush field if you need live updates.
    /// </summary>
    public static class Theme {
        /// <summary>The default background color for windows.</summary>
        public static Color WindowBackground { get; set; } = Colors.White;
        /// <summary>The default fill color for control surfaces.</summary>
        public static Color ControlSurface { get; set; } = Colors.LightGray;
        /// <summary>The default fill color for text boxes.</summary>
        public static Color TextBoxFill { get; set; } = Colors.White;
        /// <summary>The default color for primary text.</summary>
        public static Color TextPrimary { get; set; } = Colors.Black;
        /// <summary>The default color for borders.</summary>
        public static Color Border { get; set; } = Colors.Black;
        /// <summary>The default color for the focus ring drawn around focused controls.</summary>
        public static Color FocusRing { get; set; } = Colors.CornflowerBlue;
        /// <summary>The default color used to highlight selected content.</summary>
        public static Color SelectionHighlight { get; set; } = Colors.Teal;

        /// <summary>Default Scale9Image alpha for surface-rendered controls (Button, CheckBox, RadioButton, ProgressBar, ComboBox, Slider). ~78% opacity.</summary>
        public static ushort DefaultAlpha { get; set; } = 0xC8;

        /// <summary>Default corner radius (in pixels) for Scale9Image-rendered surfaces.</summary>
        public static int DefaultRadiusBorder { get; set; } = 5;

        /// <summary>The shared brush for window backgrounds.</summary>
        public static readonly SolidColorBrush WindowBackgroundBrush = new SolidColorBrush(Colors.White);
        /// <summary>The shared brush for control surfaces.</summary>
        public static readonly SolidColorBrush ControlSurfaceBrush = new SolidColorBrush(Colors.LightGray);
        /// <summary>The shared brush for text box fills.</summary>
        public static readonly SolidColorBrush TextBoxFillBrush = new SolidColorBrush(Colors.White);
        /// <summary>The shared brush for primary text.</summary>
        public static readonly SolidColorBrush TextPrimaryBrush = new SolidColorBrush(Colors.Black);
        /// <summary>The shared brush used to highlight selected content.</summary>
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
