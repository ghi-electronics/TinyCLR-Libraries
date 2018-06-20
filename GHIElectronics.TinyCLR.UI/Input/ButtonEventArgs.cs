////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    ///     The ButtonEventArgs class contains information about button states.
    /// </summary>
    /// <ExternalAPI/>
    public class ButtonEventArgs : InputEventArgs {
        /// <summary>
        ///     Constructs an instance of the ButtonEventArgs class.
        /// </summary>
        /// <param name="buttonDevice">
        ///     The button device associated with this event.
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured. (machine time)
        /// </param>
        /// <param name="button">
        ///     The button referenced by the event.
        /// </param>
        public ButtonEventArgs(ButtonDevice buttonDevice, PresentationSource inputSource, DateTime timestamp, HardwareButton button)
            : base(buttonDevice, timestamp) {
            this.InputSource = inputSource;
            this.Button = button;
        }

        /// <summary>
        ///     The Button referenced by the event.
        /// </summary>
        public readonly HardwareButton Button;

        /// <summary>
        ///     The state of the button referenced by the event.
        /// </summary>
        public ButtonState ButtonState => ((ButtonDevice)this.Device).GetButtonState(this.Button);

        /// <summary>
        /// The source for this button
        /// </summary>
        public readonly PresentationSource InputSource;

        /// <summary>
        ///     Whether the button pressed is a repeated button or not.
        /// </summary>
        public bool IsRepeat => this._isRepeat;

        internal bool _isRepeat;
    }
}


