////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;

namespace System.Xml
{
  public abstract class XmlReader : IDisposable
  {
    private static uint IsTextualNodeBitmap = 24600;
    private static uint CanReadContentAsBitmap = 123324;
    private static uint HasValueBitmap = 157084;
    internal const int DefaultBufferSize = 4096;
    internal const int BiggerBufferSize = 8192;
    internal const int MaxStreamLengthForDefaultBufferSize = 65536;

    public virtual XmlReaderSettings Settings
    {
      get
      {
        return (XmlReaderSettings) null;
      }
    }

    public abstract XmlNodeType NodeType { get; }

    public virtual string Name
    {
      get
      {
        if (this.Prefix.Length == 0)
          return this.LocalName;
        return this.NameTable.Add(this.Prefix + ":" + this.LocalName);
      }
    }

    public abstract string LocalName { get; }

    public abstract string NamespaceURI { get; }

    public abstract string Prefix { get; }

    public abstract bool HasValue { get; }

    public abstract string Value { get; }

    public abstract int Depth { get; }

    public abstract string BaseURI { get; }

    public abstract bool IsEmptyElement { get; }

    public virtual bool IsDefault
    {
      get
      {
        return false;
      }
    }

    public virtual char QuoteChar
    {
      get
      {
        return '"';
      }
    }

    public virtual XmlSpace XmlSpace
    {
      get
      {
        return XmlSpace.None;
      }
    }

    public virtual string XmlLang
    {
      get
      {
        return "";
      }
    }

    public virtual Type ValueType
    {
      get
      {
        return typeof (string);
      }
    }

    public virtual object ReadContentAsObject()
    {
      if (!XmlReader.CanReadContentAs(this.NodeType))
        throw this.CreateReadContentAsException(nameof (ReadContentAsObject));
      return (object) this.InternalReadContentAsString();
    }

    public abstract int AttributeCount { get; }

    public abstract string GetAttribute(string name);

    public abstract string GetAttribute(string name, string namespaceURI);

    public abstract string GetAttribute(int i);

    public virtual string this[int i]
    {
      get
      {
        return this.GetAttribute(i);
      }
    }

    public virtual string this[string name]
    {
      get
      {
        return this.GetAttribute(name);
      }
    }

    public virtual string this[string name, string namespaceURI]
    {
      get
      {
        return this.GetAttribute(name, namespaceURI);
      }
    }

    public abstract bool MoveToAttribute(string name);

    public abstract bool MoveToAttribute(string name, string ns);

    public virtual void MoveToAttribute(int i)
    {
      if (i < 0 || i >= this.AttributeCount)
        throw new ArgumentOutOfRangeException(nameof (i));
      this.MoveToElement();
      this.MoveToFirstAttribute();
      for (int index = 0; index < i; ++index)
        this.MoveToNextAttribute();
    }

    public abstract bool MoveToFirstAttribute();

    public abstract bool MoveToNextAttribute();

    public abstract bool MoveToElement();

    public abstract bool ReadAttributeValue();

    public abstract bool Read();

    public abstract bool EOF { get; }

    public abstract void Close();

    public abstract ReadState ReadState { get; }

    public virtual void Skip()
    {
      this.SkipSubtree();
    }

    public abstract XmlNameTable NameTable { get; }

    public abstract string LookupNamespace(string prefix);

    public virtual bool CanResolveEntity
    {
      get
      {
        return false;
      }
    }

    public abstract void ResolveEntity();

    public virtual bool CanReadBinaryContent
    {
      get
      {
        return false;
      }
    }

    public virtual int ReadContentAsBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException(Res.GetString(48, nameof (ReadContentAsBase64)));
    }

    public virtual int ReadElementContentAsBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException(Res.GetString(48, nameof (ReadElementContentAsBase64)));
    }

    public virtual int ReadContentAsBinHex(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException(Res.GetString(48, nameof (ReadContentAsBinHex)));
    }

    public virtual int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException(Res.GetString(48, nameof (ReadElementContentAsBinHex)));
    }

    public virtual bool CanReadValueChunk
    {
      get
      {
        return false;
      }
    }

    public virtual int ReadValueChunk(char[] buffer, int index, int count)
    {
      throw new NotSupportedException(Res.GetString(49));
    }

    public virtual string ReadString()
    {
      if (this.ReadState != ReadState.Interactive)
        return "";
      this.MoveToElement();
      if (this.NodeType == XmlNodeType.Element)
      {
        if (this.IsEmptyElement)
          return "";
        if (!this.Read())
          throw new InvalidOperationException(Res.GetString(2));
        if (this.NodeType == XmlNodeType.EndElement)
          return "";
      }
      string str = "";
      while (XmlReader.IsTextualNode(this.NodeType))
      {
        str += this.Value;
        if (!this.Read())
          throw new InvalidOperationException(Res.GetString(2));
      }
      return str;
    }

    public virtual XmlNodeType MoveToContent()
    {
      do
      {
        switch (this.NodeType)
        {
          case XmlNodeType.Element:
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
          case XmlNodeType.EntityReference:
          case XmlNodeType.EndElement:
          case XmlNodeType.EndEntity:
            return this.NodeType;
          case XmlNodeType.Attribute:
            this.MoveToElement();
            goto case XmlNodeType.Element;
          default:
            continue;
        }
      }
      while (this.Read());
      return this.NodeType;
    }

    public virtual void ReadStartElement()
    {
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      this.Read();
    }

    public virtual void ReadStartElement(string name)
    {
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (!(this.Name == name))
        throw new XmlException(33, name, this as IXmlLineInfo);
      this.Read();
    }

    public virtual void ReadStartElement(string localname, string ns)
    {
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (this.LocalName == localname && this.NamespaceURI == ns)
        this.Read();
      else
        throw new XmlException(34, new string[2]
        {
          localname,
          ns
        }, this as IXmlLineInfo);
    }

    public virtual string ReadElementString()
    {
      string str = "";
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (!this.IsEmptyElement)
      {
        this.Read();
        str = this.ReadString();
        if (this.NodeType != XmlNodeType.EndElement)
          throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
        this.Read();
      }
      else
        this.Read();
      return str;
    }

    public virtual string ReadElementString(string name)
    {
      string str = "";
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (this.Name != name)
        throw new XmlException(33, name, this as IXmlLineInfo);
      if (!this.IsEmptyElement)
      {
        str = this.ReadString();
        if (this.NodeType != XmlNodeType.EndElement)
          throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
        this.Read();
      }
      else
        this.Read();
      return str;
    }

    public virtual string ReadElementString(string localname, string ns)
    {
      string str = "";
      if (this.MoveToContent() != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (this.LocalName != localname || this.NamespaceURI != ns)
        throw new XmlException(34, new string[2]
        {
          localname,
          ns
        }, this as IXmlLineInfo);
      if (!this.IsEmptyElement)
      {
        str = this.ReadString();
        if (this.NodeType != XmlNodeType.EndElement)
          throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
        this.Read();
      }
      else
        this.Read();
      return str;
    }

    public virtual void ReadEndElement()
    {
      if (this.MoveToContent() != XmlNodeType.EndElement)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      this.Read();
    }

    public virtual bool IsStartElement()
    {
      return this.MoveToContent() == XmlNodeType.Element;
    }

    public virtual bool IsStartElement(string name)
    {
      if (this.MoveToContent() == XmlNodeType.Element)
        return this.Name == name;
      return false;
    }

    public virtual bool IsStartElement(string localname, string ns)
    {
      if (this.MoveToContent() == XmlNodeType.Element && this.LocalName == localname)
        return this.NamespaceURI == ns;
      return false;
    }

    public virtual bool ReadToFollowing(string name)
    {
      if (name == null || name.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(name, nameof (name));
      name = this.NameTable.Add(name);
      while (this.Read())
      {
        if (this.NodeType == XmlNodeType.Element && Ref.Equal(name, this.Name))
          return true;
      }
      return false;
    }

    public virtual bool ReadToFollowing(string localName, string namespaceURI)
    {
      if (localName == null || localName.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, nameof (localName));
      if (namespaceURI == null)
        throw new ArgumentNullException(nameof (namespaceURI));
      localName = this.NameTable.Add(localName);
      namespaceURI = this.NameTable.Add(namespaceURI);
      while (this.Read())
      {
        if (this.NodeType == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
          return true;
      }
      return false;
    }

    public virtual bool ReadToDescendant(string name)
    {
      if (name == null || name.Length == 0)
        throw new ArgumentException(name, nameof (name));
      int depth = this.Depth;
      if (this.NodeType != XmlNodeType.Element)
      {
        if (this.ReadState != ReadState.Initial)
          return false;
        --depth;
      }
      else if (this.IsEmptyElement)
        return false;
      name = this.NameTable.Add(name);
      while (this.Read() && this.Depth > depth)
      {
        if (this.NodeType == XmlNodeType.Element && Ref.Equal(name, this.Name))
          return true;
      }
      return false;
    }

    public virtual bool ReadToDescendant(string localName, string namespaceURI)
    {
      if (localName == null || localName.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, nameof (localName));
      if (namespaceURI == null)
        throw new ArgumentNullException(nameof (namespaceURI));
      int depth = this.Depth;
      if (this.NodeType != XmlNodeType.Element)
      {
        if (this.ReadState != ReadState.Initial)
          return false;
        --depth;
      }
      else if (this.IsEmptyElement)
        return false;
      localName = this.NameTable.Add(localName);
      namespaceURI = this.NameTable.Add(namespaceURI);
      while (this.Read() && this.Depth > depth)
      {
        if (this.NodeType == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
          return true;
      }
      return false;
    }

    public virtual bool ReadToNextSibling(string name)
    {
      if (name == null || name.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(name, nameof (name));
      name = this.NameTable.Add(name);
      XmlNodeType nodeType;
      do
      {
        this.SkipSubtree();
        nodeType = this.NodeType;
        if (nodeType == XmlNodeType.Element && Ref.Equal(name, this.Name))
          return true;
      }
      while (nodeType != XmlNodeType.EndElement && !this.EOF);
      return false;
    }

    public virtual bool ReadToNextSibling(string localName, string namespaceURI)
    {
      if (localName == null || localName.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, nameof (localName));
      if (namespaceURI == null)
        throw new ArgumentNullException(nameof (namespaceURI));
      localName = this.NameTable.Add(localName);
      namespaceURI = this.NameTable.Add(namespaceURI);
      XmlNodeType nodeType;
      do
      {
        this.SkipSubtree();
        nodeType = this.NodeType;
        if (nodeType == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
          return true;
      }
      while (nodeType != XmlNodeType.EndElement && !this.EOF);
      return false;
    }

    public static bool IsName(string str)
    {
      return XmlCharType.Instance.IsName(str);
    }

    public static bool IsNameToken(string str)
    {
      return XmlCharType.Instance.IsNmToken(str);
    }

    public virtual bool HasAttributes
    {
      get
      {
        return this.AttributeCount > 0;
      }
    }

    public virtual void Dispose()
    {
      if (this.ReadState == ReadState.Closed)
        return;
      this.Close();
    }

    internal static bool IsTextualNode(XmlNodeType nodeType)
    {
      return 0L != ((long) XmlReader.IsTextualNodeBitmap & (long) (1 << (int) (nodeType & (XmlNodeType.EndElement | XmlNodeType.EndEntity))));
    }

    internal static bool CanReadContentAs(XmlNodeType nodeType)
    {
      return 0L != ((long) XmlReader.CanReadContentAsBitmap & (long) (1 << (int) (nodeType & (XmlNodeType.EndElement | XmlNodeType.EndEntity))));
    }

    internal static bool HasValueInternal(XmlNodeType nodeType)
    {
      return 0L != ((long) XmlReader.HasValueBitmap & (long) (1 << (int) (nodeType & (XmlNodeType.EndElement | XmlNodeType.EndEntity))));
    }

    private void SkipSubtree()
    {
      if (this.ReadState != ReadState.Interactive)
        return;
      this.MoveToElement();
      if (this.NodeType == XmlNodeType.Element && !this.IsEmptyElement)
      {
        int depth = this.Depth;
        do
          ;
        while (this.Read() && depth < this.Depth);
        if (this.NodeType != XmlNodeType.EndElement)
          return;
        this.Read();
      }
      else
        this.Read();
    }

    internal void CheckElement(string localName, string namespaceURI)
    {
      if (localName == null || localName.Length == 0)
        throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, nameof (localName));
      if (namespaceURI == null)
        throw new ArgumentNullException(nameof (namespaceURI));
      if (this.NodeType != XmlNodeType.Element)
        throw new XmlException(27, this.NodeType.ToString(), this as IXmlLineInfo);
      if (this.LocalName != localName || this.NamespaceURI != namespaceURI)
        throw new XmlException(34, new string[2]
        {
          localName,
          namespaceURI
        }, this as IXmlLineInfo);
    }

    internal Exception CreateReadContentAsException(string methodName)
    {
      return XmlReader.CreateReadContentAsException(methodName, this.NodeType);
    }

    internal Exception CreateReadElementContentAsException(string methodName)
    {
      return XmlReader.CreateReadElementContentAsException(methodName, this.NodeType);
    }

    internal static Exception CreateReadContentAsException(string methodName, XmlNodeType nodeType)
    {
      return (Exception) new InvalidOperationException(Res.GetString(50, (object[]) new string[2]
      {
        methodName,
        nodeType.ToString()
      }));
    }

    internal static Exception CreateReadElementContentAsException(string methodName, XmlNodeType nodeType)
    {
      return (Exception) new InvalidOperationException(Res.GetString(51, (object[]) new string[2]
      {
        methodName,
        nodeType.ToString()
      }));
    }

    internal string InternalReadContentAsString()
    {
      string str = "";
      BufferBuilder bufferBuilder = (BufferBuilder) null;
      do
      {
        switch (this.NodeType)
        {
          case XmlNodeType.Attribute:
            return this.Value;
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
          case XmlNodeType.Whitespace:
          case XmlNodeType.SignificantWhitespace:
            if (str.Length == 0)
            {
              str = this.Value;
              goto case XmlNodeType.ProcessingInstruction;
            }
            else
            {
              if (bufferBuilder == null)
              {
                bufferBuilder = new BufferBuilder();
                bufferBuilder.Append(str);
              }
              bufferBuilder.Append(this.Value);
              goto case XmlNodeType.ProcessingInstruction;
            }
          case XmlNodeType.EntityReference:
            if (this.CanResolveEntity)
            {
              this.ResolveEntity();
              goto case XmlNodeType.ProcessingInstruction;
            }
            else
              goto label_11;
          case XmlNodeType.ProcessingInstruction:
          case XmlNodeType.Comment:
          case XmlNodeType.EndEntity:
            continue;
          default:
            goto label_11;
        }
      }
      while ((this.AttributeCount != 0 ? (this.ReadAttributeValue() ? 1 : 0) : (this.Read() ? 1 : 0)) != 0);
label_11:
      if (bufferBuilder != null)
        return bufferBuilder.ToString();
      return str;
    }

    private bool SetupReadElementContentAsXxx(string methodName)
    {
      if (this.NodeType != XmlNodeType.Element)
        throw this.CreateReadElementContentAsException(methodName);
      bool isEmptyElement = this.IsEmptyElement;
      this.Read();
      if (isEmptyElement)
        return false;
      switch (this.NodeType)
      {
        case XmlNodeType.Element:
          throw new XmlException(52, "", this as IXmlLineInfo);
        case XmlNodeType.EndElement:
          this.Read();
          return false;
        default:
          return true;
      }
    }

    private void FinishReadElementContentAsXxx()
    {
      if (this.NodeType != XmlNodeType.EndElement)
        throw new XmlException(27, this.NodeType.ToString());
      this.Read();
    }

    internal static Encoding GetEncoding(XmlReader reader)
    {
      return XmlReader.GetXmlTextReader(reader)?.Encoding;
    }

    internal static ConformanceLevel GetV1ConformanceLevel(XmlReader reader)
    {
      XmlTextReader xmlTextReader = XmlReader.GetXmlTextReader(reader);
      if (xmlTextReader == null)
        return ConformanceLevel.Document;
      return xmlTextReader.V1ComformanceLevel;
    }

    private static XmlTextReader GetXmlTextReader(XmlReader reader)
    {
      return reader as XmlTextReader ?? (XmlTextReader) null;
    }

    public static XmlReader Create(Stream input)
    {
      XmlReaderSettings settings = new XmlReaderSettings();
      return XmlReader.CreateReaderImpl(input, settings, "", settings.CloseInput);
    }

    public static XmlReader Create(Stream input, XmlReaderSettings settings)
    {
      return XmlReader.Create(input, settings, "");
    }

    public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri)
    {
      if (settings == null)
        settings = new XmlReaderSettings();
      return XmlReader.CreateReaderImpl(input, settings, baseUri, settings.CloseInput);
    }

    private static XmlReader CreateReaderImpl(Stream input, XmlReaderSettings settings, string baseUriStr, bool closeInput)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      if (baseUriStr == null)
        baseUriStr = "";
      return (XmlReader) new XmlTextReader(input, (byte[]) null, 0, settings, baseUriStr, closeInput);
    }

    internal static int CalcBufferSize(Stream input)
    {
      int num = 4096;
      if (input.CanSeek)
      {
        long length = input.Length;
        if (length < (long) num)
          num = (int) length;
        else if (length > 65536L)
          num = 8192;
      }
      return num;
    }
  }
}
