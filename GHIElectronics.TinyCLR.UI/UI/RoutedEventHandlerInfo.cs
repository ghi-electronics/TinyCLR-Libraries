////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI {

    /// <summary>
    ///     RoutedEventHandler Definition
    /// </summary>
    /// <ExternalAPI/>
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    /// <summary>
    ///     Container for handler instance and other
    ///     invocation preferences for this handler
    ///     instance
    /// </summary>
    /// <remarks>
    ///     RoutedEventHandlerInfo constitutes the
    ///     handler instance and flag that indicates if
    ///     or not this handler must be invoked for
    ///     already handled events <para/>
    ///     <para/>
    ///
    ///     This class needs to be public because it is
    ///     used by ContentElement in the Framework
    ///     to store Instance EventHandlers
    /// </remarks>
    public class RoutedEventHandlerInfo {
        #region Construction

        /// <summary>
        ///     Construtor for RoutedEventHandlerInfo
        /// </summary>
        /// <param name="handler">
        ///     Non-null handler
        /// </param>
        /// <param name="handledEventsToo">
        ///     Flag that indicates if or not the handler must
        ///     be invoked for already handled events
        /// </param>
        internal RoutedEventHandlerInfo(RoutedEventHandler handler, bool handledEventsToo) {
            this._handler = handler;
            this._handledEventsToo = handledEventsToo;
        }

        #endregion Construction

        #region Operations

        /// <summary>
        ///     Returns associated handler instance
        /// </summary>
        public RoutedEventHandler Handler => this._handler;

        /// <summary>
        ///     Returns HandledEventsToo Flag
        /// </summary>
        public bool InvokeHandledEventsToo => this._handledEventsToo;

        /// <summary>
        ///     Is the given object equivalent to the current one
        /// </summary>
        public override bool Equals(object obj) {
            var tmp = obj as RoutedEventHandlerInfo;

            if (tmp == null)
                return false;

            return Equals(tmp);
        }

        /// <summary>
        ///     Is the given RoutedEventHandlerInfo equals the current
        /// </summary>
        public bool Equals(RoutedEventHandlerInfo handlerInfo) => this._handler == handlerInfo._handler && this._handledEventsToo == handlerInfo._handledEventsToo;

        /// <summary>
        ///     Serves as a hash function for a particular type, suitable for use in
        ///     hashing algorithms and data structures like a hash table
        /// </summary>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        ///     Equals operator overload
        /// </summary>
        public static bool operator ==(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2) => handlerInfo1.Equals(handlerInfo2);

        /// <summary>
        ///     NotEquals operator overload
        /// </summary>
        public static bool operator !=(RoutedEventHandlerInfo handlerInfo1, RoutedEventHandlerInfo handlerInfo2) => !handlerInfo1.Equals(handlerInfo2);

        #endregion Operations

        #region Data

        internal RoutedEventHandler _handler;
        internal bool _handledEventsToo;

        #endregion Data
    }
}


