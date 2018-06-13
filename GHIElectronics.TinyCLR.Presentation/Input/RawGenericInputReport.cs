////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    ///     The RawGenericInputReport class encapsulates the raw input
    ///     provided from a keyboard.
    /// </summary>
    /// <remarks>
    ///     It is important to note that the InputReport class only contains
    ///     blittable types.  This is required so that the report can be
    ///     marshalled across application domains.
    /// </remarks>
    public class RawGenericInputReport : InputReport {
        /// <summary>
        ///     Constructs an instance of the RawKeyboardInputReport class.
        /// </summary>
        /// <param name="inputSource">
        ///     source of the input
        /// </param>
        /// <param name="timestamp">
        ///     The time when the input occured.
        /// </param>
        public RawGenericInputReport(PresentationSource inputSource, GenericEvent genericEvent)
            : base(inputSource, genericEvent.Time) {
            this.InternalEvent = genericEvent;
            this.Target = null;
        }

        public RawGenericInputReport(PresentationSource inputSource,
                        GenericEvent genericEvent, UIElement destTarget)
            : base(inputSource, genericEvent.Time) {
            this.InternalEvent = genericEvent;
            this.Target = destTarget;
        }

        public readonly UIElement Target;

        public readonly GenericEvent InternalEvent;
    }
}


