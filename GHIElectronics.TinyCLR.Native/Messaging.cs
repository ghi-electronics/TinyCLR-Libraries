////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Native.Messaging
{
    public sealed class EndPoint
    {
        [FieldNoReflection]
#pragma warning disable CS0169 // The field is never used
        private object m_handle;
#pragma warning restore CS0169 // The field is never used

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public EndPoint(Type selector, uint id);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Check(Type selector, uint id, int timeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Message GetMessage(int timeout);
        //--//
        
        public object SendMessage(Type selector, uint id, int timeout, object payload)
        {
            byte[] res = SendMessageRaw(selector, id, timeout, Reflection.Serialize(payload, null));

            if (res == null) return null;

            return Reflection.Deserialize(res, null);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public byte[] SendMessageRaw(Type selector, uint id, int timeout, byte[] payload);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern internal void ReplyRaw(Message msg, byte[] data);
        //--//
    }

    public sealed class Message
    {
        [Serializable()]
        public class RemotedException
        {
            public string m_message;

            public RemotedException(Exception payload)
            {
                m_message = payload.Message;
            }

            public void Raise()
            {

                throw new Exception(m_message);
            }
        }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        EndPoint m_source;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0169 // The field is never used
        Type m_selector;
        uint m_id;
        uint m_seq;
#pragma warning restore CS0169 // The field is never used
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        byte[] m_payload;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        //--//

        public object Payload
        {
            get
            {
                return Reflection.Deserialize(m_payload, null);
            }
        }

        public byte[] PayloadRaw
        {
            get
            {
                return m_payload;
            }
        }

        //--//

        public void Reply(object data)
        {
            m_source.ReplyRaw(this, Reflection.Serialize(data, null));
        }

        public void ReplyRaw(byte[] data)
        {
            m_source.ReplyRaw(this, data);
        }
    }
}


