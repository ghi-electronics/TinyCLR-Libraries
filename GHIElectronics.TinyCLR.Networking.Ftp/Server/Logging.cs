using System.IO;
using System.Text;
using System;

namespace GHIElectronics.TinyCLR.Networking.Ftp
{
    /// <summary>
    /// Logging choices
    /// </summary>
    [Serializable]
    internal enum LoggingMethod
    {
        None = 0,
        Debug = 1,
        File = 2
    }

    /// <summary>
    /// Loggings
    /// </summary>
    internal static class Logging
    {
        /// <summary>
        /// Reset Logging method
        /// </summary>
        public static LoggingMethod LogMethod
        {
            get
            {
                return s_Method;
            }
            set
            {
                if (value != s_Method)
                {
                    s_Method = value;
                }
            }
        }

        /// <summary>
        /// Reset log file path
        /// </summary>
        public static string LogFilePath
        {
            get
            {
                return s_LoggingPath;
            }
            set
            {
                if (s_LoggingPath != value)
                {
                    s_LoggingPath = value;
                    CloseLogFile();
                }
            }
        }

        // local variables with default values
        private static LoggingMethod s_Method = LoggingMethod.Debug;
        private static string s_LoggingPath = "\\ROOT\\Logging.log";
        private static FileStream s_DataFile = null;
        private static object s_Lock = new object();

        /// <summary>
        /// Log the message
        /// </summary>
        /// <param name="s"></param>
        public static void Print(string s)
        {
            if (((uint)s_Method & (uint)LoggingMethod.Debug) != 0)
            {
                System.Diagnostics.Debug.WriteLine(s);
            }
            if (((uint)s_Method & (uint)LoggingMethod.File) != 0)
            {
                if (s_DataFile == null)
                    CreateLogFile();
                s += "\r\n";
                byte[] buffer = Encoding.UTF8.GetBytes(s);
                lock (s_Lock)
                {
                    s_DataFile.Write(buffer, 0, buffer.Length);
                    s_DataFile.Flush();
                }
            }
        }


        // Auxilary methods to be used internally
        private static void CreateLogFile()
        {
            System.Diagnostics.Debug.WriteLine("Creating log file: " + s_LoggingPath);
            
            lock (s_Lock)
            {
                if (s_DataFile == null)
                {
                    s_DataFile = new FileStream(s_LoggingPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Data file already exists.");      
                }
            }
        }

        private static void CloseLogFile()
        {
            // in case there are data to be written in the file stream
            lock (s_Lock)
            {
                if (s_DataFile != null)
                {
                    s_DataFile.Flush();
                    s_DataFile.Dispose();
                    s_DataFile = null;
                }
            }
        }
    }
}
