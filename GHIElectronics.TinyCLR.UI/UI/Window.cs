////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>Represents a top-level window that hosts content and is managed by the window manager.</summary>
    public class Window : ContentControl {
        //---------------------------------------------------
        //
        // Constructors
        //
        //---------------------------------------------------
        #region Constructors

        /// <summary>
        ///     Constructs a window object
        /// </summary>
        /// <remarks>
        ///     Automatic determination of current Dispatcher. Use alternative constructor
        ///     that accepts a Dispatcher for best performance.
        /// REFACTOR -- consider specifying app default window sizes to cover Aux case for default window size.
        /// </remarks>
        ///     Initializes the Width/Height, Top/Left properties to use windows
        ///     default. Updates Application object properties if inside app.
        public Window() {
            //There is only one WindowManager.  All Windows currently are forced to be created
            //and to live on the same thread.
            if (WindowManager.Instance == null) throw new InvalidOperationException();

            this._windowManager = WindowManager.Instance;

            this._background = Theme.WindowBackgroundBrush;
            //
            // dependency property initialization.
            // we don't have them, so we just update the properties on the base class,
            // like normal *bleep* fearing developers.
            //
            // Visibility HAS to be set to Collapsed prior to adding this child to the
            // window manager, otherwise the window manager sets the focus to this window
            this.Visibility = Visibility.Collapsed;
            this.IsTabStop = false;
            this.ShowFocusVisual = false;

            // register us with the window manager, like a good little boy
            this._windowManager.Children.Add(this);

            var app = GHIElectronics.TinyCLR.UI.Application.Current;

            // check if within an app && on the same thread
            if (app != null) {
                if (app.Dispatcher.Thread == Threading.Dispatcher.CurrentDispatcher.Thread) {
                    // add to window collection
                    // use internal version since we want to update the underlying collection
                    app.WindowsInternal.Add(this);
                    if (app.MainWindow == null) {
                        app.MainWindow = this;
                    }
                }
                else {
                    app.NonAppWindowsInternal.Add(this);
                }
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>Makes the window visible and brings it to the front, WPF-style. Use with <see cref="Hide"/> to
        /// switch between screens: <c>current.Hide(); next.Show();</c>. (Prefer this over the low-level
        /// <see cref="Topmost"/> property.)</summary>
        public void Show() {
            VerifyAccess();

            this.Visibility = Visibility.Visible;
            this.Activate();
        }

        /// <summary>Hides the window without closing it, WPF-style. It stays alive and can be shown again with
        /// <see cref="Show"/>.</summary>
        public void Hide() {
            VerifyAccess();

            this.Visibility = Visibility.Hidden;
        }

        /// <summary>Brings the window to the front of the z-order and gives it input focus, WPF-style.</summary>
        public void Activate() {
            VerifyAccess();

            // Bring to the front of the z-order. (This no longer captures the window - see WindowManager - so touch
            // still hit-tests down to the controls.)
            this._windowManager?.SetTopMost(this);

            // Make this the active window so keyboard focus navigation (Tab) and modal dialogs scope to it, then put
            // focus on its first control so the keyboard/Select works immediately.
            var app = GHIElectronics.TinyCLR.UI.Application.Current;
            if (app != null) {
                app.MainWindow = this;
            }

            Input.FocusNavigator.TryMoveFocus(true, this);
        }

        /// <summary>Closes the window and removes it from the application and window manager.</summary>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Close() {
            var app = GHIElectronics.TinyCLR.UI.Application.Current;
            if (app != null) {
                app.WindowsInternal.Remove(this);
                app.NonAppWindowsInternal.Remove(this);
            }

            if (this._windowManager != null) {
                this._windowManager.Children.Remove(this);
                this._windowManager = null;
            }
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Auto size Window to its content's size
        /// </summary>
        /// <remarks>
        /// 1. SizeToContent can be applied to Width Height independently
        /// 2. After SizeToContent is set, setting Width/Height does not take affect if that
        ///    dimension is sizing to content.
        /// </remarks>
        /// <value>
        /// Default value is SizeToContent.Manual
        /// </value>
        public SizeToContent SizeToContent {
            get => this._sizeToContent;

            set {
                VerifyAccess();
                this._sizeToContent = value;
            }
        }

        /// <summary>
        ///     Position for Top of the host window
        /// </summary>
        /// <value></value>
        public int Top {
            get => Canvas.GetTop(this);

            set {
                VerifyAccess();
                Canvas.SetTop(this, value);
            }
        }

        /// <summary>Gets or sets the position of the left edge of the window.</summary>
        public int Left {
            get => Canvas.GetLeft(this);

            set {
                VerifyAccess();

                Canvas.SetLeft(this, value);
            }
        }

        /// <summary>
        ///     Determines if this window is always on the top.
        /// </summary>
        public bool Topmost {
            get => this._windowManager.IsTopMost(this);

            set {
                VerifyAccess();

                this._windowManager.SetTopMost(this);
            }
        }

        #endregion Public Properties

        //---------------------------------------------------
        //
        // Public Events
        //
        //---------------------------------------------------
        #region Public Events

        #endregion Public Events

        //---------------------------------------------------
        //
        // Protected Methods
        //
        //---------------------------------------------------
        #region Protected Methods

        // REFACTOR -- need to track if our parent changes.

        /// <summary>
        ///     Measurement override. Implements content sizing logic.
        /// </summary>
        /// <remarks>
        ///     Deducts the frame size from the constraint and then passes it on
        ///     to it's child.  Only supports one Visual child (just like control)
        /// </remarks>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var children = this.LogicalChildren;

            if (children.Count > 0) {
                var child = (UIElement)children[0];
                if (child != null) {
                    // REFACTOR --we need to subtract the frame & chrome around the visual child.
                    child.Measure(availableWidth, availableHeight);
                    child.GetDesiredSize(out desiredWidth, out desiredHeight);

                    return;
                }
            }

            desiredWidth = availableWidth;
            desiredHeight = availableHeight;
        }

        /// <summary>
        ///     ArrangeOverride allows for the customization of the positioning of children.
        /// </summary>
        /// <remarks>
        ///     Deducts the frame size of the window from the constraint and then
        ///     arranges it's child.  Supports only one child.
        /// </remarks>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var children = this.LogicalChildren;

            if (children.Count > 0) {
                if (children[0] is UIElement child) {
                    child.Arrange(0, 0, arrangeWidth, arrangeHeight);
                }
            }
        }

        #endregion Protected Methods

        //---------------------------------------------------
        //
        // Internal Methods
        //
        //---------------------------------------------------
        #region Internal Methods

        #endregion Internal Methods

        //----------------------------------------------
        //
        // Internal Properties
        //
        //----------------------------------------------
        #region Internal Properties

        #endregion Internal Properties

        //----------------------------------------------
        //
        // Private Methods
        //
        //----------------------------------------------
        #region Private Methods

        //
        // These are the callbacks used by the windowSource to notify the window
        // about what the window manager wants it to do.
        //
        #region WindowManager internal methods

        #endregion WindowManager internal methods

        #endregion Private Methods

        //----------------------------------------------
        //
        // Private Properties
        //
        //----------------------------------------------
        #region Private Properties

        #endregion Private Properties

        //----------------------------------------------
        //
        // Private Fields
        //
        //----------------------------------------------
        #region Private Fields

        private SizeToContent _sizeToContent;
        private WindowManager _windowManager;

        #endregion Private Fields
    }
}


