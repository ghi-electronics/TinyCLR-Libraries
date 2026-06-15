using System.IO;
using System.Threading;
using System;
using System.Net;

namespace GHIElectronics.TinyCLR.Networking.Ftp
{
    /// <summary>An FTP listener that serves files from a real filesystem directory under a virtual root.</summary>
    public class FtpFilesystemListener : FtpListener
    {
        /// <summary>Creates a listener that maps the virtual root to the filesystem directory, allowing uploads.</summary>
        public FtpFilesystemListener(String virtualRoot, String filesystemRoot)
            : this(virtualRoot, filesystemRoot, true)
        {
        }

        /// <summary>Creates a listener that maps the virtual root to the filesystem directory, optionally allowing uploads.</summary>
        public FtpFilesystemListener(String virtualRoot, String filesystemRoot, bool uploadsAllowed)
        {
            if (filesystemRoot[filesystemRoot.Length - 1] != Path.DirectorySeparatorChar)
            {
                filesystemRoot += Path.DirectorySeparatorChar;
            }

            this.FilesystemRoot = filesystemRoot;
            this.VirtualRoot = virtualRoot;
            this.UploadsAllowed = uploadsAllowed;

            this.Prefixes.Add(virtualRoot);
        }

        /// <summary>The filesystem directory whose contents are served.</summary>
        public String FilesystemRoot { get; private set; }
        /// <summary>The virtual root path that clients see, mapped onto the filesystem directory.</summary>
        public String VirtualRoot { get; private set; }
        /// <summary>Whether clients are allowed to upload files.</summary>
        public bool UploadsAllowed { get; private set; }

        public override void Start()
        {
            base.Start();

            m_worker = new Thread(Run);
            m_worker.Start();
        }

        public override void Stop()
        {
            base.Stop();
            m_ContextQueueLock.Set();
            try
            {
                m_worker.Abort();
            }
            catch
            {
            }
            m_worker = null;
        }

        private Thread m_worker;


        private void Run()
        {
            for (; ; )
            {
                FtpListenerContext context = this.GetContext();

                if (context == null)
                {
                    // the listener has been closed
                    return;
                }
                try
                {
                    switch (context.Request.Method)
                    {
                        case WebRequestMethodsEx.Ftp.ChangeDirectory:
                            {
                                String fsPath = MapToFilesystem(context.Request.QueryString);

                                fsPath = StripTrailingSeparator(fsPath);

                                context.Response.StatusCode = Directory.Exists(fsPath) ? FtpStatusCode.FileActionOK : FtpStatusCode.ActionNotTakenFileUnavailable;
                            }
                            break;
                        case WebRequestMethods.Ftp.ListDirectory:
                        case WebRequestMethods.Ftp.ListDirectoryDetails:
                            {
                                String fsPath = MapToFilesystem(context.Request.QueryString);

                                fsPath = StripTrailingSeparator(fsPath);

                                if (WriteDirInfo(fsPath, context.Response.OutputStream))
                                {
                                    context.Response.StatusCode = FtpStatusCode.ClosingData;
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                                }
                            }
                            break;

                        case WebRequestMethods.Ftp.MakeDirectory:
                            {
                                String fsPath = MapToFilesystem(context.Request.QueryString);

                                fsPath = StripTrailingSeparator(fsPath);

                                DirectoryInfo dinfo = Directory.CreateDirectory(fsPath);
                                context.Response.StatusCode = (null != dinfo && dinfo.Exists) ? FtpStatusCode.PathnameCreated : FtpStatusCode.ActionNotTakenFileUnavailable;
                            }
                            break;

                        case WebRequestMethods.Ftp.RemoveDirectory:
                            {
                                DirectoryInfo info = new DirectoryInfo(MapToFilesystem(context.Request.QueryString));
                                if (info.Exists)
                                {
                                    info.Delete();
                                    context.Response.StatusCode = FtpStatusCode.FileActionOK;
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFilenameNotAllowed;
                                }
                            }
                            break;


                        case WebRequestMethods.Ftp.DownloadFile:
                            {
                                FileInfo info = new FileInfo(MapToFilesystem(context.Request.QueryString));
                                if (info.Exists)
                                {
                                    using (FileStream dataFile = new FileStream(MapToFilesystem(context.Request.QueryString), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        context.Response.OutputStream.Write(dataFile);
                                    }
                                    context.Response.StatusCode = FtpStatusCode.ClosingData;
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                                }
                            }
                            break;

                        case WebRequestMethods.Ftp.UploadFile:
                            if (!UploadsAllowed)
                            {
                                Logging.Print("Uploads forbidden by configuration: " + context.Request.QueryString);
                                context.Response.StatusCode = FtpStatusCode.FileActionAborted;
                            }
                            else
                            {
                                FileInfo info = new FileInfo(MapToFilesystem(context.Request.QueryString));

                                FtpResponseStream istream = context.Request.InputStream;

                                using (FileStream dataFile = new FileStream(MapToFilesystem(context.Request.QueryString), FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                                {
                                    istream.ReadTo(dataFile);
                                    dataFile.Close();
                                }

                                context.Response.StatusCode = FtpStatusCode.ClosingData;
                            }
                            break;

                        case WebRequestMethods.Ftp.GetFileSize:
                            {
                                FileInfo info = new FileInfo(MapToFilesystem(context.Request.QueryString));
                                if (info.Exists)
                                {
                                    context.Response.OutputStream.Write(((int)info.Length).ToString());
                                    context.Response.StatusCode = FtpStatusCode.FileStatus;
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable;
                                }
                            }
                            break;

                        case WebRequestMethods.Ftp.DeleteFile:
                            {
                                FileInfo info = new FileInfo(MapToFilesystem(context.Request.QueryString));
                                if (info.Exists)
                                {
                                    info.Delete();
                                    context.Response.StatusCode = FtpStatusCode.FileActionOK;
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFilenameNotAllowed;
                                }
                            }
                            break;

                        case WebRequestMethodsEx.Ftp.RenameFrom:
                            {
                                FileInfo fInfo = new FileInfo(MapToFilesystem(context.Request.QueryString));

                                String fsPath = MapToFilesystem(context.Request.QueryString);
                                fsPath = StripTrailingSeparator(fsPath);
                                DirectoryInfo dInfo = new DirectoryInfo(fsPath);

                                if (fInfo.Exists)
                                {
                                    context.Response.StatusCode = FtpStatusCode.FileCommandPending;
                                    context.Response.OutputStream.Close();

                                    context = this.GetContext();

                                    if (context.Request.Method != WebRequestMethodsEx.Ftp.RenameTo)
                                    {
                                        context.Response.StatusCode = FtpStatusCode.BadCommandSequence;
                                    }
                                    else
                                    {
                                        File.Move(fInfo.FullName, MapToFilesystem(context.Request.QueryString));

                                        context.Response.StatusCode = FtpStatusCode.FileActionOK;
                                    }
                                }
                                else if (dInfo.Exists)
                                {
                                    context.Response.StatusCode = FtpStatusCode.FileCommandPending;
                                    context.Response.OutputStream.Close();

                                    context = this.GetContext();

                                    if (context.Request.Method != WebRequestMethodsEx.Ftp.RenameTo)
                                    {
                                        context.Response.StatusCode = FtpStatusCode.BadCommandSequence;
                                    }
                                    else
                                    {
                                        fsPath = MapToFilesystem(context.Request.QueryString);
                                        fsPath = StripTrailingSeparator(fsPath);

                                        Directory.Move(dInfo.FullName, fsPath);

                                        context.Response.StatusCode = FtpStatusCode.FileActionOK;
                                    }
                                }
                                else
                                {
                                    context.Response.StatusCode = FtpStatusCode.ActionNotTakenFilenameNotAllowed;
                                }
                            }
                            break;

                        default:
                            context.Response.StatusCode = FtpStatusCode.FileActionAborted;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Print("could not process command: " + e);

                    context.Response.StatusCode = FtpStatusCode.ActionAbortedLocalProcessingError;
                }

                context.Response.OutputStream.Close();
            }
        }

        private String MapToFilesystem(String virtualPath)
        {
            if (Path.DirectorySeparatorChar != '/')
            {
                char[] cArray = virtualPath.ToCharArray();

                for (int i = 0; i < cArray.Length; i++)
                {
                    if (cArray[i] == '/')
                    {
                        cArray[i] = Path.DirectorySeparatorChar;
                    }
                }

                virtualPath = new string(cArray);
            }

            String filesystemPath = this.FilesystemRoot + (virtualPath.Substring(this.VirtualRoot.Length));
            return filesystemPath;
        }

        // Issue #1055: stripping a trailing '\' off a drive root like "A:\"
        // leaves "A:" — which Directory.Exists / Directory.GetDirectories on
        // TinyCLR (matching Windows behaviour) does NOT recognize as the
        // drive's root. Calls then return false / empty and the FTP layer
        // reports the LIST/CWD as "Bad sequence of commands" (the LIST/NList
        // branches in FtpListenerResponse only emit 226 for ClosingData;
        // anything else collapses to 503).
        //
        // Only strip when the result is still a real directory path —
        // i.e. longer than the bare "X:\" form (3 chars). For deeper paths
        // ("A:\foo\") removing the trailing slash is fine; for the root
        // ("A:\") we must keep it.
        private static string StripTrailingSeparator(string path)
        {
            if (path != null && path.Length > 3 && path[path.Length - 1] == Path.DirectorySeparatorChar)
                return path.Substring(0, path.Length - 1);
            return path;
        }

        private static bool WriteDirInfo(string path, FtpResponseStream stream)
        {
            if (!Directory.Exists(path)) return false;

            string[] dirs = Directory.GetDirectories(path);
            foreach (string d in dirs)
            {
                DirectoryInfo info = new DirectoryInfo(d);
                stream.Write(info);
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                stream.Write(info);
            }

            return true;
        }
    }
}
