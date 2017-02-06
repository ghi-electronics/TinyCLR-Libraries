using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Internal
{
    internal class I2CDevice : IDisposable
    {
        public class I2CTransaction
        {
            public readonly byte[] Buffer;

            //--//

            protected I2CTransaction(byte[] buffer) => this.Buffer = buffer ?? throw new ArgumentNullException();
        }

        //--//

        sealed public class I2CReadTransaction : I2CTransaction
        {
            internal I2CReadTransaction(byte[] buffer)
                : base(buffer)
            {
            }
        }

        //--//

        sealed public class I2CWriteTransaction : I2CTransaction
        {
            internal I2CWriteTransaction(byte[] buffer)
                : base(buffer)
            {
            }
        }

        //--//

        public class Configuration
        {
            public readonly ushort Address;
            public readonly int ClockRateKhz;

            //--//

            public Configuration(ushort address, int clockRateKhz)
            {
                this.Address = address;
                this.ClockRateKhz = clockRateKhz;
            }
        }

        //--//

#pragma warning disable CS0169 // The field is never used
        private object m_xAction;
#pragma warning restore CS0169 // The field is never used

        //--//

        public Configuration Config;

        //--//

        protected bool m_disposed;

        //--//

        public I2CDevice(Configuration config)
        {
            this.Config = config;

            var hwProvider = HardwareProvider.HwProvider;

            if (hwProvider != null)
            {

                hwProvider.GetI2CPins(out var scl, out var sda);

                if (scl != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(scl, true);
                }

                if (sda != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(sda, true);
                }
            }

            Initialize();

            this.m_disposed = false;
        }

        ~I2CDevice()
        {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void Dispose(bool fDisposing)
        {
            if (!this.m_disposed)
            {
                try
                {
                    var hwProvider = HardwareProvider.HwProvider;

                    if (hwProvider != null)
                    {

                        hwProvider.GetI2CPins(out var scl, out var sda);

                        if (scl != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(scl, false);
                        }

                        if (sda != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(sda, false);
                        }
                    }
                }
                finally
                {
                    this.m_disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public static I2CReadTransaction CreateReadTransaction(byte[] buffer) => new I2CReadTransaction(buffer);

        public static I2CWriteTransaction CreateWriteTransaction(byte[] buffer) => new I2CWriteTransaction(buffer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public int Execute(I2CTransaction[] xActions, int timeout);

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void Initialize();
    }

    internal class Port
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool ReservePin(Cpu.Pin pin, bool fReserve);
    }

    internal class HardwareProvider
    {
        private static HardwareProvider s_hwProvider = null;

        public static HardwareProvider HwProvider {
            get {
                if (s_hwProvider == null)
                {
                    s_hwProvider = new HardwareProvider();
                }

                return s_hwProvider;
            }
        }

        public virtual void GetI2CPins(out Cpu.Pin scl, out Cpu.Pin sda)
        {
            scl = Cpu.Pin.GPIO_NONE;
            sda = Cpu.Pin.GPIO_NONE;

            NativeGetI2CPins(out scl, out sda);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeGetI2CPins(out Cpu.Pin scl, out Cpu.Pin sda);
    }

    internal static class Cpu
    {
        public enum Pin : int
        {
            GPIO_NONE = -1,
        }
    }

    internal interface IEventProcessor
    {
        BaseEvent ProcessEvent(uint data1, uint data2, DateTime time);
    }

    internal interface IEventListener
    {
        void InitializeForEventSource();
        bool OnEvent(BaseEvent ev);
    }

    internal class BaseEvent
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public ushort Source;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        public byte EventMessage;
    }

    internal class GenericEvent : BaseEvent
    {
        public byte EventCategory;
        public uint EventData;
        public int X;
        public int Y;
        public DateTime Time;
    }

    internal enum EventCategory
    {
        Gpio = 9,
    }

    internal delegate void NativeEventHandler(uint data1, uint data2, DateTime time);

    internal class EventSink : NativeEventDispatcher
    {
        private class EventInfo
        {
            public EventInfo()
            {
                this.EventListener = null;
                this.EventFilter = null;
            }

            public IEventListener EventListener;
            public IEventListener EventFilter;
            public IEventProcessor EventProcessor;
            public EventCategory Category;
        }

        static EventSink()
        {
            _eventSink = new EventSink();
            _eventSink.OnInterrupt += new NativeEventHandler(_eventSink.EventDispatchCallback);
        }

        // Pass the name to the base so it connects to driver
        private EventSink()
            : base("EventSink", 0)
        {
        }

        private void ProcessEvent(EventInfo eventInfo, BaseEvent ev)
        {
            if (eventInfo == null)
                return;

            if (eventInfo.EventFilter != null)
            {
                if (!eventInfo.EventFilter.OnEvent(ev))
                    return;
            }

            if (eventInfo.EventListener != null)
            {
                eventInfo.EventListener.OnEvent(ev);
            }
        }

        private void EventDispatchCallback(uint data1, uint data2, DateTime time)
        {
            EventInfo eventInfo = null;
            BaseEvent ev = null;

            GetEvent(data1, data2, time, ref eventInfo, ref ev);

            ProcessEvent(eventInfo, ev);
        }

        ///

        /// Add/RemoveEventFilter/Listener/Processor today supports only one listener and one filter
        /// to reduce complexity, but this will certainly be not the case in future when
        /// multiple parties will want to listent or filter same EventCategory. This was
        /// one of the request from SideShow team, we will have to look into that.
        ///

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void AddEventListener(EventCategory eventCategory, IEventListener eventListener)
        {
            var eventInfo = GetEventInfo(eventCategory);
            eventInfo.EventListener = eventListener;
            eventListener.InitializeForEventSource();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void AddEventProcessor(EventCategory eventCategory, IEventProcessor eventProcessor)
        {
            var eventInfo = GetEventInfo(eventCategory);
            eventInfo.EventProcessor = eventProcessor;
        }

        private static EventInfo GetEventInfo(EventCategory category)
        {

            /// What we need here is hashtable. Until we get one, we have this implementation where we browse through
            /// registered eventInfos and attempt retrieving matchine one.
            ///

            EventInfo eventInfo = null;
            for (var i = 0; i < _eventInfoTable.Count; i++)
            {
                if (((EventInfo)_eventInfoTable[i]).Category == category)
                {
                    eventInfo = (EventInfo)_eventInfoTable[i];
                    break;
                }
            }

            if (eventInfo == null)
            {
                eventInfo = new EventInfo() {
                    Category = category
                };
                _eventInfoTable.Add(eventInfo);
            }

            return eventInfo;
        }

        private static void GetEvent(uint data1, uint data2, DateTime time, ref EventInfo eventInfo, ref BaseEvent ev)
        {
            var category = (byte)((data1 >> 8) & 0xFF);

            eventInfo = GetEventInfo((EventCategory)category);
            if (eventInfo.EventProcessor != null)
            {
                ev = eventInfo.EventProcessor.ProcessEvent(data1, data2, time);
            }
            else
            {
                var genericEvent = new GenericEvent() {
                    Y = (int)(data2 & 0xFFFF),
                    X = (int)((data2 >> 16) & 0xFFFF),
                    Time = time,
                    EventMessage = (byte)(data1 & 0xFF),
                    EventCategory = category,
                    EventData = (data1 >> 16) & 0xFFFF
                };

                ev = genericEvent;
            }
        }

        private static EventSink _eventSink = null;
        private static ArrayList _eventInfoTable = new ArrayList();
    }

    internal class NativeEventDispatcher : IDisposable
    {
        protected NativeEventHandler m_threadSpawn = null;
        protected NativeEventHandler m_callbacks = null;
        protected bool m_disposed = false;
        private object m_NativeEventDispatcher;

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public NativeEventDispatcher(string strDriverName, ulong drvData);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void EnableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void DisableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected virtual void Dispose(bool disposing);

        //--//

        ~NativeEventDispatcher()
        {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public virtual void Dispose()
        {
            if (!this.m_disposed)
            {
                Dispose(true);

                GC.SuppressFinalize(this);

                this.m_disposed = true;
            }
        }

        public event NativeEventHandler OnInterrupt {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                NativeEventHandler callbacksOld = this.m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    this.m_callbacks = callbacksNew;

                    if (callbacksNew != null)
                    {
                        if (callbacksOld == null)
                        {
                            EnableInterrupt();
                        }

                        if (callbacksNew.Equals(value) == false)
                        {
                            callbacksNew = new NativeEventHandler(this.MultiCastCase);
                        }
                    }

                    this.m_threadSpawn = callbacksNew;
                }
                catch
                {
                    this.m_callbacks = callbacksOld;

                    if (callbacksOld == null)
                    {
                        DisableInterrupt();
                    }

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove {
                if (this.m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                NativeEventHandler callbacksOld = this.m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    this.m_callbacks = (NativeEventHandler)callbacksNew;

                    if (callbacksNew == null && callbacksOld != null)
                    {
                        DisableInterrupt();
                    }
                }
                catch
                {
                    this.m_callbacks = callbacksOld;

                    throw;
                }
            }
        }

        private void MultiCastCase(uint port, uint state, DateTime time) => this.m_callbacks?.Invoke(port, state, time);
    }
}
