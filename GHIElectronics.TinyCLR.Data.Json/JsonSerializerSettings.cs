using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Data.Json {
    /// <summary>Settings that control how objects are serialized to JSON tokens.</summary>
    public class JsonSerializerSettings
    {
        /// <summary>Gets or sets how type names are emitted for serialized objects.</summary>
        public TypeNameHandling TypeNameHandling { get; set; } = TypeNameHandling.None;
    }
}
