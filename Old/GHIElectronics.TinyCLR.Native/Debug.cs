using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native
{
    public static class Debug
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void EnableGCMessages(bool enable);
    }
}


