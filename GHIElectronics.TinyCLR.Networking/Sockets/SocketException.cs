////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    public class SocketException : Exception
    {
        private int _errorCode;

        // Default constructor — last-error semantics on full .NET, but TinyCLR
        // doesn't track last-error so we fall back to the generic SocketError.
        public SocketException() : this((int)SocketError.SocketError) { }

        public SocketException(SocketError errorCode) : this((int)errorCode) { }

        // The int-based ctor matches full .NET (which takes a Win32 error
        // code). Stores the code; ErrorCode and SocketErrorCode both expose it.
        public SocketException(int errorCode) => this._errorCode = errorCode;

        // .NET-compatible alias of ErrorCode that returns the strongly-typed enum.
        public SocketError SocketErrorCode => (SocketError)this._errorCode;

        public int ErrorCode => this._errorCode;

        public override string Message => "A socket operation failed with error code " + this._errorCode + " (" + this.SocketErrorCode + ").";

    }; // class SocketException

} // namespace System.Net


