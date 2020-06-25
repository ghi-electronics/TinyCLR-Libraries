////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class IncrementalReadDummyDecoder : IncrementalReadDecoder
  {
    internal override int DecodedCount
    {
      get
      {
        return -1;
      }
    }

    internal override bool IsFull
    {
      get
      {
        return false;
      }
    }

    internal override void SetNextOutputBuffer(Array array, int offset, int len)
    {
    }

    internal override int Decode(char[] chars, int startPos, int len)
    {
      return len;
    }

    internal override int Decode(string str, int startPos, int len)
    {
      return len;
    }

    internal override void Reset()
    {
    }
  }
}
