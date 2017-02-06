using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native
{
    public static class Math
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public int Cos(int angle);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public int Sin(int angle);
    }
}


