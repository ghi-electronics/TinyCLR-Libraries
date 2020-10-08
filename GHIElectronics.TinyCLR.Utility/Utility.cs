
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Utility
{
    public static class Utility {        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint ExtractValueFromArray(byte[] data, int pos, int size);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void InsertValueIntoArray(byte[] data, int pos, int size, uint val);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] ExtractRangeFromArray(byte[] data, int offset, int count);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] CombineArrays(byte[] src1, byte[] src2);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public byte[] CombineArrays(byte[] src1, int offset1, int count1, byte[] src2, int offset2, int count2);        
    }
}
