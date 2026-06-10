////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections;

namespace System.IO
{

    /// <summary>Base class for writing a sequential series of characters.</summary>
    [Serializable]
    public abstract class TextWriter : MarshalByRefObject, IDisposable
    {
        private const string InitialNewLine = "\r\n";

        //--//

        /// <summary>The characters written for a line terminator.</summary>
        protected char[] CoreNewLine = new char[] { '\r', '\n' };

        /// <summary>Closes the writer and releases its resources.</summary>
        public virtual void Close() => Dispose();

        /// <summary>Releases the resources used by the writer.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources used by the writer.</summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>Flushes any buffered data to the underlying store.</summary>
        public virtual void Flush()
        {
        }

        /// <summary>The character encoding the writer uses.</summary>
        public abstract Encoding Encoding
        {
            get;
        }

        /// <summary>The line terminator string used by the writer.</summary>
        public virtual string NewLine {
            get => new string(this.CoreNewLine);
            set {
                if (value == null)
                    value = InitialNewLine;
                this.CoreNewLine = value.ToCharArray();
            }
        }

        /// <summary>Writes a character.</summary>
        public virtual void Write(char value)
        {
        }

        /// <summary>Writes a character array.</summary>
        public virtual void Write(char[] buffer)
        {
            if (buffer != null) Write(buffer, 0, buffer.Length);
        }

        /// <summary>Writes a range of characters from an array.</summary>
        public virtual void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (count < 0)
                throw new ArgumentOutOfRangeException();
            if (buffer.Length - index < count)
                throw new ArgumentException();

            for (var i = 0; i < count; i++) Write(buffer[index + i]);
        }

        /// <summary>Writes the text representation of a boolean value.</summary>
        public virtual void Write(bool value) => Write(value);

        /// <summary>Writes the text representation of an integer.</summary>
        public virtual void Write(int value) => Write(value.ToString());

        /// <summary>Writes the text representation of an unsigned integer.</summary>
        public virtual void Write(uint value) => Write(value.ToString());

        /// <summary>Writes the text representation of a long integer.</summary>
        public virtual void Write(long value) => Write(value.ToString());

        /// <summary>Writes the text representation of an unsigned long integer.</summary>
        public virtual void Write(ulong value) => Write(value.ToString());

        /// <summary>Writes the text representation of a single-precision number.</summary>
        public virtual void Write(float value) => Write(value.ToString());

        /// <summary>Writes the text representation of a double-precision number.</summary>
        public virtual void Write(double value) => Write(value.ToString());

        /// <summary>Writes a string.</summary>
        public virtual void Write(string value)
        {
            if (value != null) Write(value.ToCharArray());
        }

        /// <summary>Writes the text representation of an object.</summary>
        public virtual void Write(object value)
        {
            if (value != null)
            {
                Write(value.ToString());
            }
        }

        /// <summary>Writes a line terminator.</summary>
        public virtual void WriteLine() => Write(this.CoreNewLine);

        /// <summary>Writes a character followed by a line terminator.</summary>
        public virtual void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes a character array followed by a line terminator.</summary>
        public virtual void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        /// <summary>Writes a range of characters followed by a line terminator.</summary>
        public virtual void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        /// <summary>Writes a boolean value followed by a line terminator.</summary>
        public virtual void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes an integer followed by a line terminator.</summary>
        public virtual void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes an unsigned integer followed by a line terminator.</summary>
        public virtual void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes a long integer followed by a line terminator.</summary>
        public virtual void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes an unsigned long integer followed by a line terminator.</summary>
        public virtual void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes a single-precision number followed by a line terminator.</summary>
        public virtual void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes a double-precision number followed by a line terminator.</summary>
        public virtual void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes a string followed by a line terminator.</summary>
        public virtual void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>Writes the text representation of an object followed by a line terminator.</summary>
        public virtual void WriteLine(object value)
        {
            if (value == null)
            {
                WriteLine();
            }
            else
            {
                WriteLine(value.ToString());
            }
        }
    }
}


