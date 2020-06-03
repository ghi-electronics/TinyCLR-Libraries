////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  public class XmlException : Exception
  {
    private int res;
    private int lineNumber;
    private int linePosition;
    private string sourceUri;
    private string message;

    public XmlException()
      : this((string) null)
    {
    }

    public XmlException(string message)
      : this(message, (Exception) null, 0, 0)
    {
    }

    public XmlException(string message, Exception innerException)
      : this(message, innerException, 0, 0)
    {
    }

    public XmlException(string message, Exception innerException, int lineNumber, int linePosition)
      : this(message, innerException, lineNumber, linePosition, (string) null)
    {
    }

    internal XmlException(string message, Exception innerException, int lineNumber, int linePosition, string sourceUri)
      : this(message == null ? 1 : 0, new string[1]
      {
        message
      }, innerException, lineNumber, linePosition, sourceUri)
    {
    }

    internal XmlException(int res, string[] args)
      : this(res, args, (Exception) null, 0, 0, (string) null)
    {
    }

    internal XmlException(int res, string[] args, string sourceUri)
      : this(res, args, (Exception) null, 0, 0, sourceUri)
    {
    }

    internal XmlException(int res, string arg)
      : this(res, new string[1]{ arg }, (Exception) null, 0, 0, (string) null)
    {
    }

    internal XmlException(int res, string arg, string sourceUri)
      : this(res, new string[1]{ arg }, (Exception) null, 0, 0, sourceUri)
    {
    }

    internal XmlException(int res, string arg, IXmlLineInfo lineInfo)
      : this(res, new string[1]{ arg }, lineInfo, (string) null)
    {
    }

    internal XmlException(int res, string arg, Exception innerException, IXmlLineInfo lineInfo)
      : this(res, new string[1]{ arg }, innerException, lineInfo == null ? 0 : lineInfo.LineNumber, lineInfo == null ? 0 : lineInfo.LinePosition, (string) null)
    {
    }

    internal XmlException(int res, string arg, IXmlLineInfo lineInfo, string sourceUri)
      : this(res, new string[1]{ arg }, lineInfo, sourceUri)
    {
    }

    internal XmlException(int res, string[] args, IXmlLineInfo lineInfo)
      : this(res, args, lineInfo, (string) null)
    {
    }

    internal XmlException(int res, string[] args, IXmlLineInfo lineInfo, string sourceUri)
      : this(res, args, (Exception) null, lineInfo == null ? 0 : lineInfo.LineNumber, lineInfo == null ? 0 : lineInfo.LinePosition, sourceUri)
    {
    }

    internal XmlException(int res, int lineNumber, int linePosition)
      : this(res, (string[]) null, (Exception) null, lineNumber, linePosition)
    {
    }

    internal XmlException(int res, string arg, int lineNumber, int linePosition)
      : this(res, new string[1]{ arg }, (Exception) null, lineNumber, linePosition, (string) null)
    {
    }

    internal XmlException(int res, string arg, int lineNumber, int linePosition, string sourceUri)
      : this(res, new string[1]{ arg }, (Exception) null, lineNumber, linePosition, sourceUri)
    {
    }

    internal XmlException(int res, string[] args, int lineNumber, int linePosition)
      : this(res, args, (Exception) null, lineNumber, linePosition, (string) null)
    {
    }

    internal XmlException(int res, string[] args, int lineNumber, int linePosition, string sourceUri)
      : this(res, args, (Exception) null, lineNumber, linePosition, sourceUri)
    {
    }

    internal XmlException(int res, string[] args, Exception innerException, int lineNumber, int linePosition)
      : this(res, args, innerException, lineNumber, linePosition, (string) null)
    {
    }

    internal XmlException(int res, string[] args, Exception innerException, int lineNumber, int linePosition, string sourceUri)
      : base(XmlException.CreateMessage(res, args, lineNumber, linePosition), innerException)
    {
      this.res = res;
      this.sourceUri = sourceUri;
      this.lineNumber = lineNumber;
      this.linePosition = linePosition;
    }

    private static string CreateMessage(int res, string[] args, int lineNumber, int linePosition)
    {
      string str = Res.GetString(res, (object[]) args);
      if (lineNumber != 0)
      {
        string[] strArray = new string[2]
        {
          lineNumber.ToString(),
          linePosition.ToString()
        };
        str = str + " " + Res.GetString(14, (object[]) strArray);
      }
      return str;
    }

    internal static string[] BuildCharExceptionStr(char ch)
    {
      return new string[2]
      {
        ch != char.MinValue ? ch.ToString() : ".",
        Utility.ToHexDigits((uint) ch)
      };
    }

    public int LineNumber
    {
      get
      {
        return this.lineNumber;
      }
    }

    public int LinePosition
    {
      get
      {
        return this.linePosition;
      }
    }

    public string SourceUri
    {
      get
      {
        return this.sourceUri;
      }
    }

    public override string Message
    {
      get
      {
        if (this.message != null)
          return this.message;
        return base.Message;
      }
    }

    internal int ResId
    {
      get
      {
        return this.res;
      }
    }
  }
}
