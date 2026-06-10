////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    ///     The RawTouchInputReport class encapsulates the raw input
    ///     provided from a multitouch source.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public class RawTouchInputReport : InputReport {
        /// <summary>
        ///     Constructs an instance of the RawKeyboardInputReport class.
        /// </summary>
        /// <param name="inputSource">
        ///     source of the input
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        public RawTouchInputReport(PresentationSource inputSource, DateTime timestamp, byte eventMessage, TouchInput[] touches)
            : base(inputSource, timestamp) {
            this.EventMessage = eventMessage;
            this.Touches = touches;
        }

        /// <summary>Constructs an instance of the RawTouchInputReport class targeting a specific element.</summary>
        public RawTouchInputReport(PresentationSource inputSource,
                    DateTime timestamp, byte eventMessage, TouchInput[] touches, UIElement destTarget)
            : base(inputSource, timestamp) {
            this.EventMessage = eventMessage;
            this.Touches = touches;
            this.Target = destTarget;
        }

        /// <summary>Read-only access to the element this report is directed at, or null.</summary>
        public readonly UIElement Target;
        /// <summary>Read-only access to the touch message code.</summary>
        public readonly byte EventMessage;
        /// <summary>Read-only access to the touch points reported.</summary>
        public readonly TouchInput[] Touches;
    }

    /// <summary>Describes the raw actions reported for a touch.</summary>
    public enum RawTouchActions {
        /// <summary>A touch press occurred.</summary>
        TouchDown = 0x01,
        /// <summary>A touch release occurred.</summary>
        TouchUp = 0x02,
        /// <summary>The touch input source became active.</summary>
        Activate = 0x04,
        /// <summary>The touch input source became inactive.</summary>
        Deactivate = 0x08,
        /// <summary>A touch move occurred.</summary>
        TouchMove = 0x10,
    }
}


