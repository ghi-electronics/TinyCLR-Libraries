using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Data.Json
{
    /// <summary>Options that control how JSON text is formatted during serialization.</summary>
    public class JsonSerializationOptions
    {
        /// <summary>Gets or sets whether the JSON output is indented and line-wrapped.</summary>
        public bool Indented { get; set; } = true;
    }
}
