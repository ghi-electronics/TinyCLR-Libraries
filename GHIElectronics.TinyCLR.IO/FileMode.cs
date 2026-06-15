////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.IO
{
    // Contains constants for specifying how the OS should open a file.
    // These will control whether you overwrite a file, open an existing
    // file, or some combination thereof.
    //
    // To append to a file, use Append (which maps to OpenOrCreate then we seek
    // to the end of the file).  To truncate a file or create it if it doesn't
    // exist, use Create.
    //
    /// <summary>Specifies how the operating system should open a file.</summary>
    [Serializable]
    public enum FileMode
    {
        /// <summary>Creates a new file; throws if the file already exists.</summary>
        // Creates a new file. An exception is raised if the file already exists.
        CreateNew = 1,

        /// <summary>Creates a new file, overwriting one if it already exists.</summary>
        // Creates a new file. If the file already exists, it is overwritten.
        Create = 2,

        /// <summary>Opens an existing file; throws if the file does not exist.</summary>
        // Opens an existing file. An exception is raised if the file does not exist.
        Open = 3,

        /// <summary>Opens the file if it exists; otherwise creates a new file.</summary>
        // Opens the file if it exists. Otherwise, creates a new file.
        OpenOrCreate = 4,

        /// <summary>Opens an existing file and truncates it to zero bytes; throws if it does not exist.</summary>
        // Opens an existing file. Once opened, the file is truncated so that its
        // size is zero bytes. The calling process must open the file with at least
        // WRITE access. An exception is raised if the file does not exist.
        Truncate = 5,

        /// <summary>Opens the file and seeks to the end, or creates a new file, for appending.</summary>
        // Opens the file if it exists and seeks to the end.  Otherwise,
        // creates a new file.
        Append = 6,
    }
}


