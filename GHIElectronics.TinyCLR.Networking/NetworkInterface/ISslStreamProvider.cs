using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace GHIElectronics.TinyCLR.Net.NetworkInterface {
    public interface ISslStreamProvider {
        int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols);
        int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols);
        void Close(int handle);

        int Read(int handle, byte[] buffer, int offset, int count, int timeout);
        int Write(int handle, byte[] buffer, int offset, int count, int timeout);

        int Available(int handle);
    }
}
