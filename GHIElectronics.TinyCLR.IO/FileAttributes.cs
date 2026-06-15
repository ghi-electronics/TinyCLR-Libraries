////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.IO
{
    /// <summary>Attributes that can be set on a file or directory.</summary>
    [Flags]
    public enum FileAttributes
    {
        /// <summary>The file or directory is read-only.</summary>
        ReadOnly = 0x1,
        /// <summary>The file or directory is hidden.</summary>
        Hidden = 0x2,
        /// <summary>The file or directory is part of the operating system.</summary>
        System = 0x4,
        /// <summary>The entry is a directory.</summary>
        Directory = 0x10,
        /// <summary>The file is marked for backup or removal.</summary>
        Archive = 0x20,
        /// <summary>The file has no other attributes set.</summary>
        Normal = 0x80,
    }
}


