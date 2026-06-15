using System.IO;

namespace System.Net
{
    /// <summary>
    /// Only contains a stream to write data
    /// </summary>
    public class FtpWebResponse : WebResponse
    {
        private Stream m_ResponseStream = null;
        //private FtpWebRequest m_Request = null;

        internal FtpWebResponse()
        {
        }

        internal FtpWebResponse(Stream stream)
        {
            m_ResponseStream = stream;
        }

        /// <summary>Returns the stream used to read the response data.</summary>
        public override Stream GetResponseStream()
        {
            return m_ResponseStream;
        }

        /// <summary>Closes the response and releases its stream.</summary>
        public override void Close()
        {
            if (m_ResponseStream != null)
            {
                m_ResponseStream.Close();
            }
            //m_Request.Close();
        }

    }
}
