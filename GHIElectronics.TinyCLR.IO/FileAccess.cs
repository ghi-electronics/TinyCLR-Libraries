////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.IO
{
    // Contains constants for specifying the access you want for a file.
    // You can have Read, Write or ReadWrite access.
    //
    /// <summary>Specifies the access a file can be opened with.</summary>
    [Serializable, Flags]
    public enum FileAccess
    {
        /// <summary>Read access to the file.</summary>
        // Specifies read access to the file. Data can be read from the file and
        // the file pointer can be moved. Combine with WRITE for read-write access.
        Read = 1,

        /// <summary>Write access to the file.</summary>
        // Specifies write access to the file. Data can be written to the file and
        // the file pointer can be moved. Combine with READ for read-write access.
        Write = 2,

        /// <summary>Read and write access to the file.</summary>
        // Specifies read and write access to the file. Data can be written to the
        // file and the file pointer can be moved. Data can also be read from the
        // file.
        ReadWrite = 3,
    }
}


