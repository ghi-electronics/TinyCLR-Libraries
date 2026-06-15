// Matches System.Net.Sockets.SocketShutdown in full .NET. Values match
// Berkeley sockets / lwIP shutdown(2) constants.
namespace System.Net.Sockets {
    /// <summary>Specifies which socket operations to disable when shutting down a socket.</summary>
    public enum SocketShutdown {
        /// <summary>Disables receiving on the socket.</summary>
        Receive = 0,
        /// <summary>Disables sending on the socket.</summary>
        Send = 1,
        /// <summary>Disables both sending and receiving on the socket.</summary>
        Both = 2,
    }
}
