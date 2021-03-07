using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Data.Json {
    public class JsonSerializerSettings
    {
        public TypeNameHandling TypeNameHandling { get; set; } = TypeNameHandling.None;
    }
}
