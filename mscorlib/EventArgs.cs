using System.Runtime.InteropServices;

namespace System {
    [Serializable]
    [ComVisible(true)]
    public class EventArgs {
        public static readonly EventArgs Empty = new EventArgs();
    }
}
