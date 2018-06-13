////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.SPOT.Input;

namespace Microsoft.SPOT.Presentation {
    /// <summary>
    /// Presentation source is our connection to the rest of the managed system.
    ///
    /// </summary>
    public class PresentationSource : DispatcherObject {
        //------------------------------------------------------
        //
        // Constructors
        //
        //------------------------------------------------------

        #region Constructors
        /// <summary>
        ///     Constructs an instance of the PresentationSource object.
        /// </summary>
        public PresentationSource() {
        }

        #endregion

        /// <summary>
        /// The Root UIElement for this source.
        /// </summary>
        public UIElement RootUIElement {
            get => this._rootUIElement;

            set {
                VerifyAccess();

                if (this._rootUIElement != value) {
                    var oldRoot = this._rootUIElement;

                    this._rootUIElement = value;

                    if (value != null) {
                        /*  need layout events
                          _rootUIElement.LayoutUpdated += new EventHandler(OnLayoutUpdated);
                        */
                    }

                    if (oldRoot != null) {
                        /* we need layout events
                        oldRoot.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
                        */
                    }

                    /* we need to generate an event here
                    RootChanged(oldRoot, value);
                    */

                    // set up the size.
                    value.Measure(Media.Constants.MaxExtent, Media.Constants.MaxExtent);
                    value.GetDesiredSize(out var desiredWidth, out var desiredHeight);
                    value.Arrange(0, 0, desiredWidth, desiredHeight);

                    // update focus.
                    Buttons.Focus(value);
                }
            }
        }

        private UIElement _rootUIElement;
    }
}


