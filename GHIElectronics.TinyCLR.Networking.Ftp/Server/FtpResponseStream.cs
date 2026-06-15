using System.IO;
using System.Net.Sockets;
using System.Text;
using GHIElectronics.TinyCLR.Networking.Ftp;
using System.Net;
using System;

namespace GHIElectronics.TinyCLR.Networking
{
    /// <summary>
    /// A virtual stream to do the read / write job from / to sockets
    /// </summary>
    public class FtpResponseStream : Stream
    {
        private const int c_DefaultBufferSize = 512;
        //--//
        private string m_Memory = "";                       // internal buffer
        private FtpListenerResponse m_Response = null;      // the response
        private Stream m_InternalStream = null;             // input stream to be read from

        /// <summary>Creates a response stream associated with the given response.</summary>
        public FtpResponseStream(FtpListenerResponse responese)
        {
            m_Response = responese;
        }

        /// <summary>Creates a response stream that reads from or writes to the given underlying stream.</summary>
        public FtpResponseStream(FtpListenerResponse responese, Stream stream)
            : this(responese)
        {
            m_InternalStream = stream;
        }

        /// <summary>Gets a value indicating whether the stream supports reading; always false.</summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>Gets a value indicating whether the stream supports seeking; always false.</summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>Gets a value indicating whether the stream supports writing; always true.</summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>Flushes the stream; this implementation does nothing.</summary>
        public override void Flush()
        {

        }

        /// <summary>Gets the number of bytes currently held in the internal buffer.</summary>
        public override long Length
        {
            get
            {
                if (m_Memory != null)
                {
                    return m_Memory.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>Getting or setting the position is not supported by this stream.</summary>
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>Closes the stream and sends the pending FTP response.</summary>
        public override void Close()
        {
            if (m_Response != null)
                m_Response.SendResponse();
            base.Close();
        }

        /// <summary>Reading bytes from this stream is not supported.</summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>Seeking is not supported by this stream.</summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>Setting the length is not supported by this stream.</summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>Writing a byte buffer is not supported by this stream.</summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a string to the buffer
        /// </summary>
        /// <param name="s"></param>
        public void Write(string s)
        {
            m_Memory += s;
        }

        /// <summary>
        /// Read data from the stream and write them to the socket
        /// </summary>
        /// <param name="stream"></param>
        public void Write(FileStream stream)
        {
            if (m_InternalStream == null)
            {
                throw new NotSupportedException("Does not allow this input.");
            }
            else
            {
                try
                {
                    byte[] buffer = new byte[c_DefaultBufferSize];
                    if (stream != null && stream.CanRead)
                    {
                        int len = stream.Read(buffer, 0, c_DefaultBufferSize);
                        while (len > 0)
                        {
                            m_InternalStream.Write(buffer, 0, len);
                            len = stream.Read(buffer, 0, c_DefaultBufferSize);
                        }
                    }
                }
                catch (IOException ioe)
                {
                    Logging.Print(ioe.Message);
                }
                catch (SocketException se)
                {
                    Logging.Print(se.ErrorCode.ToString());
                }
            }
        }

        /// <summary>
        /// Read data the file stream provided in the paramenter
        /// </summary>
        /// <param name="stream"></param>
        public void ReadTo(FileStream stream)
        {
            if (m_InternalStream == null)
            {
                throw new NotSupportedException("Does not allow this input.");
            }
            else
            {
                try
                {
                    byte[] buffer = new byte[c_DefaultBufferSize];
                    if (stream != null && stream.CanWrite)
                    {
                        int len = m_InternalStream.Read(buffer, 0, c_DefaultBufferSize);
                        while (len > 0)
                        {
                            stream.Write(buffer, 0, len);
                            len = m_InternalStream.Read(buffer, 0, c_DefaultBufferSize);
                        }
                    }
                }
                catch (IOException ioe)
                {
                    Logging.Print(ioe.Message);
                }
                catch (SocketException se)
                {
                    Logging.Print(se.ErrorCode.ToString());
                }
            }
        }

        /// <summary>
        /// Write a fileinfo or directoryinfo to the socket
        /// </summary>
        /// <param name="stream"></param>
        public void Write(FileSystemInfo info)
        {
            if (m_InternalStream == null)
            {
                throw new NotSupportedException("Does not allow this input.");
            }
            DateTime dt;
            try
            {
                dt = info.LastWriteTime;
            }
            catch (IOException)
            {
                Logging.Print("Last change time for: " + info.FullName);
                dt = DateTime.Now;
            }
            if (info is FileInfo)
            {
                FileInfo file = info as FileInfo;
                long size = 0;
                try
                {
                    size = file.Length;
                }
                catch (IOException)
                {
                    Logging.Print("Size info for: " + info.FullName);
                    size = 0;
                }
                byte[] output = null;
                if (m_Response.RequestMethod == WebRequestMethods.Ftp.ListDirectoryDetails)
                {
                    output = Encoding.UTF8.GetBytes("-rwxrwsrwx   3 root   root\t"
                        + Utility.MyToString(size) + " " + Utility.PrintDate(dt) + "\t" + file.Name + "\r\n");
                }
                else
                {
                    output = Encoding.UTF8.GetBytes(file.Name + "\r\n");
                }
                m_InternalStream.Write(output, 0, output.Length);
            }
            else if (info is DirectoryInfo)
            {
                DirectoryInfo dir = info as DirectoryInfo;
                byte[] output = null;
                if (m_Response.RequestMethod == WebRequestMethods.Ftp.ListDirectoryDetails)
                {
                    output = Encoding.UTF8.GetBytes("drwxrwsrwx   3 root   root\t0 "
                        + Utility.PrintDate(dt) + "\t" + dir.Name + "\r\n");
                }
                else
                {
                    output = Encoding.UTF8.GetBytes(dir.Name + "\r\n");
                }
                m_InternalStream.Write(output, 0, output.Length);
            }
        }

        /// <summary>
        /// Read the data from internal buffer
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int Read(out string s)
        {
            s = m_Memory;
            return s.Length;
        }
    }
}
