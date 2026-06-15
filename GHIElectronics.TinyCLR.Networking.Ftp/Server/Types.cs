using System.IO;
using System.Threading;
using GHIElectronics.TinyCLR.Networking.Ftp;
using System;

namespace GHIElectronics.TinyCLR.Networking
{
    /// <summary>Additional FTP protocol method names used internally by the server.</summary>
    public static class WebRequestMethodsEx
    {

        /// <summary>FTP protocol method names used by the server's command handling.</summary>
        public static class Ftp
        {
            /// <summary>Represents the FTP CWD protocol method that changes the current directory.</summary>
            //
            // Summary:
            //     Represents the FTP CWD protocol method that changes the current directory.
            public const string ChangeDirectory = "CWD";

            /// <summary>Identifies the source name of a rename operation (RNFR).</summary>
            //
            // Summary:
            //     Represents the FTP RENAME protocol method that renames a directory.
            public const string RenameFrom = "RENAMEFROM";
            /// <summary>Identifies the target name of a rename operation (RNTO).</summary>
            public const string RenameTo = "RENAMETO";
        }
    }

    /// <summary>
    /// Interface to communcate with context manager
    /// </summary>
    internal interface IContextManager
    {
        void AddContext(FtpListenerContext context);
    }

    /// <summary>
    /// Interface to communcate with ftp session
    /// </summary>
    internal interface IDataManager
    {
        bool IsDataStreamAvailable
        {
            get;
            set;
        }

        Stream DataStream
        {
            get;
        }

        ManualResetEvent DataChannelEstablished
        {
            get;
        }

        void ChangeCurrentDirectory(FilePath path);

        void CloseDataChannel();

        void SendResponse(string s);
    }
    
    /// <summary>Represents the method that handles user authentication for the FTP server.</summary>
    public delegate void UserAuthenticator(object sender, UserAuthenticatorArgs e);

    /// <summary>Provides the user name and password for an authentication request and carries back the result.</summary>
    public class UserAuthenticatorArgs : 
        EventArgs
    {
        /// <summary>The user name supplied by the client.</summary>
        public string User;
        /// <summary>The password supplied by the client.</summary>
        public string Password;
        private UserAuthenticationResult m_Result;

        /// <summary>Gets or sets the result of the authentication request.</summary>
        public UserAuthenticationResult Result
        {
            get
            {
                return m_Result;
            }
            set
            {
                if (m_Result == UserAuthenticationResult.Unspecified)
                {
                    m_Result = value;
                }
                else if (m_Result != value)
                {
                    m_Result = UserAuthenticationResult.Conflicting;
                }
            }
        }

        /// <summary>Creates authentication arguments for the given user name and password.</summary>
        public UserAuthenticatorArgs(string user, string pass)
        {
            User = user;
            Password = pass;
            m_Result = UserAuthenticationResult.Unspecified;
        }
    }

    /// <summary>
    /// User anthentication result
    /// </summary>
    public enum UserAuthenticationResult
    {
        /// <summary>No authentication decision has been made.</summary>
        Unspecified = 0,
        /// <summary>The user is allowed to log in.</summary>
        Approved = 1,
        /// <summary>The user is rejected.</summary>
        Denied = 2,
        /// <summary>Conflicting decisions were set by multiple handlers.</summary>
        Conflicting = 3
    }


    /// <summary>
    /// FTP Command Type
    /// </summary>
    [Serializable]
    internal enum FtpCommandType
    {
        User = 0,
        Pass = 1,
        Cwd = 2,
        Quit = 3,
        Pasv = 4,
        Type = 5,
        List = 6,
        Port = 7,
        Sys = 8,
        Feature = 9,
        Pwd = 10,
        Retr = 11,
        Mdtm = 12,
        Size = 13,
        Store = 14,
        Noop = 15,
        Delete = 16,
        MkDir = 17,
        Rmd = 18,
        Rnfr = 19,
        Rnto = 20,
        NList = 21,
        Opts = 22,
        Unknown = 100
    }

   [Serializable]
    internal enum FtpState
    {
        WaitUser = 1,
        WaitPwd = 2,
        WaitCommand = 3,
        WaitRname = 4,
        Unknown = 100
    }
}
