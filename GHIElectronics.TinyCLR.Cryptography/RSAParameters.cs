using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    [Serializable]
    public struct RSAParameters {
        public byte[] Exponent;
        public byte[] Modulus;

        [NonSerialized] public byte[] P;
        [NonSerialized] public byte[] Q;
        [NonSerialized] public byte[] DP;
        [NonSerialized] public byte[] DQ;
        [NonSerialized] public byte[] InverseQ;
        [NonSerialized] public byte[] D;

    }
}
