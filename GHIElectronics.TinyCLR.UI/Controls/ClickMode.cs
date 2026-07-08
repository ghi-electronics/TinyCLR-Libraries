namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Specifies when the <c>Click</c> event fires for button-family controls
    /// (<see cref="Button"/>, <see cref="CheckBox"/>, <see cref="RadioButton"/>). Mirrors WPF's
    /// <c>System.Windows.Controls.ClickMode</c>. Governs touch activation; the default is
    /// <see cref="Release"/>.</summary>
    public enum ClickMode {
        /// <summary>Click fires when the touch is pressed and then released over the control (default,
        /// matches WPF). Allows dragging off the control before release to cancel.</summary>
        Release = 0,
        /// <summary>Click fires as soon as the control is pressed (touch down) for the snappiest response.
        /// This matches the TinyCLR 2.x behavior; there is no drag-off-to-cancel in this mode.</summary>
        Press = 1,
        /// <summary>Reserved for pointer hover. A touch-only panel never hovers, so this behaves like
        /// <see cref="Release"/> for touch input.</summary>
        Hover = 2
    }
}
