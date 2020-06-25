////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class ValidateNames
  {
    private ValidateNames()
    {
    }

    public static int ParseNCName(string s, int offset)
    {
      int num = offset;
      XmlCharType instance = XmlCharType.Instance;
      if (offset < s.Length && (s[offset] > 'ÿ' || ((int) instance.charProperties[(int) s[offset]] & 4) != 0))
      {
        ++offset;
        while (offset < s.Length && (s[offset] > 'ÿ' || ((int) instance.charProperties[(int) s[offset]] & 8) != 0))
          ++offset;
      }
      return offset - num;
    }

    public static string ParseNCNameThrow(string s)
    {
      ValidateNames.ParseNCNameInternal(s, true);
      return s;
    }

    private static bool ParseNCNameInternal(string s, bool throwOnError)
    {
      int ncName = ValidateNames.ParseNCName(s, 0);
      if (ncName != 0 && ncName == s.Length)
        return true;
      if (throwOnError)
        ValidateNames.ThrowInvalidName(s, 0, ncName);
      return false;
    }

    public static int ParseQName(string s, int offset, out int colonOffset)
    {
      colonOffset = 0;
      int ncName1 = ValidateNames.ParseNCName(s, offset);
      if (ncName1 != 0)
      {
        offset += ncName1;
        if (offset < s.Length && s[offset] == ':')
        {
          int ncName2 = ValidateNames.ParseNCName(s, offset + 1);
          if (ncName2 != 0)
          {
            colonOffset = offset;
            ncName1 += ncName2 + 1;
          }
        }
      }
      return ncName1;
    }

    public static void ParseQNameThrow(string s, out string prefix, out string localName)
    {
      int colonOffset;
      int qname = ValidateNames.ParseQName(s, 0, out colonOffset);
      if (qname == 0 || qname != s.Length)
        ValidateNames.ThrowInvalidName(s, 0, qname);
      if (colonOffset != 0)
      {
        prefix = s.Substring(0, colonOffset);
        localName = s.Substring(colonOffset + 1);
      }
      else
      {
        prefix = "";
        localName = s;
      }
    }

    public static void ParseNameTestThrow(string s, out string prefix, out string localName)
    {
      int index1;
      if (s.Length != 0 && s[0] == '*')
      {
        prefix = localName = (string) null;
        index1 = 1;
      }
      else
      {
        index1 = ValidateNames.ParseNCName(s, 0);
        if (index1 != 0)
        {
          localName = s.Substring(0, index1);
          if (index1 < s.Length && s[index1] == ':')
          {
            prefix = localName;
            int index2 = index1 + 1;
            if (index2 < s.Length && s[index2] == '*')
            {
              localName = (string) null;
              index1 += 2;
            }
            else
            {
              int ncName = ValidateNames.ParseNCName(s, index2);
              if (ncName != 0)
              {
                localName = s.Substring(index2, ncName);
                index1 += ncName + 1;
              }
            }
          }
          else
            prefix = "";
        }
        else
          prefix = localName = (string) null;
      }
      if (index1 != 0 && index1 == s.Length)
        return;
      ValidateNames.ThrowInvalidName(s, 0, index1);
    }

    public static void ThrowInvalidName(string s, int offsetStartChar, int offsetBadChar)
    {
      if (offsetStartChar >= s.Length)
        throw new XmlException(59, "");
      if (XmlCharType.Instance.IsNCNameChar(s[offsetBadChar]) && !XmlCharType.Instance.IsStartNCNameChar(s[offsetBadChar]))
        throw new XmlException(7, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
      throw new XmlException(8, XmlException.BuildCharExceptionStr(s[offsetBadChar]));
    }

    public static string GetInvalidNameErrorMessage(string s, int offsetStartChar, int offsetBadChar)
    {
      if (offsetStartChar >= s.Length)
        return Res.GetString(59, "");
      if (XmlCharType.Instance.IsNCNameChar(s[offsetBadChar]) && !XmlCharType.Instance.IsStartNCNameChar(s[offsetBadChar]))
        return Res.GetString(7, (object[]) XmlException.BuildCharExceptionStr(s[offsetBadChar]));
      return Res.GetString(8, (object[]) XmlException.BuildCharExceptionStr(s[offsetBadChar]));
    }

    private static string CreateName(string prefix, string localName)
    {
      if (prefix.Length == 0)
        return localName;
      return prefix + ":" + localName;
    }

    public enum Flags
    {
      NCNames = 1,
      CheckLocalName = 2,
      AllExceptPrefixMapping = 3,
      CheckPrefixMapping = 4,
      AllExceptNCNames = 6,
      All = 7,
    }
  }
}
