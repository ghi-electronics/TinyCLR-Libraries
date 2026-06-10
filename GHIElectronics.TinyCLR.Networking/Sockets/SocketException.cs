////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>The exception that is thrown when a socket error occurs.</summary>
    [Serializable]
    public class SocketException : Exception
    {
        private int _errorCode;

        /// <summary>Initializes a new instance with a generic socket error.</summary>
        // Default constructor — last-error semantics on full .NET, but TinyCLR
        // doesn't track last-error so we fall back to the generic SocketError.
        public SocketException() : this((int)SocketError.SocketError) { }

        /// <summary>Initializes a new instance with the specified socket error.</summary>
        public SocketException(SocketError errorCode) : this((int)errorCode) { }

        /// <summary>Initializes a new instance with the specified error code.</summary>
        // The int-based ctor matches full .NET (which takes a Win32 error
        // code). Stores the code; ErrorCode and SocketErrorCode both expose it.
        public SocketException(int errorCode) => this._errorCode = errorCode;

        /// <summary>The error associated with this exception as a strongly-typed value.</summary>
        // .NET-compatible alias of ErrorCode that returns the strongly-typed enum.
        public SocketError SocketErrorCode => (SocketError)this._errorCode;

        /// <summary>The numeric error code associated with this exception.</summary>
        public int ErrorCode => this._errorCode;

        /// <summary>A message that describes the socket error.</summary>
        public override string Message => "A socket operation failed with error code " + this._errorCode + " (" + this.SocketErrorCode + ").";

    }; // class SocketException

} // namespace System.Net


