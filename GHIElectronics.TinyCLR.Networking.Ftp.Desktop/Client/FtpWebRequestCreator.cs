extern alias ours;
using WebRequest = ours::System.Net.WebRequest;
using IWebRequestCreate = ours::System.Net.IWebRequestCreate;

namespace System.Net {
    /// <summary>
    /// Creator for ftp webrequest. Registers itself with our WebRequest's
    /// prefix table (not BCL's) so that user code calling our
    /// WebRequest.Create("ftp://...") on Desktop dispatches into our FtpWebRequest.
    /// </summary>
    internal class FTPWebRequestCreator : IWebRequestCreate {
        FTPWebRequestCreator() {
        }

        static FTPWebRequestCreator() => WebRequest.RegisterPrefix("ftp:", new FTPWebRequestCreator());

        public static void Register() {
        }

        public WebRequest Create(Uri uri) => new FtpWebRequest(uri);
    }
}
