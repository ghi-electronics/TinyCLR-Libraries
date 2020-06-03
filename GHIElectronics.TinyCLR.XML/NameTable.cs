////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  public class NameTable : XmlNameTable
  {
    private NameTable.Entry[] entries;
    private int count;
    private int mask;

    public NameTable()
    {
      this.mask = 31;
      this.entries = new NameTable.Entry[this.mask + 1];
    }

    public override string Add(string array)
    {
      if (array == null)
        throw new ArgumentNullException("key");
      if (array.Length == 0)
        return "";
      int length = array.Length;
      for (int index = 0; index < array.Length; ++index)
        length += length << 7 ^ (int) array[index];
      int num1 = length - (length >> 17);
      int num2 = num1 - (num1 >> 11);
      int hashCode = num2 - (num2 >> 5);
      for (NameTable.Entry entry = this.entries[hashCode & this.mask]; entry != null; entry = entry.next)
      {
        if (entry.hashCode == hashCode && string.Compare(entry.str, array) == 0)
          return entry.str;
      }
      return this.AddEntry(array, hashCode);
    }

    public override string Add(char[] array, int offset, int length)
    {
      if (length == 0)
        return "";
      int num1 = length;
      int num2 = num1 + (num1 << 7 ^ (int) array[offset]);
      int num3 = offset + length;
      for (int index = offset + 1; index < num3; ++index)
        num2 += num2 << 7 ^ (int) array[index];
      int num4 = num2 - (num2 >> 17);
      int num5 = num4 - (num4 >> 11);
      int hashCode = num5 - (num5 >> 5);
      for (NameTable.Entry entry = this.entries[hashCode & this.mask]; entry != null; entry = entry.next)
      {
        if (entry.hashCode == hashCode && NameTable.TextEquals(entry.str, array, offset))
          return entry.str;
      }
      return this.AddEntry(new string(array, offset, length), hashCode);
    }

    public override string Get(string array)
    {
      if (array == null)
        throw new ArgumentNullException("value");
      if (array.Length == 0)
        return "";
      int length = array.Length;
      for (int index = 0; index < array.Length; ++index)
        length += length << 7 ^ (int) array[index];
      int num1 = length - (length >> 17);
      int num2 = num1 - (num1 >> 11);
      int num3 = num2 - (num2 >> 5);
      for (NameTable.Entry entry = this.entries[num3 & this.mask]; entry != null; entry = entry.next)
      {
        if (entry.hashCode == num3 && string.Compare(entry.str, array) == 0)
          return entry.str;
      }
      return (string) null;
    }

    public override string Get(char[] array, int offset, int length)
    {
      if (length == 0)
        return "";
      int num1 = length;
      int num2 = num1 + (num1 << 7 ^ (int) array[offset]);
      int num3 = offset + length;
      for (int index = offset + 1; index < num3; ++index)
        num2 += num2 << 7 ^ (int) array[index];
      int num4 = num2 - (num2 >> 17);
      int num5 = num4 - (num4 >> 11);
      int num6 = num5 - (num5 >> 5);
      for (NameTable.Entry entry = this.entries[num6 & this.mask]; entry != null; entry = entry.next)
      {
        if (entry.hashCode == num6 && NameTable.TextEquals(entry.str, array, offset))
          return entry.str;
      }
      return (string) null;
    }

    private string AddEntry(string str, int hashCode)
    {
      int index = hashCode & this.mask;
      NameTable.Entry entry = new NameTable.Entry(str, hashCode, this.entries[index]);
      this.entries[index] = entry;
      if (this.count++ == this.mask)
        this.Grow();
      return entry.str;
    }

    private void Grow()
    {
      int num = this.mask * 2 + 1;
      NameTable.Entry[] entries = this.entries;
      NameTable.Entry[] entryArray = new NameTable.Entry[num + 1];
      NameTable.Entry next;
      for (int index1 = 0; index1 < entries.Length; ++index1)
      {
        for (NameTable.Entry entry = entries[index1]; entry != null; entry = next)
        {
          int index2 = entry.hashCode & num;
          next = entry.next;
          entry.next = entryArray[index2];
          entryArray[index2] = entry;
        }
      }
      this.entries = entryArray;
      this.mask = num;
    }

    private static bool TextEquals(string array, char[] text, int start)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        if ((int) array[index] != (int) text[start + index])
          return false;
      }
      return true;
    }

    private class Entry
    {
      internal string str;
      internal int hashCode;
      internal NameTable.Entry next;

      internal Entry(string str, int hashCode, NameTable.Entry next)
      {
        this.str = str;
        this.hashCode = hashCode;
        this.next = next;
      }
    }
  }
}
