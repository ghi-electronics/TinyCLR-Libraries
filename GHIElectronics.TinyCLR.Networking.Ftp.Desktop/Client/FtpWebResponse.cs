extern alias ours;
using System.IO;
using WebResponse = ours::System.Net.WebResponse;

namespace System.Net {
    public class FtpWebResponse : WebResponse {
        private Stream m_ResponseStream = null;

        internal FtpWebResponse() {
        }

        internal FtpWebResponse(Stream stream) => m_ResponseStream = stream;

        public override Stream GetResponseStream() => m_ResponseStream;

        public override void Close() {
            if (m_ResponseStream != null) {
                m_ResponseStream.Close();
            }
        }
    }
}
