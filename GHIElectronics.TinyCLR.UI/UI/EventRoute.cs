////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.UI {
    /// <summary>
    ///     Container for the route to be followed
    ///     by a RoutedEvent when raised
    /// </summary>
    /// <remarks>
    ///     EventRoute constitues <para/>
    ///     a non-null <see cref="RoutedEvent"/>
    ///     and <para/>
    ///     an ordered list of (target object, handler list)
    ///     pairs <para/>
    ///     <para/>
    ///
    ///     It facilitates adding new entries to this list
    ///     and also allows for the handlers in the list
    ///     to be invoked
    /// </remarks>
    public sealed class EventRoute {
        #region Construction

        /// <summary>
        ///     Constructor for <see cref="EventRoute"/> given
        ///     the associated <see cref="RoutedEvent"/>
        /// </summary>
        /// <param name="routedEvent">
        ///     Non-null <see cref="RoutedEvent"/> to be associated with
        ///     this <see cref="EventRoute"/>
        /// </param>
        public EventRoute(RoutedEvent routedEvent) {
            this.RoutedEvent = routedEvent ?? throw new ArgumentNullException("routedEvent");
            this._routeItemList = new ArrayList();
        }

        #endregion Construction

        #region External API

        /// <summary>
        ///     Adds this handler for the
        ///     specified target to the route
        /// </summary>
        /// <remarks>
        ///     NOTE: It is not an error to add a
        ///     handler for a particular target instance
        ///     twice (handler will simply be called twice).
        /// </remarks>
        /// <param name="target">
        ///     Target object whose handler is to be
        ///     added to the route
        /// </param>
        /// <param name="handler">
        ///     Handler to be added to the route
        /// </param>
        /// <param name="handledEventsToo">
        ///     Flag indicating whether or not the listener wants to
        ///     hear about events that have already been handled
        /// </param>
        // need to consider creating an AddImpl, and consider if we even need to
        // make this public?
        public void Add(object target, RoutedEventHandler handler, bool handledEventsToo) {
            if (target == null) {
                throw new ArgumentNullException("target");
            }

            if (handler == null) {
                throw new ArgumentNullException("handler");
            }

            var routeItem = new RouteItem(target, new RoutedEventHandlerInfo(handler, handledEventsToo));

            this._routeItemList.Add(routeItem);
        }

        /// <summary>
        ///     Invokes all the handlers that have been
        ///     added to the route
        /// </summary>
        /// <remarks>
        ///     NOTE: If the <see cref="RoutingStrategy"/>
        ///     of the associated <see cref="RoutedEvent"/>
        ///     is <see cref="RoutingStrategy.Bubble"/>
        ///     the last handlers added are the
        ///     last ones invoked <para/>
        ///     However if the <see cref="RoutingStrategy"/>
        ///     of the associated <see cref="RoutedEvent"/>
        ///     is <see cref="RoutingStrategy.Tunnel"/>,
        ///     the last handlers added are the
        ///     first ones invoked.
        ///     However the handlers for a particular object
        ///     are always invoked in the order they were added
        ///     regardless of whether its a tunnel or buble.
        ///
        /// </remarks>
        /// <param name="source">
        ///     <see cref="RoutedEventArgs.Source"/>
        ///     that raised the RoutedEvent
        /// </param>
        /// <param name="args">
        ///     <see cref="RoutedEventArgs"/> that carry
        ///     all the details specific to this RoutedEvent
        /// </param>
        internal void InvokeHandlers(object source, RoutedEventArgs args) {
            // Check RoutingStrategy to know the order of invocation
            if (args.RoutedEvent.RoutingStrategy == RoutingStrategy.Bubble ||
                args.RoutedEvent.RoutingStrategy == RoutingStrategy.Direct) {

                // If the RoutingStrategy of the associated is
                // Bubble the handlers for the last target
                // added are the last ones invoked
                // Invoke class listeners
                for (int i = 0, count = this._routeItemList.Count; i < count; i++) {
                    var ri = ((RouteItem)this._routeItemList[i]);
                    args.InvokeHandler(ri);
                }
            }
            else {
                var endTargetIndex = this._routeItemList.Count - 1;
                int startTargetIndex;

                // If the RoutingStrategy of the associated is
                // Tunnel the handlers for the last target
                // added are the first ones invoked
                while (endTargetIndex >= 0) {
                    // For tunnel events we need to invoke handlers for the last target first.
                    // However the handlers for that individual target must be fired in the right order.
                    var currTarget = ((RouteItem)this._routeItemList[endTargetIndex]).Target;
                    for (startTargetIndex = endTargetIndex; startTargetIndex >= 0; startTargetIndex--) {
                        if (((RouteItem)this._routeItemList[startTargetIndex]).Target != currTarget) {
                            if (startTargetIndex == endTargetIndex && endTargetIndex > 0) {
                                endTargetIndex--;
                            }
                            else {
                                break;
                            }
                        }
                    }

                    for (var i = startTargetIndex + 1; i <= endTargetIndex; i++) {
                        var ri = ((RouteItem)this._routeItemList[i]);
                        args.InvokeHandler(ri);
                    }

                    endTargetIndex = startTargetIndex;
                }
            }
        }

        #endregion External API

        #region Operations

        // Return associated RoutedEvent
        internal RoutedEvent RoutedEvent;

        #endregion Operations

        #region Data

        // Stores the routed event handlers to be
        // invoked for the associated RoutedEvent
        private ArrayList _routeItemList;

        #endregion Data
    }
}


