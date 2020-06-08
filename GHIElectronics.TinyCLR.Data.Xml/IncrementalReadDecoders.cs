////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GHIElectronics.TinyCLR.Data.Xml
{

    //
    //  IncrementalReadDecoder abstract class
    //
    internal abstract class IncrementalReadDecoder
    {
        internal abstract int DecodedCount { get; }
        internal abstract bool IsFull { get; }
        internal abstract void SetNextOutputBuffer(Array array, int offset, int len);
        internal abstract int Decode(char[] chars, int startPos, int len);
        internal abstract int Decode(string str, int startPos, int len);
        internal abstract void Reset();
    }

    //
    //  Dummy IncrementalReadDecoder
    //
    internal class IncrementalReadDummyDecoder : IncrementalReadDecoder
    {
        internal override int DecodedCount => -1;
        internal override bool IsFull => false;
        internal override void SetNextOutputBuffer(Array array, int offset, int len) { }
        internal override int Decode(char[] chars, int startPos, int len) => len;
        internal override int Decode(string str, int startPos, int len) => len;
        internal override void Reset() { }
    }

    //
    //  IncrementalReadDecoder for ReadChars
    //
    internal class IncrementalReadCharsDecoder : IncrementalReadDecoder
    {
        char[] buffer;
        int startIndex;
        int curIndex;
        int endIndex;

        internal IncrementalReadCharsDecoder()
        {
        }

        internal override int DecodedCount => this.curIndex - this.startIndex;

        internal override bool IsFull => this.curIndex == this.endIndex;

        internal override int Decode(char[] chars, int startPos, int len)
        {
            Debug.Assert(len > 0);

            var copyCount = this.endIndex - this.curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }

            Array.Copy(chars, startPos, this.buffer, this.curIndex, copyCount);
            this.curIndex += copyCount;

            return copyCount;
        }

        internal override int Decode(string str, int startPos, int len)
        {
            Debug.Assert(len > 0);

            var copyCount = this.endIndex - this.curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }

            for (var i = 0; i < copyCount; i++)
            {
                this.buffer[this.curIndex + i] = str[startPos + i];
            }

            this.curIndex += copyCount;

            return copyCount;
        }

        internal override void Reset()
        {
        }

        internal override void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            Debug.Assert((buffer as char[]) != null);
            this.buffer = (char[])buffer;
            this.startIndex = index;
            this.curIndex = index;
            this.endIndex = index + count;
        }
    }

}


