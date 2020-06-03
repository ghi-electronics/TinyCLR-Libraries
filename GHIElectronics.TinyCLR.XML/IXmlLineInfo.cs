////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class PositionInfo : IXmlLineInfo
  {
    public virtual bool HasLineInfo()
    {
      return false;
    }

    public virtual int LineNumber
    {
      get
      {
        return 0;
      }
    }

    public virtual int LinePosition
    {
      get
      {
        return 0;
      }
    }

    public static PositionInfo GetPositionInfo(object o)
    {
      IXmlLineInfo lineInfo = o as IXmlLineInfo;
      if (lineInfo != null)
        return (PositionInfo) new ReaderPositionInfo(lineInfo);
      return new PositionInfo();
    }
  }
}
