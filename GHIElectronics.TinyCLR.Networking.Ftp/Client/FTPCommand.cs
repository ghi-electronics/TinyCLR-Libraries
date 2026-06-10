
namespace System.Net
{
    /// <summary>
    /// Ftp command class
    /// </summary>
    public class FTPCommand
    {
        /// <summary>The FTP command name (upper-cased).</summary>
        public string CommandName;
        /// <summary>The argument text that follows the command name.</summary>
        public string CommandContent;
        /// <summary>Creates an FTP command from a name and its content.</summary>
        public FTPCommand(string name, string content)
        {
            CommandName = name.Trim().ToUpper();
            CommandContent = content;
        }

    }
}
