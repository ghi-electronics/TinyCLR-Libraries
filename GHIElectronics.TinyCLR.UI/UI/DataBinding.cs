namespace GHIElectronics.TinyCLR.UI {
    /// <summary>PC-style change notification for simple MVVM binding (TinyCLR subset).</summary>
    public delegate void BindablePropertyChangedEventHandler(object sender, string propertyName);

    /// <summary>
    /// Implement on view-models; raise <see cref="BindablePropertyChanged"/> with the property name
    /// (or null / empty to refresh all bindings on the object).
    /// </summary>
    public interface INotifyBindablePropertyChanged {
        /// <summary>Occurs when a bound property value changes.</summary>
        event BindablePropertyChangedEventHandler BindablePropertyChanged;
    }
}
