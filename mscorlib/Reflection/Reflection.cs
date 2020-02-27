using System.Runtime.CompilerServices;

namespace System.Reflection {
    ////////////////////////////////////////////////////////////////////////////////////////////////
    public static class Reflection
    {        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] Serialize(object o, Type t);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public object Deserialize(byte[] v, Type t);
    }
}


