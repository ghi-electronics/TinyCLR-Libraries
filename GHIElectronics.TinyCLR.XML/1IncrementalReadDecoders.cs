////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class IncrementalReadCharsDecoder : IncrementalReadDecoder
  {
    private char[] buffer;
    private int startIndex;
    private int curIndex;
    private int endIndex;

    internal IncrementalReadCharsDecoder()
    {
    }

    internal override int DecodedCount
    {
      get
      {
        return this.curIndex - this.startIndex;
      }
    }

    internal override bool IsFull
    {
      get
      {
        return this.curIndex == this.endIndex;
      }
    }

    internal override int Decode(char[] chars, int startPos, int len)
    {
      int length = this.endIndex - this.curIndex;
      if (length > len)
        length = len;
      Array.Copy((Array) chars, startPos, (Array) this.buffer, this.curIndex, length);
      this.curIndex += length;
      return length;
    }

    internal override int Decode(string str, int startPos, int len)
    {
      int num = this.endIndex - this.curIndex;
      if (num > len)
        num = len;
      for (int index = 0; index < num; ++index)
        this.buffer[this.curIndex + index] = str[startPos + index];
      this.curIndex += num;
      return num;
    }

    internal override void Reset()
    {
    }

    internal override void SetNextOutputBuffer(Array buffer, int index, int count)
    {
      this.buffer = (char[]) buffer;
      this.startIndex = index;
      this.curIndex = index;
      this.endIndex = index + count;
    }
  }
}
