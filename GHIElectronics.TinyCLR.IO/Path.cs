////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;

namespace System.IO {
    /*
     * Provides methods for processing directory strings in an ideally
     * cross-platform manner.  Most of the methods don't do a complete
     * full parsing (such as examining a UNC hostname), but they will
     * handle most string operations.
     *
     * <p>File names cannot contain backslash (\), slash (/), colon (:),
     * asterick (*), question mark (?), quote ("), less than (&lt;),
     * greater than (&gt;), or pipe (|).  The first three are used as directory
     * separators on various platforms.  Asterick and question mark are treated
     * as wild cards.  Less than, Greater than, and pipe all redirect input
     * or output from a program to a file or some combination thereof.  Quotes
     * are special.
     *
     * <p>We are guaranteeing that <var>Path.SeparatorChar</var> is the correct
     * directory separator on all platforms, and we will support
     * <var>Path.AltSeparatorChar</var> as well.  To write cross platform
     * code with minimal pain, you can use slash (/) as a directory separator in
     * your strings.
     */
    // Class contains only static data, no need to serialize
    public sealed class Path {

        private Path() {
        }

        /*
         * Platform specific directory separator character.  This is backslash
         * ('\') on Windows, slash ('/') on Unix, and colon (':') on Mac.
         *
         * Make this platform specific when we port.
         */
        public static readonly char DirectorySeparatorChar = '\\';
        /*
         * Platform specific invalid list of characters in a path.
         *
         * Make this platform specific when we port.
         */
        public static readonly char[] InvalidPathChars = { '/', '\"', '<', '>', '|', '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31 };

        /*
         * Changes the extension of a file path. The <code>path</code> parameter
         * specifies a file path, and the <code>extension</code> parameter
         * specifies a file extension (with a leading period, such as
         * <code>".exe"</code> or <code>".cool"</code>).<p>
         *
         * The function returns a file path with the same root, directory, and base
         * name parts as <code>path</code>, but with the file extension changed to
         * the specified extension. If <code>path</code> is null, the function
         * returns null. If <code>path</code> does not contain a file extension,
         * the new file extension is appended to the path. If <code>extension</code>
         * is null, any exsiting extension is removed from <code>path</code>.
         *
         * @param path The path for which to change file extension.
         * @param extension The new file extension (with a leading period), or
         * null to remove the extension.
         * @return Path with the new file extension.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #getExtension
         * @see #hasExtension
         */
        public static string ChangeExtension(string path, string extension) {
            if (path != null) {
                CheckInvalidPathChars(path);

                var s = path;
                for (var i = path.Length; --i >= 0;) {
                    var ch = path[i];
                    if (ch == '.') {
                        s = path.Substring(0, i);
                        break;
                    }

                    if (ch == DirectorySeparatorChar) break;
                }

                if (extension != null && path.Length != 0) {
                    if (extension.Length == 0 || extension[0] != '.') {
                        s = s + ".";
                    }

                    s = s + extension;
                }

                return s;
            }

            return null;
        }

        /*
       * Returns the directory path of a file path. This method effectively
       * removes the last element of the given file path, i.e. it returns a
       * string consisting of all characters up to but not including the last
       * backslash ("\") in the file path. The returned value is null if the file
       * path is null or if the file path denotes a root (such as "\", "C:", or
       * "\\server\share").
       *
       * @param path The path of a file or directory.
       * @return The directory path of the given path, or null if the given path
       * denotes a root.
       * @exception ArgumentException if <var>path</var> contains invalid characters.
       * @see #GetExtension
       * @see #GetFileNameFromPath
       * @see #GetRoot
       * @see #IsRooted
       */
        public static string GetDirectoryName(string path) {
            if (path != null) {
                NormalizePath(path, false);

                var root = GetRootLength(path);

                var i = path.Length;
                if (i > root) {
                    i = path.Length;
                    if (i == root) return null;
                    while (i > root && path[--i] != DirectorySeparatorChar) ;
                    return path.Substring(0, i);
                }
            }

            return null;
        }

        /*
         * Gets the length of the root DirectoryInfo or whatever DirectoryInfo markers
         * are specified for the first part of the DirectoryInfo name.
         *
         * @internalonly
         */
        internal static int GetRootLength(string path) {
            CheckInvalidPathChars(path);

            return path.IndexOf('\\') is var i && i != -1 ? i + 1 : 0;
        }

        internal static bool IsDirectorySeparator(char c) => c == DirectorySeparatorChar;

        public static char[] GetInvalidPathChars() => (char[])InvalidPathChars.Clone();

        public static string GetFullPath(string path) {
            ValidateNullOrEmpty(path);

            if (!Path.IsPathRooted(path)) {
                var currDir = Directory.GetCurrentDirectory();
                path = Path.Combine(currDir, path);
            }

            return NormalizePath(path, false);
        }

        /*
         * Returns the extension of the given path. The returned value includes the
         * period (".") character of the extension except when you have a terminal period when you get String.Empty, such as <code>".exe"</code> or
         * <code>".cpp"</code>. The returned value is null if the given path is
         * null or if the given path does not include an extension.
         *
         * @param path The path of a file or directory.
         * @return The extension of the given path, or null if the given path does
         * not include an extension.
         * @exception ArgumentException if <var>path</var> contains invalid
         * characters.
         * @see #GetDirectoryNameFromPath
         * @see #GetFileNameFromPath
         * @see #GetRoot
         * @see #HasExtension
         */
        public static string GetExtension(string path) {
            if (path == null)
                return null;

            CheckInvalidPathChars(path);
            var length = path.Length;
            for (var i = length; --i >= 0;) {
                var ch = path[i];
                if (ch == '.') {
                    if (i != length - 1)
                        return path.Substring(i, length - i);
                    else
                        return string.Empty;
                }

                if (ch == DirectorySeparatorChar)
                    break;
            }

            return string.Empty;
        }

        /*
         * Returns the name and extension parts of the given path. The resulting
         * string contains the characters of <code>path</code> that follow the last
         * backslash ("\"), slash ("/"), or colon (":") character in
         * <code>path</code>. The resulting string is the entire path if <code>path</code>
         * contains no backslash after removing trailing slashes, slash, or colon characters. The resulting
         * string is null if <code>path</code> is null.
         *
         * @param path The path of a file or directory.
         * @return The name and extension parts of the given path.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetDirectoryNameFromPath
         * @see #GetExtension
         * @see #GetRoot
         */
        public static string GetFileName(string path) {
            if (path != null) {
                CheckInvalidPathChars(path);

                var length = path.Length;
                for (var i = length; --i >= 0;) {
                    var ch = path[i];
                    if (ch == DirectorySeparatorChar)
                        return path.Substring(i + 1, length - i - 1);

                }
            }

            return path;
        }

        public static string GetFileNameWithoutExtension(string path) {
            path = GetFileName(path);
            if (path != null) {
                int i;
                if ((i = path.LastIndexOf('.')) == -1)
                    return path; // No path extension found
                else
                    return path.Substring(0, i);
            }

            return null;
        }

        /*
         * Returns the root portion of the given path. The resulting string
         * consists of those rightmost characters of the path that constitute the
         * root of the path. Possible patterns for the resulting string are: An
         * empty string (a relative path on the current drive), "\" (an absolute
         * path on the current drive), "X:" (a relative path on a given drive,
         * where X is the drive letter), "X:\" (an absolute path on a given drive),
         * and "\\server\share" (a UNC path for a given server and share name).
         * The resulting string is null if <code>path</code> is null.
         *
         * @param path The path of a file or directory.
         * @return The root portion of the given path.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetDirectory
         * @see #GetExtension
         * @see #GetName
         * @see #IsRooted
         */
        public static string GetPathRoot(string path) {
            if (path == null) return null;
            return path.Substring(0, GetRootLength(path));
        }

        /*
        * Tests if a path includes a file extension. The result is
        * <code>true</code> if the characters that follow the last directory
        * separator ('\\' or '/') or volume separator (':') in the path include
        * a period (".") other than a terminal period. The result is <code>false</code> otherwise.
        *
        * @param path The path to test.
        * @return Boolean indicating whether the path includes a file extension.
        * @exception ArgumentException if <var>path</var> contains invalid characters.
        * @see #ChangeExtension
        * @see #GetExtension
        */
        public static bool HasExtension(string path) {
            if (path != null) {
                CheckInvalidPathChars(path);

                for (var i = path.Length; --i >= 0;) {
                    var ch = path[i];
                    if (ch == '.') {
                        if (i != path.Length - 1)
                            return true;
                        else
                            return false;
                    }

                    if (ch == DirectorySeparatorChar) break;
                }
            }

            return false;
        }

        /*
         * Tests if the given path contains a root. A path is considered rooted
         * if it starts with a backslash ("\") or a drive letter and a colon (":").
         *
         * @param path The path to test.
         * @return Boolean indicating whether the path is rooted.
         * @exception ArgumentException if <var>path</var> contains invalid characters.
         * @see #GetRoot
         */
        public static bool IsPathRooted(string path) {
            if (path != null) {
                CheckInvalidPathChars(path);

                var length = path.Length;
                if (length >= 3 && path[0] >= 'A' && path[0] <= 'Z' && path[1] == ':' && (path[2] == DirectorySeparatorChar))
                    return true;
            }

            return false;
        }

        public static string Combine(string path1, string path2) {
            if (path1 == null || path2 == null)
                throw new ArgumentNullException(/*(path1==null) ? "path1" : "path2"*/);
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);

            if (path2.Length == 0)
                return path1;

            if (path1.Length == 0)
                return path2;

            if (IsPathRooted(path2))
                return path2;

            var ch = path1[path1.Length - 1];
            if (ch != DirectorySeparatorChar)
                return path1 + DirectorySeparatorChar + path2;
            return path1 + path2;
        }

        //--//

        internal static void CheckInvalidPathChars(string path) {
            if (-1 != path.IndexOfAny(InvalidPathChars))
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidPathChars")*/);
        }

        // Windows API definitions
        internal const int MAX_PATH = 260;  // From WinDef.h

        internal static char[] m_illegalCharacters = { '?', '*' };

        internal static void ValidateNullOrEmpty(string str) {
            if (str == null)
                throw new ArgumentNullException();

            if (str.Length == 0)
                throw new ArgumentException();
        }

        internal const int FSMaxPathLength = 260 - 2; // From FS_decl.h
        internal const int FSMaxFilenameLength = 256; // From FS_decl.h

        internal static string NormalizePath(string path, bool pattern) {
            ValidateNullOrEmpty(path);

            var pathLength = path.Length;
            var rootedPath = Path.IsPathRooted(path);
            var i = 0;

            if (rootedPath) {
                if (pathLength - 3 >= FSMaxPathLength) // if the "relative" path exceeds the limit
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }
            }
            else // For non-rooted paths (i.e. server paths or relative paths), we follow the MAX_PATH (260) limit from desktop
            {
                if (pathLength >= MAX_PATH) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }
            }

            var pathParts = path.Split(DirectorySeparatorChar);
            if (pattern && (pathParts.Length > 1))
                throw new ArgumentException();

            var finalPathSegments = new ArrayList();
            int pathPartLen;
            for (var e = 0; e < pathParts.Length; e++) {
                pathPartLen = pathParts[e].Length;
                if (pathPartLen == 0) {
                    /// Do nothing. Apparently paths like c:\\folder\\\file.txt works fine in Windows.
                    continue;
                }
                else if (pathPartLen >= FSMaxFilenameLength) {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.PathTooLong);
                }

                if (pathParts[e].IndexOfAny(InvalidPathChars) != -1)
                    throw new ArgumentException();

                if (!pattern) {
                    if (pathParts[e].IndexOfAny(m_illegalCharacters) != -1)
                        throw new ArgumentException();
                }

                /// verify whether pathParts[e] is all '.'s. If it is
                /// we have some special cases. Also path with both dots
                /// and spaces only are invalid.
                var length = pathParts[e].Length;
                var spaceFound = false;

                for (i = 0; i < length; i++) {
                    if (pathParts[e][i] == '.')
                        continue;
                    if (pathParts[e][i] == ' ') {
                        spaceFound = true;
                        continue;
                    }

                    break;
                }

                if (i >= length) {
                    if (!spaceFound) {
                        /// Dots only.
                        if (i == 1) {
                            /// Stay in same directory.
                        }
                        else if (i == 2) {
                            if (finalPathSegments.Count == 0)
                                throw new ArgumentException();

                            finalPathSegments.RemoveAt(finalPathSegments.Count - 1);
                        }
                        else {
                            throw new ArgumentException();
                        }
                    }
                    else {
                        /// Just dots and spaces doesn't make the cut.
                        throw new ArgumentException();
                    }
                }
                else {
                    var trim = length - 1;
                    while (pathParts[e][trim] == ' ' || pathParts[e][trim] == '.') {
                        trim--;
                    }

                    finalPathSegments.Add(pathParts[e].Substring(0, trim + 1));
                }
            }

            var normalizedPath = "";
            var firstSegment = true;
            for (var e = 0; e < finalPathSegments.Count; e++) {
                if (!firstSegment) {
                    normalizedPath += "\\";
                }
                else {
                    firstSegment = false;
                }

                normalizedPath += (string)finalPathSegments[e];
            }

            if (rootedPath && normalizedPath.Length == 2) { // only add if root folder
                normalizedPath += @"\";
            }

            return normalizedPath;
        }
    }
}


