////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.Collections;

namespace System.IO
{
    /// <summary>Base class for reading a sequential series of characters.</summary>
    [Serializable()]
    public abstract class TextReader : MarshalByRefObject, IDisposable
    {
        /// <summary>Initializes the reader.</summary>
        protected TextReader() { }

        /// <summary>Closes the reader and releases its resources.</summary>
        public virtual void Close() => Dispose();

        /// <summary>Releases the resources used by the reader.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources used by the reader.</summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>Returns the next character without consuming it, or -1 at end of stream.</summary>
        public virtual int Peek() => -1;

        /// <summary>Reads and consumes the next character, or returns -1 at end of stream.</summary>
        public virtual int Read() => -1;

        /// <summary>Reads up to count characters into the buffer, returning the number read.</summary>
        public virtual int Read(char[] buffer, int index, int count) => -1;

        /// <summary>Reads count characters into the buffer, blocking until they are available or the stream ends.</summary>
        public virtual int ReadBlock(char[] buffer, int index, int count)
        {
            int i, n = 0;
            do
            {
                n += (i = Read(buffer, index + n, count - n));
            } while (i > 0 && n < count);
            return n;
        }

        /// <summary>Reads all remaining characters and returns them as a string.</summary>
        public virtual string ReadToEnd() => null;

        /// <summary>Reads a line of characters and returns it, or null at end of stream.</summary>
        public virtual string ReadLine() => null;

    }
}


