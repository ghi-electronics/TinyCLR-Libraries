// Matches System.Net.Sockets.SocketShutdown in full .NET. Values match
// Berkeley sockets / lwIP shutdown(2) constants.
namespace System.Net.Sockets {
    public enum SocketShutdown {
        Receive = 0,
        Send = 1,
        Both = 2,
    }
}
