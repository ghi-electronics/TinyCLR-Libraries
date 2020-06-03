////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class XmlWrappingReader : XmlReader, IXmlLineInfo
  {
    protected XmlReader reader;
    protected IXmlLineInfo readerAsIXmlLineInfo;

    internal XmlWrappingReader(XmlReader baseReader)
    {
      this.Reader = baseReader;
    }

    public override XmlReaderSettings Settings
    {
      get
      {
        return this.reader.Settings;
      }
    }

    public override XmlNodeType NodeType
    {
      get
      {
        return this.reader.NodeType;
      }
    }

    public override string Name
    {
      get
      {
        return this.reader.Name;
      }
    }

    public override string LocalName
    {
      get
      {
        return this.reader.LocalName;
      }
    }

    public override string NamespaceURI
    {
      get
      {
        return this.reader.NamespaceURI;
      }
    }

    public override string Prefix
    {
      get
      {
        return this.reader.Prefix;
      }
    }

    public override bool HasValue
    {
      get
      {
        return this.reader.HasValue;
      }
    }

    public override string Value
    {
      get
      {
        return this.reader.Value;
      }
    }

    public override int Depth
    {
      get
      {
        return this.reader.Depth;
      }
    }

    public override string BaseURI
    {
      get
      {
        return this.reader.BaseURI;
      }
    }

    public override bool IsEmptyElement
    {
      get
      {
        return this.reader.IsEmptyElement;
      }
    }

    public override bool IsDefault
    {
      get
      {
        return this.reader.IsDefault;
      }
    }

    public override char QuoteChar
    {
      get
      {
        return this.reader.QuoteChar;
      }
    }

    public override XmlSpace XmlSpace
    {
      get
      {
        return this.reader.XmlSpace;
      }
    }

    public override string XmlLang
    {
      get
      {
        return this.reader.XmlLang;
      }
    }

    public override Type ValueType
    {
      get
      {
        return this.reader.ValueType;
      }
    }

    public override int AttributeCount
    {
      get
      {
        return this.reader.AttributeCount;
      }
    }

    public override string this[int i]
    {
      get
      {
        return this[i];
      }
    }

    public override string this[string name]
    {
      get
      {
        return this[name];
      }
    }

    public override string this[string name, string namespaceURI]
    {
      get
      {
        return this[name, namespaceURI];
      }
    }

    public override bool CanResolveEntity
    {
      get
      {
        return this.reader.CanResolveEntity;
      }
    }

    public override bool EOF
    {
      get
      {
        return this.reader.EOF;
      }
    }

    public override ReadState ReadState
    {
      get
      {
        return this.reader.ReadState;
      }
    }

    public override bool HasAttributes
    {
      get
      {
        return this.reader.HasAttributes;
      }
    }

    public override XmlNameTable NameTable
    {
      get
      {
        return this.reader.NameTable;
      }
    }

    public override string GetAttribute(string name)
    {
      return this.reader.GetAttribute(name);
    }

    public override string GetAttribute(string name, string namespaceURI)
    {
      return this.reader.GetAttribute(name, namespaceURI);
    }

    public override string GetAttribute(int i)
    {
      return this.reader.GetAttribute(i);
    }

    public override bool MoveToAttribute(string name)
    {
      return this.reader.MoveToAttribute(name);
    }

    public override bool MoveToAttribute(string name, string ns)
    {
      return this.reader.MoveToAttribute(name, ns);
    }

    public override void MoveToAttribute(int i)
    {
      this.reader.MoveToAttribute(i);
    }

    public override bool MoveToFirstAttribute()
    {
      return this.reader.MoveToFirstAttribute();
    }

    public override bool MoveToNextAttribute()
    {
      return this.reader.MoveToNextAttribute();
    }

    public override bool MoveToElement()
    {
      return this.reader.MoveToElement();
    }

    public override bool Read()
    {
      return this.reader.Read();
    }

    public override void Close()
    {
      this.reader.Close();
    }

    public override void Skip()
    {
      this.reader.Skip();
    }

    public override string LookupNamespace(string prefix)
    {
      return this.reader.LookupNamespace(prefix);
    }

    public override void ResolveEntity()
    {
      this.reader.ResolveEntity();
    }

    public override bool ReadAttributeValue()
    {
      return this.reader.ReadAttributeValue();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this.reader == null)
        return;
      this.reader.Dispose();
      this.reader = (XmlReader) null;
    }

    public virtual bool HasLineInfo()
    {
      if (this.readerAsIXmlLineInfo != null)
        return this.readerAsIXmlLineInfo.HasLineInfo();
      return false;
    }

    public virtual int LineNumber
    {
      get
      {
        if (this.readerAsIXmlLineInfo != null)
          return this.readerAsIXmlLineInfo.LineNumber;
        return 0;
      }
    }

    public virtual int LinePosition
    {
      get
      {
        if (this.readerAsIXmlLineInfo != null)
          return this.readerAsIXmlLineInfo.LinePosition;
        return 0;
      }
    }

    protected XmlReader Reader
    {
      get
      {
        return this.reader;
      }
      set
      {
        this.reader = value;
        this.readerAsIXmlLineInfo = value as IXmlLineInfo;
      }
    }
  }
}
