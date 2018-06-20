namespace GHIElectronics.TinyCLR.UI {
    // An item in the EventRoute
    //
    // RouteItem constitutes
    // the target object and
    // list of RoutedEventHandlerInfo that need
    // to be invoked upon the target object
    internal class RouteItem {
        #region Construction

        // Constructor for RouteItem
        internal RouteItem(object target, RoutedEventHandlerInfo routedEventHandlerInfo) {
            this._target = target;
            this._routedEventHandlerInfo = routedEventHandlerInfo;
        }

        #endregion Construction

        #region Operations

        // Returns target
        internal object Target => this._target;

        /// <summary>
        ///     Is the given object equals the current
        /// </summary>
        public override bool Equals(object o) => Equals((RouteItem)o);

        /// <summary>
        ///     Is the given RouteItem equals the current
        /// </summary>
        public bool Equals(RouteItem routeItem) => (
                routeItem._target == this._target &&
                routeItem._routedEventHandlerInfo == this._routedEventHandlerInfo);

        /// <summary>
        ///     Serves as a hash function for a particular type, suitable for use in
        ///     hashing algorithms and data structures like a hash table
        /// </summary>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        ///     Equals operator overload
        /// </summary>
        public static bool operator ==(RouteItem routeItem1, RouteItem routeItem2) => routeItem1.Equals(routeItem2);

        /// <summary>
        ///     NotEquals operator overload
        /// </summary>
        public static bool operator !=(RouteItem routeItem1, RouteItem routeItem2) => !routeItem1.Equals(routeItem2);

        #endregion Operations

        #region Data

        internal object _target;
        internal RoutedEventHandlerInfo _routedEventHandlerInfo;

        #endregion Data
    }
}


