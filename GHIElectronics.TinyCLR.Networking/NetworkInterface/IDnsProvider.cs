namespace System.Net.NetworkInterface {
    public interface IDnsProvider {
        void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);
    }
}
