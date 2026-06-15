////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.Devices.Display;

using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>Represents a method that performs custom drawing after the window tree has been rendered.</summary>
    public delegate void PostRenderEventHandler(DrawingContext dc);

    /// <summary>The root container that hosts all windows and drives rendering for a display.</summary>
    public class WindowManager : Controls.Canvas {
        /// <summary>Gets the display controller this window manager renders to.</summary>
        public DisplayController DisplayController { get; }

        private WindowManager(DisplayController displayController) {

            this.DisplayController = displayController ?? DisplayController.FromProvider(null);

            //
            // initially measure and arrange ourselves.
            //
            Instance = this;

            //
            // WindowManagers have no parents but they are Visible.
            //
            this._flags = this._flags | Flags.IsVisibleCache;

            Measure(Media.Constants.MaxExtent, Media.Constants.MaxExtent);
            GetDesiredSize(out var desiredWidth, out var desiredHeight);

            Arrange(0, 0, desiredWidth, desiredHeight);
        }

        internal static WindowManager EnsureInstance(DisplayController displayController) {
            if (Instance == null) {
                var wm = new WindowManager(displayController);
                // implicitly the window manager is responsible for posting renders
                wm._flags |= Flags.ShouldPostRender;
            }

            return Instance;
        }

        /// <summary>Measures the window manager to the size of the active display configuration.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            base.MeasureOverride(availableWidth, availableHeight, out desiredWidth, out desiredHeight);
            desiredWidth = (int)this.DisplayController.ActiveConfiguration.Width;
            desiredHeight = (int)this.DisplayController.ActiveConfiguration.Height;
        }

        internal void SetTopMost(Window window) {
            var children = this.LogicalChildren;

            if (!IsTopMost(window)) {
                children.Remove(window);
                children.Add(window);
            }
        }

        internal bool IsTopMost(Window window) {
            var index = this.LogicalChildren.IndexOf(window);
            return (index >= 0 && index == this.LogicalChildren.Count - 1);
        }

        //
        // this was added for aux, behavior needs to change for watch.
        //
        /// <summary>Updates focus and touch capture when a window is added to or removed from the manager.</summary>
        protected internal override void OnChildrenChanged(UIElement added, UIElement removed, int indexAffected) {
            base.OnChildrenChanged(added, removed, indexAffected);

            var children = this.LogicalChildren;
            var last = children.Count - 1;

            // something was added, and it's the topmost. Make sure it is visible before setting focus
            if (added != null && indexAffected == last && Visibility.Visible == added.Visibility) {
                Input.Buttons.Focus(added);
                Input.TouchCapture.Capture(added);
            }

            // something was removed and it lost focus to us.
            if (removed != null && this.IsFocused) {
                // we still have a window left, so make it focused.
                if (last >= 0) {
                    Input.Buttons.Focus(children[last]);
                    Input.TouchCapture.Capture(children[last]);
                }
            }
        }

        //--//

        /// <summary>The singleton window manager instance for the application.</summary>
        public static WindowManager Instance;

        //--//

        private PostRenderEventHandler _postRenderHandler;

        /// <summary>Occurs after the window tree has been rendered, allowing custom overlay drawing.</summary>
        public event PostRenderEventHandler PostRender {
            add {
                this._postRenderHandler += value;
            }

            remove {
                this._postRenderHandler -= value;
            }
        }

        /// <summary>Renders the window tree and then raises the <see cref="PostRender"/> event.</summary>
        protected internal override void RenderRecursive(DrawingContext dc) {
            base.RenderRecursive(dc);

            this._postRenderHandler?.Invoke(dc);
        }
    }

}


