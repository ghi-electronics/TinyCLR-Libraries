////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;

namespace System.Xml
{
  public class XmlTextReader : XmlReader, IXmlLineInfo
  {
    private static char[] WhitespaceChars = new char[4]
    {
      ' ',
      '\t',
      '\n',
      '\r'
    };
    private XmlCharType xmlCharType = XmlCharType.Instance;
    private int curAttrIndex = -1;
    private XmlNodeType fragmentType = XmlNodeType.Document;
    private const int MaxBytesToMove = 128;
    private const int ApproxXmlDeclLength = 80;
    private const int NodesInitialSize = 8;
    private const int InitialAttributesCount = 4;
    private const int InitialParsingStateStackSize = 2;
    private const int InitialParsingStatesDepth = 2;
    private const int DtdChidrenInitialSize = 2;
    private const int MaxByteSequenceLen = 6;
    private const int MaxAttrDuplWalkCount = 250;
    private const int MinWhitespaceLookahedCount = 4096;
    private const string XmlDeclarationBegining = "<?xml";
    internal const int SurHighStart = 55296;
    internal const int SurHighEnd = 56319;
    internal const int SurLowStart = 56320;
    internal const int SurLowEnd = 57343;
    private XmlTextReader.ParsingState ps;
    private XmlTextReader.ParsingFunction parsingFunction;
    private XmlTextReader.ParsingFunction nextParsingFunction;
    private XmlTextReader.ParsingFunction nextNextParsingFunction;
    private XmlTextReader.NodeData[] nodes;
    private XmlTextReader.NodeData curNode;
    private int index;
    private int attrCount;
    private int attrHashtable;
    private int attrDuplWalkCount;
    private bool fullAttrCleanup;
    private XmlNameTable nameTable;
    private bool nameTableFromSettings;
    private bool normalize;
    private WhitespaceHandling whitespaceHandling;
    private EntityHandling entityHandling;
    private bool ignorePIs;
    private bool ignoreComments;
    private bool checkCharacters;
    private int lineNumberOffset;
    private int linePositionOffset;
    private bool closeInput;
    private XmlTextReader.XmlContext xmlContext;
    private string reportedBaseUri;
    private Encoding reportedEncoding;
    private IncrementalReadDecoder incReadDecoder;
    private XmlTextReader.IncrementalReadState incReadState;
    private int incReadDepth;
    private int incReadLeftStartPos;
    private int incReadLeftEndPos;
    private LineInfo incReadLineInfo;
    private IncrementalReadCharsDecoder readCharsDecoder;
    private BufferBuilder stringBuilder;
    private bool rootElementParsed;
    private bool standalone;
    private XmlTextReader.ParsingMode parsingMode;
    private ReadState readState;
    private int documentStartBytePos;
    private int readValueOffset;
    private string Xml;
    private string XmlNs;

    internal XmlTextReader()
    {
      this.curNode = new XmlTextReader.NodeData();
      this.parsingFunction = XmlTextReader.ParsingFunction.NoData;
    }

    internal XmlTextReader(XmlNameTable nt)
    {
      this.nameTable = nt;
      nt.Add("");
      this.Xml = nt.Add("xml");
      this.XmlNs = nt.Add("xmlns");
      this.nodes = new XmlTextReader.NodeData[8];
      this.nodes[0] = new XmlTextReader.NodeData();
      this.curNode = this.nodes[0];
      this.stringBuilder = new BufferBuilder();
      this.xmlContext = new XmlTextReader.XmlContext();
      this.parsingFunction = XmlTextReader.ParsingFunction.SwitchToInteractiveXmlDecl;
      this.nextParsingFunction = XmlTextReader.ParsingFunction.DocumentContent;
      this.entityHandling = EntityHandling.ExpandCharEntities;
      this.whitespaceHandling = WhitespaceHandling.All;
      this.closeInput = true;
      this.ps.lineNo = 1;
      this.ps.lineStartPos = -1;
    }

    private XmlTextReader(XmlReaderSettings settings)
    {
      this.xmlContext = new XmlTextReader.XmlContext();
      XmlNameTable xmlNameTable = settings.NameTable;
      if (xmlNameTable == null)
        xmlNameTable = (XmlNameTable) new System.Xml.NameTable();
      else
        this.nameTableFromSettings = true;
      this.nameTable = xmlNameTable;
      xmlNameTable.Add("");
      this.Xml = xmlNameTable.Add("xml");
      this.XmlNs = xmlNameTable.Add("xmlns");
      this.nodes = new XmlTextReader.NodeData[8];
      this.nodes[0] = new XmlTextReader.NodeData();
      this.curNode = this.nodes[0];
      this.stringBuilder = new BufferBuilder();
      this.entityHandling = EntityHandling.ExpandEntities;
      this.whitespaceHandling = settings.IgnoreWhitespace ? WhitespaceHandling.Significant : WhitespaceHandling.All;
      this.normalize = true;
      this.ignorePIs = settings.IgnoreProcessingInstructions;
      this.ignoreComments = settings.IgnoreComments;
      this.checkCharacters = settings.CheckCharacters;
      this.lineNumberOffset = settings.LineNumberOffset;
      this.linePositionOffset = settings.LinePositionOffset;
      this.ps.lineNo = this.lineNumberOffset + 1;
      this.ps.lineStartPos = -this.linePositionOffset - 1;
      this.curNode.SetLineInfo(this.ps.LineNo - 1, this.ps.LinePos - 1);
      this.parsingFunction = XmlTextReader.ParsingFunction.SwitchToInteractiveXmlDecl;
      this.nextParsingFunction = XmlTextReader.ParsingFunction.DocumentContent;
      switch (settings.ConformanceLevel)
      {
        case ConformanceLevel.Auto:
          this.fragmentType = XmlNodeType.None;
          break;
        case ConformanceLevel.Fragment:
          this.fragmentType = XmlNodeType.Element;
          break;
        default:
          this.fragmentType = XmlNodeType.Document;
          break;
      }
    }

    public XmlTextReader(Stream input)
      : this("", input, (XmlNameTable) new System.Xml.NameTable())
    {
    }

    public XmlTextReader(Stream input, XmlNameTable nt)
      : this("", input, nt)
    {
    }

    internal XmlTextReader(string url, Stream input)
      : this(url, input, (XmlNameTable) new System.Xml.NameTable())
    {
    }

    internal XmlTextReader(string url, Stream input, XmlNameTable nt)
      : this(nt)
    {
      if (url == null || url.Length == 0)
        this.InitStreamInput(input);
      else
        this.InitStreamInput(url, input);
      this.reportedBaseUri = this.ps.baseUriStr;
      this.reportedEncoding = this.ps.encoding;
    }

    internal XmlTextReader(Stream stream, byte[] bytes, int byteCount, XmlReaderSettings settings, string baseUriStr, bool closeInput)
      : this(settings)
    {
      this.InitStreamInput(baseUriStr, stream, bytes, byteCount);
      this.closeInput = closeInput;
      this.reportedBaseUri = this.ps.baseUriStr;
      this.reportedEncoding = this.ps.encoding;
    }

    public override XmlReaderSettings Settings
    {
      get
      {
        XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
        if (this.nameTableFromSettings)
          xmlReaderSettings.NameTable = this.nameTable;
        switch (this.fragmentType)
        {
          case XmlNodeType.Element:
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
            break;
          case XmlNodeType.Document:
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            break;
          default:
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
            break;
        }
        xmlReaderSettings.CheckCharacters = this.checkCharacters;
        xmlReaderSettings.LineNumberOffset = this.lineNumberOffset;
        xmlReaderSettings.LinePositionOffset = this.linePositionOffset;
        xmlReaderSettings.IgnoreWhitespace = this.whitespaceHandling == WhitespaceHandling.Significant;
        xmlReaderSettings.IgnoreProcessingInstructions = this.ignorePIs;
        xmlReaderSettings.IgnoreComments = this.ignoreComments;
        xmlReaderSettings.ReadOnly = true;
        return xmlReaderSettings;
      }
    }

    public override XmlNodeType NodeType
    {
      get
      {
        return this.curNode.type;
      }
    }

    public override string Name
    {
      get
      {
        return this.curNode.GetNameWPrefix(this.nameTable);
      }
    }

    public override string LocalName
    {
      get
      {
        return this.curNode.localName;
      }
    }

    public override string NamespaceURI
    {
      get
      {
        return this.curNode.ns;
      }
    }

    public override string Prefix
    {
      get
      {
        return this.curNode.prefix;
      }
    }

    public override bool HasValue
    {
      get
      {
        return XmlReader.HasValueInternal(this.curNode.type);
      }
    }

    public override string Value
    {
      get
      {
        if (this.parsingFunction >= XmlTextReader.ParsingFunction.PartialTextValue)
        {
          if (this.parsingFunction == XmlTextReader.ParsingFunction.PartialTextValue)
          {
            this.FinishPartialValue();
            this.parsingFunction = this.nextParsingFunction;
          }
          else
            this.FinishOtherValueIterator();
        }
        return this.curNode.StringValue;
      }
    }

    public override int Depth
    {
      get
      {
        return this.curNode.depth;
      }
    }

    public override string BaseURI
    {
      get
      {
        return this.reportedBaseUri;
      }
    }

    public override bool IsEmptyElement
    {
      get
      {
        return this.curNode.IsEmptyElement;
      }
    }

    public override bool IsDefault
    {
      get
      {
        return this.curNode.IsDefaultAttribute;
      }
    }

    public override char QuoteChar
    {
      get
      {
        if (this.curNode.type != XmlNodeType.Attribute)
          return '"';
        return this.curNode.quoteChar;
      }
    }

    public override XmlSpace XmlSpace
    {
      get
      {
        return this.xmlContext.xmlSpace;
      }
    }

    public override string XmlLang
    {
      get
      {
        return this.xmlContext.xmlLang;
      }
    }

    public override ReadState ReadState
    {
      get
      {
        return this.readState;
      }
    }

    public override bool EOF
    {
      get
      {
        return this.parsingFunction == XmlTextReader.ParsingFunction.Eof;
      }
    }

    public override XmlNameTable NameTable
    {
      get
      {
        return this.nameTable;
      }
    }

    public override bool CanResolveEntity
    {
      get
      {
        return false;
      }
    }

    public override int AttributeCount
    {
      get
      {
        return this.attrCount;
      }
    }

    public override string GetAttribute(string name)
    {
      int index = name.IndexOf(':') != -1 ? this.GetIndexOfAttributeWithPrefix(name) : this.GetIndexOfAttributeWithoutPrefix(name);
      if (index < 0)
        return (string) null;
      return this.nodes[index].StringValue;
    }

    public override string GetAttribute(string name, string namespaceURI)
    {
      if (namespaceURI == null || namespaceURI.Length == 0)
        return this.GetAttribute(name);
      return (string) null;
    }

    public override string GetAttribute(int i)
    {
      if (i < 0 || i >= this.attrCount)
        throw new ArgumentOutOfRangeException(nameof (i));
      return this.nodes[this.index + i + 1].StringValue;
    }

    public override bool MoveToAttribute(string name)
    {
      int index = name.IndexOf(':') != -1 ? this.GetIndexOfAttributeWithPrefix(name) : this.GetIndexOfAttributeWithoutPrefix(name);
      if (index < 0)
        return false;
      if (this.InAttributeValueIterator)
        this.FinishAttributeValueIterator();
      this.curAttrIndex = index - this.index - 1;
      this.curNode = this.nodes[index];
      return true;
    }

    public override bool MoveToAttribute(string name, string ns)
    {
      if (ns == null || ns.Length == 0)
        return this.MoveToAttribute(name);
      return false;
    }

    public override void MoveToAttribute(int i)
    {
      if (i < 0 || i >= this.attrCount)
        throw new ArgumentOutOfRangeException(nameof (i));
      if (this.InAttributeValueIterator)
        this.FinishAttributeValueIterator();
      this.curAttrIndex = i;
      this.curNode = this.nodes[this.index + 1 + this.curAttrIndex];
    }

    public override bool MoveToFirstAttribute()
    {
      if (this.attrCount == 0)
        return false;
      if (this.InAttributeValueIterator)
        this.FinishAttributeValueIterator();
      this.curAttrIndex = 0;
      this.curNode = this.nodes[this.index + 1];
      return true;
    }

    public override bool MoveToNextAttribute()
    {
      if (this.curAttrIndex + 1 >= this.attrCount)
        return false;
      if (this.InAttributeValueIterator)
        this.FinishAttributeValueIterator();
      this.curNode = this.nodes[this.index + 1 + ++this.curAttrIndex];
      return true;
    }

    public override bool MoveToElement()
    {
      if (this.InAttributeValueIterator)
        this.FinishAttributeValueIterator();
      else if (this.curNode.type != XmlNodeType.Attribute)
        return false;
      this.curAttrIndex = -1;
      this.curNode = this.nodes[this.index];
      return true;
    }

    public override bool Read()
    {
      while (true)
      {
        switch (this.parsingFunction)
        {
          case XmlTextReader.ParsingFunction.ElementContent:
            goto label_1;
          case XmlTextReader.ParsingFunction.NoData:
            goto label_17;
          case XmlTextReader.ParsingFunction.SwitchToInteractive:
            this.readState = ReadState.Interactive;
            this.parsingFunction = this.nextParsingFunction;
            continue;
          case XmlTextReader.ParsingFunction.SwitchToInteractiveXmlDecl:
            this.readState = ReadState.Interactive;
            this.parsingFunction = this.nextParsingFunction;
            if (!this.ParseXmlDeclaration())
            {
              this.reportedEncoding = this.ps.encoding;
              continue;
            }
            goto label_5;
          case XmlTextReader.ParsingFunction.DocumentContent:
            goto label_2;
          case XmlTextReader.ParsingFunction.MoveToElementContent:
            this.ResetAttributes();
            ++this.index;
            this.curNode = this.AddNode(this.index, this.index);
            this.parsingFunction = XmlTextReader.ParsingFunction.ElementContent;
            continue;
          case XmlTextReader.ParsingFunction.PopElementContext:
            this.PopElementContext();
            this.parsingFunction = this.nextParsingFunction;
            continue;
          case XmlTextReader.ParsingFunction.PopEmptyElementContext:
            this.curNode = this.nodes[this.index];
            this.curNode.IsEmptyElement = false;
            this.ResetAttributes();
            this.PopElementContext();
            this.parsingFunction = this.nextParsingFunction;
            continue;
          case XmlTextReader.ParsingFunction.ResetAttributesRootLevel:
            this.ResetAttributes();
            this.curNode = this.nodes[this.index];
            this.parsingFunction = this.index == 0 ? XmlTextReader.ParsingFunction.DocumentContent : XmlTextReader.ParsingFunction.ElementContent;
            continue;
          case XmlTextReader.ParsingFunction.Error:
          case XmlTextReader.ParsingFunction.Eof:
          case XmlTextReader.ParsingFunction.ReaderClosed:
            goto label_16;
          case XmlTextReader.ParsingFunction.InIncrementalRead:
            goto label_12;
          case XmlTextReader.ParsingFunction.FragmentAttribute:
            goto label_13;
          case XmlTextReader.ParsingFunction.XmlDeclarationFragment:
            goto label_14;
          case XmlTextReader.ParsingFunction.GoToEof:
            goto label_15;
          case XmlTextReader.ParsingFunction.PartialTextValue:
            this.SkipPartialTextValue();
            continue;
          case XmlTextReader.ParsingFunction.InReadAttributeValue:
            this.FinishAttributeValueIterator();
            this.curNode = this.nodes[this.index];
            continue;
          case XmlTextReader.ParsingFunction.InReadValueChunk:
            this.FinishReadValueChunk();
            continue;
          default:
            continue;
        }
      }
label_1:
      return this.ParseElementContent();
label_2:
      return this.ParseDocumentContent();
label_5:
      this.reportedEncoding = this.ps.encoding;
      return true;
label_12:
      this.FinishIncrementalRead();
      return true;
label_13:
      return this.ParseFragmentAttribute();
label_14:
      this.ParseXmlDeclarationFragment();
      this.parsingFunction = XmlTextReader.ParsingFunction.GoToEof;
      return true;
label_15:
      this.OnEof();
      return false;
label_16:
      return false;
label_17:
      this.ThrowWithoutLineInfo(22);
      return false;
    }

    public override void Close()
    {
      this.Close(this.closeInput);
    }

    public override void Skip()
    {
      if (this.readState != ReadState.Interactive)
        return;
      switch (this.parsingFunction)
      {
        case XmlTextReader.ParsingFunction.InIncrementalRead:
          this.FinishIncrementalRead();
          break;
        case XmlTextReader.ParsingFunction.PartialTextValue:
          this.SkipPartialTextValue();
          break;
        case XmlTextReader.ParsingFunction.InReadAttributeValue:
          this.FinishAttributeValueIterator();
          this.curNode = this.nodes[this.index];
          break;
        case XmlTextReader.ParsingFunction.InReadValueChunk:
          this.FinishReadValueChunk();
          break;
      }
      switch (this.curNode.type)
      {
        case XmlNodeType.Element:
          if (!this.curNode.IsEmptyElement)
          {
            int index = this.index;
            this.parsingMode = XmlTextReader.ParsingMode.SkipContent;
            do
              ;
            while (this.Read() && this.index > index);
            this.parsingMode = XmlTextReader.ParsingMode.Full;
            break;
          }
          break;
        case XmlNodeType.Attribute:
          this.MoveToElement();
          goto case XmlNodeType.Element;
      }
      this.Read();
    }

    public override string LookupNamespace(string prefix)
    {
      XmlTextReader.XmlContext xmlContext = this.xmlContext;
      XmlNamespace xmlNamespace = xmlContext.xmlNamespaces[prefix];
      while (xmlNamespace == null && xmlContext.previousContext != null)
      {
        xmlContext = xmlContext.previousContext;
        if ((xmlNamespace = xmlContext.xmlNamespaces[prefix]) != null)
          break;
      }
      if (xmlNamespace != null)
        return xmlNamespace.NamespaceURI;
      return "";
    }

    public override bool ReadAttributeValue()
    {
      if (this.parsingFunction != XmlTextReader.ParsingFunction.InReadAttributeValue)
      {
        if (this.curNode.type != XmlNodeType.Attribute || this.readState != ReadState.Interactive || this.curAttrIndex < 0)
          return false;
        if (this.parsingFunction == XmlTextReader.ParsingFunction.InReadValueChunk)
          this.FinishReadValueChunk();
        if (this.curNode.nextAttrValueChunk == null || this.entityHandling == EntityHandling.ExpandEntities)
        {
          XmlTextReader.NodeData nodeData = this.AddNode(this.index + this.attrCount + 1, this.curNode.depth + 1);
          nodeData.SetValueNode(XmlNodeType.Text, this.curNode.StringValue);
          nodeData.lineInfo = this.curNode.lineInfo2;
          nodeData.depth = this.curNode.depth + 1;
          this.curNode = nodeData;
        }
        else
        {
          this.curNode = this.curNode.nextAttrValueChunk;
          this.AddNode(this.index + this.attrCount + 1, this.index + 2);
          this.nodes[this.index + this.attrCount + 1] = this.curNode;
          this.fullAttrCleanup = true;
        }
        this.nextParsingFunction = this.parsingFunction;
        this.parsingFunction = XmlTextReader.ParsingFunction.InReadAttributeValue;
        return true;
      }
      if (this.curNode.nextAttrValueChunk == null)
        return false;
      this.curNode = this.curNode.nextAttrValueChunk;
      this.nodes[this.index + this.attrCount + 1] = this.curNode;
      return true;
    }

    public override void ResolveEntity()
    {
      throw new NotSupportedException();
    }

    public override bool CanReadBinaryContent
    {
      get
      {
        return false;
      }
    }

    public override bool CanReadValueChunk
    {
      get
      {
        return true;
      }
    }

    public override int ReadValueChunk(char[] buffer, int index, int count)
    {
      if (!XmlReader.HasValueInternal(this.curNode.type))
        throw new InvalidOperationException(Res.GetString(56, (object) this.curNode.type));
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count));
      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof (index));
      if (buffer.Length - index < count)
        throw new ArgumentOutOfRangeException(nameof (count));
      if (this.parsingFunction != XmlTextReader.ParsingFunction.InReadValueChunk)
      {
        if (this.readState != ReadState.Interactive)
          return 0;
        if (this.parsingFunction == XmlTextReader.ParsingFunction.PartialTextValue)
        {
          this.incReadState = XmlTextReader.IncrementalReadState.ReadValueChunk_OnPartialValue;
        }
        else
        {
          this.incReadState = XmlTextReader.IncrementalReadState.ReadValueChunk_OnCachedValue;
          this.nextNextParsingFunction = this.nextParsingFunction;
          this.nextParsingFunction = this.parsingFunction;
        }
        this.parsingFunction = XmlTextReader.ParsingFunction.InReadValueChunk;
        this.readValueOffset = 0;
      }
      int num1 = 0;
      int num2 = this.curNode.CopyTo(this.readValueOffset, buffer, index + num1, count - num1);
      int num3 = num1 + num2;
      this.readValueOffset += num2;
      if (num3 == count || this.incReadState != XmlTextReader.IncrementalReadState.ReadValueChunk_OnPartialValue)
        return num3;
      this.curNode.SetValue("");
      bool flag = false;
      int startPos = 0;
      int endPos = 0;
      while (num3 < count && !flag)
      {
        int outOrChars = 0;
        flag = this.ParseText(out startPos, out endPos, ref outOrChars);
        int length = count - num3;
        if (length > endPos - startPos)
          length = endPos - startPos;
        Array.Copy((Array) this.ps.chars, startPos, (Array) buffer, index + num3, length);
        num3 += length;
        startPos += length;
      }
      this.incReadState = flag ? XmlTextReader.IncrementalReadState.ReadValueChunk_OnCachedValue : XmlTextReader.IncrementalReadState.ReadValueChunk_OnPartialValue;
      this.readValueOffset = 0;
      this.curNode.SetValue(this.ps.chars, startPos, endPos - startPos);
      return num3;
    }

    public bool HasLineInfo()
    {
      return true;
    }

    public int LineNumber
    {
      get
      {
        return this.curNode.LineNo;
      }
    }

    public int LinePosition
    {
      get
      {
        return this.curNode.LinePos;
      }
    }

    internal bool Namespaces
    {
      get
      {
        return false;
      }
      set
      {
        throw new InvalidOperationException(Res.GetString(2));
      }
    }

    internal bool Normalization
    {
      get
      {
        return this.normalize;
      }
      set
      {
        if (this.readState == ReadState.Closed)
          throw new InvalidOperationException(Res.GetString(2));
        this.normalize = value;
        this.ps.eolNormalized = !value;
      }
    }

    internal Encoding Encoding
    {
      get
      {
        if (this.readState != ReadState.Interactive)
          return (Encoding) null;
        return this.reportedEncoding;
      }
    }

    internal WhitespaceHandling WhitespaceHandling
    {
      get
      {
        return this.whitespaceHandling;
      }
      set
      {
        if (this.readState == ReadState.Closed)
          throw new InvalidOperationException(Res.GetString(2));
        if ((uint) value > 2U)
          throw new XmlException(40, "");
        this.whitespaceHandling = value;
      }
    }

    internal EntityHandling EntityHandling
    {
      get
      {
        return this.entityHandling;
      }
      set
      {
        if (value != EntityHandling.ExpandEntities && value != EntityHandling.ExpandCharEntities)
          throw new XmlException(42, "");
        this.entityHandling = value;
      }
    }

    internal int ReadChars(char[] buffer, int index, int count)
    {
      if (this.parsingFunction == XmlTextReader.ParsingFunction.InIncrementalRead)
      {
        if (this.incReadDecoder != this.readCharsDecoder)
        {
          if (this.readCharsDecoder == null)
            this.readCharsDecoder = new IncrementalReadCharsDecoder();
          this.readCharsDecoder.Reset();
          this.incReadDecoder = (IncrementalReadDecoder) this.readCharsDecoder;
        }
        return this.IncrementalRead((Array) buffer, index, count);
      }
      if (this.curNode.type != XmlNodeType.Element)
        return 0;
      if (this.curNode.IsEmptyElement)
      {
        this.Read();
        return 0;
      }
      if (this.readCharsDecoder == null)
        this.readCharsDecoder = new IncrementalReadCharsDecoder();
      this.InitIncrementalRead((IncrementalReadDecoder) this.readCharsDecoder);
      return this.IncrementalRead((Array) buffer, index, count);
    }

    private void Throw(int pos, int res, string arg)
    {
      this.ps.charPos = pos;
      this.Throw(res, arg);
    }

    private void Throw(int pos, int res, string[] args)
    {
      this.ps.charPos = pos;
      this.Throw(res, args);
    }

    private void Throw(int pos, int res)
    {
      this.ps.charPos = pos;
      this.Throw(res, "");
    }

    private void Throw(int res)
    {
      this.Throw(res, "");
    }

    private void Throw(int res, int lineNo, int linePos)
    {
      this.Throw((Exception) new XmlException(res, "", lineNo, linePos, this.ps.baseUriStr));
    }

    private void Throw(int res, string arg)
    {
      this.Throw((Exception) new XmlException(res, arg, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));
    }

    private void Throw(int res, string arg, int lineNo, int linePos)
    {
      this.Throw((Exception) new XmlException(res, arg, lineNo, linePos, this.ps.baseUriStr));
    }

    private void Throw(int res, string[] args)
    {
      this.Throw((Exception) new XmlException(res, args, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));
    }

    private void Throw(Exception e)
    {
      this.SetErrorState();
      XmlException xmlException = e as XmlException;
      if (xmlException != null)
        this.curNode.SetLineInfo(xmlException.LineNumber, xmlException.LinePosition);
      throw e;
    }

    private void ReThrow(Exception e, int lineNo, int linePos)
    {
      this.Throw((Exception) new XmlException(e.Message, (Exception) null, lineNo, linePos, this.ps.baseUriStr));
    }

    private void ThrowWithoutLineInfo(int res)
    {
      this.Throw((Exception) new XmlException(res, "", this.ps.baseUriStr));
    }

    private void ThrowWithoutLineInfo(int res, string arg)
    {
      this.Throw((Exception) new XmlException(res, arg, this.ps.baseUriStr));
    }

    private void ThrowWithoutLineInfo(int res, string[] args)
    {
      this.Throw((Exception) new XmlException(res, args, this.ps.baseUriStr));
    }

    private void ThrowInvalidChar(int pos, char invChar)
    {
      this.Throw(pos, 35, XmlException.BuildCharExceptionStr(invChar));
    }

    private void SetErrorState()
    {
      this.parsingFunction = XmlTextReader.ParsingFunction.Error;
      this.readState = ReadState.Error;
    }

    private bool InAttributeValueIterator
    {
      get
      {
        if (this.attrCount > 0)
          return this.parsingFunction >= XmlTextReader.ParsingFunction.InReadAttributeValue;
        return false;
      }
    }

    private void FinishAttributeValueIterator()
    {
      if (this.parsingFunction == XmlTextReader.ParsingFunction.InReadValueChunk)
        this.FinishReadValueChunk();
      if (this.parsingFunction != XmlTextReader.ParsingFunction.InReadAttributeValue)
        return;
      this.parsingFunction = this.nextParsingFunction;
      this.nextParsingFunction = this.index > 0 ? XmlTextReader.ParsingFunction.ElementContent : XmlTextReader.ParsingFunction.DocumentContent;
    }

    private bool DtdValidation
    {
      get
      {
        return false;
      }
    }

    private void InitStreamInput(Stream stream)
    {
      this.InitStreamInput("", stream, (byte[]) null, 0);
    }

    private void InitStreamInput(string baseUriStr, Stream stream)
    {
      this.InitStreamInput(baseUriStr, stream, (byte[]) null, 0);
    }

    private void InitStreamInput(string baseUriStr, Stream stream, byte[] bytes, int byteCount)
    {
      this.ps.stream = stream;
      this.ps.baseUriStr = baseUriStr;
      int length;
      if (bytes != null)
      {
        this.ps.bytes = bytes;
        this.ps.bytesUsed = byteCount;
        length = this.ps.bytes.Length;
      }
      else
      {
        length = XmlReader.CalcBufferSize(stream);
        if (this.ps.bytes == null || this.ps.bytes.Length < length)
          this.ps.bytes = new byte[length];
      }
      if (this.ps.chars == null || this.ps.chars.Length < length + 1)
        this.ps.chars = new char[length + 1];
      this.ps.bytePos = 0;
      while (this.ps.bytesUsed < 4 && this.ps.bytes.Length - this.ps.bytesUsed > 0)
      {
        int num = stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);
        if (num == 0)
        {
          this.ps.isStreamEof = true;
          break;
        }
        this.ps.bytesUsed += num;
      }
      this.ValidateEncoding();
      Encoding encoding = (Encoding) new UTF8Encoding();
      this.ps.encoding = encoding;
      this.ps.decoder = encoding.GetDecoder();
      byte[] numArray = new byte[3]
      {
        (byte) 239,
        (byte) 187,
        (byte) 191
      };
      int index = 0;
      while (index < 3 && index < this.ps.bytesUsed && (int) this.ps.bytes[index] == (int) numArray[index])
        ++index;
      if (index == 3)
        this.ps.bytePos = 3;
      this.documentStartBytePos = this.ps.bytePos;
      this.ps.eolNormalized = !this.normalize;
      this.ps.appendMode = true;
      this.ReadData();
    }

    private void InitStringInput(string baseUriStr, Encoding originalEncoding, string str)
    {
      this.ps.baseUriStr = baseUriStr;
      char[] charArray = str.ToCharArray();
      int length = charArray.Length;
      this.ps.chars = new char[length + 1];
      Array.Copy((Array) charArray, (Array) this.ps.chars, length);
      this.ps.charsUsed = length;
      this.ps.chars[length] = char.MinValue;
      this.ps.encoding = originalEncoding;
      this.ps.eolNormalized = !this.normalize;
      this.ps.isEof = true;
    }

    private void ValidateEncoding()
    {
      if (this.ps.bytesUsed < 2)
        return;
      int num1 = (int) this.ps.bytes[0] << 8 | (int) this.ps.bytes[1];
      int num2 = this.ps.bytesUsed >= 4 ? (int) this.ps.bytes[2] << 8 | (int) this.ps.bytes[3] : 0;
      switch (num1)
      {
        case 0:
          switch (num2)
          {
            case 60:
              this.Throw(12, "Ucs4Encoding.UCS4_Bigendian");
              return;
            case 15360:
              this.Throw(12, "Ucs4Encoding.UCS4_2143");
              return;
            case 65279:
              this.Throw(12, "Ucs4Encoding.UCS4_Bigendian");
              return;
            case 65534:
              this.Throw(12, "Ucs4Encoding.UCS4_2143");
              return;
            default:
              return;
          }
        case 60:
          switch (num2)
          {
            case 0:
              this.Throw(12, "Ucs4Encoding.UCS4_3412");
              return;
            case 63:
              this.Throw(12, "BigEndianUnicode");
              return;
            default:
              return;
          }
        case 15360:
          switch (num2)
          {
            case 0:
              this.Throw(12, "Ucs4Encoding.UCS4_Littleendian");
              return;
            case 16128:
              this.Throw(12, "Unicode");
              return;
            default:
              return;
          }
        case 19567:
          if (num2 != 42900)
            break;
          this.Throw(12, "ebcdic");
          break;
        case 61371:
          int num3 = num2 & 65280;
          break;
        case 65279:
          if (num2 == 0)
          {
            this.Throw(12, "Ucs4Encoding.UCS4_3412");
            break;
          }
          this.Throw(12, "BigEndianUnicode");
          break;
        case 65534:
          if (num2 == 0)
          {
            this.Throw(12, "Ucs4Encoding.UCS4_Littleendian");
            break;
          }
          this.Throw(12, "Unicode");
          break;
      }
    }

    private void SetupEncoding(Encoding encoding)
    {
    }

    private int ReadData()
    {
      if (this.ps.isEof)
        return 0;
      int maxCharsCount;
      if (this.ps.appendMode)
      {
        if (this.ps.charsUsed == this.ps.chars.Length - 1)
        {
          for (int index = 0; index < this.attrCount; ++index)
            this.nodes[this.index + index + 1].OnBufferInvalidated();
          char[] chArray = new char[this.ps.chars.Length * 2];
          Array.Copy((Array) this.ps.chars, 0, (Array) chArray, 0, this.ps.chars.Length);
          this.ps.chars = chArray;
        }
        if (this.ps.stream != null && this.ps.bytesUsed - this.ps.bytePos < 6 && this.ps.bytes.Length - this.ps.bytesUsed < 6)
        {
          byte[] numArray = new byte[this.ps.bytes.Length * 2];
          Array.Copy((Array) this.ps.bytes, 0, (Array) numArray, 0, this.ps.bytesUsed);
          this.ps.bytes = numArray;
        }
        maxCharsCount = this.ps.chars.Length - this.ps.charsUsed - 1;
        if (maxCharsCount > 80)
          maxCharsCount = 80;
      }
      else
      {
        int length1 = this.ps.chars.Length;
        if (length1 - this.ps.charsUsed <= length1 / 2)
        {
          for (int index = 0; index < this.attrCount; ++index)
            this.nodes[this.index + index + 1].OnBufferInvalidated();
          int length2 = this.ps.charsUsed - this.ps.charPos;
          if (length2 < length1 - 1)
          {
            this.ps.lineStartPos -= this.ps.charPos;
            if (length2 > 0)
              Array.Copy((Array) this.ps.chars, this.ps.charPos, (Array) this.ps.chars, 0, length2);
            this.ps.charPos = 0;
            this.ps.charsUsed = length2;
          }
          else
          {
            char[] chArray = new char[this.ps.chars.Length * 2];
            Array.Copy((Array) this.ps.chars, 0, (Array) chArray, 0, this.ps.chars.Length);
            this.ps.chars = chArray;
          }
        }
        if (this.ps.stream != null)
        {
          int length2 = this.ps.bytesUsed - this.ps.bytePos;
          if (length2 <= 128)
          {
            if (length2 == 0)
            {
              this.ps.bytesUsed = 0;
            }
            else
            {
              Array.Copy((Array) this.ps.bytes, this.ps.bytePos, (Array) this.ps.bytes, 0, length2);
              this.ps.bytesUsed = length2;
            }
            this.ps.bytePos = 0;
          }
        }
        maxCharsCount = this.ps.chars.Length - this.ps.charsUsed - 1;
      }
      int num1;
      if (this.ps.stream != null)
      {
        if (!this.ps.isStreamEof && this.ps.bytes.Length - this.ps.bytesUsed > 0)
        {
          int num2 = this.ps.stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);
          if (num2 == 0)
            this.ps.isStreamEof = true;
          this.ps.bytesUsed += num2;
        }
        int bytePos = this.ps.bytePos;
        num1 = this.GetChars(maxCharsCount);
        if (num1 == 0 && this.ps.bytePos != bytePos)
          return this.ReadData();
      }
      else if (this.ps.textReader != null)
      {
        num1 = this.ps.textReader.Read(this.ps.chars, this.ps.charsUsed, this.ps.chars.Length - this.ps.charsUsed - 1);
        this.ps.charsUsed += num1;
      }
      else
        num1 = 0;
      if (num1 == 0)
        this.ps.isEof = true;
      this.ps.chars[this.ps.charsUsed] = char.MinValue;
      return num1;
    }

    private int GetChars(int maxCharsCount)
    {
      int bytesUsed = this.ps.bytesUsed - this.ps.bytePos;
      if (bytesUsed == 0)
        return 0;
      int num;
      try
      {
        bool completed;
        this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos, bytesUsed, this.ps.chars, this.ps.charsUsed, maxCharsCount, false, out bytesUsed, out num, out completed);
      }
      catch (ArgumentException ex)
      {
        this.InvalidCharRecovery(ref bytesUsed, out num);
      }
      this.ps.bytePos += bytesUsed;
      this.ps.charsUsed += num;
      return num;
    }

    private void InvalidCharRecovery(ref int bytesCount, out int charsCount)
    {
      int num1 = 0;
      int num2 = 0;
      try
      {
        while (num2 < bytesCount)
        {
          int bytesUsed;
          int charsUsed;
          bool completed;
          this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos + num2, 1, this.ps.chars, this.ps.charsUsed + num1, 1, false, out bytesUsed, out charsUsed, out completed);
          num1 += charsUsed;
          num2 += bytesUsed;
        }
      }
      catch (ArgumentException ex)
      {
      }
      if (num1 == 0)
        this.Throw(this.ps.charsUsed, 13);
      charsCount = num1;
      bytesCount = num2;
    }

    internal void Close(bool closeInput)
    {
      if (this.parsingFunction == XmlTextReader.ParsingFunction.ReaderClosed)
        return;
      this.ps.Close(closeInput);
      this.curNode = XmlTextReader.NodeData.None;
      this.parsingFunction = XmlTextReader.ParsingFunction.ReaderClosed;
      this.reportedEncoding = (Encoding) null;
      this.reportedBaseUri = "";
      this.readState = ReadState.Closed;
      this.fullAttrCleanup = false;
      this.ResetAttributes();
    }

    private void ShiftBuffer(int sourcePos, int destPos, int count)
    {
      Array.Copy((Array) this.ps.chars, sourcePos, (Array) this.ps.chars, destPos, count);
    }

    private bool StrEqual(char[] chars, int strPos1, int strLen1, string str2)
    {
      if (strLen1 != str2.Length)
        return false;
      int index = 0;
      while (index < strLen1 && (int) chars[strPos1 + index] == (int) str2[index])
        ++index;
      return index == strLen1;
    }

    private bool ParseXmlDeclaration()
    {
      while (this.ps.charsUsed - this.ps.charPos < 6)
      {
        if (this.ReadData() == 0)
          goto label_53;
      }
      if (this.StrEqual(this.ps.chars, this.ps.charPos, 5, "<?xml") && !this.xmlCharType.IsNameChar(this.ps.chars[this.ps.charPos + 5]))
      {
        this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos + 2);
        this.curNode.SetNamedNode(XmlNodeType.XmlDeclaration, this.Xml);
        this.ps.charPos += 5;
        BufferBuilder stringBuilder = this.stringBuilder;
        int num1 = 0;
label_5:
        while (true)
        {
          do
          {
            int length = stringBuilder.Length;
            int num2 = this.EatWhitespaces(num1 == 0 ? (BufferBuilder) null : stringBuilder);
            if (this.ps.chars[this.ps.charPos] == '?')
            {
              stringBuilder.Length = length;
              if (this.ps.chars[this.ps.charPos + 1] == '>')
              {
                if (num1 == 0)
                  this.Throw(26);
                this.ps.charPos += 2;
                this.curNode.SetValue(stringBuilder.ToString());
                stringBuilder.Length = 0;
                this.nextParsingFunction = this.parsingFunction;
                this.parsingFunction = XmlTextReader.ParsingFunction.ResetAttributesRootLevel;
                this.ps.appendMode = false;
                return true;
              }
              if (this.ps.charPos + 1 != this.ps.charsUsed)
                this.ThrowUnexpectedToken("'>'");
              else
                goto label_51;
            }
            if (num2 == 0 && num1 != 0)
              this.ThrowUnexpectedToken("?>");
            int name = this.ParseName();
            XmlTextReader.NodeData nodeData = (XmlTextReader.NodeData) null;
            switch (this.ps.chars[this.ps.charPos])
            {
              case 'e':
                if (this.StrEqual(this.ps.chars, this.ps.charPos, name - this.ps.charPos, "encoding") && num1 == 1)
                {
                  nodeData = this.AddAttributeNoChecks("encoding", 0);
                  num1 = 1;
                  break;
                }
                goto default;
              case 's':
                if (this.StrEqual(this.ps.chars, this.ps.charPos, name - this.ps.charPos, "standalone") && (num1 == 1 || num1 == 2))
                {
                  nodeData = this.AddAttributeNoChecks("standalone", 0);
                  num1 = 2;
                  break;
                }
                goto default;
              case 'v':
                if (this.StrEqual(this.ps.chars, this.ps.charPos, name - this.ps.charPos, "version") && num1 == 0)
                {
                  nodeData = this.AddAttributeNoChecks("version", 0);
                  break;
                }
                goto default;
              default:
                this.Throw(26);
                break;
            }
            nodeData.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            stringBuilder.Append(this.ps.chars, this.ps.charPos, name - this.ps.charPos);
            this.ps.charPos = name;
            if (this.ps.chars[this.ps.charPos] != '=')
            {
              this.EatWhitespaces(stringBuilder);
              if (this.ps.chars[this.ps.charPos] != '=')
                this.ThrowUnexpectedToken("=");
            }
            stringBuilder.Append('=');
            ++this.ps.charPos;
            char ch = this.ps.chars[this.ps.charPos];
            switch (ch)
            {
              case '"':
              case '\'':
                stringBuilder.Append(ch);
                ++this.ps.charPos;
                nodeData.quoteChar = ch;
                nodeData.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);
                int charPos = this.ps.charPos;
                do
                {
                  char[] chars = this.ps.chars;
                  while (chars[charPos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[charPos]] & 128) != 0)
                    ++charPos;
                  if ((int) this.ps.chars[charPos] == (int) ch)
                  {
                    switch (num1)
                    {
                      case 0:
                        if (this.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "1.0"))
                        {
                          nodeData.SetValue(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                          num1 = 1;
                          break;
                        }
                        this.Throw(30, new string(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos));
                        break;
                      case 1:
                        string str = new string(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                        if (string.Compare(str.ToLower(), "utf-8") != 0)
                          this.Throw(12, str);
                        nodeData.SetValue(str);
                        num1 = 2;
                        break;
                      case 2:
                        if (this.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "yes"))
                          this.standalone = true;
                        else if (this.StrEqual(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos, "no"))
                          this.standalone = false;
                        else
                          this.Throw(26, this.ps.LineNo, this.ps.LinePos - 1);
                        nodeData.SetValue(this.ps.chars, this.ps.charPos, charPos - this.ps.charPos);
                        num1 = 3;
                        break;
                    }
                    stringBuilder.Append(chars, this.ps.charPos, charPos - this.ps.charPos);
                    stringBuilder.Append(ch);
                    this.ps.charPos = charPos + 1;
                    goto label_5;
                  }
                  else if (charPos != this.ps.charsUsed)
                    goto label_50;
                }
                while (this.ReadData() != 0);
                this.Throw(3);
                break;
label_50:
                this.Throw(26);
                break;
              default:
                this.EatWhitespaces(stringBuilder);
                ch = this.ps.chars[this.ps.charPos];
                if (ch != '"' && ch != '\'')
                {
                  this.ThrowUnexpectedToken("\"", "'");
                  goto case '"';
                }
                else
                  goto case '"';
            }
label_51:;
          }
          while (!this.ps.isEof && this.ReadData() != 0);
          this.Throw(5);
        }
      }
label_53:
      this.parsingFunction = this.nextParsingFunction;
      this.ps.appendMode = false;
      return false;
    }

    private bool ParseDocumentContent()
    {
      bool flag;
      while (true)
      {
        int charPos1;
        do
        {
          flag = false;
          charPos1 = this.ps.charPos;
          char[] chars = this.ps.chars;
          if (chars[charPos1] == '<')
          {
            flag = true;
            if (this.ps.charsUsed - charPos1 >= 4)
            {
              int pos = charPos1 + 1;
              switch (chars[pos])
              {
                case '!':
                  int index = pos + 1;
                  if (this.ps.charsUsed - index >= 2)
                  {
                    if (chars[index] == '-')
                    {
                      if (chars[index + 1] == '-')
                      {
                        this.ps.charPos = index + 2;
                        if (this.ParseComment())
                          return true;
                        continue;
                      }
                      this.ThrowUnexpectedToken(index + 1, "-");
                      goto label_47;
                    }
                    else
                    {
                      if (chars[index] != '[')
                        throw new NotSupportedException();
                      if (this.fragmentType != XmlNodeType.Document)
                      {
                        int num = index + 1;
                        if (this.ps.charsUsed - num >= 6)
                        {
                          if (this.StrEqual(chars, num, 6, "CDATA["))
                          {
                            this.ps.charPos = num + 6;
                            this.ParseCData();
                            if (this.fragmentType == XmlNodeType.None)
                              this.fragmentType = XmlNodeType.Element;
                            return true;
                          }
                          this.ThrowUnexpectedToken(num, "CDATA[");
                          goto label_47;
                        }
                        else
                          goto label_47;
                      }
                      else
                      {
                        this.Throw(this.ps.charPos, 24);
                        goto label_47;
                      }
                    }
                  }
                  else
                    goto label_47;
                case '/':
                  this.Throw(pos + 1, 20);
                  goto label_47;
                case '?':
                  this.ps.charPos = pos + 1;
                  if (this.ParsePI())
                    return true;
                  continue;
                default:
                  if (this.rootElementParsed)
                  {
                    if (this.fragmentType == XmlNodeType.Document)
                      this.Throw(pos, 23);
                    if (this.fragmentType == XmlNodeType.None)
                      this.fragmentType = XmlNodeType.Element;
                  }
                  this.ps.charPos = pos;
                  this.rootElementParsed = true;
                  this.ParseElement();
                  return true;
              }
            }
            else
              goto label_47;
          }
          else if (chars[charPos1] == '&')
          {
            if (this.fragmentType == XmlNodeType.Document)
            {
              this.Throw(charPos1, 24);
              goto label_47;
            }
            else
            {
              if (this.fragmentType == XmlNodeType.None)
                this.fragmentType = XmlNodeType.Element;
              int charRefEndPos;
              switch (this.HandleEntityReference(false, XmlTextReader.EntityExpandType.OnlyGeneral, out charRefEndPos))
              {
                case XmlTextReader.EntityType.CharacterDec:
                case XmlTextReader.EntityType.CharacterHex:
                case XmlTextReader.EntityType.CharacterNamed:
                  continue;
                case XmlTextReader.EntityType.Unexpanded:
                  throw new NotSupportedException();
                default:
                  goto label_37;
              }
            }
          }
          else
            goto label_38;
        }
        while (!this.ParseText());
        break;
label_37:
        char[] chars1 = this.ps.chars;
        int charPos2 = this.ps.charPos;
        continue;
label_38:
        if (charPos1 != this.ps.charsUsed)
        {
          if (this.fragmentType == XmlNodeType.Document)
          {
            if (this.ParseRootLevelWhitespace())
              goto label_41;
          }
          else if (this.ParseText())
            goto label_43;
          chars1 = this.ps.chars;
          charPos2 = this.ps.charPos;
          continue;
        }
label_47:
        if (this.ReadData() != 0)
        {
          charPos2 = this.ps.charPos;
          charPos2 = this.ps.charPos;
          chars1 = this.ps.chars;
        }
        else
          goto label_49;
      }
      return true;
label_41:
      return true;
label_43:
      if (this.fragmentType == XmlNodeType.None && this.curNode.type == XmlNodeType.Text)
        this.fragmentType = XmlNodeType.Element;
      return true;
label_49:
      if (flag)
        this.Throw(24);
      if (!this.rootElementParsed && this.fragmentType == XmlNodeType.Document)
        this.ThrowWithoutLineInfo(22);
      if (this.fragmentType == XmlNodeType.None)
        this.fragmentType = this.rootElementParsed ? XmlNodeType.Document : XmlNodeType.Element;
      this.OnEof();
      return false;
    }

    private bool ParseElementContent()
    {
      while (true)
      {
        do
        {
          int charPos1;
          do
          {
            charPos1 = this.ps.charPos;
            char[] chars = this.ps.chars;
            switch (chars[charPos1])
            {
              case '&':
                int charRefEndPos;
                switch (this.HandleEntityReference(false, XmlTextReader.EntityExpandType.OnlyGeneral, out charRefEndPos))
                {
                  case XmlTextReader.EntityType.CharacterDec:
                  case XmlTextReader.EntityType.CharacterHex:
                  case XmlTextReader.EntityType.CharacterNamed:
                    continue;
                  case XmlTextReader.EntityType.Unexpanded:
                    throw new NotSupportedException();
                  default:
                    goto label_25;
                }
              case '<':
                switch (chars[charPos1 + 1])
                {
                  case '!':
                    int pos = charPos1 + 2;
                    if (this.ps.charsUsed - pos >= 2)
                    {
                      if (chars[pos] == '-')
                      {
                        if (chars[pos + 1] == '-')
                        {
                          this.ps.charPos = pos + 2;
                          if (this.ParseComment())
                            return true;
                          continue;
                        }
                        this.ThrowUnexpectedToken(pos + 1, "-");
                        goto label_29;
                      }
                      else if (chars[pos] == '[')
                      {
                        int num = pos + 1;
                        if (this.ps.charsUsed - num >= 6)
                        {
                          if (this.StrEqual(chars, num, 6, "CDATA["))
                          {
                            this.ps.charPos = num + 6;
                            this.ParseCData();
                            return true;
                          }
                          this.ThrowUnexpectedToken(num, "CDATA[");
                          goto label_29;
                        }
                        else
                          goto label_29;
                      }
                      else if (this.ParseUnexpectedToken(pos) == "DOCTYPE")
                      {
                        this.Throw(32);
                        goto label_29;
                      }
                      else
                      {
                        this.ThrowUnexpectedToken(pos, "<!--", "<[CDATA[");
                        goto label_29;
                      }
                    }
                    else
                      goto label_29;
                  case '/':
                    this.ps.charPos = charPos1 + 2;
                    this.ParseEndElement();
                    return true;
                  case '?':
                    this.ps.charPos = charPos1 + 2;
                    if (this.ParsePI())
                      return true;
                    continue;
                  default:
                    if (charPos1 + 1 != this.ps.charsUsed)
                    {
                      this.ps.charPos = charPos1 + 1;
                      this.ParseElement();
                      return true;
                    }
                    goto label_29;
                }
              default:
                goto label_26;
            }
          }
          while (!this.ParseText());
          return true;
label_25:
          char[] chars1 = this.ps.chars;
          int charPos2 = this.ps.charPos;
          continue;
label_26:
          if (charPos1 != this.ps.charsUsed)
          {
            if (this.ParseText())
              return true;
            continue;
          }
label_29:;
        }
        while (this.ReadData() != 0);
        if (this.ps.charsUsed - this.ps.charPos != 0)
          this.ThrowUnclosedElements();
        if (this.index != 0 || this.fragmentType == XmlNodeType.Document)
          this.ThrowUnclosedElements();
        else
          break;
      }
      this.OnEof();
      return false;
    }

    private void ThrowUnclosedElements()
    {
      if (this.index == 0 && this.curNode.type != XmlNodeType.Element)
      {
        this.Throw(this.ps.charsUsed, 5);
      }
      else
      {
        int index = this.parsingFunction == XmlTextReader.ParsingFunction.InIncrementalRead ? this.index : this.index - 1;
        this.stringBuilder.Length = 0;
        for (; index >= 0; --index)
        {
          XmlTextReader.NodeData node = this.nodes[index];
          if (node.type == XmlNodeType.Element)
          {
            this.stringBuilder.Append(node.GetNameWPrefix(this.nameTable));
            if (index > 0)
              this.stringBuilder.Append(", ");
            else
              this.stringBuilder.Append(".");
          }
        }
        this.Throw(this.ps.charsUsed, 6, this.stringBuilder.ToString());
      }
    }

    private void ParseElement()
    {
      int index = this.ps.charPos;
      char[] chars = this.ps.chars;
      int colonPos = -1;
      this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      int pos;
      for (; chars[index] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[index]] & 4) != 0; index = pos + 1)
      {
        pos = index + 1;
        while (true)
        {
          while (chars[pos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[pos]] & 8) != 0)
            ++pos;
          if (chars[pos] == ':')
          {
            if (colonPos != -1)
              ++pos;
            else
              break;
          }
          else
            goto label_9;
        }
        colonPos = pos;
        continue;
label_9:
        if (pos < this.ps.charsUsed)
          goto label_11;
        else
          break;
      }
      pos = this.ParseQName(out colonPos);
      chars = this.ps.chars;
label_11:
      if (colonPos > -1)
      {
        string nameWPrefix = this.nameTable.Add(chars, this.ps.charPos, pos - this.ps.charPos);
        this.curNode.SetNamedNode(XmlNodeType.Element, new string(chars, colonPos + 1, pos - (colonPos + 1)), new string(chars, this.ps.charPos, colonPos - this.ps.charPos), nameWPrefix);
      }
      else
        this.curNode.SetNamedNode(XmlNodeType.Element, this.nameTable.Add(chars, this.ps.charPos, pos - this.ps.charPos));
      char ch = chars[pos];
      if (ch <= 'ÿ' && ((int) this.xmlCharType.charProperties[(int) ch] & 1) != 0)
      {
        this.ps.charPos = pos;
        this.ParseAttributes();
        this.curNode.ns = this.LookupNamespace(this.curNode.prefix);
      }
      else
      {
        switch (ch)
        {
          case '/':
            if (pos + 1 == this.ps.charsUsed)
            {
              this.ps.charPos = pos;
              if (this.ReadData() == 0)
                this.Throw(pos, 4, ">");
              pos = this.ps.charPos;
              chars = this.ps.chars;
            }
            if (chars[pos + 1] == '>')
            {
              this.ps.charPos = pos;
              this.ParseAttributes();
              this.curNode.ns = this.LookupNamespace(this.curNode.prefix);
              break;
            }
            this.ThrowUnexpectedToken(pos, ">");
            break;
          case '>':
            this.ps.charPos = pos + 1;
            this.curNode.ns = this.LookupNamespace(this.curNode.prefix);
            this.parsingFunction = XmlTextReader.ParsingFunction.MoveToElementContent;
            break;
          default:
            this.Throw(pos, 8, XmlException.BuildCharExceptionStr(ch));
            break;
        }
      }
    }

    private void ParseEndElement()
    {
      XmlTextReader.NodeData node = this.nodes[this.index - 1];
      int length1 = node.prefix.Length;
      int length2 = node.localName.Length;
      do
        ;
      while (this.ps.charsUsed - this.ps.charPos < length1 + length2 + 1 && this.ReadData() != 0);
      char[] chars1 = this.ps.chars;
      int num;
      if (node.prefix.Length == 0)
      {
        if (!this.StrEqual(chars1, this.ps.charPos, length2, node.localName))
          this.ThrowTagMismatch(node);
        num = length2;
      }
      else
      {
        int index = this.ps.charPos + length1;
        if (!this.StrEqual(chars1, this.ps.charPos, length1, node.prefix) || chars1[index] != ':' || !this.StrEqual(chars1, index + 1, length2, node.localName))
          this.ThrowTagMismatch(node);
        num = length2 + length1 + 1;
      }
      int pos;
      while (true)
      {
        do
        {
          pos = this.ps.charPos + num;
          char[] chars2 = this.ps.chars;
          if (pos != this.ps.charsUsed)
          {
            if (chars2[pos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars2[pos]] & 8) != 0 || chars2[pos] == ':')
              this.ThrowTagMismatch(node);
            while (chars2[pos] <= 'ÿ' && ((int) this.xmlCharType.charProperties[(int) chars2[pos]] & 1) != 0)
              ++pos;
            if (chars2[pos] != '>')
            {
              if (pos != this.ps.charsUsed)
                this.ThrowUnexpectedToken(pos, ">");
            }
            else
              goto label_19;
          }
        }
        while (this.ReadData() != 0);
        this.ThrowUnclosedElements();
      }
label_19:
      --this.index;
      this.curNode = this.nodes[this.index];
      node.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      node.type = XmlNodeType.EndElement;
      this.ps.charPos = pos + 1;
      this.nextParsingFunction = this.index > 0 ? this.parsingFunction : XmlTextReader.ParsingFunction.DocumentContent;
      this.parsingFunction = XmlTextReader.ParsingFunction.PopElementContext;
    }

    private void ThrowTagMismatch(XmlTextReader.NodeData startTag)
    {
      if (startTag.type == XmlNodeType.Element)
      {
        int colonPos;
        int qname = this.ParseQName(out colonPos);
        this.Throw(19, new string[3]
        {
          startTag.GetNameWPrefix(this.nameTable),
          startTag.lineInfo.lineNo.ToString(),
          new string(this.ps.chars, this.ps.charPos, qname - this.ps.charPos)
        });
      }
      else
        this.Throw(20);
    }

    private void ParseAttributes()
    {
      int pos = this.ps.charPos;
      char[] chars = this.ps.chars;
      while (true)
      {
        int num;
        XmlTextReader.NodeData attr;
        do
        {
          num = 0;
          char ch1;
          for (; (ch1 = chars[pos]) < 'ÿ' && ((int) this.xmlCharType.charProperties[(int) ch1] & 1) != 0; ++pos)
          {
            switch (ch1)
            {
              case '\n':
                this.OnNewLine(pos + 1);
                ++num;
                break;
              case '\r':
                if (chars[pos + 1] == '\n')
                {
                  this.OnNewLine(pos + 2);
                  ++num;
                  ++pos;
                  break;
                }
                if (pos + 1 != this.ps.charsUsed)
                {
                  this.OnNewLine(pos + 1);
                  ++num;
                  break;
                }
                this.ps.charPos = pos;
                goto label_51;
            }
          }
          char ch2;
          if ((ch2 = chars[pos]) <= 'ÿ' && ((int) this.xmlCharType.charProperties[(int) ch2] & 4) == 0)
          {
            switch (ch2)
            {
              case '/':
                if (pos + 1 != this.ps.charsUsed)
                {
                  if (chars[pos + 1] == '>')
                  {
                    this.ps.charPos = pos + 2;
                    this.curNode.IsEmptyElement = true;
                    this.nextParsingFunction = this.parsingFunction;
                    this.parsingFunction = XmlTextReader.ParsingFunction.PopEmptyElementContext;
                    goto label_54;
                  }
                  else
                  {
                    this.ThrowUnexpectedToken(pos + 1, ">");
                    break;
                  }
                }
                else
                  goto label_51;
              case '>':
                this.ps.charPos = pos + 1;
                this.parsingFunction = XmlTextReader.ParsingFunction.MoveToElementContent;
                goto label_54;
              default:
                if (pos != this.ps.charsUsed)
                {
                  if (ch2 != ':')
                  {
                    this.Throw(pos, 7, XmlException.BuildCharExceptionStr(ch2));
                    break;
                  }
                  break;
                }
                goto label_51;
            }
          }
          if (pos == this.ps.charPos)
            this.Throw(18, this.ParseUnexpectedToken());
          this.ps.charPos = pos;
          int linePos = this.ps.LinePos;
          int colonPos = -1;
          int endNamePos = pos + 1;
          while (true)
          {
            char ch3;
            while ((ch3 = chars[endNamePos]) > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) ch3] & 8) != 0)
              ++endNamePos;
            if (ch3 == ':')
            {
              if (colonPos != -1)
              {
                ++endNamePos;
              }
              else
              {
                colonPos = endNamePos;
                int index = endNamePos + 1;
                if (chars[index] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[index]] & 4) != 0)
                  endNamePos = index + 1;
                else
                  break;
              }
            }
            else
              goto label_32;
          }
          endNamePos = this.ParseQName(out colonPos);
          chars = this.ps.chars;
          goto label_34;
label_32:
          if (endNamePos == this.ps.charsUsed)
          {
            endNamePos = this.ParseQName(out colonPos);
            chars = this.ps.chars;
          }
label_34:
          attr = this.AddAttribute(endNamePos, colonPos);
          attr.SetLineInfo(this.ps.LineNo, linePos);
          if (chars[endNamePos] != '=')
          {
            this.ps.charPos = endNamePos;
            this.EatWhitespaces((BufferBuilder) null);
            endNamePos = this.ps.charPos;
            if (chars[endNamePos] != '=')
              this.ThrowUnexpectedToken("=");
          }
          int index1 = endNamePos + 1;
          char quoteChar = chars[index1];
          switch (quoteChar)
          {
            case '"':
            case '\'':
              int curPos = index1 + 1;
              this.ps.charPos = curPos;
              attr.quoteChar = quoteChar;
              attr.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);
              char ch4;
              while ((ch4 = chars[curPos]) > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) ch4] & 128) != 0)
                ++curPos;
              if ((int) ch4 == (int) quoteChar)
              {
                attr.SetValue(chars, this.ps.charPos, curPos - this.ps.charPos);
                pos = curPos + 1;
                this.ps.charPos = pos;
              }
              else
              {
                this.ParseAttributeValueSlow(curPos, quoteChar, attr);
                pos = this.ps.charPos;
                chars = this.ps.chars;
              }
              if (attr.prefix.Length != 0)
              {
                if (Ref.Equal(attr.prefix, this.Xml) || Ref.Equal(attr.prefix, this.XmlNs))
                {
                  this.OnXmlReservedAttribute(attr);
                  continue;
                }
                continue;
              }
              continue;
            default:
              this.ps.charPos = index1;
              this.EatWhitespaces((BufferBuilder) null);
              index1 = this.ps.charPos;
              quoteChar = chars[index1];
              if (quoteChar != '"' && quoteChar != '\'')
              {
                this.ThrowUnexpectedToken("\"", "'");
                goto case '"';
              }
              else
                goto case '"';
          }
        }
        while (!(attr.localName == this.XmlNs));
        attr.localName = string.Empty;
        attr.prefix = string.Empty;
        this.OnXmlReservedAttribute(attr);
        continue;
label_51:
        this.ps.lineNo -= num;
        if (this.ReadData() != 0)
        {
          pos = this.ps.charPos;
          chars = this.ps.chars;
        }
        else
          this.ThrowUnclosedElements();
      }
label_54:
      if (this.attrDuplWalkCount < 250)
        return;
      this.AttributeDuplCheck();
    }

    private void AttributeDuplCheck()
    {
      for (int index1 = this.index + 1; index1 < this.index + 1 + this.attrCount; ++index1)
      {
        XmlTextReader.NodeData node = this.nodes[index1];
        for (int index2 = index1 + 1; index2 < this.index + 1 + this.attrCount; ++index2)
        {
          if (Ref.Equal(node.localName, this.nodes[index2].localName) && Ref.Equal(node.ns, this.nodes[index2].ns))
            this.Throw(31, this.nodes[index2].GetNameWPrefix(this.nameTable), this.nodes[index2].LineNo, this.nodes[index2].LinePos);
        }
      }
    }

    private void OnXmlReservedAttribute(XmlTextReader.NodeData attr)
    {
      switch (attr.localName)
      {
        case "space":
          if (!this.curNode.xmlContextPushed)
            this.PushXmlContext();
          switch (attr.StringValue.Trim(XmlTextReader.WhitespaceChars))
          {
            case "preserve":
              this.xmlContext.xmlSpace = XmlSpace.Preserve;
              return;
            case "default":
              this.xmlContext.xmlSpace = XmlSpace.Default;
              return;
            default:
              this.Throw(29, attr.StringValue, attr.lineInfo.lineNo, attr.lineInfo.linePos);
              return;
          }
        case "lang":
          if (!this.curNode.xmlContextPushed)
            this.PushXmlContext();
          this.xmlContext.xmlLang = attr.StringValue;
          break;
        default:
          if (!this.curNode.xmlContextPushed)
            this.PushXmlContext();
          this.xmlContext.xmlNamespaces.Add(attr.localName, attr.StringValue);
          break;
      }
    }

    private void ParseAttributeValueSlow(int curPos, char quoteChar, XmlTextReader.NodeData attr)
    {
      int charRefEndPos = curPos;
      char[] chars = this.ps.chars;
      int startIndex = 0;
      LineInfo lineInfo1 = new LineInfo(this.ps.lineNo, this.ps.LinePos);
      XmlTextReader.NodeData lastChunk = (XmlTextReader.NodeData) null;
      while (true)
      {
        do
        {
          while (chars[charRefEndPos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[charRefEndPos]] & 128) != 0)
            ++charRefEndPos;
          if (charRefEndPos - this.ps.charPos > 0)
          {
            this.stringBuilder.Append(chars, this.ps.charPos, charRefEndPos - this.ps.charPos);
            this.ps.charPos = charRefEndPos;
          }
          if ((int) chars[charRefEndPos] != (int) quoteChar)
          {
            switch (chars[charRefEndPos])
            {
              case '\t':
                ++charRefEndPos;
                continue;
              case '\n':
                ++charRefEndPos;
                this.OnNewLine(charRefEndPos);
                if (this.normalize)
                {
                  this.stringBuilder.Append(' ');
                  ++this.ps.charPos;
                  continue;
                }
                continue;
              case '\r':
                if (chars[charRefEndPos + 1] == '\n')
                {
                  charRefEndPos += 2;
                  if (this.normalize)
                  {
                    this.stringBuilder.Append(this.ps.eolNormalized ? "  " : " ");
                    this.ps.charPos = charRefEndPos;
                  }
                }
                else if (charRefEndPos + 1 < this.ps.charsUsed || this.ps.isEof)
                {
                  ++charRefEndPos;
                  if (this.normalize)
                  {
                    this.stringBuilder.Append(' ');
                    this.ps.charPos = charRefEndPos;
                  }
                }
                else
                  goto label_33;
                this.OnNewLine(charRefEndPos);
                continue;
              case '"':
              case '\'':
              case '>':
                goto label_18;
              case '&':
                goto label_20;
              case '<':
                goto label_19;
              default:
                goto label_27;
            }
          }
          else
            goto label_40;
        }
        while (!this.normalize);
        this.stringBuilder.Append(' ');
        ++this.ps.charPos;
        continue;
label_18:
        ++charRefEndPos;
        continue;
label_19:
        this.Throw(charRefEndPos, 21, XmlException.BuildCharExceptionStr('<'));
        goto label_33;
label_20:
        if (charRefEndPos - this.ps.charPos > 0)
          this.stringBuilder.Append(chars, this.ps.charPos, charRefEndPos - this.ps.charPos);
        this.ps.charPos = charRefEndPos;
        LineInfo lineInfo2 = new LineInfo(this.ps.lineNo, this.ps.LinePos + 1);
        switch (this.HandleEntityReference(true, XmlTextReader.EntityExpandType.All, out charRefEndPos))
        {
          case XmlTextReader.EntityType.CharacterDec:
          case XmlTextReader.EntityType.CharacterHex:
          case XmlTextReader.EntityType.CharacterNamed:
            chars = this.ps.chars;
            continue;
          case XmlTextReader.EntityType.ExpandedInAttribute:
            goto label_24;
          case XmlTextReader.EntityType.Unexpanded:
            goto label_23;
          default:
            charRefEndPos = this.ps.charPos;
            goto case XmlTextReader.EntityType.CharacterDec;
        }
label_27:
        if (charRefEndPos != this.ps.charsUsed)
        {
          char invChar = chars[charRefEndPos];
          if (invChar >= '\xD800' && invChar <= '\xDBFF')
          {
            if (charRefEndPos + 1 != this.ps.charsUsed)
            {
              ++charRefEndPos;
              if (chars[charRefEndPos] >= '\xDC00' && chars[charRefEndPos] <= '\xDFFF')
              {
                ++charRefEndPos;
                continue;
              }
            }
            else
              goto label_33;
          }
          this.ThrowInvalidChar(charRefEndPos, invChar);
        }
label_33:
        if (this.ReadData() == 0)
        {
          if (this.ps.charsUsed - this.ps.charPos > 0)
          {
            if (this.ps.chars[this.ps.charPos] != '\r')
              this.Throw(5);
          }
          else if (this.fragmentType != XmlNodeType.Attribute)
            this.Throw(3);
          else
            goto label_40;
        }
        charRefEndPos = this.ps.charPos;
        chars = this.ps.chars;
      }
label_23:
      throw new NotSupportedException();
label_24:
      throw new NotSupportedException();
label_40:
      if (attr.nextAttrValueChunk != null)
      {
        int len = this.stringBuilder.Length - startIndex;
        if (len > 0)
        {
          XmlTextReader.NodeData chunk = new XmlTextReader.NodeData();
          chunk.lineInfo = lineInfo1;
          chunk.depth = attr.depth + 1;
          chunk.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString(startIndex, len));
          this.AddAttributeChunkToList(attr, chunk, ref lastChunk);
        }
      }
      this.ps.charPos = charRefEndPos + 1;
      attr.SetValue(this.stringBuilder.ToString());
      this.stringBuilder.Length = 0;
    }

    private void AddAttributeChunkToList(XmlTextReader.NodeData attr, XmlTextReader.NodeData chunk, ref XmlTextReader.NodeData lastChunk)
    {
      if (lastChunk == null)
      {
        lastChunk = chunk;
        attr.nextAttrValueChunk = chunk;
      }
      else
      {
        lastChunk.nextAttrValueChunk = chunk;
        lastChunk = chunk;
      }
    }

    private bool ParseText()
    {
      int outOrChars = 0;
      if (this.parsingMode != XmlTextReader.ParsingMode.Full)
      {
        int startPos;
        int endPos;
        do
          ;
        while (!this.ParseText(out startPos, out endPos, ref outOrChars));
        return false;
      }
      this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      int startPos1;
      int endPos1;
      if (this.ParseText(out startPos1, out endPos1, ref outOrChars))
      {
        XmlNodeType textNodeType = this.GetTextNodeType(outOrChars);
        if (textNodeType != XmlNodeType.None)
        {
          this.curNode.SetValueNode(textNodeType, this.ps.chars, startPos1, endPos1 - startPos1);
          return true;
        }
      }
      else
      {
        if (outOrChars > 32)
        {
          this.curNode.SetValueNode(XmlNodeType.Text, this.ps.chars, startPos1, endPos1 - startPos1);
          this.nextParsingFunction = this.parsingFunction;
          this.parsingFunction = XmlTextReader.ParsingFunction.PartialTextValue;
          return true;
        }
        this.stringBuilder.Append(this.ps.chars, startPos1, endPos1 - startPos1);
        bool text;
        do
        {
          text = this.ParseText(out startPos1, out endPos1, ref outOrChars);
          this.stringBuilder.Append(this.ps.chars, startPos1, endPos1 - startPos1);
        }
        while (!text && outOrChars <= 32 && this.stringBuilder.Length < 4096);
        XmlNodeType type = this.stringBuilder.Length < 4096 ? this.GetTextNodeType(outOrChars) : XmlNodeType.Text;
        if (type == XmlNodeType.None)
        {
          this.stringBuilder.Length = 0;
          if (!text)
          {
            while (!this.ParseText(out startPos1, out endPos1, ref outOrChars))
              ;
          }
        }
        else
        {
          this.curNode.SetValueNode(type, this.stringBuilder.ToString());
          this.stringBuilder.Length = 0;
          if (!text)
          {
            this.nextParsingFunction = this.parsingFunction;
            this.parsingFunction = XmlTextReader.ParsingFunction.PartialTextValue;
          }
          return true;
        }
      }
      return false;
    }

    private bool ParseText(out int startPos, out int endPos, ref int outOrChars)
    {
      char[] chars = this.ps.chars;
      int charRefEndPos = this.ps.charPos;
      int num1 = 0;
      int destPos = -1;
      int num2 = outOrChars;
      char ch;
      while (true)
      {
        int charCount=0;
        int charRefInline=0;
        do
        {
          for (; (ch = chars[charRefEndPos]) > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) ch] & 64) != 0; ++charRefEndPos)
            num2 |= (int) ch;
          switch (ch)
          {
            case '\t':
              ++charRefEndPos;
              continue;
            case '\n':
            case '\r':
              if (this.xmlContext.xmlSpace != XmlSpace.Preserve)
              {
                int num3 = 1;
                if (chars[charRefEndPos] == '\r' && chars[charRefEndPos + 1] == '\n')
                  num3 = 2;
                if (charRefEndPos - this.ps.charPos > 0)
                {
                  if (num1 == 0)
                  {
                    num1 = num3;
                    destPos = charRefEndPos;
                  }
                  else
                  {
                    this.ShiftBuffer(destPos + num1, destPos, charRefEndPos - destPos - num1);
                    destPos = charRefEndPos - num1;
                    num1 += num3;
                  }
                }
                else
                  this.ps.charPos += num3;
                charRefEndPos += num3;
              }
              else if (chars[charRefEndPos] == '\n')
                ++charRefEndPos;
              else if (chars[charRefEndPos + 1] == '\n')
              {
                if (!this.ps.eolNormalized && this.parsingMode == XmlTextReader.ParsingMode.Full)
                {
                  if (charRefEndPos - this.ps.charPos > 0)
                  {
                    if (num1 == 0)
                    {
                      num1 = 1;
                      destPos = charRefEndPos;
                    }
                    else
                    {
                      this.ShiftBuffer(destPos + num1, destPos, charRefEndPos - destPos - num1);
                      destPos = charRefEndPos - num1;
                      ++num1;
                    }
                  }
                  else
                    ++this.ps.charPos;
                }
                charRefEndPos += 2;
              }
              else if (charRefEndPos + 1 < this.ps.charsUsed || this.ps.isEof)
              {
                if (!this.ps.eolNormalized)
                  chars[charRefEndPos] = '\n';
                ++charRefEndPos;
              }
              else
                goto label_51;
              this.OnNewLine(charRefEndPos);
              continue;
            case '&':
              XmlTextReader.EntityType entityType;
              if ((charRefInline = this.ParseCharRefInline(charRefEndPos, out charCount, out entityType)) > 0)
              {
                if (num1 > 0)
                  this.ShiftBuffer(destPos + num1, destPos, charRefEndPos - destPos - num1);
                destPos = charRefEndPos - num1;
                num1 += charRefInline - charRefEndPos - charCount;
                charRefEndPos = charRefInline;
                continue;
              }
              goto label_34;
            case '<':
              goto label_58;
            case ']':
              goto label_41;
            default:
              goto label_45;
          }
        }
        while (this.xmlCharType.IsWhiteSpace(chars[charRefInline - charCount]));
        num2 |= (int) byte.MaxValue;
        continue;
label_34:
        if (charRefEndPos <= this.ps.charPos)
        {
          switch (this.HandleEntityReference(false, XmlTextReader.EntityExpandType.All, out charRefEndPos))
          {
            case XmlTextReader.EntityType.CharacterDec:
            case XmlTextReader.EntityType.CharacterHex:
            case XmlTextReader.EntityType.CharacterNamed:
              if (!this.xmlCharType.IsWhiteSpace(this.ps.chars[charRefEndPos - 1]))
              {
                num2 |= (int) byte.MaxValue;
                break;
              }
              break;
            case XmlTextReader.EntityType.Unexpanded:
              goto label_36;
            default:
              charRefEndPos = this.ps.charPos;
              break;
          }
          chars = this.ps.chars;
          continue;
        }
        goto label_58;
label_41:
        if (this.ps.charsUsed - charRefEndPos >= 3 || this.ps.isEof)
        {
          if (chars[charRefEndPos + 1] == ']' && chars[charRefEndPos + 2] == '>')
            this.Throw(charRefEndPos, 43);
          num2 |= 93;
          ++charRefEndPos;
          continue;
        }
        goto label_51;
label_45:
        if (charRefEndPos != this.ps.charsUsed)
        {
          char invChar = chars[charRefEndPos];
          if (invChar >= '\xD800' && invChar <= '\xDBFF')
          {
            if (charRefEndPos + 1 != this.ps.charsUsed)
            {
              ++charRefEndPos;
              if (chars[charRefEndPos] >= '\xDC00' && chars[charRefEndPos] <= '\xDFFF')
              {
                ++charRefEndPos;
                num2 |= (int) invChar;
                continue;
              }
            }
            else
              goto label_51;
          }
          this.ThrowInvalidChar(this.ps.charPos + (charRefEndPos - this.ps.charPos), invChar);
        }
label_51:
        if (charRefEndPos <= this.ps.charPos)
        {
          if (this.ReadData() == 0)
          {
            if (this.ps.charsUsed - this.ps.charPos > 0)
            {
              if (this.ps.chars[this.ps.charPos] != '\r')
                this.Throw(5);
            }
            else
              goto label_56;
          }
          charRefEndPos = this.ps.charPos;
          chars = this.ps.chars;
        }
        else
          goto label_58;
      }
label_36:
      throw new NotSupportedException();
label_56:
      startPos = endPos = charRefEndPos;
      return true;
label_58:
      if (this.parsingMode == XmlTextReader.ParsingMode.Full && num1 > 0)
        this.ShiftBuffer(destPos + num1, destPos, charRefEndPos - destPos - num1);
      startPos = this.ps.charPos;
      endPos = charRefEndPos - num1;
      this.ps.charPos = charRefEndPos;
      outOrChars = num2;
      return ch == '<';
    }

    private void FinishPartialValue()
    {
      this.curNode.CopyTo(this.readValueOffset, this.stringBuilder);
      int outOrChars = 0;
      int startPos;
      int endPos;
      while (!this.ParseText(out startPos, out endPos, ref outOrChars))
        this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
      this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
      this.curNode.SetValue(this.stringBuilder.ToString());
      this.stringBuilder.Length = 0;
    }

    private void FinishOtherValueIterator()
    {
      switch (this.parsingFunction)
      {
        case XmlTextReader.ParsingFunction.InReadValueChunk:
          if (this.incReadState == XmlTextReader.IncrementalReadState.ReadValueChunk_OnPartialValue)
          {
            this.FinishPartialValue();
            this.incReadState = XmlTextReader.IncrementalReadState.ReadValueChunk_OnCachedValue;
            break;
          }
          if (this.readValueOffset <= 0)
            break;
          this.curNode.SetValue(this.curNode.StringValue.Substring(this.readValueOffset));
          this.readValueOffset = 0;
          break;
      }
    }

    private void SkipPartialTextValue()
    {
      int outOrChars = 0;
      this.parsingFunction = this.nextParsingFunction;
      int startPos;
      int endPos;
      do
        ;
      while (!this.ParseText(out startPos, out endPos, ref outOrChars));
    }

    private void FinishReadValueChunk()
    {
      this.readValueOffset = 0;
      if (this.incReadState == XmlTextReader.IncrementalReadState.ReadValueChunk_OnPartialValue)
      {
        this.SkipPartialTextValue();
      }
      else
      {
        this.parsingFunction = this.nextParsingFunction;
        this.nextParsingFunction = this.nextNextParsingFunction;
      }
    }

    private bool ParseRootLevelWhitespace()
    {
      XmlNodeType whitespaceType = this.GetWhitespaceType();
      if (whitespaceType == XmlNodeType.None)
      {
        this.EatWhitespaces((BufferBuilder) null);
        if (this.ps.chars[this.ps.charPos] == '<' || this.ps.charsUsed - this.ps.charPos == 0)
          return false;
      }
      else
      {
        this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
        this.EatWhitespaces(this.stringBuilder);
        if (this.ps.chars[this.ps.charPos] == '<' || this.ps.charsUsed - this.ps.charPos == 0)
        {
          if (this.stringBuilder.Length <= 0)
            return false;
          this.curNode.SetValueNode(whitespaceType, this.stringBuilder.ToString());
          this.stringBuilder.Length = 0;
          return true;
        }
      }
      if (this.ps.chars[this.ps.charPos] == char.MinValue)
        ++this.ps.charPos;
      else if (this.xmlCharType.IsCharData(this.ps.chars[this.ps.charPos]))
        this.Throw(24);
      else
        this.ThrowInvalidChar(this.ps.charPos, this.ps.chars[this.ps.charPos]);
      return false;
    }

    private XmlTextReader.EntityType HandleEntityReference(bool isInAttributeValue, XmlTextReader.EntityExpandType expandType, out int charRefEndPos)
    {
      if (this.ps.charPos + 1 == this.ps.charsUsed && this.ReadData() == 0)
        this.Throw(5);
      if (this.ps.chars[this.ps.charPos + 1] == '#')
      {
        XmlTextReader.EntityType entityType;
        charRefEndPos = this.ParseNumericCharRef(expandType != XmlTextReader.EntityExpandType.OnlyGeneral, (BufferBuilder) null, out entityType);
        return entityType;
      }
      charRefEndPos = this.ParseNamedCharRef(expandType != XmlTextReader.EntityExpandType.OnlyGeneral, (BufferBuilder) null);
      return charRefEndPos >= 0 ? XmlTextReader.EntityType.CharacterNamed : XmlTextReader.EntityType.Unexpanded;
    }

    private bool ParsePI()
    {
      return this.ParsePI((BufferBuilder) null);
    }

    private bool ParsePI(BufferBuilder piInDtdStringBuilder)
    {
      if (this.parsingMode == XmlTextReader.ParsingMode.Full)
        this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      int name = this.ParseName();
      string str = this.nameTable.Add(this.ps.chars, this.ps.charPos, name - this.ps.charPos);
      if (string.Compare(str, "xml") == 0)
        this.Throw(str.Equals((object) "xml") ? 25 : 28, str);
      this.ps.charPos = name;
      if (piInDtdStringBuilder == null)
      {
        if (!this.ignorePIs && this.parsingMode == XmlTextReader.ParsingMode.Full)
          this.curNode.SetNamedNode(XmlNodeType.ProcessingInstruction, str);
      }
      else
        piInDtdStringBuilder.Append(str);
      char ch = this.ps.chars[this.ps.charPos];
      if (this.EatWhitespaces(piInDtdStringBuilder) == 0)
      {
        if (this.ps.charsUsed - this.ps.charPos < 2)
          this.ReadData();
        if (ch != '?' || this.ps.chars[this.ps.charPos + 1] != '>')
          this.Throw(8, XmlException.BuildCharExceptionStr(ch));
      }
      int outStartPos;
      int outEndPos;
      if (this.ParsePIValue(out outStartPos, out outEndPos))
      {
        if (piInDtdStringBuilder == null)
        {
          if (this.ignorePIs)
            return false;
          if (this.parsingMode == XmlTextReader.ParsingMode.Full)
            this.curNode.SetValue(this.ps.chars, outStartPos, outEndPos - outStartPos);
        }
        else
          piInDtdStringBuilder.Append(this.ps.chars, outStartPos, outEndPos - outStartPos);
      }
      else
      {
        BufferBuilder bufferBuilder;
        if (piInDtdStringBuilder == null)
        {
          if (this.ignorePIs || this.parsingMode != XmlTextReader.ParsingMode.Full)
          {
            do
              ;
            while (!this.ParsePIValue(out outStartPos, out outEndPos));
            return false;
          }
          bufferBuilder = this.stringBuilder;
        }
        else
          bufferBuilder = piInDtdStringBuilder;
        do
        {
          bufferBuilder.Append(this.ps.chars, outStartPos, outEndPos - outStartPos);
        }
        while (!this.ParsePIValue(out outStartPos, out outEndPos));
        bufferBuilder.Append(this.ps.chars, outStartPos, outEndPos - outStartPos);
        if (piInDtdStringBuilder == null)
        {
          this.curNode.SetValue(this.stringBuilder.ToString());
          this.stringBuilder.Length = 0;
        }
      }
      return true;
    }

    private bool ParsePIValue(out int outStartPos, out int outEndPos)
    {
      if (this.ps.charsUsed - this.ps.charPos < 2 && this.ReadData() == 0)
        this.Throw(this.ps.charsUsed, 4, "PI");
      int charPos = this.ps.charPos;
      char[] chars = this.ps.chars;
      int num = 0;
      int destPos = -1;
      while (true)
      {
        while (chars[charPos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[charPos]] & 64) != 0 && chars[charPos] != '?')
          ++charPos;
        switch (chars[charPos])
        {
          case '\t':
          case '&':
          case '<':
          case ']':
            ++charPos;
            continue;
          case '\n':
            ++charPos;
            this.OnNewLine(charPos);
            continue;
          case '\r':
            if (chars[charPos + 1] == '\n')
            {
              if (!this.ps.eolNormalized && this.parsingMode == XmlTextReader.ParsingMode.Full)
              {
                if (charPos - this.ps.charPos > 0)
                {
                  if (num == 0)
                  {
                    num = 1;
                    destPos = charPos;
                  }
                  else
                  {
                    this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
                    destPos = charPos - num;
                    ++num;
                  }
                }
                else
                  ++this.ps.charPos;
              }
              charPos += 2;
            }
            else if (charPos + 1 < this.ps.charsUsed || this.ps.isEof)
            {
              if (!this.ps.eolNormalized)
                chars[charPos] = '\n';
              ++charPos;
            }
            else
              goto label_34;
            this.OnNewLine(charPos);
            continue;
          case '?':
            if (chars[charPos + 1] != '>')
            {
              if (charPos + 1 != this.ps.charsUsed)
              {
                ++charPos;
                continue;
              }
              goto label_34;
            }
            else
              goto label_7;
          default:
            if (charPos != this.ps.charsUsed)
            {
              char invChar = chars[charPos];
              if (invChar >= '\xD800' && invChar <= '\xDBFF')
              {
                if (charPos + 1 != this.ps.charsUsed)
                {
                  ++charPos;
                  if (chars[charPos] >= '\xDC00' && chars[charPos] <= '\xDFFF')
                  {
                    ++charPos;
                    continue;
                  }
                }
                else
                  goto label_34;
              }
              this.ThrowInvalidChar(charPos, invChar);
              continue;
            }
            goto label_34;
        }
      }
label_7:
      if (num > 0)
      {
        this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
        outEndPos = charPos - num;
      }
      else
        outEndPos = charPos;
      outStartPos = this.ps.charPos;
      this.ps.charPos = charPos + 2;
      return true;
label_34:
      if (num > 0)
      {
        this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
        outEndPos = charPos - num;
      }
      else
        outEndPos = charPos;
      outStartPos = this.ps.charPos;
      this.ps.charPos = charPos;
      return false;
    }

    private bool ParseComment()
    {
      if (this.ignoreComments)
      {
        XmlTextReader.ParsingMode parsingMode = this.parsingMode;
        this.parsingMode = XmlTextReader.ParsingMode.SkipNode;
        this.ParseCDataOrComment(XmlNodeType.Comment);
        this.parsingMode = parsingMode;
        return false;
      }
      this.ParseCDataOrComment(XmlNodeType.Comment);
      return true;
    }

    private void ParseCData()
    {
      this.ParseCDataOrComment(XmlNodeType.CDATA);
    }

    private void ParseCDataOrComment(XmlNodeType type)
    {
      if (this.parsingMode == XmlTextReader.ParsingMode.Full)
      {
        this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
        int outStartPos;
        int outEndPos;
        if (this.ParseCDataOrComment(type, out outStartPos, out outEndPos))
        {
          this.curNode.SetValueNode(type, this.ps.chars, outStartPos, outEndPos - outStartPos);
        }
        else
        {
          do
          {
            this.stringBuilder.Append(this.ps.chars, outStartPos, outEndPos - outStartPos);
          }
          while (!this.ParseCDataOrComment(type, out outStartPos, out outEndPos));
          this.stringBuilder.Append(this.ps.chars, outStartPos, outEndPos - outStartPos);
          this.curNode.SetValueNode(type, this.stringBuilder.ToString());
          this.stringBuilder.Length = 0;
        }
      }
      else
      {
        int outStartPos;
        int outEndPos;
        do
          ;
        while (!this.ParseCDataOrComment(type, out outStartPos, out outEndPos));
      }
    }

    private bool ParseCDataOrComment(XmlNodeType type, out int outStartPos, out int outEndPos)
    {
      if (this.ps.charsUsed - this.ps.charPos < 3 && this.ReadData() == 0)
        this.Throw(4, type == XmlNodeType.Comment ? "Comment" : "CDATA");
      int charPos = this.ps.charPos;
      char[] chars = this.ps.chars;
      int num = 0;
      int destPos = -1;
      char ch = type == XmlNodeType.Comment ? '-' : ']';
      char invChar;
      while (true)
      {
        while (chars[charPos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[charPos]] & 64) != 0 && (int) chars[charPos] != (int) ch)
          ++charPos;
        if ((int) chars[charPos] == (int) ch)
        {
          if ((int) chars[charPos + 1] == (int) ch)
          {
            if (chars[charPos + 2] != '>')
            {
              if (charPos + 2 != this.ps.charsUsed)
              {
                if (type == XmlNodeType.Comment)
                  this.Throw(charPos, 58);
              }
              else
                goto label_39;
            }
            else
              break;
          }
          else if (charPos + 1 == this.ps.charsUsed)
            goto label_39;
          ++charPos;
        }
        else
        {
          switch (chars[charPos])
          {
            case '\t':
            case '&':
            case '<':
            case ']':
              ++charPos;
              continue;
            case '\n':
              ++charPos;
              this.OnNewLine(charPos);
              continue;
            case '\r':
              if (chars[charPos + 1] == '\n')
              {
                if (!this.ps.eolNormalized && this.parsingMode == XmlTextReader.ParsingMode.Full)
                {
                  if (charPos - this.ps.charPos > 0)
                  {
                    if (num == 0)
                    {
                      num = 1;
                      destPos = charPos;
                    }
                    else
                    {
                      this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
                      destPos = charPos - num;
                      ++num;
                    }
                  }
                  else
                    ++this.ps.charPos;
                }
                charPos += 2;
              }
              else if (charPos + 1 < this.ps.charsUsed || this.ps.isEof)
              {
                if (!this.ps.eolNormalized)
                  chars[charPos] = '\n';
                ++charPos;
              }
              else
                goto label_39;
              this.OnNewLine(charPos);
              continue;
            default:
              if (charPos != this.ps.charsUsed)
              {
                invChar = chars[charPos];
                if (invChar >= '\xD800' && invChar <= '\xDBFF')
                {
                  if (charPos + 1 != this.ps.charsUsed)
                  {
                    ++charPos;
                    if (chars[charPos] >= '\xDC00' && chars[charPos] <= '\xDFFF')
                    {
                      ++charPos;
                      continue;
                    }
                    goto label_38;
                  }
                  else
                    goto label_39;
                }
                else
                  goto label_38;
              }
              else
                goto label_39;
          }
        }
      }
      if (num > 0)
      {
        this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
        outEndPos = charPos - num;
      }
      else
        outEndPos = charPos;
      outStartPos = this.ps.charPos;
      this.ps.charPos = charPos + 3;
      return true;
label_38:
      this.ThrowInvalidChar(charPos, invChar);
label_39:
      if (num > 0)
      {
        this.ShiftBuffer(destPos + num, destPos, charPos - destPos - num);
        outEndPos = charPos - num;
      }
      else
        outEndPos = charPos;
      outStartPos = this.ps.charPos;
      this.ps.charPos = charPos;
      return false;
    }

    private int EatWhitespaces(BufferBuilder sb)
    {
      int charPos = this.ps.charPos;
      int num = 0;
      char[] chars = this.ps.chars;
      while (true)
      {
        switch (chars[charPos])
        {
          case '\t':
          case ' ':
            ++charPos;
            continue;
          case '\n':
            ++charPos;
            this.OnNewLine(charPos);
            continue;
          case '\r':
            if (chars[charPos + 1] == '\n')
            {
              int count = charPos - this.ps.charPos;
              if (sb != null && !this.ps.eolNormalized)
              {
                if (count > 0)
                {
                  this.stringBuilder.Append(chars, this.ps.charPos, count);
                  num += count;
                }
                this.ps.charPos = charPos + 1;
              }
              charPos += 2;
            }
            else if (charPos + 1 < this.ps.charsUsed || this.ps.isEof)
            {
              if (!this.ps.eolNormalized)
                chars[charPos] = '\n';
              ++charPos;
            }
            else
              break;
            this.OnNewLine(charPos);
            continue;
          default:
            if (charPos == this.ps.charsUsed)
              break;
            goto label_16;
        }
        int count1 = charPos - this.ps.charPos;
        if (count1 > 0)
        {
          sb?.Append(this.ps.chars, this.ps.charPos, count1);
          this.ps.charPos = charPos;
          num += count1;
        }
        if (this.ReadData() == 0)
        {
          if (this.ps.charsUsed - this.ps.charPos != 0)
          {
            if (this.ps.chars[this.ps.charPos] != '\r')
              this.Throw(5);
          }
          else
            goto label_27;
        }
        charPos = this.ps.charPos;
        chars = this.ps.chars;
      }
label_16:
      int count2 = charPos - this.ps.charPos;
      if (count2 > 0)
      {
        sb?.Append(this.ps.chars, this.ps.charPos, count2);
        this.ps.charPos = charPos;
        num += count2;
      }
      return num;
label_27:
      return num;
    }

    private int ParseCharRefInline(int startPos, out int charCount, out XmlTextReader.EntityType entityType)
    {
      if (this.ps.chars[startPos + 1] == '#')
        return this.ParseNumericCharRefInline(startPos, true, (BufferBuilder) null, out charCount, out entityType);
      charCount = 1;
      entityType = XmlTextReader.EntityType.CharacterNamed;
      return this.ParseNamedCharRefInline(startPos, true, (BufferBuilder) null);
    }

    private int ParseNumericCharRef(bool expand, BufferBuilder internalSubsetBuilder, out XmlTextReader.EntityType entityType)
    {
      int charCount;
      int numericCharRefInline;
      while ((numericCharRefInline = this.ParseNumericCharRefInline(this.ps.charPos, expand, internalSubsetBuilder, out charCount, out entityType)) == -2)
      {
        if (this.ReadData() == 0)
          this.Throw(4);
      }
      if (expand)
        this.ps.charPos = numericCharRefInline - charCount;
      return numericCharRefInline;
    }

    private int ParseNumericCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder, out int charCount, out XmlTextReader.EntityType entityType)
    {
      int num1 = 0;
      char[] chars = this.ps.chars;
      int pos = startPos + 2;
      charCount = 0;
      int res;
      if (chars[pos] == 'x')
      {
        ++pos;
        res = 10;
        while (true)
        {
          char ch = chars[pos];
          if (ch >= '0' && ch <= '9')
            num1 = num1 * 16 + (int) ch - 48;
          else if (ch >= 'a' && ch <= 'f')
            num1 = num1 * 16 + 10 + (int) ch - 97;
          else if (ch >= 'A' && ch <= 'F')
            num1 = num1 * 16 + 10 + (int) ch - 65;
          else
            break;
          ++pos;
        }
        entityType = XmlTextReader.EntityType.CharacterHex;
      }
      else if (pos < this.ps.charsUsed)
      {
        res = 9;
        for (; chars[pos] >= '0' && chars[pos] <= '9'; ++pos)
          num1 = num1 * 10 + (int) chars[pos] - 48;
        entityType = XmlTextReader.EntityType.CharacterDec;
      }
      else
      {
        entityType = XmlTextReader.EntityType.Unexpanded;
        return -2;
      }
      if (chars[pos] != ';')
      {
        if (pos == this.ps.charsUsed)
          return -2;
        this.Throw(pos, res);
      }
      if (num1 <= (int) ushort.MaxValue)
      {
        char ch = (char) num1;
        if ((!this.xmlCharType.IsCharData(ch) || ch >= '\xDC00' && ch <= '\xDEFF') && this.checkCharacters)
          this.ThrowInvalidChar(this.ps.chars[this.ps.charPos + 2] == 'x' ? this.ps.charPos + 3 : this.ps.charPos + 2, ch);
        if (expand)
        {
          internalSubsetBuilder?.Append(this.ps.chars, this.ps.charPos, pos - this.ps.charPos + 1);
          chars[pos] = ch;
        }
        charCount = 1;
        return pos + 1;
      }
      int num2 = num1 - 65536;
      int num3 = 56320 + num2 % 1024;
      int num4 = 55296 + num2 / 1024;
      if (this.normalize)
      {
        char ch1 = (char) num4;
        if (ch1 >= '\xD800' && ch1 <= '\xDBFF')
        {
          char ch2 = (char) num3;
          if (ch2 >= '\xDC00' && ch2 <= '\xDFFF')
            goto label_32;
        }
        this.ThrowInvalidChar(this.ps.chars[this.ps.charPos + 2] == 'x' ? this.ps.charPos + 3 : this.ps.charPos + 2, (char) num1);
      }
label_32:
      if (expand)
      {
        internalSubsetBuilder?.Append(this.ps.chars, this.ps.charPos, pos - this.ps.charPos + 1);
        chars[pos - 1] = (char) num4;
        chars[pos] = (char) num3;
      }
      charCount = 2;
      return pos + 1;
    }

    private int ParseNamedCharRef(bool expand, BufferBuilder internalSubsetBuilder)
    {
      int namedCharRefInline;
      do
      {
        switch (namedCharRefInline = this.ParseNamedCharRefInline(this.ps.charPos, expand, internalSubsetBuilder))
        {
          case -2:
            continue;
          case -1:
            return -1;
          default:
            goto label_4;
        }
      }
      while (this.ReadData() != 0);
      return -1;
label_4:
      if (expand)
        this.ps.charPos = namedCharRefInline - 1;
      return namedCharRefInline;
    }

    private int ParseNamedCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder)
    {
      int index1 = startPos + 1;
      char[] chars = this.ps.chars;
      int num;
      char ch;
      switch (chars[index1])
      {
        case 'a':
          int index2 = index1 + 1;
          if (chars[index2] == 'm')
          {
            if (this.ps.charsUsed - index2 >= 3)
            {
              if (chars[index2 + 1] != 'p' || chars[index2 + 2] != ';')
                return -1;
              num = index2 + 3;
              ch = '&';
              goto label_27;
            }
            else
              break;
          }
          else if (chars[index2] == 'p')
          {
            if (this.ps.charsUsed - index2 >= 4)
            {
              if (chars[index2 + 1] != 'o' || chars[index2 + 2] != 's' || chars[index2 + 3] != ';')
                return -1;
              num = index2 + 4;
              ch = '\'';
              goto label_27;
            }
            else
              break;
          }
          else
          {
            if (index2 < this.ps.charsUsed)
              return -1;
            break;
          }
        case 'g':
          if (this.ps.charsUsed - index1 >= 3)
          {
            if (chars[index1 + 1] != 't' || chars[index1 + 2] != ';')
              return -1;
            num = index1 + 3;
            ch = '>';
            goto label_27;
          }
          else
            break;
        case 'l':
          if (this.ps.charsUsed - index1 >= 3)
          {
            if (chars[index1 + 1] != 't' || chars[index1 + 2] != ';')
              return -1;
            num = index1 + 3;
            ch = '<';
            goto label_27;
          }
          else
            break;
        case 'q':
          if (this.ps.charsUsed - index1 >= 5)
          {
            if (chars[index1 + 1] != 'u' || chars[index1 + 2] != 'o' || (chars[index1 + 3] != 't' || chars[index1 + 4] != ';'))
              return -1;
            num = index1 + 5;
            ch = '"';
            goto label_27;
          }
          else
            break;
        default:
          return -1;
      }
      return -2;
label_27:
      if (expand)
      {
        internalSubsetBuilder?.Append(this.ps.chars, this.ps.charPos, num - this.ps.charPos);
        this.ps.chars[num - 1] = ch;
      }
      return num;
    }

    private int ParseName()
    {
      int colonPos;
      return this.ParseQName(false, 0, out colonPos);
    }

    private int ParseQName(out int colonPos)
    {
      return this.ParseQName(true, 0, out colonPos);
    }

    private int ParseQName(bool isQName, int startOffset, out int colonPos)
    {
      int num = -1;
      int pos = this.ps.charPos + startOffset;
      while (true)
      {
        char[] chars;
        do
        {
          chars = this.ps.chars;
          if (chars[pos] <= 'ÿ' && ((int) this.xmlCharType.charProperties[(int) chars[pos]] & 4) == 0)
          {
            if (pos != this.ps.charsUsed)
              goto label_5;
          }
          else
            goto label_7;
        }
        while (this.ReadDataInName(ref pos));
        this.Throw(pos, 4, "Name");
label_5:
        if (chars[pos] != ':')
          this.Throw(pos, 7, XmlException.BuildCharExceptionStr(chars[pos]));
label_7:
        ++pos;
        while (true)
        {
          while (chars[pos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars[pos]] & 8) != 0)
            ++pos;
          if (chars[pos] != ':')
          {
            if (pos == this.ps.charsUsed)
            {
              if (this.ReadDataInName(ref pos))
                chars = this.ps.chars;
              else
                goto label_15;
            }
            else
              goto label_16;
          }
          else
            break;
        }
        num = pos - this.ps.charPos;
        ++pos;
      }
label_15:
      this.Throw(pos, 4, "Name");
label_16:
      colonPos = num == -1 ? -1 : this.ps.charPos + num;
      return pos;
    }

    private bool ReadDataInName(ref int pos)
    {
      int num = pos - this.ps.charPos;
      bool flag = this.ReadData() != 0;
      pos = this.ps.charPos + num;
      return flag;
    }

    private XmlTextReader.NodeData AddNode(int nodeIndex, int nodeDepth)
    {
      XmlTextReader.NodeData node = this.nodes[nodeIndex];
      if (node == null)
        return this.AllocNode(nodeIndex, nodeDepth);
      node.depth = nodeDepth;
      return node;
    }

    private XmlTextReader.NodeData AllocNode(int nodeIndex, int nodeDepth)
    {
      if (nodeIndex >= this.nodes.Length - 1)
      {
        XmlTextReader.NodeData[] nodeDataArray = new XmlTextReader.NodeData[this.nodes.Length * 2];
        Array.Copy((Array) this.nodes, 0, (Array) nodeDataArray, 0, this.nodes.Length);
        this.nodes = nodeDataArray;
      }
      XmlTextReader.NodeData nodeData = this.nodes[nodeIndex];
      if (nodeData == null)
      {
        nodeData = new XmlTextReader.NodeData();
        this.nodes[nodeIndex] = nodeData;
      }
      nodeData.depth = nodeDepth;
      return nodeData;
    }

    private XmlTextReader.NodeData AddAttributeNoChecks(string name, int attrDepth)
    {
      XmlTextReader.NodeData nodeData = this.AddNode(this.index + this.attrCount + 1, attrDepth);
      nodeData.SetNamedNode(XmlNodeType.Attribute, this.nameTable.Add(name));
      ++this.attrCount;
      return nodeData;
    }

    private XmlTextReader.NodeData AddAttribute(int endNamePos, int colonPos)
    {
      string str = this.nameTable.Add(this.ps.chars, this.ps.charPos, endNamePos - this.ps.charPos);
      if (colonPos > -1)
        return this.AddAttribute(new string(this.ps.chars, colonPos + 1, endNamePos - (colonPos + 1)), new string(this.ps.chars, this.ps.charPos, colonPos - this.ps.charPos), str);
      return this.AddAttribute(str, "", str);
    }

    private XmlTextReader.NodeData AddAttribute(string localName, string prefix, string nameWPrefix)
    {
      XmlTextReader.NodeData nodeData = this.AddNode(this.index + this.attrCount + 1, this.index + 1);
      nodeData.SetNamedNode(XmlNodeType.Attribute, localName, prefix, nameWPrefix);
      int num = 1 << (int) localName[0];
      if ((this.attrHashtable & num) == 0)
        this.attrHashtable |= num;
      else if (this.attrDuplWalkCount < 250)
      {
        ++this.attrDuplWalkCount;
        for (int index = this.index + 1; index < this.index + this.attrCount + 1; ++index)
        {
          if (Ref.Equal(this.nodes[index].localName, nodeData.localName))
          {
            this.attrDuplWalkCount = 250;
            break;
          }
        }
      }
      ++this.attrCount;
      return nodeData;
    }

    private void PopElementContext()
    {
      if (!this.curNode.xmlContextPushed)
        return;
      this.PopXmlContext();
    }

    private void OnNewLine(int pos)
    {
      ++this.ps.lineNo;
      this.ps.lineStartPos = pos - 1;
    }

    private void OnEof()
    {
      this.curNode = this.nodes[0];
      this.curNode.Clear(XmlNodeType.None);
      this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      this.parsingFunction = XmlTextReader.ParsingFunction.Eof;
      this.readState = ReadState.EndOfFile;
      this.reportedEncoding = (Encoding) null;
    }

    private void ResetAttributes()
    {
      if (this.fullAttrCleanup)
        this.FullAttributeCleanup();
      this.curAttrIndex = -1;
      this.attrCount = 0;
      this.attrHashtable = 0;
      this.attrDuplWalkCount = 0;
    }

    private void FullAttributeCleanup()
    {
      for (int index = this.index + 1; index < this.index + this.attrCount + 1; ++index)
      {
        XmlTextReader.NodeData node = this.nodes[index];
        node.nextAttrValueChunk = (XmlTextReader.NodeData) null;
        node.IsDefaultAttribute = false;
      }
      this.fullAttrCleanup = false;
    }

    private void PushXmlContext()
    {
      this.xmlContext = new XmlTextReader.XmlContext(this.xmlContext);
      this.curNode.xmlContextPushed = true;
    }

    private void PopXmlContext()
    {
      this.xmlContext = this.xmlContext.previousContext;
      this.curNode.xmlContextPushed = false;
    }

    private XmlNodeType GetWhitespaceType()
    {
      if (this.whitespaceHandling != WhitespaceHandling.None)
      {
        if (this.xmlContext.xmlSpace == XmlSpace.Preserve)
          return XmlNodeType.SignificantWhitespace;
        if (this.whitespaceHandling == WhitespaceHandling.All)
          return XmlNodeType.Whitespace;
      }
      return XmlNodeType.None;
    }

    private XmlNodeType GetTextNodeType(int orChars)
    {
      if (orChars > 32)
        return XmlNodeType.Text;
      return this.GetWhitespaceType();
    }

    private void InitIncrementalRead(IncrementalReadDecoder decoder)
    {
      this.ResetAttributes();
      decoder.Reset();
      this.incReadDecoder = decoder;
      this.incReadState = XmlTextReader.IncrementalReadState.Text;
      this.incReadDepth = 1;
      this.incReadLeftStartPos = this.ps.charPos;
      this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);
      this.parsingFunction = XmlTextReader.ParsingFunction.InIncrementalRead;
    }

    private int IncrementalRead(Array array, int index, int count)
    {
      if (array == null)
        throw new ArgumentNullException(this.incReadDecoder is IncrementalReadCharsDecoder ? "buffer" : nameof (array));
      if (count < 0)
        throw new ArgumentOutOfRangeException(this.incReadDecoder is IncrementalReadCharsDecoder ? nameof (count) : "len");
      if (index < 0)
        throw new ArgumentOutOfRangeException(this.incReadDecoder is IncrementalReadCharsDecoder ? nameof (index) : "offset");
      if (array.Length - index < count)
        throw new ArgumentException(this.incReadDecoder is IncrementalReadCharsDecoder ? nameof (count) : "len");
      if (count == 0)
        return 0;
      this.curNode.lineInfo = this.incReadLineInfo;
      this.incReadDecoder.SetNextOutputBuffer(array, index, count);
      this.IncrementalRead();
      return this.incReadDecoder.DecodedCount;
    }

    private int IncrementalRead()
    {
      int num1 = 0;
label_1:
      int len1 = this.incReadLeftEndPos - this.incReadLeftStartPos;
      if (len1 > 0)
      {
        int num2;
        try
        {
          num2 = this.incReadDecoder.Decode(this.ps.chars, this.incReadLeftStartPos, len1);
        }
        catch (XmlException ex)
        {
          this.ReThrow((Exception) ex, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
          return 0;
        }
        if (num2 < len1)
        {
          this.incReadLeftStartPos += num2;
          this.incReadLineInfo.linePos += num2;
          return num2;
        }
        this.incReadLeftStartPos = 0;
        this.incReadLeftEndPos = 0;
        this.incReadLineInfo.linePos += num2;
        if (this.incReadDecoder.IsFull)
          return num2;
      }
      int outStartPos = 0;
      int outEndPos = 0;
      int num3;
      do
      {
        int len2;
        do
        {
          switch (this.incReadState)
          {
            case XmlTextReader.IncrementalReadState.PI:
              if (this.ParsePIValue(out outStartPos, out outEndPos))
              {
                this.ps.charPos -= 2;
                this.incReadState = XmlTextReader.IncrementalReadState.Text;
                break;
              }
              break;
            case XmlTextReader.IncrementalReadState.CDATA:
              if (this.ParseCDataOrComment(XmlNodeType.CDATA, out outStartPos, out outEndPos))
              {
                this.ps.charPos -= 3;
                this.incReadState = XmlTextReader.IncrementalReadState.Text;
                break;
              }
              break;
            case XmlTextReader.IncrementalReadState.Comment:
              if (this.ParseCDataOrComment(XmlNodeType.Comment, out outStartPos, out outEndPos))
              {
                this.ps.charPos -= 3;
                this.incReadState = XmlTextReader.IncrementalReadState.Text;
                break;
              }
              break;
            case XmlTextReader.IncrementalReadState.ReadData:
              if (this.ReadData() == 0)
                this.ThrowUnclosedElements();
              this.incReadState = XmlTextReader.IncrementalReadState.Text;
              outStartPos = this.ps.charPos;
              outEndPos = outStartPos;
              goto default;
            case XmlTextReader.IncrementalReadState.EndElement:
              this.parsingFunction = XmlTextReader.ParsingFunction.PopElementContext;
              this.nextParsingFunction = this.index > 0 || this.fragmentType != XmlNodeType.Document ? XmlTextReader.ParsingFunction.ElementContent : XmlTextReader.ParsingFunction.DocumentContent;
              this.Read();
              this.incReadState = XmlTextReader.IncrementalReadState.End;
              goto case XmlTextReader.IncrementalReadState.End;
            case XmlTextReader.IncrementalReadState.End:
              return num1;
            default:
              char[] chars = this.ps.chars;
              outStartPos = this.ps.charPos;
              outEndPos = outStartPos;
              int qname1;
              int qname2;
              while (true)
              {
                do
                {
                  this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);
                  if (this.incReadState == XmlTextReader.IncrementalReadState.Attributes)
                  {
                    char ch;
                    while ((ch = chars[outEndPos]) > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) ch] & 128) != 0 && ch != '/')
                      ++outEndPos;
                  }
                  else
                  {
                    char ch;
                    while ((ch = chars[outEndPos]) > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) ch] & 128) != 0)
                      ++outEndPos;
                  }
                  if (chars[outEndPos] == '&' || chars[outEndPos] == '\t')
                    ++outEndPos;
                  else if (outEndPos - outStartPos <= 0)
                  {
                    switch (chars[outEndPos])
                    {
                      case '\n':
                        ++outEndPos;
                        this.OnNewLine(outEndPos);
                        continue;
                      case '\r':
                        if (chars[outEndPos + 1] == '\n')
                          outEndPos += 2;
                        else if (outEndPos + 1 < this.ps.charsUsed)
                          ++outEndPos;
                        else
                          goto label_75;
                        this.OnNewLine(outEndPos);
                        continue;
                      case '"':
                      case '\'':
                        goto label_68;
                      case '/':
                        goto label_60;
                      case '<':
                        if (this.incReadState != XmlTextReader.IncrementalReadState.Text)
                        {
                          ++outEndPos;
                          continue;
                        }
                        if (this.ps.charsUsed - outEndPos >= 2)
                        {
                          switch (chars[outEndPos + 1])
                          {
                            case '!':
                              if (this.ps.charsUsed - outEndPos >= 4)
                              {
                                if (chars[outEndPos + 2] == '-' && chars[outEndPos + 3] == '-')
                                {
                                  outEndPos += 4;
                                  this.incReadState = XmlTextReader.IncrementalReadState.Comment;
                                  goto label_76;
                                }
                                else
                                {
                                  if (this.ps.charsUsed - outEndPos >= 9)
                                    continue;
                                  goto label_75;
                                }
                              }
                              else
                                goto label_75;
                            case '/':
                              goto label_48;
                            case '?':
                              outEndPos += 2;
                              this.incReadState = XmlTextReader.IncrementalReadState.PI;
                              goto label_76;
                            default:
                              goto label_57;
                          }
                        }
                        else
                          goto label_75;
                      case '>':
                        goto label_65;
                      default:
                        goto label_73;
                    }
                  }
                  else
                    goto label_76;
                }
                while (!this.StrEqual(chars, outEndPos + 2, 7, "[CDATA["));
                break;
label_48:
                int colonPos1;
                qname1 = this.ParseQName(true, 2, out colonPos1);
                if (this.StrEqual(chars, this.ps.charPos + 2, qname1 - this.ps.charPos - 2, this.curNode.GetNameWPrefix(this.nameTable)) && (this.ps.chars[qname1] == '>' || this.xmlCharType.IsWhiteSpace(this.ps.chars[qname1])))
                {
                  if (--this.incReadDepth > 0)
                  {
                    outEndPos = qname1 + 1;
                    continue;
                  }
                  goto label_51;
                }
                else
                {
                  outEndPos = qname1;
                  continue;
                }
label_57:
                int colonPos2;
                qname2 = this.ParseQName(true, 1, out colonPos2);
                if (!this.StrEqual(this.ps.chars, this.ps.charPos + 1, qname2 - this.ps.charPos - 1, this.curNode.localName) || this.ps.chars[qname2] != '>' && this.ps.chars[qname2] != '/' && !this.xmlCharType.IsWhiteSpace(this.ps.chars[qname2]))
                {
                  outEndPos = qname2;
                  outStartPos = this.ps.charPos;
                  chars = this.ps.chars;
                  continue;
                }
                goto label_58;
label_60:
                if (this.incReadState == XmlTextReader.IncrementalReadState.Attributes)
                {
                  if (this.ps.charsUsed - outEndPos >= 2)
                  {
                    if (chars[outEndPos + 1] == '>')
                    {
                      this.incReadState = XmlTextReader.IncrementalReadState.Text;
                      --this.incReadDepth;
                    }
                  }
                  else
                    goto label_75;
                }
                ++outEndPos;
                continue;
label_65:
                if (this.incReadState == XmlTextReader.IncrementalReadState.Attributes)
                  this.incReadState = XmlTextReader.IncrementalReadState.Text;
                ++outEndPos;
                continue;
label_68:
                switch (this.incReadState)
                {
                  case XmlTextReader.IncrementalReadState.Attributes:
                    this.curNode.quoteChar = chars[outEndPos];
                    this.incReadState = XmlTextReader.IncrementalReadState.AttributeValue;
                    break;
                  case XmlTextReader.IncrementalReadState.AttributeValue:
                    if ((int) chars[outEndPos] == (int) this.curNode.quoteChar)
                    {
                      this.incReadState = XmlTextReader.IncrementalReadState.Attributes;
                      break;
                    }
                    break;
                }
                ++outEndPos;
                continue;
label_73:
                if (outEndPos != this.ps.charsUsed)
                  ++outEndPos;
                else
                  goto label_75;
              }
              outEndPos += 9;
              this.incReadState = XmlTextReader.IncrementalReadState.CDATA;
              goto label_76;
label_51:
              this.ps.charPos = qname1;
              if (this.xmlCharType.IsWhiteSpace(this.ps.chars[qname1]))
                this.EatWhitespaces((BufferBuilder) null);
              if (this.ps.chars[this.ps.charPos] != '>')
                this.ThrowUnexpectedToken(">");
              ++this.ps.charPos;
              this.incReadState = XmlTextReader.IncrementalReadState.EndElement;
              goto label_1;
label_58:
              ++this.incReadDepth;
              this.incReadState = XmlTextReader.IncrementalReadState.Attributes;
              outEndPos = qname2;
              goto label_76;
label_75:
              this.incReadState = XmlTextReader.IncrementalReadState.ReadData;
label_76:
              this.ps.charPos = outEndPos;
              break;
          }
          len2 = outEndPos - outStartPos;
        }
        while (len2 <= 0);
        try
        {
          num3 = this.incReadDecoder.Decode(this.ps.chars, outStartPos, len2);
        }
        catch (XmlException ex)
        {
          this.ReThrow((Exception) ex, this.incReadLineInfo.lineNo, this.incReadLineInfo.linePos);
          return 0;
        }
        num1 += num3;
      }
      while (!this.incReadDecoder.IsFull);
      this.incReadLeftStartPos = outStartPos + num3;
      this.incReadLeftEndPos = outEndPos;
      this.incReadLineInfo.linePos += num3;
      return num1;
    }

    private void FinishIncrementalRead()
    {
      this.incReadDecoder = (IncrementalReadDecoder) new IncrementalReadDummyDecoder();
      this.IncrementalRead();
      this.incReadDecoder = (IncrementalReadDecoder) null;
    }

    private bool ParseFragmentAttribute()
    {
      if (this.curNode.type == XmlNodeType.None)
      {
        this.curNode.type = XmlNodeType.Attribute;
        this.curAttrIndex = 0;
        this.ParseAttributeValueSlow(this.ps.charPos, '"', this.curNode);
      }
      else
        this.parsingFunction = XmlTextReader.ParsingFunction.InReadAttributeValue;
      if (this.ReadAttributeValue())
      {
        this.parsingFunction = XmlTextReader.ParsingFunction.FragmentAttribute;
        return true;
      }
      this.OnEof();
      return false;
    }

    private bool ParseAttributeValueChunk()
    {
      char[] chars1 = this.ps.chars;
      int charRefEndPos = this.ps.charPos;
      this.curNode = this.AddNode(this.index + this.attrCount + 1, this.index + 2);
      this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
      while (true)
      {
        while (chars1[charRefEndPos] > 'ÿ' || ((int) this.xmlCharType.charProperties[(int) chars1[charRefEndPos]] & 128) != 0)
          ++charRefEndPos;
        switch (chars1[charRefEndPos])
        {
          case '\t':
          case '\n':
            if (this.normalize)
              chars1[charRefEndPos] = ' ';
            ++charRefEndPos;
            continue;
          case '\r':
            ++charRefEndPos;
            continue;
          case '"':
          case '\'':
          case '>':
            ++charRefEndPos;
            continue;
          case '&':
            if (charRefEndPos - this.ps.charPos > 0)
              this.stringBuilder.Append(chars1, this.ps.charPos, charRefEndPos - this.ps.charPos);
            this.ps.charPos = charRefEndPos;
            switch (this.HandleEntityReference(true, XmlTextReader.EntityExpandType.OnlyCharacter, out charRefEndPos))
            {
              case XmlTextReader.EntityType.CharacterDec:
              case XmlTextReader.EntityType.CharacterHex:
              case XmlTextReader.EntityType.CharacterNamed:
                char[] chars2 = this.ps.chars;
                if (this.normalize && this.xmlCharType.IsWhiteSpace(chars2[this.ps.charPos]) && charRefEndPos - this.ps.charPos == 1)
                {
                  chars2[this.ps.charPos] = ' ';
                  break;
                }
                break;
              case XmlTextReader.EntityType.Unexpanded:
                goto label_15;
            }
            chars1 = this.ps.chars;
            continue;
          case '<':
            this.Throw(charRefEndPos, 21, XmlException.BuildCharExceptionStr('<'));
            break;
          default:
            if (charRefEndPos != this.ps.charsUsed)
            {
              char invChar = chars1[charRefEndPos];
              if (invChar >= '\xD800' && invChar <= '\xDBFF')
              {
                if (charRefEndPos + 1 != this.ps.charsUsed)
                {
                  ++charRefEndPos;
                  if (chars1[charRefEndPos] >= '\xDC00' && chars1[charRefEndPos] <= '\xDFFF')
                  {
                    ++charRefEndPos;
                    continue;
                  }
                }
                else
                  break;
              }
              this.ThrowInvalidChar(charRefEndPos, invChar);
              break;
            }
            break;
        }
        if (charRefEndPos - this.ps.charPos > 0)
        {
          this.stringBuilder.Append(chars1, this.ps.charPos, charRefEndPos - this.ps.charPos);
          this.ps.charPos = charRefEndPos;
        }
        if (this.ReadData() != 0 || this.stringBuilder.Length <= 0)
        {
          charRefEndPos = this.ps.charPos;
          chars1 = this.ps.chars;
        }
        else
          goto label_27;
      }
label_15:
      throw new NotSupportedException();
label_27:
      if (charRefEndPos - this.ps.charPos > 0)
      {
        this.stringBuilder.Append(chars1, this.ps.charPos, charRefEndPos - this.ps.charPos);
        this.ps.charPos = charRefEndPos;
      }
      this.curNode.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString());
      this.stringBuilder.Length = 0;
      return true;
    }

    private void ParseXmlDeclarationFragment()
    {
      try
      {
        this.ParseXmlDeclaration();
      }
      catch (XmlException ex)
      {
        this.ReThrow((Exception) ex, ex.LineNumber, ex.LinePosition - 6);
      }
    }

    private void ThrowUnexpectedToken(int pos, string expectedToken)
    {
      this.ThrowUnexpectedToken(pos, expectedToken, (string) null);
    }

    private void ThrowUnexpectedToken(string expectedToken1)
    {
      this.ThrowUnexpectedToken(expectedToken1, (string) null);
    }

    private void ThrowUnexpectedToken(int pos, string expectedToken1, string expectedToken2)
    {
      this.ps.charPos = pos;
      this.ThrowUnexpectedToken(expectedToken1, expectedToken2);
    }

    private void ThrowUnexpectedToken(string expectedToken1, string expectedToken2)
    {
      string unexpectedToken = this.ParseUnexpectedToken();
      if (expectedToken2 != null)
        this.Throw(17, new string[3]
        {
          unexpectedToken,
          expectedToken1,
          expectedToken2
        });
      else
        this.Throw(15, new string[2]
        {
          unexpectedToken,
          expectedToken1
        });
    }

    private string ParseUnexpectedToken(int pos)
    {
      this.ps.charPos = pos;
      return this.ParseUnexpectedToken();
    }

    private string ParseUnexpectedToken()
    {
      if (!this.xmlCharType.IsNCNameChar(this.ps.chars[this.ps.charPos]))
        return new string(this.ps.chars, this.ps.charPos, 1);
      int index = this.ps.charPos + 1;
      while (this.xmlCharType.IsNCNameChar(this.ps.chars[index]))
        ++index;
      return new string(this.ps.chars, this.ps.charPos, index - this.ps.charPos);
    }

    private int GetIndexOfAttributeWithoutPrefix(string name)
    {
      name = this.nameTable.Get(name);
      if (name == null)
        return -1;
      for (int index = this.index + 1; index < this.index + this.attrCount + 1; ++index)
      {
        if (Ref.Equal(this.nodes[index].localName, name) && this.nodes[index].prefix.Length == 0)
          return index;
      }
      return -1;
    }

    private int GetIndexOfAttributeWithPrefix(string name)
    {
      name = this.nameTable.Add(name);
      if (name == null)
        return -1;
      for (int index = this.index + 1; index < this.index + this.attrCount + 1; ++index)
      {
        if (Ref.Equal(this.nodes[index].GetNameWPrefix(this.nameTable), name))
          return index;
      }
      return -1;
    }

    private bool MoveToNextContentNode(bool moveIfOnContentNode)
    {
      do
      {
        switch (this.curNode.type)
        {
          case XmlNodeType.Attribute:
            return !moveIfOnContentNode;
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
          case XmlNodeType.Whitespace:
          case XmlNodeType.SignificantWhitespace:
            if (!moveIfOnContentNode)
              return true;
            goto case XmlNodeType.ProcessingInstruction;
          case XmlNodeType.EntityReference:
            this.ResolveEntity();
            goto case XmlNodeType.ProcessingInstruction;
          case XmlNodeType.ProcessingInstruction:
          case XmlNodeType.Comment:
          case XmlNodeType.EndEntity:
            moveIfOnContentNode = false;
            continue;
          default:
            return false;
        }
      }
      while (this.Read());
      return false;
    }

    internal XmlNodeType FragmentType
    {
      get
      {
        return this.fragmentType;
      }
    }

    internal void ChangeCurrentNodeType(XmlNodeType newNodeType)
    {
      this.curNode.type = newNodeType;
    }

    internal object InternalTypedValue
    {
      get
      {
        return this.curNode.typedValue;
      }
      set
      {
        this.curNode.typedValue = value;
      }
    }

    internal bool StandAlone
    {
      get
      {
        return this.standalone;
      }
    }

    internal ConformanceLevel V1ComformanceLevel
    {
      get
      {
        return this.fragmentType != XmlNodeType.Element ? ConformanceLevel.Document : ConformanceLevel.Fragment;
      }
    }

    internal static void AdjustLineInfo(char[] chars, int startPos, int endPos, bool isNormalized, ref LineInfo lineInfo)
    {
      int num = -1;
      for (int index = startPos; index < endPos; ++index)
      {
        switch (chars[index])
        {
          case '\n':
            ++lineInfo.lineNo;
            num = index;
            break;
          case '\r':
            if (!isNormalized)
            {
              ++lineInfo.lineNo;
              num = index;
              if (index + 1 < endPos && chars[index + 1] == '\n')
              {
                ++index;
                ++num;
                break;
              }
              break;
            }
            break;
        }
      }
      if (num < 0)
        return;
      lineInfo.linePos = endPos - num;
    }

    private enum ParsingFunction
    {
      ElementContent,
      NoData,
      OpenUrl,
      SwitchToInteractive,
      SwitchToInteractiveXmlDecl,
      DocumentContent,
      MoveToElementContent,
      PopElementContext,
      PopEmptyElementContext,
      ResetAttributesRootLevel,
      Error,
      Eof,
      ReaderClosed,
      InIncrementalRead,
      FragmentAttribute,
      XmlDeclarationFragment,
      GoToEof,
      PartialTextValue,
      InReadAttributeValue,
      InReadValueChunk,
    }

    private enum ParsingMode
    {
      Full,
      SkipNode,
      SkipContent,
    }

    private enum EntityType
    {
      CharacterDec,
      CharacterHex,
      CharacterNamed,
      Expanded,
      ExpandedInAttribute,
      Skipped,
      Unexpanded,
      FakeExpanded,
    }

    private enum EntityExpandType
    {
      OnlyGeneral,
      OnlyCharacter,
      All,
    }

    private enum IncrementalReadState
    {
      Text,
      StartTag,
      PI,
      CDATA,
      Comment,
      Attributes,
      AttributeValue,
      ReadData,
      EndElement,
      End,
      ReadValueChunk_OnCachedValue,
      ReadValueChunk_OnPartialValue,
    }

    private struct ParsingState
    {
      internal char[] chars;
      internal int charPos;
      internal int charsUsed;
      internal Encoding encoding;
      internal bool appendMode;
      internal Stream stream;
      internal Decoder decoder;
      internal byte[] bytes;
      internal int bytePos;
      internal int bytesUsed;
      internal TextReader textReader;
      internal int lineNo;
      internal int lineStartPos;
      internal string baseUriStr;
      internal bool isEof;
      internal bool isStreamEof;
      internal bool eolNormalized;

      internal void Clear()
      {
        this.chars = (char[]) null;
        this.charPos = 0;
        this.charsUsed = 0;
        this.encoding = (Encoding) null;
        this.stream = (Stream) null;
        this.decoder = (Decoder) null;
        this.bytes = (byte[]) null;
        this.bytePos = 0;
        this.bytesUsed = 0;
        this.textReader = (TextReader) null;
        this.lineNo = 1;
        this.lineStartPos = -1;
        this.baseUriStr = "";
        this.isEof = false;
        this.isStreamEof = false;
        this.eolNormalized = true;
      }

      internal void Close(bool closeInput)
      {
        if (!closeInput || this.stream == null)
          return;
        this.stream.Close();
      }

      internal int LineNo
      {
        get
        {
          return this.lineNo;
        }
      }

      internal int LinePos
      {
        get
        {
          return this.charPos - this.lineStartPos;
        }
      }
    }

    private class XmlContext
    {
      internal XmlSpace xmlSpace;
      internal string xmlLang;
      internal XmlNamespaces xmlNamespaces;
      internal XmlTextReader.XmlContext previousContext;

      internal XmlContext()
      {
        this.xmlSpace = XmlSpace.None;
        this.xmlLang = "";
        this.previousContext = (XmlTextReader.XmlContext) null;
        this.xmlNamespaces = new XmlNamespaces();
      }

      internal XmlContext(XmlTextReader.XmlContext previousContext)
      {
        this.xmlSpace = previousContext.xmlSpace;
        this.xmlLang = previousContext.xmlLang;
        this.previousContext = previousContext;
        this.xmlNamespaces = previousContext.xmlNamespaces;
      }
    }

    private class NodeData : IComparable
    {
      internal LineInfo lineInfo2 = new LineInfo(0, 0);
      private static XmlTextReader.NodeData s_None;
      internal XmlNodeType type;
      internal string localName;
      internal string prefix;
      internal string ns;
      internal string nameWPrefix;
      private string value;
      private char[] chars;
      private int valueStartPos;
      private int valueLength;
      internal LineInfo lineInfo;
      internal char quoteChar;
      internal int depth;
      private bool isEmptyOrDefault;
      internal bool xmlContextPushed;
      internal XmlTextReader.NodeData nextAttrValueChunk;
      internal object schemaType;
      internal object typedValue;

      internal static XmlTextReader.NodeData None
      {
        get
        {
          if (XmlTextReader.NodeData.s_None == null)
            XmlTextReader.NodeData.s_None = new XmlTextReader.NodeData();
          return XmlTextReader.NodeData.s_None;
        }
      }

      internal NodeData()
      {
        this.Clear(XmlNodeType.None);
        this.xmlContextPushed = false;
      }

      internal int LineNo
      {
        get
        {
          return this.lineInfo.lineNo;
        }
      }

      internal int LinePos
      {
        get
        {
          return this.lineInfo.linePos;
        }
      }

      internal bool IsEmptyElement
      {
        get
        {
          if (this.type == XmlNodeType.Element)
            return this.isEmptyOrDefault;
          return false;
        }
        set
        {
          this.isEmptyOrDefault = value;
        }
      }

      internal bool IsDefaultAttribute
      {
        get
        {
          if (this.type == XmlNodeType.Attribute)
            return this.isEmptyOrDefault;
          return false;
        }
        set
        {
          this.isEmptyOrDefault = value;
        }
      }

      internal bool ValueBuffered
      {
        get
        {
          return this.value == null;
        }
      }

      internal string StringValue
      {
        get
        {
          if (this.value == null)
            this.value = new string(this.chars, this.valueStartPos, this.valueLength);
          return this.value;
        }
      }

      internal void Clear(XmlNodeType type)
      {
        this.type = type;
        this.ClearName();
        this.value = "";
        this.valueStartPos = -1;
        this.nameWPrefix = "";
        this.schemaType = (object) null;
        this.typedValue = (object) null;
      }

      internal void ClearName()
      {
        this.localName = "";
        this.prefix = "";
        this.ns = "";
        this.nameWPrefix = "";
      }

      internal void SetLineInfo(int lineNo, int linePos)
      {
        this.lineInfo.Set(lineNo, linePos);
      }

      internal void SetLineInfo2(int lineNo, int linePos)
      {
        this.lineInfo2.Set(lineNo, linePos);
      }

      internal void SetValueNode(XmlNodeType type, string value)
      {
        this.type = type;
        this.ClearName();
        this.value = value;
        this.valueStartPos = -1;
      }

      internal void SetValueNode(XmlNodeType type, char[] chars, int startPos, int len)
      {
        this.type = type;
        this.ClearName();
        this.value = (string) null;
        this.chars = chars;
        this.valueStartPos = startPos;
        this.valueLength = len;
      }

      internal void SetNamedNode(XmlNodeType type, string localName)
      {
        this.SetNamedNode(type, localName, "", localName);
      }

      internal void SetNamedNode(XmlNodeType type, string localName, string prefix, string nameWPrefix)
      {
        this.type = type;
        this.localName = localName;
        this.prefix = prefix;
        this.nameWPrefix = nameWPrefix;
        this.ns = "";
        this.value = "";
        this.valueStartPos = -1;
      }

      internal void SetValue(string value)
      {
        this.valueStartPos = -1;
        this.value = value;
      }

      internal void SetValue(char[] chars, int startPos, int len)
      {
        this.value = (string) null;
        this.chars = chars;
        this.valueStartPos = startPos;
        this.valueLength = len;
      }

      internal void OnBufferInvalidated()
      {
        if (this.value == null)
          this.value = new string(this.chars, this.valueStartPos, this.valueLength);
        this.valueStartPos = -1;
      }

      internal string GetAtomizedValue(XmlNameTable nameTable)
      {
        if (this.value == null)
          return nameTable.Add(this.chars, this.valueStartPos, this.valueLength);
        return nameTable.Add(this.value);
      }

      internal void CopyTo(BufferBuilder sb)
      {
        this.CopyTo(0, sb);
      }

      internal void CopyTo(int valueOffset, BufferBuilder sb)
      {
        if (this.value == null)
          sb.Append(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
        else if (valueOffset <= 0)
          sb.Append(this.value);
        else
          sb.Append(this.value, valueOffset, this.value.Length - valueOffset);
      }

      internal int CopyTo(int valueOffset, char[] buffer, int offset, int length)
      {
        if (this.value == null)
        {
          int length1 = this.valueLength - valueOffset;
          if (length1 > length)
            length1 = length;
          Array.Copy((Array) this.chars, this.valueStartPos + valueOffset, (Array) buffer, offset, length1);
          return length1;
        }
        int num = this.value.Length - valueOffset;
        if (num > length)
          num = length;
        for (int index = 0; index < num; ++index)
          buffer[offset + index] = this.value[valueOffset + index];
        return num;
      }

      internal int CopyToBinary(IncrementalReadDecoder decoder, int valueOffset)
      {
        if (this.value == null)
          return decoder.Decode(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
        return decoder.Decode(this.value, valueOffset, this.value.Length - valueOffset);
      }

      internal void AdjustLineInfo(int valueOffset, bool isNormalized, ref LineInfo lineInfo)
      {
        if (valueOffset == 0)
          return;
        if (this.valueStartPos != -1)
        {
          XmlTextReader.AdjustLineInfo(this.chars, this.valueStartPos, this.valueStartPos + valueOffset, isNormalized, ref lineInfo);
        }
        else
        {
          char[] charArray = this.value.Substring(0, valueOffset).ToCharArray();
          XmlTextReader.AdjustLineInfo(charArray, 0, charArray.Length, isNormalized, ref lineInfo);
        }
      }

      internal string GetNameWPrefix(XmlNameTable nt)
      {
        if (this.nameWPrefix != null)
          return this.nameWPrefix;
        return this.CreateNameWPrefix(nt);
      }

      internal string CreateNameWPrefix(XmlNameTable nt)
      {
        this.nameWPrefix = this.prefix.Length != 0 ? nt.Add(this.prefix + ":" + this.localName) : this.localName;
        return this.nameWPrefix;
      }

      int IComparable.CompareTo(object obj)
      {
        XmlTextReader.NodeData nodeData = obj as XmlTextReader.NodeData;
        if (nodeData == null)
          return 1;
        if (!Ref.Equal(this.localName, nodeData.localName))
          return string.Compare(this.localName, nodeData.localName);
        if (Ref.Equal(this.ns, nodeData.ns))
          return 0;
        return string.Compare(this.ns, nodeData.ns);
      }
    }
  }
}
