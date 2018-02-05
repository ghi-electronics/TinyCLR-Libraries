////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Net.NetworkInformation
{
    public class NetworkAvailabilityEventArgs : EventArgs
    {
        private bool _isAvailable;

        internal NetworkAvailabilityEventArgs(bool isAvailable) => this._isAvailable = isAvailable;

        public bool IsAvailable => this._isAvailable;
    }

    public delegate void NetworkAvailabilityChangedEventHandler(object sender, NetworkAvailabilityEventArgs e);
    public delegate void NetworkAddressChangedEventHandler(object sender, EventArgs e);

    public static class NetworkChange
    {
        [Flags]
        internal enum NetworkEventType : byte
        {
            Invalid = 0,
            AvailabilityChanged = 1,
            AddressChanged = 2,
        }

        [Flags]
        internal enum NetworkEventFlags : byte
        {
            NetworkAvailable = 0x1,
        }

        internal class NetworkEvent : BaseEvent
        {
            public NetworkEventType EventType;
            public byte Flags;
            public DateTime Time;
        }

        internal class NetworkChangeListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                var networkEvent = new NetworkEvent {
                    EventType = (NetworkEventType)(data1 & 0xFF),
                    Flags = (byte)((data1 >> 16) & 0xFF),
                    Time = time
                };

                return networkEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is NetworkEvent)
                {
                    NetworkChange.OnNetworkChangeCallback((NetworkEvent)ev);
                }

                return true;
            }
        }

        /// Events
        public static event NetworkAddressChangedEventHandler NetworkAddressChanged;
        public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;

        static NetworkChange()
        {
            var networkChangeListener = new NetworkChangeListener();
            Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.Network, networkChangeListener);
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.Network, networkChangeListener);
        }

        internal static void OnNetworkChangeCallback(NetworkEvent networkEvent)
        {
            switch (networkEvent.EventType)
            {
                case NetworkEventType.AvailabilityChanged:
                    {
                        if (NetworkAvailabilityChanged != null)
                        {
                            var isAvailable = ((networkEvent.Flags & (byte)NetworkEventFlags.NetworkAvailable) != 0);
                            var args = new NetworkAvailabilityEventArgs(isAvailable);

                            NetworkAvailabilityChanged(null, args);
                        }
                        break;
                    }
                case NetworkEventType.AddressChanged:
                    {
                        if (NetworkAddressChanged != null)
                        {
                            var args = new EventArgs();
                            NetworkAddressChanged(null, args);
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}


