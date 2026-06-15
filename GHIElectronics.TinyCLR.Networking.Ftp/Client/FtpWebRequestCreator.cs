
namespace System.Net
{
    /// <summary>
    /// Creator for ftp webrequest
    /// </summary>
    internal class FTPWebRequestCreator : IWebRequestCreate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        FTPWebRequestCreator()
        {
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static FTPWebRequestCreator() => WebRequest.RegisterPrefix("ftp:", new FTPWebRequestCreator());

        // Reflection entry point invoked from Networking.Http's WebRequest static
        // ctor to trigger our type initializer (which self-registers above) without
        // requiring System.Activator (absent on TinyCLR).
        public static void Register() {
        }

        #region IWebRequestCreate Members

        public WebRequest Create(Uri uri) => new FtpWebRequest(uri);

        #endregion
    }
}
