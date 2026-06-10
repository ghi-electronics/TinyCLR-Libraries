using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Data.Json {
    /// <summary>Controls whether type names are written into the JSON for serialized objects.</summary>
    public enum TypeNameHandling
    {
        /// <summary>Do not write type names.</summary>
        None = 0,
        /// <summary>Write type names for objects.</summary>
        Objects = 1,
        // Not supported yet...
        //Arrays = 2,
        //All = 3,
        /// <summary>Write type names only when the value's type differs from the declared type.</summary>
        Auto = 4,
    }
}
