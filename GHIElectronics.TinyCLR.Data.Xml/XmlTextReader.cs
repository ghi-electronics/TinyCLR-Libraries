////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Threading;

using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace System.Text
{
    internal abstract class TextReader
    {
        public abstract int Read(char[] dest, int index, int count);
    }
}

/////////////////////////////////
namespace GHIElectronics.TinyCLR.Data.Xml
{

    public class XmlTextReader : XmlReader, IXmlLineInfo
    {
        //
        // Private helper types
        //
        // ParsingFunction = what should the reader do when the next Read() is called
        enum ParsingFunction
        {
            ElementContent = 0,
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

            // these two states must be last; see InAttributeValueIterator property
            InReadAttributeValue,
            InReadValueChunk,
        }

        enum ParsingMode
        {
            Full,
            SkipNode,
            SkipContent,
        }

        enum EntityType
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

        enum EntityExpandType
        {
            OnlyGeneral,
            OnlyCharacter,
            All,
        }

        enum IncrementalReadState
        {
            // Following values are used in ReadText, ReadBase64 and ReadBinHex (V1 streaming methods)
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

            // Following values are used in ReadTextChunk, ReadContentAsBase64 and ReadBinHexChunk (V2 streaming methods)
            ReadValueChunk_OnCachedValue,
            ReadValueChunk_OnPartialValue,
        }

        //
        // Fields
        //
        // XmlCharType instance
        XmlCharType xmlCharType = XmlCharType.Instance;

        // current parsing state (aka. scanner data)
        ParsingState ps;

        // parsing function = what to do in the next Read() (3-items-long stack, usually used just 2 level)
        ParsingFunction parsingFunction;
        ParsingFunction nextParsingFunction;
        ParsingFunction nextNextParsingFunction;

        // stack of nodes
        NodeData[] nodes;

        // current node
        NodeData curNode;

        // current index
        int index = 0;

        // attributes info
        int curAttrIndex = -1;
        int attrCount;
        int attrHashtable;
        int attrDuplWalkCount;
        bool fullAttrCleanup;

        // name table
        XmlNameTable nameTable;
        bool nameTableFromSettings;

        // settings
        bool normalize;
        WhitespaceHandling whitespaceHandling;
        EntityHandling entityHandling;
        bool ignorePIs;
        bool ignoreComments;
        bool checkCharacters;
        int lineNumberOffset;
        int linePositionOffset;
        bool closeInput;

        // xml context (xml:space, xml:lang, default namespace)
        XmlContext xmlContext;

        // current node base uri and encoding
        string reportedBaseUri;
        Encoding reportedEncoding;

        // fragment parsing
        XmlNodeType fragmentType = XmlNodeType.Document;

        // incremental read
        IncrementalReadDecoder incReadDecoder;
        IncrementalReadState incReadState;
        int incReadDepth;
        int incReadLeftStartPos;
        int incReadLeftEndPos;
        LineInfo incReadLineInfo;
        IncrementalReadCharsDecoder readCharsDecoder;

        // XmlValidatingReader helpers
#if SCHEMA_VALIDATION
        ValidationEventHandler  validationEventHandler;
#endif //SCHEMA_VALIDATION

        // misc
        BufferBuilder stringBuilder;
        bool rootElementParsed;
        bool standalone;
        ParsingMode parsingMode;
        ReadState readState = ReadState.Initial;
#if SCHEMA_VALIDATION
        SchemaEntity    lastEntity;
#endif //SCHEMA_VALIDATION

        //bool afterResetState;
        int documentStartBytePos;
        int readValueOffset;
        //
        // Atomized string constants
        //
        private string Xml;
        private string XmlNs;

        //
        // Constants
        //
        private const int MaxBytesToMove = 128;
        private const int ApproxXmlDeclLength = 80;
        private const int NodesInitialSize = 8;
        private const int InitialAttributesCount = 4;
        private const int InitialParsingStateStackSize = 2;
        private const int InitialParsingStatesDepth = 2;
        private const int DtdChidrenInitialSize = 2;
        private const int MaxByteSequenceLen = 6;  // max bytes per character
        private const int MaxAttrDuplWalkCount = 250;
        private const int MinWhitespaceLookahedCount = 4096;

        private const string XmlDeclarationBegining = "<?xml";

        internal const int SurHighStart = 0xd800;
        internal const int SurHighEnd = 0xdbff;
        internal const int SurLowStart = 0xdc00;
        internal const int SurLowEnd = 0xdfff;

        //
        // Constructors
        //
        internal XmlTextReader()
        {
            this.curNode = new NodeData();
            this.parsingFunction = ParsingFunction.NoData;
        }

        // Initializes a new instance of the XmlTextReader class with the specified XmlNameTable.
        internal XmlTextReader(XmlNameTable nt)
        {
            Debug.Assert(nt != null);

            this.nameTable = nt;
            nt.Add("");
            this.Xml = nt.Add("xml");
            this.XmlNs = nt.Add("xmlns");

            Debug.Assert(this.index == 0);
            this.nodes = new NodeData[NodesInitialSize];
            this.nodes[0] = new NodeData();
            this.curNode = this.nodes[0];

            this.stringBuilder = new BufferBuilder();
            this.xmlContext = new XmlContext();

            this.parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            this.nextParsingFunction = ParsingFunction.DocumentContent;

            this.entityHandling = EntityHandling.ExpandCharEntities;
            this.whitespaceHandling = WhitespaceHandling.All;
            this.closeInput = true;

            this.ps.lineNo = 1;
            this.ps.lineStartPos = -1;
        }

        // This constructor is used when creating XmlTextReader reader via "XmlReader.Create(..)"
        private XmlTextReader(XmlReaderSettings settings)
        {
            this.xmlContext = new XmlContext();

            // create nametable
            var nt = settings.NameTable;
            if (nt == null)
            {
                nt = new NameTable();
                Debug.Assert(this.nameTableFromSettings == false);
            }
            else
            {
                this.nameTableFromSettings = true;
            }

            this.nameTable = nt;

            nt.Add("");
            this.Xml = nt.Add("xml");
            this.XmlNs = nt.Add("xmlns");

            Debug.Assert(this.index == 0);

            this.nodes = new NodeData[NodesInitialSize];
            this.nodes[0] = new NodeData();
            this.curNode = this.nodes[0];

            this.stringBuilder = new BufferBuilder();

            this.entityHandling = EntityHandling.ExpandEntities;
            this.whitespaceHandling = (settings.IgnoreWhitespace) ? WhitespaceHandling.Significant : WhitespaceHandling.All;
            this.normalize = true;
            this.ignorePIs = settings.IgnoreProcessingInstructions;
            this.ignoreComments = settings.IgnoreComments;
            this.checkCharacters = settings.CheckCharacters;
            this.lineNumberOffset = settings.LineNumberOffset;
            this.linePositionOffset = settings.LinePositionOffset;
            this.ps.lineNo = this.lineNumberOffset + 1;
            this.ps.lineStartPos = -this.linePositionOffset - 1;
            this.curNode.SetLineInfo(this.ps.LineNo - 1, this.ps.LinePos - 1);

            this.parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            this.nextParsingFunction = ParsingFunction.DocumentContent;

            switch (settings.ConformanceLevel)
            {
                case ConformanceLevel.Auto:
                    this.fragmentType = XmlNodeType.None;
                    break;
                case ConformanceLevel.Fragment:
                    this.fragmentType = XmlNodeType.Element;
                    break;
                case ConformanceLevel.Document:
                    this.fragmentType = XmlNodeType.Document;
                    break;
                default:
                    Debug.Assert(false);
                    goto case ConformanceLevel.Document;
            }
        }

        // Initializes a new instance of the XmlTextReader class with the specified stream, baseUri and nametable
        // This constructor is used when creating XmlTextReader for V1 XmlTextReader
        public XmlTextReader(Stream input)
            : this("", input, new NameTable())
        {
        }

        public XmlTextReader(Stream input, XmlNameTable nt)
            : this("", input, nt)
        {
        }

        internal XmlTextReader(string url, Stream input)
            : this(url, input, new NameTable())
        {
        }

        internal XmlTextReader(string url, Stream input, XmlNameTable nt)
            : this(nt)
        {
            if (url == null || url.Length == 0)
            {
                this.InitStreamInput(input);
            }
            else
            {
                this.InitStreamInput(url, input);
            }

            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
        }

        // Initializes a new instance of the XmlTextReader class with the specified arguments.
        // This constructor is used when creating XmlTextReader via XmlReader.Create
        internal XmlTextReader(Stream stream, byte[] bytes, int byteCount, XmlReaderSettings settings, string baseUriStr,
                                    bool closeInput)
            : this(settings)
        {

            // init ParsingState
            this.InitStreamInput(baseUriStr, stream, bytes, byteCount);

            this.closeInput = closeInput;

            this.reportedBaseUri = this.ps.baseUriStr;
            this.reportedEncoding = this.ps.encoding;
        }

        //
        // XmlReader members
        //
        // Returns the current settings of the reader
        public override XmlReaderSettings Settings
        {
            get
            {

                var settings = new XmlReaderSettings();

                if (this.nameTableFromSettings)
                {
                    settings.NameTable = this.nameTable;
                }

                switch (this.fragmentType)
                {
                    case XmlNodeType.None: settings.ConformanceLevel = ConformanceLevel.Auto; break;
                    case XmlNodeType.Element: settings.ConformanceLevel = ConformanceLevel.Fragment; break;
                    case XmlNodeType.Document: settings.ConformanceLevel = ConformanceLevel.Document; break;
                    default: Debug.Assert(false); goto case XmlNodeType.None;
                }
                settings.CheckCharacters = this.checkCharacters;
                settings.LineNumberOffset = this.lineNumberOffset;
                settings.LinePositionOffset = this.linePositionOffset;
                settings.IgnoreWhitespace = (this.whitespaceHandling == WhitespaceHandling.Significant);
                settings.IgnoreProcessingInstructions = this.ignorePIs;
                settings.IgnoreComments = this.ignoreComments;
                settings.ReadOnly = true;
                return settings;
            }
        }

        // Returns the type of the current node.
        public override XmlNodeType NodeType => this.curNode.type;

        // Returns the name of the current node, including prefix.
        public override string Name => this.curNode.GetNameWPrefix(this.nameTable);

        // Returns local name of the current node (without prefix)
        public override string LocalName => this.curNode.localName;

        // Returns namespace name of the current node.
        public override string NamespaceURI =>
                // WsdModification - Adds support for namespace Uri's
                this.curNode.ns;

        // Returns prefix associated with the current node.
        public override string Prefix => this.curNode.prefix;

        // Returns true if the current node can have Value property != string.Empty.
        public override bool HasValue => XmlReader.HasValueInternal(this.curNode.type);

        // Returns the text value of the current node.
        public override string Value
        {
            get
            {
                if (this.parsingFunction >= ParsingFunction.PartialTextValue)
                {
                    if (this.parsingFunction == ParsingFunction.PartialTextValue)
                    {
                        this.FinishPartialValue();
                        this.parsingFunction = this.nextParsingFunction;
                    }
                    else
                    {
                        this.FinishOtherValueIterator();
                    }
                }

                return this.curNode.StringValue;
            }
        }

        // Returns the depth of the current node in the XML element stack
        public override int Depth => this.curNode.depth;

        // Returns the base URI of the current node.
        public override string BaseURI => this.reportedBaseUri;

        // Returns true if the current node is an empty element (for example, <MyElement/>).
        public override bool IsEmptyElement => this.curNode.IsEmptyElement;

        // Returns true of the current node is a default attribute declared in DTD.
        public override bool IsDefault => this.curNode.IsDefaultAttribute;

        // Returns the quote character used in the current attribute declaration
        public override char QuoteChar => this.curNode.type == XmlNodeType.Attribute ? this.curNode.quoteChar : '"';

        // Returns the current xml:space scope.
        public override XmlSpace XmlSpace => this.xmlContext.xmlSpace;

        // Returns the current xml:lang scope.</para>
        public override string XmlLang => this.xmlContext.xmlLang;

        // Returns the current read state of the reader
        public override ReadState ReadState => this.readState;

        // Returns true if the reader reached end of the input data
        public override bool EOF => this.parsingFunction == ParsingFunction.Eof;

        // Returns the XmlNameTable associated with this XmlReader
        public override XmlNameTable NameTable => this.nameTable;

        // Returns true if the XmlReader knows how to resolve general entities
        public override bool CanResolveEntity => false;

        // Returns the number of attributes on the current node.
        public override int AttributeCount => this.attrCount;

        // Returns value of an attribute with the specified Name
        public override string GetAttribute(string name)
        {
            int i;
            if (name.IndexOf(':') == -1)
            {
                i = this.GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                i = this.GetIndexOfAttributeWithPrefix(name);
            }

            return (i >= 0) ? this.nodes[i].StringValue : null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            if (namespaceURI == null || namespaceURI.Length == 0)
            {
                return this.GetAttribute(name);
            }

            return null;
        }

        // Returns value of an attribute at the specified index (position)
        public override string GetAttribute(int i)
        {
            if (i < 0 || i >= this.attrCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }

            return this.nodes[this.index + i + 1].StringValue;
        }

        // Moves to an attribute with the specified Name
        public override bool MoveToAttribute(string name)
        {
            int i;
            if (name.IndexOf(':') == -1)
            {
                i = this.GetIndexOfAttributeWithoutPrefix(name);
            }
            else
            {
                i = this.GetIndexOfAttributeWithPrefix(name);
            }

            if (i >= 0)
            {
                if (this.InAttributeValueIterator)
                {
                    this.FinishAttributeValueIterator();
                }

                this.curAttrIndex = i - this.index - 1;
                this.curNode = this.nodes[i];
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            if (ns == null || ns.Length == 0)
            {
                return this.MoveToAttribute(name);
            }

            return false;
        }

        // Moves to an attribute at the specified index (position)
        public override void MoveToAttribute(int i)
        {
            if (i < 0 || i >= this.attrCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }

            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }

            this.curAttrIndex = i;
            this.curNode = this.nodes[this.index + 1 + this.curAttrIndex];
        }

        // Moves to the first attribute of the current node
        public override bool MoveToFirstAttribute()
        {
            if (this.attrCount == 0)
            {
                return false;
            }

            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }

            this.curAttrIndex = 0;
            this.curNode = this.nodes[this.index + 1];

            return true;
        }

        // Moves to the next attribute of the current node
        public override bool MoveToNextAttribute()
        {
            if (this.curAttrIndex + 1 < this.attrCount)
            {
                if (this.InAttributeValueIterator)
                {
                    this.FinishAttributeValueIterator();
                }

                this.curNode = this.nodes[this.index + 1 + ++this.curAttrIndex];
                return true;
            }

            return false;
        }

        // If on attribute, moves to the element that contains the attribute node
        public override bool MoveToElement()
        {
            if (this.InAttributeValueIterator)
            {
                this.FinishAttributeValueIterator();
            }
            else if (this.curNode.type != XmlNodeType.Attribute)
            {
                return false;
            }

            this.curAttrIndex = -1;
            this.curNode = this.nodes[this.index];

            return true;
        }

        // Reads next node from the input data
        public override bool Read()
        {
            for (; ; )
            {
                switch (this.parsingFunction)
                {
                    case ParsingFunction.ElementContent:
                        return this.ParseElementContent();
                    case ParsingFunction.DocumentContent:
                        return this.ParseDocumentContent();
                    case ParsingFunction.SwitchToInteractive:
                        Debug.Assert(!this.ps.appendMode);
                        this.readState = ReadState.Interactive;
                        this.parsingFunction = this.nextParsingFunction;
                        continue;
                    case ParsingFunction.SwitchToInteractiveXmlDecl:
                        this.readState = ReadState.Interactive;
                        this.parsingFunction = this.nextParsingFunction;
                        if (this.ParseXmlDeclaration())
                        {
                            this.reportedEncoding = this.ps.encoding;
                            return true;
                        }

                        this.reportedEncoding = this.ps.encoding;
                        continue;
                    case ParsingFunction.ResetAttributesRootLevel:
                        this.ResetAttributes();
                        this.curNode = this.nodes[this.index];
                        this.parsingFunction = (this.index == 0) ? ParsingFunction.DocumentContent : ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.MoveToElementContent:
                        this.ResetAttributes();
                        this.index++;
                        this.curNode = this.AddNode(this.index, this.index);
                        this.parsingFunction = ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.PopElementContext:
                        this.PopElementContext();
                        this.parsingFunction = this.nextParsingFunction;
                        Debug.Assert(this.parsingFunction == ParsingFunction.ElementContent ||
                                      this.parsingFunction == ParsingFunction.DocumentContent);
                        continue;
                    case ParsingFunction.PopEmptyElementContext:
                        this.curNode = this.nodes[this.index];
                        Debug.Assert(this.curNode.type == XmlNodeType.Element);
                        this.curNode.IsEmptyElement = false;
                        this.ResetAttributes();
                        this.PopElementContext();
                        this.parsingFunction = this.nextParsingFunction;
                        continue;
                    case ParsingFunction.InReadAttributeValue:
                        this.FinishAttributeValueIterator();
                        this.curNode = this.nodes[this.index];
                        continue;
                    case ParsingFunction.InIncrementalRead:
                        this.FinishIncrementalRead();
                        return true;
                    case ParsingFunction.FragmentAttribute:
                        return this.ParseFragmentAttribute();
                    case ParsingFunction.XmlDeclarationFragment:
                        this.ParseXmlDeclarationFragment();
                        this.parsingFunction = ParsingFunction.GoToEof;
                        return true;
                    case ParsingFunction.GoToEof:
                        this.OnEof();
                        return false;
                    case ParsingFunction.Error:
                    case ParsingFunction.Eof:
                    case ParsingFunction.ReaderClosed:
                        return false;
                    case ParsingFunction.NoData:
                        this.ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                        return false;
                    case ParsingFunction.PartialTextValue:
                        this.SkipPartialTextValue();
                        continue;
                    case ParsingFunction.InReadValueChunk:
                        this.FinishReadValueChunk();
                        continue;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        // Closes the input stream ot TextReader, changes the ReadState to Closed and sets all properties to zero/string.Empty
        public override void Close() => this.Close(this.closeInput);

        // Skips the current node. If on element, skips to the end tag of the element.
        public override void Skip()
        {
            if (this.readState != ReadState.Interactive)
                return;

            switch (this.parsingFunction)
            {
                case ParsingFunction.InReadAttributeValue:
                    this.FinishAttributeValueIterator();
                    this.curNode = this.nodes[this.index];
                    break;
                case ParsingFunction.InIncrementalRead:
                    this.FinishIncrementalRead();
                    break;
                case ParsingFunction.PartialTextValue:
                    this.SkipPartialTextValue();
                    break;
                case ParsingFunction.InReadValueChunk:
                    this.FinishReadValueChunk();
                    break;
            }

            switch (this.curNode.type)
            {
                // skip subtree
                case XmlNodeType.Element:
                    if (this.curNode.IsEmptyElement)
                    {
                        break;
                    }

                    var initialDepth = this.index;
                    this.parsingMode = ParsingMode.SkipContent;
                    // skip content
                    while (this.Read() && this.index > initialDepth) ;
                    Debug.Assert(this.curNode.type == XmlNodeType.EndElement);
                    Debug.Assert(this.parsingFunction != ParsingFunction.Eof);
                    this.parsingMode = ParsingMode.Full;
                    break;
                case XmlNodeType.Attribute:
                    this.MoveToElement();
                    goto case XmlNodeType.Element;
            }

            // move to following sibling node
            this.Read();
            return;
        }

        // WsdModifications...
        // Returns NamespaceURI associated with the specified prefix in the current namespace scope.
        public override string LookupNamespace(string prefix)
        {
            // WsdModification - Added support for namespaces
            var tempContext = this.xmlContext;
            var ns = tempContext.xmlNamespaces[prefix];
            while (ns == null && tempContext.previousContext != null)
            {
                tempContext = tempContext.previousContext;
                if ((ns = tempContext.xmlNamespaces[prefix]) != null)
                    break;
            }

            if (ns != null)
                return ns.NamespaceURI;

            return "";
        }

        // Iterates through the current attribute value's text and entity references chunks.
        public override bool ReadAttributeValue()
        {
            if (this.parsingFunction != ParsingFunction.InReadAttributeValue)
            {
                if (this.curNode.type != XmlNodeType.Attribute)
                {
                    return false;
                }

                if (this.readState != ReadState.Interactive || this.curAttrIndex < 0)
                {
                    return false;
                }

                if (this.parsingFunction == ParsingFunction.InReadValueChunk)
                {
                    this.FinishReadValueChunk();
                }

                if (this.curNode.nextAttrValueChunk == null || this.entityHandling == EntityHandling.ExpandEntities)
                {
                    var simpleValueNode = this.AddNode(this.index + this.attrCount + 1, this.curNode.depth + 1);
                    simpleValueNode.SetValueNode(XmlNodeType.Text, this.curNode.StringValue);
                    simpleValueNode.lineInfo = this.curNode.lineInfo2;
                    simpleValueNode.depth = this.curNode.depth + 1;
                    this.curNode = simpleValueNode;
                    Debug.Assert(this.curNode.nextAttrValueChunk == null);
                }
                else
                {
                    this.curNode = this.curNode.nextAttrValueChunk;

                    // This will initialize the (index + attrCount + 1) place in nodes array
                    this.AddNode(this.index + this.attrCount + 1, this.index + 2);
                    this.nodes[this.index + this.attrCount + 1] = this.curNode;

                    this.fullAttrCleanup = true;
                }

                this.nextParsingFunction = this.parsingFunction;
                this.parsingFunction = ParsingFunction.InReadAttributeValue;
                return true;
            }
            else
            {
                if (this.curNode.nextAttrValueChunk != null)
                {
                    this.curNode = this.curNode.nextAttrValueChunk;
                    this.nodes[this.index + this.attrCount + 1] = this.curNode;
                    return true;
                }

                return false;
            }
        }

        // Resolves the current entity reference node
        public override void ResolveEntity() => throw new NotSupportedException();

        public override bool CanReadBinaryContent => false;

        // Returns true if ReadValue is supported
        public override bool CanReadValueChunk => true;

        // Iterates over Value property and copies it into the provided buffer
        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            // throw on elements
            if (!XmlReader.HasValueInternal(this.curNode.type))
            {
                throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidReadValueChunk, this.curNode.type));
            }

            // check arguments
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // first call of ReadValueChunk -> initialize incremental read state
            if (this.parsingFunction != ParsingFunction.InReadValueChunk)
            {
                if (this.readState != ReadState.Interactive)
                {
                    return 0;
                }

                if (this.parsingFunction == ParsingFunction.PartialTextValue)
                {
                    this.incReadState = IncrementalReadState.ReadValueChunk_OnPartialValue;
                }
                else
                {
                    this.incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    this.nextNextParsingFunction = this.nextParsingFunction;
                    this.nextParsingFunction = this.parsingFunction;
                }

                this.parsingFunction = ParsingFunction.InReadValueChunk;
                this.readValueOffset = 0;
            }

            // read what is already cached in curNode
            var readCount = 0;
            var read = this.curNode.CopyTo(this.readValueOffset, buffer, index + readCount, count - readCount);
            readCount += read;
            this.readValueOffset += read;

            if (readCount == count)
            {
                return readCount;
            }

            // if on partial value, read the rest of it
            if (this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                this.curNode.SetValue("");

                // read next chunk of text
                var endOfValue = false;
                var startPos = 0;
                var endPos = 0;
                while (readCount < count && !endOfValue)
                {
                    var orChars = 0;
                    endOfValue = this.ParseText(out startPos, out endPos, ref orChars);

                    var copyCount = count - readCount;
                    if (copyCount > endPos - startPos)
                    {
                        copyCount = endPos - startPos;
                    }

                    Array.Copy(this.ps.chars, startPos, buffer, (index + readCount), copyCount);

                    readCount += copyCount;
                    startPos += copyCount;
                }

                this.incReadState = endOfValue ? IncrementalReadState.ReadValueChunk_OnCachedValue : IncrementalReadState.ReadValueChunk_OnPartialValue;
                this.readValueOffset = 0;

                this.curNode.SetValue(this.ps.chars, startPos, endPos - startPos);
            }

            return readCount;
        }

        //
        // IXmlLineInfo members
        //
        public bool HasLineInfo() => true;

        // Returns the line number of the current node
        public int LineNumber => this.curNode.LineNo;

        // Returns the line position of the current node
        public int LinePosition => this.curNode.LinePos;

        //
        // XmlTextReader members
        //
        // Disables or enables support of W3C XML 1.0 Namespaces
        internal bool Namespaces {
            get => false;

            set => throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
        }

        // Enables or disables XML 1.0 normalization (incl. end-of-line normalization and normalization of attributes)
        internal bool Normalization {
            get => this.normalize;

            set {
                if (this.readState == ReadState.Closed) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }

                this.normalize = value;
                this.ps.eolNormalized = !value;
            }
        }

        // Returns the Encoding of the XML document
        internal Encoding Encoding => (this.readState == ReadState.Interactive) ? this.reportedEncoding : null;

        // Spefifies whitespace handling of the XML document, i.e. whether return all namespaces, only significant ones or none
        internal WhitespaceHandling WhitespaceHandling {
            get => this.whitespaceHandling;

            set {
                if (this.readState == ReadState.Closed) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }

                if ((uint)value > (uint)WhitespaceHandling.None) {
                    throw new XmlException(Res.Xml_WhitespaceHandling, "");
                }

                this.whitespaceHandling = value;
            }
        }

        // Spefifies whether general entities should be automatically expanded or not
        internal EntityHandling EntityHandling {
            get => this.entityHandling;

            set {
                if (value != EntityHandling.ExpandEntities && value != EntityHandling.ExpandCharEntities) {
                    throw new XmlException(Res.Xml_EntityHandling, "");
                }

                this.entityHandling = value;
            }
        }

        // Reads the contents of an element including markup into a character buffer. Wellformedness checks are limited.
        // This method is designed to read large streams of embedded text by calling it successively.
        internal int ReadChars(char[] buffer, int index, int count)
        {

            if (this.parsingFunction == ParsingFunction.InIncrementalRead)
            {
                if (this.incReadDecoder != this.readCharsDecoder)
                { // mixing ReadChars with ReadBase64 or ReadBinHex
                    if (this.readCharsDecoder == null)
                    {
                        this.readCharsDecoder = new IncrementalReadCharsDecoder();
                    }

                    this.readCharsDecoder.Reset();
                    this.incReadDecoder = this.readCharsDecoder;
                }

                return this.IncrementalRead(buffer, index, count);
            }
            else
            {
                if (this.curNode.type != XmlNodeType.Element)
                {
                    return 0;
                }

                if (this.curNode.IsEmptyElement)
                {
                    this.Read();
                    return 0;
                }

                if (this.readCharsDecoder == null)
                {
                    this.readCharsDecoder = new IncrementalReadCharsDecoder();
                }

                this.InitIncrementalRead(this.readCharsDecoder);
                return this.IncrementalRead(buffer, index, count);
            }
        }

        //
        // Throw methods: Sets the reader current position to pos, sets the error state and throws exception
        //
        void Throw(int pos, int res, string arg)
        {
            this.ps.charPos = pos;
            this.Throw(res, arg);
        }

        void Throw(int pos, int res, string[] args)
        {
            this.ps.charPos = pos;
            this.Throw(res, args);
        }

        void Throw(int pos, int res)
        {
            this.ps.charPos = pos;
            this.Throw(res, "");
        }

        void Throw(int res) => this.Throw(res, "");

        void Throw(int res, int lineNo, int linePos) => this.Throw(new XmlException(res, "", lineNo, linePos, this.ps.baseUriStr));

        void Throw(int res, string arg) => this.Throw(new XmlException(res, arg, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));

        void Throw(int res, string arg, int lineNo, int linePos) => this.Throw(new XmlException(res, arg, lineNo, linePos, this.ps.baseUriStr));

        void Throw(int res, string[] args) => this.Throw(new XmlException(res, args, this.ps.LineNo, this.ps.LinePos, this.ps.baseUriStr));

        void Throw(Exception e)
        {
            this.SetErrorState();
            if (e is XmlException xmlEx) {
                this.curNode.SetLineInfo(xmlEx.LineNumber, xmlEx.LinePosition);
            }

            throw e;
        }

        void ReThrow(Exception e, int lineNo, int linePos) => this.Throw(new XmlException(e.Message, (Exception)null, lineNo, linePos, this.ps.baseUriStr));

        void ThrowWithoutLineInfo(int res) => this.Throw(new XmlException(res, "", this.ps.baseUriStr));

        void ThrowWithoutLineInfo(int res, string arg) => this.Throw(new XmlException(res, arg, this.ps.baseUriStr));

        void ThrowWithoutLineInfo(int res, string[] args) => this.Throw(new XmlException(res, args, this.ps.baseUriStr));

        void ThrowInvalidChar(int pos, char invChar) => this.Throw(pos, Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionStr(invChar));

        private void SetErrorState()
        {
            this.parsingFunction = ParsingFunction.Error;
            this.readState = ReadState.Error;
        }

        //
        // Private implementation methods & properties
        //
        private bool InAttributeValueIterator => this.attrCount > 0 && this.parsingFunction >= ParsingFunction.InReadAttributeValue;

        private void FinishAttributeValueIterator()
        {
            Debug.Assert(this.InAttributeValueIterator);
            if (this.parsingFunction == ParsingFunction.InReadValueChunk)
            {
                this.FinishReadValueChunk();
            }

            if (this.parsingFunction == ParsingFunction.InReadAttributeValue)
            {
                this.parsingFunction = this.nextParsingFunction;
                this.nextParsingFunction = (this.index > 0) ? ParsingFunction.ElementContent : ParsingFunction.DocumentContent;
            }
        }

        private bool DtdValidation => false;

        private void InitStreamInput(Stream stream) => this.InitStreamInput("", stream, null, 0);

        private void InitStreamInput(string baseUriStr, Stream stream)
        {
            Debug.Assert(baseUriStr != null);
            this.InitStreamInput(baseUriStr, stream, null, 0);
        }

        private void InitStreamInput(string baseUriStr, Stream stream, byte[] bytes, int byteCount)
        {

            Debug.Assert(this.ps.charPos == 0 && this.ps.charsUsed == 0 && this.ps.textReader == null);
            this.ps.stream = stream;
            this.ps.baseUriStr = baseUriStr;

            // take over the byte buffer allocated in XmlReader.Create, if available
            int bufferSize;
            if (bytes != null)
            {
                this.ps.bytes = bytes;
                this.ps.bytesUsed = byteCount;
                bufferSize = this.ps.bytes.Length;
            }
            else
            {
                // allocate the byte buffer
                bufferSize = XmlReader.CalcBufferSize(stream);
                if (this.ps.bytes == null || this.ps.bytes.Length < bufferSize)
                {
                    this.ps.bytes = new byte[bufferSize];
                }
            }

            // allocate char buffer
            if (this.ps.chars == null || this.ps.chars.Length < bufferSize + 1)
            {
                this.ps.chars = new char[bufferSize + 1];
            }

            // make sure we have at least 4 bytes to detect the encoding (no preamble of System.Text supported encoding is longer than 4 bytes)
            this.ps.bytePos = 0;
            while (this.ps.bytesUsed < 4 && this.ps.bytes.Length - this.ps.bytesUsed > 0)
            {
                var read = stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);
                if (read == 0)
                {
                    this.ps.isStreamEof = true;
                    break;
                }

                this.ps.bytesUsed += read;
            }

            // detect & setup encoding
            this.ValidateEncoding();

            Encoding encoding = new UTF8Encoding();

            this.ps.encoding = encoding;
            this.ps.decoder = encoding.GetDecoder();

            // eat preamble
            byte[] preamble = { 0xEF, 0xBB, 0xBF }; // UTF8 preamble
            const int preambleLen = 3;
            int i;
            for (i = 0; i < 3 && i < this.ps.bytesUsed; i++)
            {
                if (this.ps.bytes[i] != preamble[i])
                {
                    break;
                }
            }

            if (i == preambleLen)
            {
                this.ps.bytePos = preambleLen;
            }

            this.documentStartBytePos = this.ps.bytePos;

            this.ps.eolNormalized = !this.normalize;

            // decode first characters
            this.ps.appendMode = true;
            this.ReadData();
        }

        private void InitStringInput(string baseUriStr, Encoding originalEncoding, string str)
        {
            Debug.Assert(this.ps.stream == null && this.ps.textReader == null);
            Debug.Assert(this.ps.charPos == 0 && this.ps.charsUsed == 0);
            Debug.Assert(baseUriStr != null);

            this.ps.baseUriStr = baseUriStr;

            var chars = str.ToCharArray();

            var len = chars.Length;
            this.ps.chars = new char[len + 1];

            Array.Copy(chars, this.ps.chars, len);

            this.ps.charsUsed = len;
            this.ps.chars[len] = (char)0;

            this.ps.encoding = originalEncoding;

            this.ps.eolNormalized = !this.normalize;
            this.ps.isEof = true;
        }

        // Stream input only: detect encoding from the first 4 bytes of the byte buffer starting at ps.bytes[ps.bytePos]
        // Validate that the input stream is of encoding that we support (UTF-8)
        private void ValidateEncoding()
        {
            Debug.Assert(this.ps.bytes != null);
            Debug.Assert(this.ps.bytePos == 0);

            if (this.ps.bytesUsed < 2)
            {
                return;
            }

            var first2Bytes = this.ps.bytes[0] << 8 | this.ps.bytes[1];
            var next2Bytes = (this.ps.bytesUsed >= 4) ? (this.ps.bytes[2] << 8 | this.ps.bytes[3]) : 0;

            switch (first2Bytes)
            {
                case 0x0000:
                    switch (next2Bytes)
                    {
                        case 0xFEFF:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Bigendian");
                            break;
                        case 0x003C:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Bigendian");
                            break;
                        case 0xFFFE:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_2143");
                            break;
                        case 0x3C00:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_2143");
                            break;
                    }
                    break;
                case 0xFEFF:
                    if (next2Bytes == 0x0000)
                    {
                        this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_3412");
                        break;
                    }
                    else
                    {
                        this.Throw(Res.Xml_UnknownEncoding, "BigEndianUnicode");
                        break;
                    }
                case 0xFFFE:
                    if (next2Bytes == 0x0000)
                    {
                        this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Littleendian");
                        break;
                    }
                    else
                    {
                        this.Throw(Res.Xml_UnknownEncoding, "Unicode");
                        break;
                    }
                case 0x3C00:
                    switch (next2Bytes)
                    {
                        case 0x0000:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_Littleendian");
                            break;
                        case 0x3F00:
                            this.Throw(Res.Xml_UnknownEncoding, "Unicode");
                            break;
                    }
                    break;
                case 0x003C:
                    switch (next2Bytes)
                    {
                        case 0x0000:
                            this.Throw(Res.Xml_UnknownEncoding, "Ucs4Encoding.UCS4_3412");
                            break;
                        case 0x003F:
                            this.Throw(Res.Xml_UnknownEncoding, "BigEndianUnicode");
                            break;
                    }
                    break;
                case 0x4C6F:
                    if (next2Bytes == 0xA794)
                    {
                        this.Throw(Res.Xml_UnknownEncoding, "ebcdic");
                    }
                    break;
                case 0xEFBB:
                    if ((next2Bytes & 0xFF00) == 0xBF00)
                    {
                        return;
                    }
                    break;
            }
        }

        private void SetupEncoding(Encoding encoding)
        {

        }

        // Reads more data to the character buffer, discarding already parsed chars / decoded bytes.
        int ReadData()
        {
            // Append Mode:  Append new bytes and characters to the buffers, do not rewrite them. Allocate new buffers
            //               if the current ones are full
            // Rewrite Mode: Reuse the buffers. If there is less than half of the char buffer left for new data, move
            //               the characters that has not been parsed yet to the front of the buffer. Same for bytes.

            if (this.ps.isEof)
            {
                return 0;
            }

            int charsRead;
            if (this.ps.appendMode)
            {
                // the character buffer is full -> allocate a new one
                if (this.ps.charsUsed == this.ps.chars.Length - 1)
                {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for (var i = 0; i < this.attrCount; i++)
                    {
                        this.nodes[this.index + i + 1].OnBufferInvalidated();
                    }

                    var newChars = new char[this.ps.chars.Length * 2];
                    Array.Copy(this.ps.chars, 0, newChars, 0, this.ps.chars.Length);
                    this.ps.chars = newChars;
                }

                if (this.ps.stream != null)
                {
                    // the byte buffer is full -> allocate a new one
                    if (this.ps.bytesUsed - this.ps.bytePos < MaxByteSequenceLen)
                    {
                        if (this.ps.bytes.Length - this.ps.bytesUsed < MaxByteSequenceLen)
                        {
                            var newBytes = new byte[this.ps.bytes.Length * 2];
                            Array.Copy(this.ps.bytes, 0, newBytes, 0, this.ps.bytesUsed);
                            this.ps.bytes = newBytes;
                        }
                    }
                }

                charsRead = this.ps.chars.Length - this.ps.charsUsed - 1;
                if (charsRead > ApproxXmlDeclLength)
                {
                    charsRead = ApproxXmlDeclLength;
                }
            }
            else
            {
                var charsLen = this.ps.chars.Length;
                if (charsLen - this.ps.charsUsed <= charsLen / 2)
                {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for (var i = 0; i < this.attrCount; i++)
                    {
                        this.nodes[this.index + i + 1].OnBufferInvalidated();
                    }

                    // move unparsed characters to front, unless the whole buffer contains unparsed characters
                    var copyCharsCount = this.ps.charsUsed - this.ps.charPos;
                    if (copyCharsCount < charsLen - 1)
                    {
                        this.ps.lineStartPos = this.ps.lineStartPos - this.ps.charPos;
                        if (copyCharsCount > 0)
                        {
                            Array.Copy(this.ps.chars, this.ps.charPos, this.ps.chars, 0, copyCharsCount);
                        }

                        this.ps.charPos = 0;
                        this.ps.charsUsed = copyCharsCount;
                    }
                    else
                    {
                        var newChars = new char[this.ps.chars.Length * 2];
                        Array.Copy(this.ps.chars, 0, newChars, 0, this.ps.chars.Length);
                        this.ps.chars = newChars;
                    }
                }

                if (this.ps.stream != null)
                {
                    // move undecoded bytes to the front to make some space in the byte buffer
                    var bytesLeft = this.ps.bytesUsed - this.ps.bytePos;
                    if (bytesLeft <= MaxBytesToMove)
                    {
                        if (bytesLeft == 0)
                        {
                            this.ps.bytesUsed = 0;
                        }
                        else
                        {
                            Array.Copy(this.ps.bytes, this.ps.bytePos, this.ps.bytes, 0, bytesLeft);
                            this.ps.bytesUsed = bytesLeft;
                        }

                        this.ps.bytePos = 0;
                    }
                }

                charsRead = this.ps.chars.Length - this.ps.charsUsed - 1;
            }

            if (this.ps.stream != null)
            {
                if (!this.ps.isStreamEof)
                {
                    // read new bytes
                    if (this.ps.bytes.Length - this.ps.bytesUsed > 0)
                    {
                        var read = this.ps.stream.Read(this.ps.bytes, this.ps.bytesUsed, this.ps.bytes.Length - this.ps.bytesUsed);

                        // Memory stream return 0: EOF
                        // File stream return -1: EOF

                        if (read < 0) 
                            read = 0;

                        if (read == 0)
                        {
                            this.ps.isStreamEof = true;
                        }

                        this.ps.bytesUsed += read;
                    }
                }

                var originalBytePos = this.ps.bytePos;

                // decode chars
                charsRead = this.GetChars(charsRead);
                if (charsRead == 0 && this.ps.bytePos != originalBytePos)
                {
                    // GetChars consumed some bytes but it was not enough bytes to form a character -> try again
                    return this.ReadData();
                }
            }
            else if (this.ps.textReader != null)
            {
                // read chars
                charsRead = this.ps.textReader.Read(this.ps.chars, this.ps.charsUsed, this.ps.chars.Length - this.ps.charsUsed - 1);
                this.ps.charsUsed += charsRead;
            }
            else
            {
                charsRead = 0;
            }

            if (charsRead == 0)
            {
                Debug.Assert(this.ps.charsUsed < this.ps.chars.Length);
                this.ps.isEof = true;
            }

            this.ps.chars[this.ps.charsUsed] = (char)0;
            return charsRead;
        }

        // Stream input only: read bytes from stream and decodes them according to the current encoding
        int GetChars(int maxCharsCount)
        {
            Debug.Assert(this.ps.stream != null && this.ps.decoder != null && this.ps.bytes != null);
            Debug.Assert(maxCharsCount <= this.ps.chars.Length - this.ps.charsUsed - 1);

            // determine the maximum number of bytes we can pass to the decoder
            var bytesCount = this.ps.bytesUsed - this.ps.bytePos;
            if (bytesCount == 0)
            {
                return 0;
            }

            int charsCount;
            try {
                // decode chars
                this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos, bytesCount, this.ps.chars, this.ps.charsUsed, maxCharsCount, false,
                                    out bytesCount, out charsCount, out var completed);
            }
            catch (ArgumentException) {
                this.InvalidCharRecovery(ref bytesCount, out charsCount);
            }

            // move pointers and return
            this.ps.bytePos += bytesCount;
            this.ps.charsUsed += charsCount;
            Debug.Assert(maxCharsCount >= charsCount);
            return charsCount;
        }

        private void InvalidCharRecovery(ref int bytesCount, out int charsCount)
        {
            var charsDecoded = 0;
            var bytesDecoded = 0;
            try
            {
                while (bytesDecoded < bytesCount)
                {
                    this.ps.decoder.Convert(this.ps.bytes, this.ps.bytePos + bytesDecoded, 1, this.ps.chars, this.ps.charsUsed + charsDecoded, 1, false, out var bDec, out var chDec, out var completed);
                    charsDecoded += chDec;
                    bytesDecoded += bDec;
                }

                Debug.Assert(false, "We should get an exception again.");
            }
            catch (ArgumentException)
            {
            }

            if (charsDecoded == 0)
            {
                this.Throw(this.ps.charsUsed, Res.Xml_InvalidCharInThisEncoding);
            }

            charsCount = charsDecoded;
            bytesCount = bytesDecoded;
        }

        internal void Close(bool closeInput)
        {
            if (this.parsingFunction == ParsingFunction.ReaderClosed)
            {
                return;
            }

            this.ps.Close(closeInput);

            this.curNode = NodeData.None;
            this.parsingFunction = ParsingFunction.ReaderClosed;
            this.reportedEncoding = null;
            this.reportedBaseUri = "";
            this.readState = ReadState.Closed;
            this.fullAttrCleanup = false;
            this.ResetAttributes();
        }

        void ShiftBuffer(int sourcePos, int destPos, int count) => Array.Copy(this.ps.chars, sourcePos, this.ps.chars, destPos, count);

        // Compares the given character interval and string and returns true if the characters are identical
        bool StrEqual(char[] chars, int strPos1, int strLen1, string str2)
        {
            if (strLen1 != str2.Length)
            {
                return false;
            }

            var i = 0;
            while (i < strLen1 && chars[strPos1 + i] == str2[i])
            {
                i++;
            }

            return i == strLen1;
        }

        // Parses the xml or text declaration and switched encoding if needed
        private bool ParseXmlDeclaration()
        {
            while (this.ps.charsUsed - this.ps.charPos < 6)
            {  // minimum "<?xml "
                if (this.ReadData() == 0)
                {
                    goto NoXmlDecl;
                }
            }

            if (!this.StrEqual(this.ps.chars, this.ps.charPos, 5, XmlDeclarationBegining) ||
                 this.xmlCharType.IsNameChar(this.ps.chars[this.ps.charPos + 5]))
            {
                goto NoXmlDecl;
            }

            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos + 2);
            this.curNode.SetNamedNode(XmlNodeType.XmlDeclaration, this.Xml);

            this.ps.charPos += 5;

            // parsing of text declarations cannot change global stringBuidler or curNode as we may be in the middle of a text node
            Debug.Assert(this.stringBuilder.Length == 0);
            var sb = this.stringBuilder;

            // parse version, encoding & standalone attributes
            var xmlDeclState = 0;   // <?xml (0) version='1.0' (1) encoding='__' (2) standalone='__' (3) ?>

            for (; ; )
            {
                var originalSbLen = sb.Length;
                var wsCount = this.EatWhitespaces(xmlDeclState == 0 ? null : sb);

                // end of xml declaration
                if (this.ps.chars[this.ps.charPos] == '?')
                {
                    sb.Length = originalSbLen;

                    if (this.ps.chars[this.ps.charPos + 1] == '>')
                    {
                        if (xmlDeclState == 0)
                        {
                            this.Throw(Res.Xml_InvalidXmlDecl);
                        }

                        this.ps.charPos += 2;

                        this.curNode.SetValue(sb.ToString());
                        sb.Length = 0;

                        this.nextParsingFunction = this.parsingFunction;
                        this.parsingFunction = ParsingFunction.ResetAttributesRootLevel;

                        this.ps.appendMode = false;
                        return true;
                    }
                    else if (this.ps.charPos + 1 == this.ps.charsUsed)
                    {
                        goto ReadData;
                    }
                    else
                    {
                        this.ThrowUnexpectedToken("'>'");
                    }
                }

                if (wsCount == 0 && xmlDeclState != 0)
                {
                    this.ThrowUnexpectedToken("?>");
                }

                // read attribute name
                var nameEndPos = this.ParseName();

                NodeData attr = null;
                switch (this.ps.chars[this.ps.charPos])
                {
                    case 'v':
                        if (this.StrEqual(this.ps.chars, this.ps.charPos, nameEndPos - this.ps.charPos, "version") && xmlDeclState == 0)
                        {
                            attr = this.AddAttributeNoChecks("version", 0);
                            break;
                        }

                        goto default;
                    case 'e':
                        if (this.StrEqual(this.ps.chars, this.ps.charPos, nameEndPos - this.ps.charPos, "encoding") &&
                            (xmlDeclState == 1))
                        {
                            attr = this.AddAttributeNoChecks("encoding", 0);
                            xmlDeclState = 1;
                            break;
                        }

                        goto default;
                    case 's':
                        if (this.StrEqual(this.ps.chars, this.ps.charPos, nameEndPos - this.ps.charPos, "standalone") &&
                             (xmlDeclState == 1 || xmlDeclState == 2))
                        {
                            attr = this.AddAttributeNoChecks("standalone", 0);
                            xmlDeclState = 2;
                            break;
                        }

                        goto default;
                    default:
                        this.Throw(Res.Xml_InvalidXmlDecl);
                        break;
                }

                attr.SetLineInfo(this.ps.LineNo, this.ps.LinePos);

                sb.Append(this.ps.chars, this.ps.charPos, nameEndPos - this.ps.charPos);
                this.ps.charPos = nameEndPos;

                // parse equals and quote char;
                if (this.ps.chars[this.ps.charPos] != '=')
                {
                    this.EatWhitespaces(sb);
                    if (this.ps.chars[this.ps.charPos] != '=')
                    {
                        this.ThrowUnexpectedToken("=");
                    }
                }

                sb.Append('=');
                this.ps.charPos++;

                var quoteChar = this.ps.chars[this.ps.charPos];
                if (quoteChar != '"' && quoteChar != '\'')
                {
                    this.EatWhitespaces(sb);
                    quoteChar = this.ps.chars[this.ps.charPos];
                    if (quoteChar != '"' && quoteChar != '\'')
                    {
                        this.ThrowUnexpectedToken("\"", "'");
                    }
                }

                sb.Append(quoteChar);
                this.ps.charPos++;

                attr.quoteChar = quoteChar;
                attr.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);

                // parse attribute value
                var pos = this.ps.charPos;
                char[] chars;
            Continue:
                chars = this.ps.chars;
                while (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (this.ps.chars[pos] == quoteChar)
                {
                    switch (xmlDeclState)
                    {
                        // version
                        case 0:
                            if (this.StrEqual(this.ps.chars, this.ps.charPos, pos - this.ps.charPos, "1.0"))
                            {
                                attr.SetValue(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
                                xmlDeclState = 1;
                            }
                            else
                            {
                                var badVersion = new string(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
                                this.Throw(Res.Xml_InvalidVersionNumber, badVersion);
                            }
                            break;
                        case 1:
                            var encName = new string(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
                            // Check Encoding
                            if (0 != string.Compare(encName.ToLower(), "utf-8"))
                            {
                                this.Throw(Res.Xml_UnknownEncoding, encName);
                            }

                            attr.SetValue(encName);
                            xmlDeclState = 2;
                            break;
                        case 2:
                            if (this.StrEqual(this.ps.chars, this.ps.charPos, pos - this.ps.charPos, "yes"))
                            {
                                this.standalone = true;
                            }
                            else if (this.StrEqual(this.ps.chars, this.ps.charPos, pos - this.ps.charPos, "no"))
                            {
                                this.standalone = false;
                            }
                            else
                            {
                                this.Throw(Res.Xml_InvalidXmlDecl, this.ps.LineNo, this.ps.LinePos - 1);
                            }

                            attr.SetValue(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
                            xmlDeclState = 3;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    sb.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                    sb.Append(quoteChar);
                    this.ps.charPos = pos + 1;
                    continue;
                }
                else if (pos == this.ps.charsUsed)
                {
                    if (this.ReadData() != 0)
                    {
                        goto Continue;
                    }
                    else
                    {
                        this.Throw(Res.Xml_UnclosedQuote);
                    }
                }
                else
                {
                    this.Throw(Res.Xml_InvalidXmlDecl);
                }

            ReadData:
                if (this.ps.isEof || this.ReadData() == 0)
                {
                    this.Throw(Res.Xml_UnexpectedEOF1);
                }
            }

        NoXmlDecl:
// no xml declaration
            this.parsingFunction = this.nextParsingFunction;
            this.ps.appendMode = false;
            return false;
        }

        // Parses the document content
        private bool ParseDocumentContent()
        {

            for (; ; )
            {
                var needMoreChars = false;
                var pos = this.ps.charPos;
                var chars = this.ps.chars;

                // some tag
                if (chars[pos] == '<')
                {
                    needMoreChars = true;
                    if (this.ps.charsUsed - pos < 4) // minimum  "<a/>"
                        goto ReadData;
                    pos++;
                    switch (chars[pos])
                    {
                        // processing instruction
                        case '?':
                            this.ps.charPos = pos + 1;
                            if (this.ParsePI())
                            {
                                return true;
                            }
                            continue;
                        case '!':
                            pos++;
                            if (this.ps.charsUsed - pos < 2) // minimum characters expected "--"
                                goto ReadData;
                            // comment
                            if (chars[pos] == '-')
                            {
                                if (chars[pos + 1] == '-')
                                {
                                    this.ps.charPos = pos + 2;
                                    if (this.ParseComment())
                                    {
                                        return true;
                                    }
                                    continue;
                                }
                                else
                                {
                                    this.ThrowUnexpectedToken(pos + 1, "-");
                                }
                            }

                            // CDATA section
                            else if (chars[pos] == '[')
                            {
                                if (this.fragmentType != XmlNodeType.Document)
                                {
                                    pos++;
                                    if (this.ps.charsUsed - pos < 6)
                                    {
                                        goto ReadData;
                                    }

                                    if (this.StrEqual(chars, pos, 6, "CDATA["))
                                    {
                                        this.ps.charPos = pos + 6;
                                        this.ParseCData();
                                        if (this.fragmentType == XmlNodeType.None)
                                        {
                                            this.fragmentType = XmlNodeType.Element;
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        this.ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else
                                {
                                    this.Throw(this.ps.charPos, Res.Xml_InvalidRootData);
                                }
                            }

                            // DOCTYPE declaration
                            else
                            {
                                Debug.WriteLine("DTDs not supported");
                                throw new NotSupportedException();
                            }
                            break;
                        case '/':
                            this.Throw(pos + 1, Res.Xml_UnexpectedEndTag);
                            break;
                        // document element start tag
                        default:
                            if (this.rootElementParsed)
                            {
                                if (this.fragmentType == XmlNodeType.Document)
                                {
                                    this.Throw(pos, Res.Xml_MultipleRoots);
                                }

                                if (this.fragmentType == XmlNodeType.None)
                                {
                                    this.fragmentType = XmlNodeType.Element;
                                }
                            }

                            this.ps.charPos = pos;
                            this.rootElementParsed = true;
                            this.ParseElement();
                            return true;
                    }
                }
                else if (chars[pos] == '&')
                {
                    if (this.fragmentType == XmlNodeType.Document)
                    {
                        this.Throw(pos, Res.Xml_InvalidRootData);
                    }
                    else
                    {
                        if (this.fragmentType == XmlNodeType.None)
                        {
                            this.fragmentType = XmlNodeType.Element;
                        }

                        switch (this.HandleEntityReference(false, EntityExpandType.OnlyGeneral, out var i)) {
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity reference in xml document");
                                throw new NotSupportedException();
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                if (this.ParseText()) {
                                    return true;
                                }
                                continue;
                            default:
                                chars = this.ps.chars;
                                pos = this.ps.charPos;
                                continue;
                        }
                    }
                }

                // end of buffer
                else if (pos == this.ps.charsUsed)
                {
                    goto ReadData;
                }

                // something else -> root level whitespaces
                else
                {
                    if (this.fragmentType == XmlNodeType.Document)
                    {
                        if (this.ParseRootLevelWhitespace())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (this.ParseText())
                        {
                            if (this.fragmentType == XmlNodeType.None && this.curNode.type == XmlNodeType.Text)
                            {
                                this.fragmentType = XmlNodeType.Element;
                            }

                            return true;
                        }
                    }

                    chars = this.ps.chars;
                    pos = this.ps.charPos;
                    continue;
                }

                Debug.Assert(pos == this.ps.charsUsed && !this.ps.isEof);

            ReadData:
                // read new characters into the buffer
                if (this.ReadData() != 0)
                {
                    pos = this.ps.charPos;
                }
                else
                {
                    if (needMoreChars)
                    {
                        this.Throw(Res.Xml_InvalidRootData);
                    }

                    Debug.Assert(this.index == 0);

                    if (!this.rootElementParsed && this.fragmentType == XmlNodeType.Document)
                    {
                        this.ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                    }

                    if (this.fragmentType == XmlNodeType.None)
                    {
                        this.fragmentType = this.rootElementParsed ? XmlNodeType.Document : XmlNodeType.Element;
                    }

                    this.OnEof();
                    return false;
                }

                pos = this.ps.charPos;
                chars = this.ps.chars;
            }
        }

        // Parses element content
        private bool ParseElementContent()
        {

            for (; ; )
            {
                var pos = this.ps.charPos;
                var chars = this.ps.chars;

                switch (chars[pos])
                {
                    // some tag
                    case '<':
                        switch (chars[pos + 1])
                        {
                            // processing instruction
                            case '?':
                                this.ps.charPos = pos + 2;
                                if (this.ParsePI())
                                {
                                    return true;
                                }
                                continue;
                            case '!':
                                pos += 2;
                                if (this.ps.charsUsed - pos < 2)
                                    goto ReadData;
                                // comment
                                if (chars[pos] == '-')
                                {
                                    if (chars[pos + 1] == '-')
                                    {
                                        this.ps.charPos = pos + 2;
                                        if (this.ParseComment())
                                        {
                                            return true;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        this.ThrowUnexpectedToken(pos + 1, "-");
                                    }
                                }

                                // CDATA section
                                else if (chars[pos] == '[')
                                {
                                    pos++;
                                    if (this.ps.charsUsed - pos < 6)
                                    {
                                        goto ReadData;
                                    }

                                    if (this.StrEqual(chars, pos, 6, "CDATA["))
                                    {
                                        this.ps.charPos = pos + 6;
                                        this.ParseCData();
                                        return true;
                                    }
                                    else
                                    {
                                        this.ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else
                                {

                                    if (this.ParseUnexpectedToken(pos) == "DOCTYPE")
                                    {
                                        this.Throw(Res.Xml_BadDTDLocation);
                                    }
                                    else
                                    {
                                        this.ThrowUnexpectedToken(pos, "<!--", "<[CDATA[");
                                    }
                                }
                                break;
                            // element end tag
                            case '/':
                                this.ps.charPos = pos + 2;

                                this.ParseEndElement();

                                return true;
                            default:
                                // end of buffer
                                if (pos + 1 == this.ps.charsUsed)
                                {
                                    goto ReadData;
                                }
                                else
                                {
                                    // element start tag
                                    this.ps.charPos = pos + 1;
                                    this.ParseElement();
                                    return true;
                                }
                        }
                        break;
                    case '&':
                        int i;
                        switch (this.HandleEntityReference(false, EntityExpandType.OnlyGeneral, out i))
                        {
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity in element content");
                                throw new NotSupportedException();
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                if (this.ParseText())
                                {
                                    return true;
                                }
                                continue;
                            default:
                                chars = this.ps.chars;
                                pos = this.ps.charPos;
                                continue;
                        }
                    default:
                        // end of buffer
                        if (pos == this.ps.charsUsed)
                        {
                            goto ReadData;
                        }
                        else
                        {
                            // text node, whitespace or entity reference
                            if (this.ParseText())
                            {
                                return true;
                            }
                            continue;
                        }
                }

            ReadData:
                // read new characters into the buffer
                if (this.ReadData() == 0)
                {
                    if (this.ps.charsUsed - this.ps.charPos != 0)
                    {
                        this.ThrowUnclosedElements();
                    }

                    if (this.index == 0 && this.fragmentType != XmlNodeType.Document)
                    {
                        this.OnEof();
                        return false;
                    }

                    this.ThrowUnclosedElements();
                }
            }
        }

        private void ThrowUnclosedElements()
        {
            if (this.index == 0 && this.curNode.type != XmlNodeType.Element)
            {
                this.Throw(this.ps.charsUsed, Res.Xml_UnexpectedEOF1);
            }
            else
            {
                var i = (this.parsingFunction == ParsingFunction.InIncrementalRead) ? this.index : this.index - 1;
                this.stringBuilder.Length = 0;
                for (; i >= 0; i--)
                {
                    var el = this.nodes[i];
                    if (el.type != XmlNodeType.Element)
                    {
                        continue;
                    }

                    this.stringBuilder.Append(el.GetNameWPrefix(this.nameTable));
                    if (i > 0)
                    {
                        this.stringBuilder.Append(", ");
                    }
                    else
                    {
                        this.stringBuilder.Append(".");
                    }
                }

                this.Throw(this.ps.charsUsed, Res.Xml_UnexpectedEOFInElementContent, this.stringBuilder.ToString());
            }
        }

        // Parses the element start tag
        private void ParseElement()
        {
            var pos = this.ps.charPos;
            var chars = this.ps.chars;
            var colonPos = -1;

            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);

            // PERF: we intentionally don't call ParseQName here to parse the element name unless a special
        // case occurs (like end of buffer, invalid name char)
        ContinueStartName:
            // check element name start char
            if (!(chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0))
            {
                goto ParseQNameSlow;
            }

            pos++;

        ContinueName:
            // parse element name
            while (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0)
            {
                pos++;
            }

            // colon -> save prefix end position and check next char if it's name start char
            if (chars[pos] == ':')
            {
                if (colonPos != -1)
                {
                    pos++;
                    goto ContinueName;
                }
                else
                {
                    colonPos = pos;
                    pos++;
                    goto ContinueStartName;
                }
            }
            else if (pos < this.ps.charsUsed)
            {
                goto SetElement;
            }

        ParseQNameSlow:
            pos = this.ParseQName(out colonPos);
            chars = this.ps.chars;

        SetElement:
            //curNode.SetNamedNode( XmlNodeType.Element, nameTable.Add( chars, ps.charPos, pos - ps.charPos ));

            // WsdModification - Add support for element namespace
            if (colonPos > -1)
            {
                var nameWPrefix = this.nameTable.Add(chars, this.ps.charPos, pos - this.ps.charPos);
                var localName = new string(chars, colonPos + 1, pos - (colonPos + 1));
                var prefix = new string(chars, this.ps.charPos, colonPos - this.ps.charPos);
                this.curNode.SetNamedNode(XmlNodeType.Element, localName, prefix, nameWPrefix);
            }
            else
                this.curNode.SetNamedNode(XmlNodeType.Element, this.nameTable.Add(chars, this.ps.charPos, pos - this.ps.charPos));

            var ch = chars[pos];
            // white space after element name -> there are probably some attributes
            var isWs = (ch <= XmlCharType.MaxAsciiChar && (this.xmlCharType.charProperties[ch] & XmlCharType.fWhitespace) != 0);
            if (isWs)
            {
                this.ps.charPos = pos;
                this.ParseAttributes();
                this.curNode.ns = this.LookupNamespace(this.curNode.prefix);

                return;
            }

            // no attributes
            else
            {
                // non-empty element
                if (ch == '>')
                {
                    this.ps.charPos = pos + 1;

                    // WsdModification - Add namespace lookup to resolve namespace for this node
                    //                    if (curNode.prefix != null && curNode.prefix != "")
                    this.curNode.ns = this.LookupNamespace(this.curNode.prefix);

                    this.parsingFunction = ParsingFunction.MoveToElementContent;
                }

                // empty element
                else if (ch == '/')
                {
                    if (pos + 1 == this.ps.charsUsed)
                    {
                        this.ps.charPos = pos;
                        if (this.ReadData() == 0)
                        {
                            this.Throw(pos, Res.Xml_UnexpectedEOF, ">");
                        }

                        pos = this.ps.charPos;
                        chars = this.ps.chars;
                    }

                    if (chars[pos + 1] == '>')
                    {
                        // even though there are no attributes, parse them anyway to setup
                        // the current element correctly.  Otherwise an empty element will only 
                        // show up as an EndElement.
                        this.ps.charPos = pos;
                        this.ParseAttributes();

                        this.curNode.ns = this.LookupNamespace(this.curNode.prefix);

                        return;
                    }
                    else
                    {
                        this.ThrowUnexpectedToken(pos, ">");
                    }
                }

                // something else after the element name
                else
                {
                    this.Throw(pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(ch));
                }
            }
        }

        // parses the element end tag
        private void ParseEndElement()
        {
            // check if the end tag name equals start tag name
            var startTagNode = this.nodes[this.index - 1];

            var prefLen = startTagNode.prefix.Length;
            var locLen = startTagNode.localName.Length;

            while (this.ps.charsUsed - this.ps.charPos < prefLen + locLen + 1)
            {
                if (this.ReadData() == 0)
                {
                    break;
                }
            }

            int nameLen;
            var chars = this.ps.chars;
            if (startTagNode.prefix.Length == 0)
            {
                if (!this.StrEqual(chars, this.ps.charPos, locLen, startTagNode.localName))
                {
                    this.ThrowTagMismatch(startTagNode);
                }

                nameLen = locLen;
            }
            else
            {
                var colonPos = this.ps.charPos + prefLen;
                if (!this.StrEqual(chars, this.ps.charPos, prefLen, startTagNode.prefix) ||
                        chars[colonPos] != ':' ||
                        !this.StrEqual(chars, colonPos + 1, locLen, startTagNode.localName))
                {
                    this.ThrowTagMismatch(startTagNode);
                }

                nameLen = locLen + prefLen + 1;
            }

            int pos;
            for (; ; )
            {
                pos = this.ps.charPos + nameLen;
                chars = this.ps.chars;

                if (pos == this.ps.charsUsed)
                {
                    goto ReadData;
                }

                if (chars[pos] > XmlCharType.MaxAsciiChar ||
                     (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0 || chars[pos] == ':')
                {
                    this.ThrowTagMismatch(startTagNode);
                }

                // eat whitespaces
                while (chars[pos] <= XmlCharType.MaxAsciiChar && (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fWhitespace) != 0)
                {
                    pos++;
                }

                if (chars[pos] == '>')
                {
                    break;
                }
                else if (pos == this.ps.charsUsed)
                {
                    goto ReadData;
                }
                else
                {
                    this.ThrowUnexpectedToken(pos, ">");
                }

                Debug.Assert(false, "We should never get to this point.");

            ReadData:
                if (this.ReadData() == 0)
                {
                    this.ThrowUnclosedElements();
                }
            }

            Debug.Assert(this.index > 0);
            this.index--;
            this.curNode = this.nodes[this.index];

            // set the element data
            Debug.Assert(this.curNode == startTagNode);
            startTagNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            startTagNode.type = XmlNodeType.EndElement;

            this.ps.charPos = pos + 1;

            // set next parsing function
            this.nextParsingFunction = (this.index > 0) ? this.parsingFunction : ParsingFunction.DocumentContent;
            this.parsingFunction = ParsingFunction.PopElementContext;
        }

        private void ThrowTagMismatch(NodeData startTag)
        {
            if (startTag.type == XmlNodeType.Element)
            {
                // parse the bad name
                var endPos = this.ParseQName(out var colonPos);

                var args = new string[3];
                args[0] = startTag.GetNameWPrefix(this.nameTable);
                ///////ISSUE: rswaney: SPOT string class doesn't have a culture-aware or case-insensitive compare

                args[1] = startTag.lineInfo.lineNo.ToString(/* CultureInfo.InvariantCulture */);
                args[2] = new string(this.ps.chars, this.ps.charPos, endPos - this.ps.charPos);
                this.Throw(Res.Xml_TagMismatch, args);
            }
            else
            {
                Debug.Assert(startTag.type == XmlNodeType.EntityReference);
                this.Throw(Res.Xml_UnexpectedEndTag);
            }
        }

        // Reads the attributes
        private void ParseAttributes()
        {
            var pos = this.ps.charPos;
            var chars = this.ps.chars;
            NodeData attr = null;

            Debug.Assert(this.attrCount == 0);

            for (; ; )
            {
                // eat whitespaces
                var lineNoDelta = 0;
                char tmpch0;

                while ((tmpch0 = chars[pos]) < XmlCharType.MaxAsciiChar && (this.xmlCharType.charProperties[tmpch0] & XmlCharType.fWhitespace) != 0)
                {
                    if (tmpch0 == (char)0xA)
                    {
                        this.OnNewLine(pos + 1);
                        lineNoDelta++;
                    }
                    else if (tmpch0 == (char)0xD)
                    {
                        if (chars[pos + 1] == (char)0xA)
                        {
                            this.OnNewLine(pos + 2);
                            lineNoDelta++;
                            pos++;
                        }
                        else if (pos + 1 != this.ps.charsUsed)
                        {
                            this.OnNewLine(pos + 1);
                            lineNoDelta++;
                        }
                        else
                        {
                            this.ps.charPos = pos;
                            goto ReadData;
                        }
                    }

                    pos++;
                }

                char tmpch1;
                var isNCStartName = ((tmpch1 = chars[pos]) > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[tmpch1] & XmlCharType.fNCStartName) != 0);
                if (!isNCStartName)
                {
                    // element end
                    if (tmpch1 == '>')
                    {
                        Debug.Assert(this.curNode.type == XmlNodeType.Element);
                        this.ps.charPos = pos + 1;
                        this.parsingFunction = ParsingFunction.MoveToElementContent;
                        goto End;
                    }

                    // empty element end
                    else if (tmpch1 == '/')
                    {
                        Debug.Assert(this.curNode.type == XmlNodeType.Element);
                        if (pos + 1 == this.ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        if (chars[pos + 1] == '>')
                        {
                            this.ps.charPos = pos + 2;
                            this.curNode.IsEmptyElement = true;
                            this.nextParsingFunction = this.parsingFunction;
                            this.parsingFunction = ParsingFunction.PopEmptyElementContext;
                            goto End;
                        }
                        else
                        {
                            this.ThrowUnexpectedToken(pos + 1, ">");
                        }
                    }
                    else if (pos == this.ps.charsUsed)
                    {
                        goto ReadData;
                    }
                    else if (tmpch1 != ':')
                    {
                        this.Throw(pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(tmpch1));
                    }
                }

                if (pos == this.ps.charPos)
                {
                    this.Throw(Res.Xml_ExpectingWhiteSpace, this.ParseUnexpectedToken());
                }

                this.ps.charPos = pos;

                // save attribute name line position
                var attrNameLinePos = this.ps.LinePos;

#if DEBUG
                var attrNameLineNo = this.ps.LineNo;
#endif

                // parse attribute name
                var colonPos = -1;

                // PERF: we intentionally don't call ParseQName here to parse the element name unless a special
                // case occurs (like end of buffer, invalid name char)
                pos++; // start name char has already been checked

                // parse attribute name
            ContinueParseName:
                char tmpch2;

                while ((tmpch2 = chars[pos]) > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[tmpch2] & XmlCharType.fNCName) != 0)
                {
                    pos++;
                }

                // colon -> save prefix end position and check next char if it's name start char
                if (tmpch2 == ':')
                {
                    if (colonPos != -1)
                    {
                        pos++;
                        goto ContinueParseName;
                    }
                    else
                    {
                        colonPos = pos;
                        pos++;

                        if (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0)
                        {
                            pos++;
                            goto ContinueParseName;
                        }

                        pos = this.ParseQName(out colonPos);
                        chars = this.ps.chars;
                    }
                }
                else if (pos == this.ps.charsUsed)
                {
                    pos = this.ParseQName(out colonPos);
                    chars = this.ps.chars;
                }

                attr = this.AddAttribute(pos, colonPos);
                attr.SetLineInfo(this.ps.LineNo, attrNameLinePos);

#if DEBUG
                Debug.Assert(attrNameLineNo == this.ps.LineNo);
#endif

                // parse equals and quote char;
                if (chars[pos] != '=')
                {
                    this.ps.charPos = pos;
                    this.EatWhitespaces(null);
                    pos = this.ps.charPos;
                    if (chars[pos] != '=')
                    {
                        this.ThrowUnexpectedToken("=");
                    }
                }

                pos++;

                var quoteChar = chars[pos];
                if (quoteChar != '"' && quoteChar != '\'')
                {
                    this.ps.charPos = pos;
                    this.EatWhitespaces(null);
                    pos = this.ps.charPos;
                    quoteChar = chars[pos];
                    if (quoteChar != '"' && quoteChar != '\'')
                    {
                        this.ThrowUnexpectedToken("\"", "'");
                    }
                }

                pos++;
                this.ps.charPos = pos;

                attr.quoteChar = quoteChar;
                attr.SetLineInfo2(this.ps.LineNo, this.ps.LinePos);

                // parse attribute value
                char tmpch3;

                while ((tmpch3 = chars[pos]) > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[tmpch3] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (tmpch3 == quoteChar)
                {
                    attr.SetValue(chars, this.ps.charPos, pos - this.ps.charPos);
                    pos++;
                    this.ps.charPos = pos;
                }
                else
                {
                    this.ParseAttributeValueSlow(pos, quoteChar, attr);
                    pos = this.ps.charPos;
                    chars = this.ps.chars;
                }

                // handle special attributes:
                if (attr.prefix.Length != 0)
                {
                    // WsdModification - Added xmlns handling for namespaces
                    if (Ref.Equal(attr.prefix, this.Xml) || Ref.Equal(attr.prefix, this.XmlNs))
                    {
                        this.OnXmlReservedAttribute(attr);
                    }
                }

                // WsdModification - Added xmlns handling for local namespace attribs
                else if (attr.localName == this.XmlNs)
                {
                    attr.localName = string.Empty;
                    attr.prefix = string.Empty;
                    this.OnXmlReservedAttribute(attr);
                }

                continue;

            ReadData:
                this.ps.lineNo -= lineNoDelta;
                if (this.ReadData() != 0)
                {
                    pos = this.ps.charPos;
                    chars = this.ps.chars;
                }
                else
                {
                    this.ThrowUnclosedElements();
                }
            }

        End:
            // check duplicate attributes
            if (this.attrDuplWalkCount >= MaxAttrDuplWalkCount)
            {
                this.AttributeDuplCheck();
            }
        }

        private void AttributeDuplCheck()
        {

            for (var i = this.index + 1; i < this.index + 1 + this.attrCount; i++)
            {
                var attr1 = this.nodes[i];
                for (var j = i + 1; j < this.index + 1 + this.attrCount; j++)
                {
                    if (Ref.Equal(attr1.localName, this.nodes[j].localName) && Ref.Equal(attr1.ns, this.nodes[j].ns))
                    {
                        this.Throw(Res.Xml_DupAttributeName, this.nodes[j].GetNameWPrefix(this.nameTable), this.nodes[j].LineNo, this.nodes[j].LinePos);
                    }
                }
            }
        }

        private static char[] WhitespaceChars = new char[] { ' ', '\t', '\n', '\r' };

        private void OnXmlReservedAttribute(NodeData attr)
        {
            switch (attr.localName)
            {
                // xml:space
                case "space":
                    if (!this.curNode.xmlContextPushed)
                    {
                        this.PushXmlContext();
                    }

                    switch (attr.StringValue.Trim(WhitespaceChars))
                    {
                        case "preserve":
                            this.xmlContext.xmlSpace = XmlSpace.Preserve;
                            break;
                        case "default":
                            this.xmlContext.xmlSpace = XmlSpace.Default;
                            break;
                        default:
                            this.Throw(Res.Xml_InvalidXmlSpace, attr.StringValue, attr.lineInfo.lineNo, attr.lineInfo.linePos);
                            break;
                    }
                    break;
                // xml:lang
                case "lang":
                    if (!this.curNode.xmlContextPushed)
                    {
                        this.PushXmlContext();
                    }

                    this.xmlContext.xmlLang = attr.StringValue;
                    break;
                // xmlns
                // WsdModification - Adds support for Namespaces
                default:
                    if (!this.curNode.xmlContextPushed)
                    {
                        this.PushXmlContext();
                    }

                    // Add this namespace attribute to the context namespaces collection
                    this.xmlContext.xmlNamespaces.Add(attr.localName, attr.StringValue);
                    break;
            }
        }

        private void ParseAttributeValueSlow(int curPos, char quoteChar, NodeData attr)
        {
            var pos = curPos;
            var chars = this.ps.chars;
            var valueChunkStartPos = 0;
            var valueChunkLineInfo = new LineInfo(this.ps.lineNo, this.ps.LinePos);
            NodeData lastChunk = null;

            Debug.Assert(this.stringBuilder.Length == 0);

            for (; ; )
            {
                // parse the rest of the attribute value
                while (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                if (pos - this.ps.charPos > 0)
                {
                    this.stringBuilder.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                    this.ps.charPos = pos;
                }

                if (chars[pos] == quoteChar)
                {
                    break;
                }
                else
                {
                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            this.OnNewLine(pos);
                            if (this.normalize)
                            {
                                this.stringBuilder.Append((char)0x20);  // CDATA normalization of 0xA
                                this.ps.charPos++;
                            }
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                pos += 2;
                                if (this.normalize)
                                {
                                    this.stringBuilder.Append(this.ps.eolNormalized ? "\u0020\u0020" : "\u0020"); // CDATA normalization of 0xD 0xA
                                    this.ps.charPos = pos;
                                }
                            }
                            else if (pos + 1 < this.ps.charsUsed || this.ps.isEof)
                            {
                                pos++;
                                if (this.normalize)
                                {
                                    this.stringBuilder.Append((char)0x20);  // CDATA normalization of 0xD and 0xD 0xA
                                    this.ps.charPos = pos;
                                }
                            }
                            else
                            {
                                goto ReadData;
                            }

                            this.OnNewLine(pos);
                            continue;
                        // tab
                        case (char)0x9:
                            pos++;
                            if (this.normalize)
                            {
                                this.stringBuilder.Append((char)0x20);  // CDATA normalization of 0x9
                                this.ps.charPos++;
                            }
                            continue;
                        case '"':
                        case '\'':
                        case '>':
                            pos++;
                            continue;
                        // attribute values cannot contain '<'
                        case '<':
                            this.Throw(pos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionStr('<'));
                            break;
                        // entity referece
                        case '&':
                            if (pos - this.ps.charPos > 0)
                            {
                                this.stringBuilder.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                            }

                            this.ps.charPos = pos;

                            var entityLineInfo = new LineInfo(this.ps.lineNo, this.ps.LinePos + 1);
                            switch (this.HandleEntityReference(true, EntityExpandType.All, out pos))
                            {
                                case EntityType.CharacterDec:
                                case EntityType.CharacterHex:
                                case EntityType.CharacterNamed:
                                    break;
                                case EntityType.Unexpanded:
                                    Debug.Assert(false, "The document contains an entity reference");
                                    throw new NotSupportedException();

                                case EntityType.ExpandedInAttribute:
                                    Debug.Assert(false, "The document contains an entity reference");
                                    throw new NotSupportedException();
                                default:
                                    pos = this.ps.charPos;
                                    break;
                            }

                            chars = this.ps.chars;
                            continue;
                        default:
                            // end of buffer
                            if (pos == this.ps.charsUsed)
                            {
                                goto ReadData;
                            }

                            // surrogate chars
                            else
                            {
                                var ch = chars[pos];
                                if (ch >= SurHighStart && ch <= SurHighEnd)
                                {
                                    if (pos + 1 == this.ps.charsUsed)
                                    {
                                        goto ReadData;
                                    }

                                    pos++;
                                    if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                    {
                                        pos++;
                                        continue;
                                    }
                                }

                                this.ThrowInvalidChar(pos, ch);
                                break;
                            }
                    }
                }

            ReadData:
                // read new characters into the buffer
                if (this.ReadData() == 0)
                {
                    if (this.ps.charsUsed - this.ps.charPos > 0)
                    {
                        if (this.ps.chars[this.ps.charPos] != (char)0xD)
                        {
                            Debug.Assert(false, "We should never get to this point.");
                            this.Throw(Res.Xml_UnexpectedEOF1);
                        }

                        Debug.Assert(this.ps.isEof);
                    }
                    else
                    {
                        if (this.fragmentType == XmlNodeType.Attribute)
                        {
                            break;
                        }

                        this.Throw(Res.Xml_UnclosedQuote);
                    }
                }

                pos = this.ps.charPos;
                chars = this.ps.chars;
            }

            if (attr.nextAttrValueChunk != null)
            {
                // construct last text value chunk
                var valueChunkLen = this.stringBuilder.Length - valueChunkStartPos;
                if (valueChunkLen > 0)
                {
                    var textChunk = new NodeData {
                        lineInfo = valueChunkLineInfo,
                        depth = attr.depth + 1
                    };
                    textChunk.SetValueNode(XmlNodeType.Text, this.stringBuilder.ToString(valueChunkStartPos, valueChunkLen));
                    this.AddAttributeChunkToList(attr, textChunk, ref lastChunk);
                }
            }

            this.ps.charPos = pos + 1;

            attr.SetValue(this.stringBuilder.ToString());
            this.stringBuilder.Length = 0;
        }

        private void AddAttributeChunkToList(NodeData attr, NodeData chunk, ref NodeData lastChunk)
        {
            if (lastChunk == null)
            {
                Debug.Assert(attr.nextAttrValueChunk == null);
                lastChunk = chunk;
                attr.nextAttrValueChunk = chunk;
            }
            else
            {
                lastChunk.nextAttrValueChunk = chunk;
                lastChunk = chunk;
            }
        }

        // Parses text or white space node.
        // Returns true if a node has been parsed and its data set to curNode.
        // Returns false when a white space has been parsed and ignored (according to current whitespace handling) or when parsing mode is not Full.
        private bool ParseText()
        {
            int startPos;
            int endPos;
            var orChars = 0;

            // skip over the text if not in full parsing mode
            if (this.parsingMode != ParsingMode.Full)
            {
                while (!this.ParseText(out startPos, out endPos, ref orChars)) ;
                return false;
            }

            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            Debug.Assert(this.stringBuilder.Length == 0);

            // the whole value is in buffer
            if (this.ParseText(out startPos, out endPos, ref orChars))
            {
                var nodeType = this.GetTextNodeType(orChars);
                if (nodeType == XmlNodeType.None)
                {
                    goto IgnoredWhitespace;
                }

                //Debug.Assert(endPos - startPos > 0);
                this.curNode.SetValueNode(nodeType, this.ps.chars, startPos, endPos - startPos);
                return true;
            }

            // only piece of the value was returned
            else
            {
                var fullValue = false;

                // if it's a partial text value, not a whitespace -> return
                if (orChars > 0x20)
                {
                    Debug.Assert(endPos - startPos > 0);
                    this.curNode.SetValueNode(XmlNodeType.Text, this.ps.chars, startPos, endPos - startPos);
                    this.nextParsingFunction = this.parsingFunction;
                    this.parsingFunction = ParsingFunction.PartialTextValue;
                    return true;
                }

                // partial whitespace -> read more data (up to 4kB) to decide if it is a whitespace or a text node
                this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
                do
                {
                    fullValue = this.ParseText(out startPos, out endPos, ref orChars);
                    this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
                } while (!fullValue && orChars <= 0x20 && this.stringBuilder.Length < MinWhitespaceLookahedCount);

                // determine the value node type
                var nodeType = (this.stringBuilder.Length < MinWhitespaceLookahedCount) ? this.GetTextNodeType(orChars) : XmlNodeType.Text;
                if (nodeType == XmlNodeType.None)
                {
                    // ignored whitespace -> skip over the rest of the value unless we already read it all
                    this.stringBuilder.Length = 0;
                    if (!fullValue)
                    {
                        while (!this.ParseText(out startPos, out endPos, ref orChars)) ;
                    }

                    goto IgnoredWhitespace;
                }

                // set value to curNode
                this.curNode.SetValueNode(nodeType, this.stringBuilder.ToString());
                this.stringBuilder.Length = 0;

                // change parsing state if the full value was not parsed
                if (!fullValue)
                {
                    this.nextParsingFunction = this.parsingFunction;
                    this.parsingFunction = ParsingFunction.PartialTextValue;
                }

                return true;
            }

        IgnoredWhitespace:
            return false;
        }

        // Parses a chunk of text starting at ps.charPos.
        //   startPos .... start position of the text chunk that has been parsed (can differ from ps.charPos before the call)
        //   endPos ...... end position of the text chunk that has been parsed (can differ from ps.charPos after the call)
        //   ourOrChars .. all parsed character bigger or equal to 0x20 or-ed (|) into a single int. It can be used for whitespace detection
        //                 (the text has a non-whitespace character if outOrChars > 0x20).
        // Returns true when the whole value has been parsed. Return false when it needs to be called again to get a next chunk of value.
        private bool ParseText(out int startPos, out int endPos, ref int outOrChars)
        {
            var chars = this.ps.chars;
            var pos = this.ps.charPos;
            var rcount = 0;
            var rpos = -1;
            var orChars = outOrChars;
            char c;

            for (; ; )
            {
                // parse text content
                while ((c = chars[pos]) > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[c] & XmlCharType.fText) != 0)
                {
                    orChars |= (int)c;
                    pos++;
                }

                switch (c)
                {
                    case (char)0x9:
                        pos++;
                        continue;

                    // eol
                    case (char)0xA:
                    case (char)0xD:

                        // For SideShow we want to ignore linefeed and return chars in text content. The user
                        // must use <br/> elements to insert line breaks in text.
                        if (this.xmlContext.xmlSpace != XmlSpace.Preserve)
                        {

                            var skipCount = 1;
                            if (chars[pos] == (char)0xD && chars[pos + 1] == (char)0xA)
                            {
                                skipCount = 2;
                            }

                            if (pos - this.ps.charPos > 0)
                            {
                                if (rcount == 0)
                                {
                                    rcount = skipCount;
                                    rpos = pos;
                                }
                                else
                                {
                                    this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                    rpos = pos - rcount;
                                    rcount += skipCount;
                                }
                            }
                            else
                            {
                                this.ps.charPos += skipCount;
                            }

                            pos += skipCount;
                        }
                        else if (chars[pos] == (char)0xA)
                        {
                            pos++;
                        }
                        else if (chars[pos + 1] == (char)0xA)
                        {

                            if (!this.ps.eolNormalized && this.parsingMode == ParsingMode.Full)
                            {
                                if (pos - this.ps.charPos > 0)
                                {
                                    if (rcount == 0)
                                    {
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else
                                    {
                                        this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else
                                {
                                    this.ps.charPos++;
                                }
                            }

                            pos += 2;
                        }
                        else if (pos + 1 < this.ps.charsUsed || this.ps.isEof)
                        {
                            if (!this.ps.eolNormalized)
                            {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }

                            pos++;
                        }
                        else
                        {
                            goto ReadData;
                        }

                        this.OnNewLine(pos);
                        continue;

                    // some tag
                    case '<':
                        goto ReturnPartialValue;
                    // entity reference
                    case '&':
                        // try to parse char entity inline
                        int charRefEndPos, charCount;
                        EntityType entityType;
                        if ((charRefEndPos = this.ParseCharRefInline(pos, out charCount, out entityType)) > 0)
                        {
                            if (rcount > 0)
                            {
                                this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                            }

                            rpos = pos - rcount;
                            rcount += (charRefEndPos - pos - charCount);
                            pos = charRefEndPos;

                            if (!this.xmlCharType.IsWhiteSpace(chars[charRefEndPos - charCount]))
                            {
                                orChars |= 0xFF;
                            }
                        }
                        else
                        {
                            if (pos > this.ps.charPos)
                            {
                                goto ReturnPartialValue;
                            }

                            switch (this.HandleEntityReference(false, EntityExpandType.All, out pos))
                            {
                                case EntityType.Unexpanded:
                                    Debug.Assert(false, "Found general entity refernce in text");
                                    throw new NotSupportedException();

                                case EntityType.CharacterDec:
                                case EntityType.CharacterHex:
                                case EntityType.CharacterNamed:
                                    if (!this.xmlCharType.IsWhiteSpace(this.ps.chars[pos - 1]))
                                    {
                                        orChars |= 0xFF;
                                    }
                                    break;
                                default:
                                    pos = this.ps.charPos;
                                    break;
                            }

                            chars = this.ps.chars;
                        }
                        continue;
                    case ']':
                        if (this.ps.charsUsed - pos < 3 && !this.ps.isEof)
                        {
                            goto ReadData;
                        }

                        if (chars[pos + 1] == ']' && chars[pos + 2] == '>')
                        {
                            this.Throw(pos, Res.Xml_CDATAEndInText);
                        }

                        orChars |= ']';
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if (pos == this.ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        // surrogate chars
                        else
                        {
                            var ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == this.ps.charsUsed)
                                {
                                    goto ReadData;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    orChars |= ch;
                                    continue;
                                }
                            }

                            var offset = pos - this.ps.charPos;
                            this.ThrowInvalidChar(this.ps.charPos + offset, ch);
                            break;
                        }
                }

            ReadData:
                if (pos > this.ps.charPos)
                {
                    goto ReturnPartialValue;
                }

                // read new characters into the buffer
                if (this.ReadData() == 0)
                {
                    if (this.ps.charsUsed - this.ps.charPos > 0)
                    {
                        if (this.ps.chars[this.ps.charPos] != (char)0xD)
                        {
                            Debug.Assert(false, "We should never get to this point.");
                            this.Throw(Res.Xml_UnexpectedEOF1);
                        }

                        Debug.Assert(this.ps.isEof);
                    }
                    else
                    {
                        startPos = endPos = pos;
                        return true;
                    }
                }

                pos = this.ps.charPos;
                chars = this.ps.chars;
                continue;
            }

        ReturnPartialValue:
            if (this.parsingMode == ParsingMode.Full && rcount > 0)
            {
                this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
            }

            startPos = this.ps.charPos;
            endPos = pos - rcount;
            this.ps.charPos = pos;
            outOrChars = orChars;
            return c == '<';
        }

        // When in ParsingState.PartialTextValue, this method parses and caches the rest of the value and stores it in curNode.
        void FinishPartialValue()
        {
            Debug.Assert(this.stringBuilder.Length == 0);
            Debug.Assert(this.parsingFunction == ParsingFunction.PartialTextValue ||
                          (this.parsingFunction == ParsingFunction.InReadValueChunk && this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue));

            this.curNode.CopyTo(this.readValueOffset, this.stringBuilder);

            int startPos;
            int endPos;
            var orChars = 0;
            while (!this.ParseText(out startPos, out endPos, ref orChars))
            {
                this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
            }

            this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);

            Debug.Assert(this.stringBuilder.Length > 0);
            this.curNode.SetValue(this.stringBuilder.ToString());
            this.stringBuilder.Length = 0;
        }

        void FinishOtherValueIterator()
        {
            switch (this.parsingFunction)
            {
                case ParsingFunction.InReadAttributeValue:
                    // do nothing, correct value is already in curNode
                    break;
                case ParsingFunction.InReadValueChunk:
                    if (this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
                    {
                        this.FinishPartialValue();
                        this.incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    }
                    else
                    {
                        if (this.readValueOffset > 0)
                        {
                            this.curNode.SetValue(this.curNode.StringValue.Substring(this.readValueOffset));
                            this.readValueOffset = 0;
                        }
                    }
                    break;
            }
        }

        // When in ParsingState.PartialTextValue, this method skips over the rest of the partial value.
        void SkipPartialTextValue()
        {
            Debug.Assert(this.parsingFunction == ParsingFunction.PartialTextValue || this.parsingFunction == ParsingFunction.InReadValueChunk);
            var orChars = 0;

            this.parsingFunction = this.nextParsingFunction;
            while (!this.ParseText(out var startPos, out var endPos, ref orChars)) ;
        }

        void FinishReadValueChunk()
        {
            Debug.Assert(this.parsingFunction == ParsingFunction.InReadValueChunk);

            this.readValueOffset = 0;
            if (this.incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue)
            {
                Debug.Assert((this.index > 0) ? this.nextParsingFunction == ParsingFunction.ElementContent : this.nextParsingFunction == ParsingFunction.DocumentContent);
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
            Debug.Assert(this.stringBuilder.Length == 0);

            var nodeType = this.GetWhitespaceType();

            if (nodeType == XmlNodeType.None)
            {
                this.EatWhitespaces(null);
                if (this.ps.chars[this.ps.charPos] == '<' || this.ps.charsUsed - this.ps.charPos == 0)
                {
                    return false;
                }
            }
            else
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                this.EatWhitespaces(this.stringBuilder);
                if (this.ps.chars[this.ps.charPos] == '<' || this.ps.charsUsed - this.ps.charPos == 0)
                {
                    if (this.stringBuilder.Length > 0)
                    {
                        this.curNode.SetValueNode(nodeType, this.stringBuilder.ToString());
                        this.stringBuilder.Length = 0;
                        return true;
                    }

                    return false;
                }
            }

            // Ignore null chars at the root level. This allows a provider to include a terminating null
            // without causing an invalid character exception.
            if (this.ps.chars[this.ps.charPos] == '\0')
            {
                this.ps.charPos++;
            }
            else if (this.xmlCharType.IsCharData(this.ps.chars[this.ps.charPos]))
            {
                this.Throw(Res.Xml_InvalidRootData);
            }
            else
            {
                this.ThrowInvalidChar(this.ps.charPos, this.ps.chars[this.ps.charPos]);
            }

            return false;
        }

        private EntityType HandleEntityReference(bool isInAttributeValue, EntityExpandType expandType, out int charRefEndPos)
        {
            Debug.Assert(this.ps.chars[this.ps.charPos] == '&');

            if (this.ps.charPos + 1 == this.ps.charsUsed)
            {
                if (this.ReadData() == 0)
                {
                    this.Throw(Res.Xml_UnexpectedEOF1);
                }
            }

            // numeric characters reference
            if (this.ps.chars[this.ps.charPos + 1] == '#')
            {
                charRefEndPos = this.ParseNumericCharRef(expandType != EntityExpandType.OnlyGeneral, null, out var entityType);
                Debug.Assert(entityType == EntityType.CharacterDec || entityType == EntityType.CharacterHex);
                return entityType;
            }

            // named reference
            else
            {
                // named character reference
                charRefEndPos = this.ParseNamedCharRef(expandType != EntityExpandType.OnlyGeneral, null);
                if (charRefEndPos >= 0)
                {
                    return EntityType.CharacterNamed;
                }

                return EntityType.Unexpanded;
            }
        }

        private bool ParsePI() => this.ParsePI(null);

        // Parses processing instruction; if piInDtdStringBuilder != null, the processing instruction is in DTD and
        // it will be saved in the passed string builder (target, whitespace & value).
        private bool ParsePI(BufferBuilder piInDtdStringBuilder)
        {
            if (this.parsingMode == ParsingMode.Full)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
            }

            Debug.Assert(this.stringBuilder.Length == 0);

            // parse target name
            var nameEndPos = this.ParseName();
            var target = this.nameTable.Add(this.ps.chars, this.ps.charPos, nameEndPos - this.ps.charPos);
            ///////ISSUE: rswaney: SPOT string class doesn't have a culture-aware or case-insensitive compare

            if (string.Compare(target, "xml" /*,true , CultureInfo.InvariantCulture */) == 0)
            {
                this.Throw(target.Equals("xml") ? Res.Xml_XmlDeclNotFirst : Res.Xml_InvalidPIName, target);
            }

            this.ps.charPos = nameEndPos;

            if (piInDtdStringBuilder == null)
            {
                if (!this.ignorePIs && this.parsingMode == ParsingMode.Full)
                {
                    this.curNode.SetNamedNode(XmlNodeType.ProcessingInstruction, target);
                }
            }
            else
            {
                piInDtdStringBuilder.Append(target);
            }

            // check mandatory whitespace
            var ch = this.ps.chars[this.ps.charPos];
            Debug.Assert(ch != 0);
            if (this.EatWhitespaces(piInDtdStringBuilder) == 0)
            {
                if (this.ps.charsUsed - this.ps.charPos < 2)
                {
                    this.ReadData();
                }

                if (ch != '?' || this.ps.chars[this.ps.charPos + 1] != '>')
                {
                    this.Throw(Res.Xml_BadNameChar, XmlException.BuildCharExceptionStr(ch));
                }
            }

            // scan processing instruction value
            if (this.ParsePIValue(out var startPos, out var endPos)) {
                if (piInDtdStringBuilder == null) {
                    if (this.ignorePIs) {
                        return false;
                    }

                    if (this.parsingMode == ParsingMode.Full) {
                        this.curNode.SetValue(this.ps.chars, startPos, endPos - startPos);
                    }
                }
                else {
                    piInDtdStringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
                }
            }
            else {
                BufferBuilder sb;
                if (piInDtdStringBuilder == null) {
                    if (this.ignorePIs || this.parsingMode != ParsingMode.Full) {
                        while (!this.ParsePIValue(out startPos, out endPos)) ;
                        return false;
                    }

                    sb = this.stringBuilder;
                    Debug.Assert(this.stringBuilder.Length == 0);
                }
                else {
                    sb = piInDtdStringBuilder;
                }

                do {
                    sb.Append(this.ps.chars, startPos, endPos - startPos);
                } while (!this.ParsePIValue(out startPos, out endPos));
                sb.Append(this.ps.chars, startPos, endPos - startPos);

                if (piInDtdStringBuilder == null) {
                    this.curNode.SetValue(this.stringBuilder.ToString());
                    this.stringBuilder.Length = 0;
                }
            }

            return true;
        }

        private bool ParsePIValue(out int outStartPos, out int outEndPos)
        {
            // read new characters into the buffer
            if (this.ps.charsUsed - this.ps.charPos < 2)
            {
                if (this.ReadData() == 0)
                {
                    this.Throw(this.ps.charsUsed, Res.Xml_UnexpectedEOF, "PI");
                }
            }

            var pos = this.ps.charPos;
            var chars = this.ps.chars;
            var rcount = 0;
            var rpos = -1;
            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar || ((this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fText) != 0 && chars[pos] != '?'))
                {
                    pos++;
                }

                switch (chars[pos])
                {
                    // possibly end of PI
                    case '?':
                        if (chars[pos + 1] == '>')
                        {
                            if (rcount > 0)
                            {
                                Debug.Assert(!this.ps.eolNormalized);
                                this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                outEndPos = pos - rcount;
                            }
                            else
                            {
                                outEndPos = pos;
                            }

                            outStartPos = this.ps.charPos;
                            this.ps.charPos = pos + 2;
                            return true;
                        }
                        else if (pos + 1 == this.ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }
                        else
                        {
                            pos++;
                            continue;
                        }

                    // eol
                    case (char)0xA:
                        pos++;
                        this.OnNewLine(pos);
                        continue;
                    case (char)0xD:
                        if (chars[pos + 1] == (char)0xA)
                        {
                            if (!this.ps.eolNormalized && this.parsingMode == ParsingMode.Full)
                            {
                                // EOL normalization of 0xD 0xA
                                if (pos - this.ps.charPos > 0)
                                {
                                    if (rcount == 0)
                                    {
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else
                                    {
                                        this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else
                                {
                                    this.ps.charPos++;
                                }
                            }

                            pos += 2;
                        }
                        else if (pos + 1 < this.ps.charsUsed || this.ps.isEof)
                        {
                            if (!this.ps.eolNormalized)
                            {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }

                            pos++;
                        }
                        else
                        {
                            goto ReturnPartial;
                        }

                        this.OnNewLine(pos);
                        continue;
                    case '<':
                    case '&':
                    case ']':
                    case (char)0x9:
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if (pos == this.ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }

                        // surrogate characters
                        else
                        {
                            var ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == this.ps.charsUsed)
                                {
                                    goto ReturnPartial;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            this.ThrowInvalidChar(pos, ch);
                            break;
                        }
                }

            }

        ReturnPartial:
            if (rcount > 0)
            {
                this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                outEndPos = pos - rcount;
            }
            else
            {
                outEndPos = pos;
            }

            outStartPos = this.ps.charPos;
            this.ps.charPos = pos;
            return false;
        }

        private bool ParseComment()
        {
            if (this.ignoreComments)
            {
                var oldParsingMode = this.parsingMode;
                this.parsingMode = ParsingMode.SkipNode;
                this.ParseCDataOrComment(XmlNodeType.Comment);
                this.parsingMode = oldParsingMode;
                return false;
            }
            else
            {
                this.ParseCDataOrComment(XmlNodeType.Comment);
                return true;
            }
        }

        private void ParseCData() => this.ParseCDataOrComment(XmlNodeType.CDATA);

        // Parses CDATA section or comment
        private void ParseCDataOrComment(XmlNodeType type)
        {
            int startPos, endPos;

            if (this.parsingMode == ParsingMode.Full)
            {
                this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);
                Debug.Assert(this.stringBuilder.Length == 0);
                if (this.ParseCDataOrComment(type, out startPos, out endPos))
                {
                    this.curNode.SetValueNode(type, this.ps.chars, startPos, endPos - startPos);
                }
                else
                {
                    do
                    {
                        this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
                    } while (!this.ParseCDataOrComment(type, out startPos, out endPos));
                    this.stringBuilder.Append(this.ps.chars, startPos, endPos - startPos);
                    this.curNode.SetValueNode(type, this.stringBuilder.ToString());
                    this.stringBuilder.Length = 0;
                }
            }
            else
            {
                while (!this.ParseCDataOrComment(type, out startPos, out endPos)) ;
            }
        }

        // Parses a chunk of CDATA section or comment. Returns true when the end of CDATA or comment was reached.
        private bool ParseCDataOrComment(XmlNodeType type, out int outStartPos, out int outEndPos)
        {
            if (this.ps.charsUsed - this.ps.charPos < 3)
            {
                // read new characters into the buffer
                if (this.ReadData() == 0)
                {
                    this.Throw(Res.Xml_UnexpectedEOF, (type == XmlNodeType.Comment) ? "Comment" : "CDATA");
                }
            }

            var pos = this.ps.charPos;
            var chars = this.ps.chars;
            var rcount = 0;
            var rpos = -1;
            var stopChar = (type == XmlNodeType.Comment) ? '-' : ']';

            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar ||
                        ((this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fText) != 0 && chars[pos] != stopChar))
                {
                    pos++;
                }

                // possibly end of comment or cdata section
                if (chars[pos] == stopChar)
                {
                    if (chars[pos + 1] == stopChar)
                    {
                        if (chars[pos + 2] == '>')
                        {
                            if (rcount > 0)
                            {
                                Debug.Assert(!this.ps.eolNormalized);
                                this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                outEndPos = pos - rcount;
                            }
                            else
                            {
                                outEndPos = pos;
                            }

                            outStartPos = this.ps.charPos;
                            this.ps.charPos = pos + 3;
                            return true;
                        }
                        else if (pos + 2 == this.ps.charsUsed)
                        {
                            goto ReturnPartial;
                        }
                        else if (type == XmlNodeType.Comment)
                        {
                            this.Throw(pos, Res.Xml_InvalidCommentChars);
                        }
                    }
                    else if (pos + 1 == this.ps.charsUsed)
                    {
                        goto ReturnPartial;
                    }

                    pos++;
                    continue;
                }
                else
                {
                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            this.OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                // EOL normalization of 0xD 0xA - shift the buffer
                                if (!this.ps.eolNormalized && this.parsingMode == ParsingMode.Full)
                                {
                                    if (pos - this.ps.charPos > 0)
                                    {
                                        if (rcount == 0)
                                        {
                                            rcount = 1;
                                            rpos = pos;
                                        }
                                        else
                                        {
                                            this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                            rpos = pos - rcount;
                                            rcount++;
                                        }
                                    }
                                    else
                                    {
                                        this.ps.charPos++;
                                    }
                                }

                                pos += 2;
                            }
                            else if (pos + 1 < this.ps.charsUsed || this.ps.isEof)
                            {
                                if (!this.ps.eolNormalized)
                                {
                                    chars[pos] = (char)0xA;             // EOL normalization of 0xD
                                }

                                pos++;
                            }
                            else
                            {
                                goto ReturnPartial;
                            }

                            this.OnNewLine(pos);
                            continue;
                        case '<':
                        case '&':
                        case ']':
                        case (char)0x9:
                            pos++;
                            continue;
                        default:
                            // end of buffer
                            if (pos == this.ps.charsUsed)
                            {
                                goto ReturnPartial;
                            }

                            // surrogate characters
                            var ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == this.ps.charsUsed)
                                {
                                    goto ReturnPartial;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            this.ThrowInvalidChar(pos, ch);
                            break;
                    }
                }

            ReturnPartial:
                if (rcount > 0)
                {
                    this.ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                    outEndPos = pos - rcount;
                }
                else
                {
                    outEndPos = pos;
                }

                outStartPos = this.ps.charPos;

                this.ps.charPos = pos;
                return false; // false == parsing of comment or CData section is not finished yet, must be called again
            }
        }

        private int EatWhitespaces(BufferBuilder sb)
        {
            var pos = this.ps.charPos;
            var wsCount = 0;
            var chars = this.ps.chars;

            for (; ; )
            {
                for (; ; )
                {
                    switch (chars[pos])
                    {
                        case (char)0xA:
                            pos++;
                            this.OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                var tmp1 = pos - this.ps.charPos;
                                if (sb != null && !this.ps.eolNormalized)
                                {
                                    if (tmp1 > 0)
                                    {
                                        this.stringBuilder.Append(chars, this.ps.charPos, tmp1);
                                        wsCount += tmp1;
                                    }

                                    this.ps.charPos = pos + 1;
                                }

                                pos += 2;
                            }
                            else if (pos + 1 < this.ps.charsUsed || this.ps.isEof)
                            {
                                if (!this.ps.eolNormalized)
                                {
                                    chars[pos] = (char)0xA;             // EOL normalization of 0xD
                                }

                                pos++;
                            }
                            else
                            {
                                goto ReadData;
                            }

                            this.OnNewLine(pos);
                            continue;
                        case (char)0x9:
                        case (char)0x20:
                            pos++;
                            continue;
                        default:
                            if (pos == this.ps.charsUsed)
                            {
                                goto ReadData;
                            }
                            else
                            {
                                var tmp2 = pos - this.ps.charPos;
                                if (tmp2 > 0)
                                {
                                    if (sb != null)
                                    {
                                        sb.Append(this.ps.chars, this.ps.charPos, tmp2);
                                    }

                                    this.ps.charPos = pos;
                                    wsCount += tmp2;
                                }

                                return wsCount;
                            }
                    }
                }

            ReadData:
                var tmp3 = pos - this.ps.charPos;
                if (tmp3 > 0)
                {
                    if (sb != null)
                    {
                        sb.Append(this.ps.chars, this.ps.charPos, tmp3);
                    }

                    this.ps.charPos = pos;
                    wsCount += tmp3;
                }

                if (this.ReadData() == 0)
                {
                    if (this.ps.charsUsed - this.ps.charPos == 0)
                    {
                        return wsCount;
                    }

                    if (this.ps.chars[this.ps.charPos] != (char)0xD)
                    {
                        Debug.Assert(false, "We should never get to this point.");
                        this.Throw(Res.Xml_UnexpectedEOF1);
                    }

                    Debug.Assert(this.ps.isEof);
                }

                pos = this.ps.charPos;
                chars = this.ps.chars;
            }
        }

        private int ParseCharRefInline(int startPos, out int charCount, out EntityType entityType)
        {
            Debug.Assert(this.ps.chars[startPos] == '&');
            if (this.ps.chars[startPos + 1] == '#')
            {
                return this.ParseNumericCharRefInline(startPos, true, null, out charCount, out entityType);
            }
            else
            {
                charCount = 1;
                entityType = EntityType.CharacterNamed;
                return this.ParseNamedCharRefInline(startPos, true, null);
            }
        }

        // Parses numeric character entity reference (e.g. &#32; &#x20;).
        //      - replaces the last one or two character of the entity reference (';' and the character before) with the referenced
        //        character or surrogates pair (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character
        private int ParseNumericCharRef(bool expand, BufferBuilder internalSubsetBuilder, out EntityType entityType)
        {
            for (; ; )
            {
                int newPos;
                switch (newPos = this.ParseNumericCharRefInline(this.ps.charPos, expand, internalSubsetBuilder, out var charCount, out entityType)) {
                    case -2:
                        // read new characters in the buffer
                        if (this.ReadData() == 0) {
                            this.Throw(Res.Xml_UnexpectedEOF);
                        }

                        Debug.Assert(this.ps.chars[this.ps.charPos] == '&');
                        continue;
                    default:
                        if (expand) {
                            this.ps.charPos = newPos - charCount;
                        }

                        return newPos;
                }
            }
        }

        // Parses numeric character entity reference (e.g. &#32; &#x20;).
        // Returns -2 if more data is needed in the buffer
        // Otherwise
        //      - replaces the last one or two character of the entity reference (';' and the character before) with the referenced
        //        character or surrogates pair (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        private int ParseNumericCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder, out int charCount, out EntityType entityType)
        {
            Debug.Assert(this.ps.chars[startPos] == '&' && this.ps.chars[startPos + 1] == '#');

            int val;
            int pos;
            char[] chars;

            val = 0;
            var badDigitExceptionString = Res.Xml_DefaultException;
            chars = this.ps.chars;
            pos = startPos + 2;
            charCount = 0;

            if (chars[pos] == 'x')
            {
                pos++;
                badDigitExceptionString = Res.Xml_BadHexEntity;
                for (; ; )
                {
                    var ch = chars[pos];
                    if (ch >= '0' && ch <= '9')
                        val = val * 16 + ch - '0';
                    else if (ch >= 'a' && ch <= 'f')
                        val = val * 16 + 10 + ch - 'a';
                    else if (ch >= 'A' && ch <= 'F')
                        val = val * 16 + 10 + ch - 'A';
                    else
                        break;
                    pos++;
                }

                entityType = EntityType.CharacterHex;
            }
            else if (pos < this.ps.charsUsed)
            {
                badDigitExceptionString = Res.Xml_BadDecimalEntity;
                while (chars[pos] >= '0' && chars[pos] <= '9')
                {
                    val = val * 10 + chars[pos] - '0';
                    pos++;
                }

                entityType = EntityType.CharacterDec;
            }
            else
            {
                // need more data in the buffer
                entityType = EntityType.Unexpanded;
                return -2;
            }

            if (chars[pos] != ';')
            {
                if (pos == this.ps.charsUsed)
                {
                    // need more data in the buffer
                    return -2;
                }
                else
                {
                    this.Throw(pos, badDigitExceptionString);
                }
            }

            // simple character
            if (val <= char.MaxValue)
            {
                var ch = (char)val;
                if ((!this.xmlCharType.IsCharData(ch) || (ch >= SurLowStart && ch <= 0xdeff)) && this.checkCharacters)
                {
                    this.ThrowInvalidChar((this.ps.chars[this.ps.charPos + 2] == 'x') ? this.ps.charPos + 3 : this.ps.charPos + 2, ch);
                }

                if (expand)
                {
                    if (internalSubsetBuilder != null)
                    {
                        internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, pos - this.ps.charPos + 1);
                    }

                    chars[pos] = ch;
                }

                charCount = 1;
                return pos + 1;
            }

            // surrogate
            else
            {
                var v = val - 0x10000;
                var low = SurLowStart + v % 1024;
                var high = SurHighStart + v / 1024;

                if (this.normalize)
                {
                    var ch = (char)high;
                    if (ch >= SurHighStart && ch <= SurHighEnd)
                    {
                        ch = (char)low;
                        if (ch >= SurLowStart && ch <= SurLowEnd)
                        {
                            goto Return;
                        }
                    }

                    this.ThrowInvalidChar((this.ps.chars[this.ps.charPos + 2] == 'x') ? this.ps.charPos + 3 : this.ps.charPos + 2, (char)val);
                }

            Return:
                Debug.Assert(pos > 0);
                if (expand)
                {
                    if (internalSubsetBuilder != null)
                    {
                        internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, pos - this.ps.charPos + 1);
                    }

                    chars[pos - 1] = (char)high;
                    chars[pos] = (char)low;
                }

                charCount = 2;
                return pos + 1;
            }
        }

        // Parses named character entity reference (&amp; &apos; &lt; &gt; &quot;).
        // Returns -1 if the reference is not a character entity reference.
        // Otherwise
        //      - replaces the last character of the entity reference (';') with the referenced character (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character
        private int ParseNamedCharRef(bool expand, BufferBuilder internalSubsetBuilder)
        {
            for (; ; )
            {
                int newPos;
                switch (newPos = this.ParseNamedCharRefInline(this.ps.charPos, expand, internalSubsetBuilder))
                {
                    case -1:
                        return -1;
                    case -2:
                        // read new characters in the buffer
                        if (this.ReadData() == 0)
                        {
                            return -1;
                        }

                        Debug.Assert(this.ps.chars[this.ps.charPos] == '&');
                        continue;
                    default:
                        if (expand)
                        {
                            this.ps.charPos = newPos - 1;
                        }

                        return newPos;
                }
            }
        }

        // Parses named character entity reference (&amp; &apos; &lt; &gt; &quot;).
        // Returns -1 if the reference is not a character entity reference.
        // Returns -2 if more data is needed in the buffer
        // Otherwise
        //      - replaces the last character of the entity reference (';') with the referenced character (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        private int ParseNamedCharRefInline(int startPos, bool expand, BufferBuilder internalSubsetBuilder)
        {
            Debug.Assert(startPos < this.ps.charsUsed);
            Debug.Assert(this.ps.chars[startPos] == '&');
            Debug.Assert(this.ps.chars[startPos + 1] != '#');

            var pos = startPos + 1;
            var chars = this.ps.chars;
            char ch;

            switch (chars[pos])
            {
                // &apos; or &amp;
                case 'a':
                    pos++;
                    // &amp;
                    if (chars[pos] == 'm')
                    {
                        if (this.ps.charsUsed - pos >= 3)
                        {
                            if (chars[pos + 1] == 'p' && chars[pos + 2] == ';')
                            {
                                pos += 3;
                                ch = '&';
                                goto FoundCharRef;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }

                    // &apos;
                    else if (chars[pos] == 'p')
                    {
                        if (this.ps.charsUsed - pos >= 4)
                        {
                            if (chars[pos + 1] == 'o' && chars[pos + 2] == 's' &&
                                    chars[pos + 3] == ';')
                            {
                                pos += 4;
                                ch = '\'';
                                goto FoundCharRef;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                    }
                    else if (pos < this.ps.charsUsed)
                    {
                        return -1;
                    }
                    break;
                // &guot;
                case 'q':
                    if (this.ps.charsUsed - pos >= 5)
                    {
                        if (chars[pos + 1] == 'u' && chars[pos + 2] == 'o' &&
                                chars[pos + 3] == 't' && chars[pos + 4] == ';')
                        {
                            pos += 5;
                            ch = '"';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                // &lt;
                case 'l':
                    if (this.ps.charsUsed - pos >= 3)
                    {
                        if (chars[pos + 1] == 't' && chars[pos + 2] == ';')
                        {
                            pos += 3;
                            ch = '<';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                // &gt;
                case 'g':
                    if (this.ps.charsUsed - pos >= 3)
                    {
                        if (chars[pos + 1] == 't' && chars[pos + 2] == ';')
                        {
                            pos += 3;
                            ch = '>';
                            goto FoundCharRef;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    break;
                default:
                    return -1;
            }

            // need more data in the buffer
            return -2;

        FoundCharRef:
            Debug.Assert(pos > 0);
            if (expand)
            {
                if (internalSubsetBuilder != null)
                {
                    internalSubsetBuilder.Append(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
                }

                this.ps.chars[pos - 1] = ch;
            }

            return pos;
        }

        private int ParseName() => this.ParseQName(false, 0, out var colonPos);

        private int ParseQName(out int colonPos) => this.ParseQName(true, 0, out colonPos);

        private int ParseQName(bool isQName, int startOffset, out int colonPos)
        {
            var colonOffset = -1;
            var pos = this.ps.charPos + startOffset;

        ContinueStartName:
            var chars = this.ps.chars;

            // start name char
            if (!(chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartName) != 0))
            {
                if (pos == this.ps.charsUsed)
                {
                    if (this.ReadDataInName(ref pos))
                    {
                        goto ContinueStartName;
                    }

                    this.Throw(pos, Res.Xml_UnexpectedEOF, "Name");
                }

                if (chars[pos] != ':')
                {
                    this.Throw(pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionStr(chars[pos]));
                }
            }

            pos++;

        ContinueName:
            // parse name
            while (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCName) != 0)
            {
                pos++;
            }

            // colon
            if (chars[pos] == ':')
            {
                colonOffset = pos - this.ps.charPos;
                pos++;
                goto ContinueStartName;
            }

            // end of buffer
            else if (pos == this.ps.charsUsed)
            {
                if (this.ReadDataInName(ref pos))
                {
                    chars = this.ps.chars;
                    goto ContinueName;
                }

                this.Throw(pos, Res.Xml_UnexpectedEOF, "Name");
            }

            // end of name
            colonPos = (colonOffset == -1) ? -1 : this.ps.charPos + colonOffset;
            return pos;
        }

        private bool ReadDataInName(ref int pos)
        {
            var offset = pos - this.ps.charPos;
            var newDataRead = (this.ReadData() != 0);
            pos = this.ps.charPos + offset;
            return newDataRead;
        }

        private NodeData AddNode(int nodeIndex, int nodeDepth)
        {
            Debug.Assert(nodeIndex < this.nodes.Length);
            Debug.Assert(this.nodes[this.nodes.Length - 1] == null);

            var n = this.nodes[nodeIndex];
            if (n != null)
            {
                n.depth = nodeDepth;
                return n;
            }

            return this.AllocNode(nodeIndex, nodeDepth);
        }

        private NodeData AllocNode(int nodeIndex, int nodeDepth)
        {
            Debug.Assert(nodeIndex < this.nodes.Length);
            if (nodeIndex >= this.nodes.Length - 1)
            {
                var newNodes = new NodeData[this.nodes.Length * 2];
                Array.Copy(this.nodes, 0, newNodes, 0, this.nodes.Length);
                this.nodes = newNodes;
            }

            Debug.Assert(nodeIndex < this.nodes.Length);

            var node = this.nodes[nodeIndex];
            if (node == null)
            {
                node = new NodeData();
                this.nodes[nodeIndex] = node;
            }

            node.depth = nodeDepth;
            return node;
        }

        private NodeData AddAttributeNoChecks(string name, int attrDepth)
        {
            var newAttr = this.AddNode(this.index + this.attrCount + 1, attrDepth);
            newAttr.SetNamedNode(XmlNodeType.Attribute, this.nameTable.Add(name));
            this.attrCount++;
            return newAttr;
        }

        //private NodeData AddAttribute( int endNamePos, int colonPos ) {
        //    // setup attribute name
        //    string localName = nameTable.Add( ps.chars, ps.charPos, endNamePos - ps.charPos );
        //    return AddAttribute( localName, "", localName );
        //}

        // WsdModification - Added support for storage of prefix, localname and nameWPrefix
        private NodeData AddAttribute(int endNamePos, int colonPos)
        {
            // setup attribute name
            var nameWPrefix = this.nameTable.Add(this.ps.chars, this.ps.charPos, endNamePos - this.ps.charPos);
            if (colonPos > -1)
            {
                var localName = new string(this.ps.chars, colonPos + 1, endNamePos - (colonPos + 1));
                var prefix = new string(this.ps.chars, this.ps.charPos, colonPos - this.ps.charPos);
                return this.AddAttribute(localName, prefix, nameWPrefix);
            }

            return this.AddAttribute(nameWPrefix, "", nameWPrefix);
        }

        private NodeData AddAttribute(string localName, string prefix, string nameWPrefix)
        {
            var newAttr = this.AddNode(this.index + this.attrCount + 1, this.index + 1);

            // set attribute name
            newAttr.SetNamedNode(XmlNodeType.Attribute, localName, prefix, nameWPrefix);

            // pre-check attribute for duplicate: hash by first local name char
            var attrHash = 1 << (localName[0] & 0x1F);
            if ((this.attrHashtable & attrHash) == 0)
            {
                this.attrHashtable |= attrHash;
            }
            else
            {
                // there are probably 2 attributes beginning with the same letter -> check all previous
                // attributes
                if (this.attrDuplWalkCount < MaxAttrDuplWalkCount)
                {
                    this.attrDuplWalkCount++;
                    for (var i = this.index + 1; i < this.index + this.attrCount + 1; i++)
                    {
                        var attr = this.nodes[i];
                        Debug.Assert(attr.type == XmlNodeType.Attribute);
                        if (Ref.Equal(attr.localName, newAttr.localName))
                        {
                            this.attrDuplWalkCount = MaxAttrDuplWalkCount;
                            break;
                        }
                    }
                }
            }

            this.attrCount++;
            return newAttr;
        }

        private void PopElementContext()
        {
            // pop xml context
            if (this.curNode.xmlContextPushed)
            {
                this.PopXmlContext();
            }
        }

        private void OnNewLine(int pos)
        {
            this.ps.lineNo++;
            this.ps.lineStartPos = pos - 1;
        }

        private void OnEof()
        {
            Debug.Assert(this.ps.isEof);
            this.curNode = this.nodes[0];
            this.curNode.Clear(XmlNodeType.None);
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);

            this.parsingFunction = ParsingFunction.Eof;
            this.readState = ReadState.EndOfFile;

            this.reportedEncoding = null;
        }

        private void ResetAttributes()
        {
            if (this.fullAttrCleanup)
            {
                this.FullAttributeCleanup();
            }

            this.curAttrIndex = -1;
            this.attrCount = 0;
            this.attrHashtable = 0;
            this.attrDuplWalkCount = 0;
        }

        private void FullAttributeCleanup()
        {
            for (var i = this.index + 1; i < this.index + this.attrCount + 1; i++)
            {
                var attr = this.nodes[i];
                attr.nextAttrValueChunk = null;
                attr.IsDefaultAttribute = false;
            }

            this.fullAttrCleanup = false;
        }

        private void PushXmlContext()
        {
            this.xmlContext = new XmlContext(this.xmlContext);
            this.curNode.xmlContextPushed = true;
        }

        private void PopXmlContext()
        {
            Debug.Assert(this.curNode.xmlContextPushed);
            this.xmlContext = this.xmlContext.previousContext;
            this.curNode.xmlContextPushed = false;
        }

        // Returns the whitespace node type according to the current whitespaceHandling setting and xml:space
        private XmlNodeType GetWhitespaceType()
        {
            if (this.whitespaceHandling != WhitespaceHandling.None)
            {
                if (this.xmlContext.xmlSpace == XmlSpace.Preserve)
                {
                    return XmlNodeType.SignificantWhitespace;
                }

                if (this.whitespaceHandling == WhitespaceHandling.All)
                {
                    return XmlNodeType.Whitespace;
                }
            }

            return XmlNodeType.None;
        }

        private XmlNodeType GetTextNodeType(int orChars)
        {
            if (orChars > 0x20)
            {
                return XmlNodeType.Text;
            }
            else
            {
                return this.GetWhitespaceType();
            }
        }

        private void InitIncrementalRead(IncrementalReadDecoder decoder)
        {
            this.ResetAttributes();

            decoder.Reset();
            this.incReadDecoder = decoder;
            this.incReadState = IncrementalReadState.Text;
            this.incReadDepth = 1;
            this.incReadLeftStartPos = this.ps.charPos;
            this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);

            this.parsingFunction = ParsingFunction.InIncrementalRead;
        }

        private int IncrementalRead(Array array, int index, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "buffer" : "array");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "index" : "offset");
            }

            if (array.Length - index < count)
            {
                throw new ArgumentException((this.incReadDecoder is IncrementalReadCharsDecoder) ? "count" : "len");
            }

            if (count == 0)
            {
                return 0;
            }

            this.curNode.lineInfo = this.incReadLineInfo;

            this.incReadDecoder.SetNextOutputBuffer(array, index, count);
            this.IncrementalRead();
            return this.incReadDecoder.DecodedCount;
        }

        private int IncrementalRead()
        {
            var charsDecoded = 0;

        OuterContinue:
            var charsLeft = this.incReadLeftEndPos - this.incReadLeftStartPos;
            if (charsLeft > 0)
            {
                int count;
                try
                {
                    count = this.incReadDecoder.Decode(this.ps.chars, this.incReadLeftStartPos, charsLeft);
                }
                catch (XmlException e)
                {
                    this.ReThrow(e, (int)this.incReadLineInfo.lineNo, (int)this.incReadLineInfo.linePos);
                    return 0;
                }

                if (count < charsLeft)
                {
                    this.incReadLeftStartPos += count;
                    this.incReadLineInfo.linePos += count; // we have never more then 1 line cached
                    return count;
                }
                else
                {
                    this.incReadLeftStartPos = 0;
                    this.incReadLeftEndPos = 0;
                    this.incReadLineInfo.linePos += count;
                    if (this.incReadDecoder.IsFull)
                    {
                        return count;
                    }
                }
            }

            var startPos = 0;
            var pos = 0;

            for (; ; )
            {

                switch (this.incReadState)
                {
                    case IncrementalReadState.Text:
                    case IncrementalReadState.Attributes:
                    case IncrementalReadState.AttributeValue:
                        break;
                    case IncrementalReadState.PI:
                        if (this.ParsePIValue(out startPos, out pos))
                        {
                            Debug.Assert(this.StrEqual(this.ps.chars, this.ps.charPos - 2, 2, "?>"));
                            this.ps.charPos -= 2;
                            this.incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.Comment:
                        if (this.ParseCDataOrComment(XmlNodeType.Comment, out startPos, out pos))
                        {
                            Debug.Assert(this.StrEqual(this.ps.chars, this.ps.charPos - 3, 3, "-->"));
                            this.ps.charPos -= 3;
                            this.incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.CDATA:
                        if (this.ParseCDataOrComment(XmlNodeType.CDATA, out startPos, out pos))
                        {
                            Debug.Assert(this.StrEqual(this.ps.chars, this.ps.charPos - 3, 3, "]]>"));
                            this.ps.charPos -= 3;
                            this.incReadState = IncrementalReadState.Text;
                        }

                        goto Append;
                    case IncrementalReadState.EndElement:
                        this.parsingFunction = ParsingFunction.PopElementContext;
                        this.nextParsingFunction = (this.index > 0 || this.fragmentType != XmlNodeType.Document) ? ParsingFunction.ElementContent
                                                                                                    : ParsingFunction.DocumentContent;
                        this.Read();
                        this.incReadState = IncrementalReadState.End;
                        goto case IncrementalReadState.End;
                    case IncrementalReadState.End:
                        return charsDecoded;
                    case IncrementalReadState.ReadData:
                        if (this.ReadData() == 0)
                        {
                            this.ThrowUnclosedElements();
                        }

                        this.incReadState = IncrementalReadState.Text;
                        startPos = this.ps.charPos;
                        pos = startPos;
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                Debug.Assert(this.incReadState == IncrementalReadState.Text ||
                              this.incReadState == IncrementalReadState.Attributes ||
                              this.incReadState == IncrementalReadState.AttributeValue);

                var chars = this.ps.chars;
                startPos = this.ps.charPos;
                pos = startPos;

                for (; ; )
                {
                    this.incReadLineInfo.Set(this.ps.LineNo, this.ps.LinePos);

                    char c;
                    if (this.incReadState == IncrementalReadState.Attributes)
                    {
                        while ((c = chars[pos]) > XmlCharType.MaxAsciiChar ||
                                ((this.xmlCharType.charProperties[c] & XmlCharType.fAttrValue) != 0 && c != '/'))
                        {
                            pos++;
                        }
                    }
                    else
                    {
                        while ((c = chars[pos]) > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[c] & XmlCharType.fAttrValue) != 0)
                        {
                            pos++;
                        }
                    }

                    if (chars[pos] == '&' || chars[pos] == (char)0x9)
                    {
                        pos++;
                        continue;
                    }

                    if (pos - startPos > 0)
                    {
                        goto AppendAndUpdateCharPos;
                    }

                    switch (chars[pos])
                    {
                        // eol
                        case (char)0xA:
                            pos++;
                            this.OnNewLine(pos);
                            continue;
                        case (char)0xD:
                            if (chars[pos + 1] == (char)0xA)
                            {
                                pos += 2;
                            }
                            else if (pos + 1 < this.ps.charsUsed)
                            {
                                pos++;
                            }
                            else
                            {
                                goto ReadData;
                            }

                            this.OnNewLine(pos);
                            continue;
                        // some tag
                        case '<':
                            if (this.incReadState != IncrementalReadState.Text)
                            {
                                pos++;
                                continue;
                            }

                            if (this.ps.charsUsed - pos < 2)
                            {
                                goto ReadData;
                            }

                            switch (chars[pos + 1])
                            {
                                // pi
                                case '?':
                                    pos += 2;
                                    this.incReadState = IncrementalReadState.PI;
                                    goto AppendAndUpdateCharPos;
                                // comment
                                case '!':
                                    if (this.ps.charsUsed - pos < 4)
                                    {
                                        goto ReadData;
                                    }

                                    if (chars[pos + 2] == '-' && chars[pos + 3] == '-')
                                    {
                                        pos += 4;
                                        this.incReadState = IncrementalReadState.Comment;
                                        goto AppendAndUpdateCharPos;
                                    }

                                    if (this.ps.charsUsed - pos < 9)
                                    {
                                        goto ReadData;
                                    }

                                    if (this.StrEqual(chars, pos + 2, 7, "[CDATA["))
                                    {
                                        pos += 9;
                                        this.incReadState = IncrementalReadState.CDATA;
                                        goto AppendAndUpdateCharPos;
                                    }
                                    else
                                    {
                                        ;//Throw( );
                                    }
                                    break;
                                // end tag
                                case '/':
                                    {
                                        Debug.Assert(this.ps.charPos - pos == 0);
                                        var endPos = this.ParseQName(true, 2, out var colonPos);
                                        if (this.StrEqual(chars, this.ps.charPos + 2, endPos - this.ps.charPos - 2, this.curNode.GetNameWPrefix(this.nameTable)) &&
                                            (this.ps.chars[endPos] == '>' || this.xmlCharType.IsWhiteSpace(this.ps.chars[endPos])))
                                        {

                                            if (--this.incReadDepth > 0)
                                            {
                                                pos = endPos + 1;
                                                continue;
                                            }

                                            this.ps.charPos = endPos;
                                            if (this.xmlCharType.IsWhiteSpace(this.ps.chars[endPos]))
                                            {
                                                this.EatWhitespaces(null);
                                            }

                                            if (this.ps.chars[this.ps.charPos] != '>')
                                            {
                                                this.ThrowUnexpectedToken(">");
                                            }

                                            this.ps.charPos++;

                                            this.incReadState = IncrementalReadState.EndElement;
                                            goto OuterContinue;
                                        }
                                        else
                                        {
                                            pos = endPos;
                                            continue;
                                        }
                                    }

                                // start tag
                                default:
                                    {
                                        Debug.Assert(this.ps.charPos - pos == 0);
                                        var endPos = this.ParseQName(true, 1, out var colonPos);
                                        if (this.StrEqual(this.ps.chars, this.ps.charPos + 1, endPos - this.ps.charPos - 1, this.curNode.localName) &&
                                            (this.ps.chars[endPos] == '>' || this.ps.chars[endPos] == '/' || this.xmlCharType.IsWhiteSpace(this.ps.chars[endPos])))
                                        {
                                            this.incReadDepth++;
                                            this.incReadState = IncrementalReadState.Attributes;
                                            pos = endPos;
                                            goto AppendAndUpdateCharPos;
                                        }

                                        pos = endPos;
                                        startPos = this.ps.charPos;
                                        chars = this.ps.chars;
                                        continue;
                                    }
                            }
                            break;
                        // end of start tag
                        case '/':
                            if (this.incReadState == IncrementalReadState.Attributes)
                            {
                                if (this.ps.charsUsed - pos < 2)
                                {
                                    goto ReadData;
                                }

                                if (chars[pos + 1] == '>')
                                {
                                    this.incReadState = IncrementalReadState.Text;
                                    this.incReadDepth--;
                                }
                            }

                            pos++;
                            continue;
                        // end of start tag
                        case '>':
                            if (this.incReadState == IncrementalReadState.Attributes)
                            {
                                this.incReadState = IncrementalReadState.Text;
                            }

                            pos++;
                            continue;
                        case '"':
                        case '\'':
                            switch (this.incReadState)
                            {
                                case IncrementalReadState.AttributeValue:
                                    if (chars[pos] == this.curNode.quoteChar)
                                    {
                                        this.incReadState = IncrementalReadState.Attributes;
                                    }
                                    break;
                                case IncrementalReadState.Attributes:
                                    this.curNode.quoteChar = chars[pos];
                                    this.incReadState = IncrementalReadState.AttributeValue;
                                    break;
                            }

                            pos++;
                            continue;
                        default:
                            // end of buffer
                            if (pos == this.ps.charsUsed)
                            {
                                goto ReadData;
                            }

                            // surrogate chars or invalid chars are ignored
                            else
                            {
                                pos++;
                                continue;
                            }
                    }
                }

            ReadData:
                this.incReadState = IncrementalReadState.ReadData;

            AppendAndUpdateCharPos:
                this.ps.charPos = pos;

            Append:
                // decode characters
                var charsParsed = pos - startPos;
                if (charsParsed > 0)
                {
                    int count;
                    try
                    {
                        count = this.incReadDecoder.Decode(this.ps.chars, startPos, charsParsed);
                    }
                    catch (XmlException e)
                    {
                        this.ReThrow(e, (int)this.incReadLineInfo.lineNo, (int)this.incReadLineInfo.linePos);
                        return 0;
                    }

                    Debug.Assert(count == charsParsed || this.incReadDecoder.IsFull, "Check if decoded consumed all characters unless it's full.");
                    charsDecoded += count;
                    if (this.incReadDecoder.IsFull)
                    {
                        this.incReadLeftStartPos = startPos + count;
                        this.incReadLeftEndPos = pos;
                        this.incReadLineInfo.linePos += count; // we have never more than 1 line cached
                        return charsDecoded;
                    }
                }
            }
        }

        private void FinishIncrementalRead()
        {
            this.incReadDecoder = new IncrementalReadDummyDecoder();
            this.IncrementalRead();
            Debug.Assert(this.IncrementalRead() == 0, "Previous call of IncrementalRead should eat up all characters!");
            this.incReadDecoder = null;
        }

        private bool ParseFragmentAttribute()
        {
            Debug.Assert(this.fragmentType == XmlNodeType.Attribute);

            // if first call then parse the whole attribute value
            if (this.curNode.type == XmlNodeType.None)
            {
                this.curNode.type = XmlNodeType.Attribute;
                this.curAttrIndex = 0;
                this.ParseAttributeValueSlow(this.ps.charPos, '"', this.curNode);
            }
            else
            {
                this.parsingFunction = ParsingFunction.InReadAttributeValue;
            }

            // return attribute value chunk
            if (this.ReadAttributeValue())
            {
                Debug.Assert(this.parsingFunction == ParsingFunction.InReadAttributeValue);
                this.parsingFunction = ParsingFunction.FragmentAttribute;
                return true;
            }
            else
            {
                this.OnEof();
                return false;
            }
        }

        private bool ParseAttributeValueChunk()
        {
            var chars = this.ps.chars;
            var pos = this.ps.charPos;

            this.curNode = this.AddNode(this.index + this.attrCount + 1, this.index + 2);
            this.curNode.SetLineInfo(this.ps.LineNo, this.ps.LinePos);

            Debug.Assert(this.stringBuilder.Length == 0);

            for (; ; )
            {
                while (chars[pos] > XmlCharType.MaxAsciiChar || (this.xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)
                {
                    pos++;
                }

                switch (chars[pos])
                {
                    // eol D
                    case (char)0xD:
                        Debug.Assert(this.ps.eolNormalized, "Entity replacement text for attribute values should be EOL-normalized!");
                        pos++;
                        continue;
                    // eol A, tab
                    case (char)0xA:
                    case (char)0x9:
                        if (this.normalize)
                        {
                            chars[pos] = (char)0x20;  // CDATA normalization of 0xA and 0x9
                        }

                        pos++;
                        continue;
                    case '"':
                    case '\'':
                    case '>':
                        pos++;
                        continue;
                    // attribute values cannot contain '<'
                    case '<':
                        this.Throw(pos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionStr('<'));
                        break;
                    // entity reference
                    case '&':
                        if (pos - this.ps.charPos > 0)
                        {
                            this.stringBuilder.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                        }

                        this.ps.charPos = pos;

                        // expand char entities but not general entities
                        switch (this.HandleEntityReference(true, EntityExpandType.OnlyCharacter, out pos))
                        {
                            case EntityType.CharacterDec:
                            case EntityType.CharacterHex:
                            case EntityType.CharacterNamed:
                                chars = this.ps.chars;
                                if (this.normalize && this.xmlCharType.IsWhiteSpace(chars[this.ps.charPos]) && pos - this.ps.charPos == 1)
                                {
                                    chars[this.ps.charPos] = (char)0x20;  // CDATA normalization of character references in entities
                                }
                                break;
                            case EntityType.Unexpanded:
                                Debug.Assert(false, "Found general entity in attribute");
                                throw new NotSupportedException();

                            default:
                                Debug.Assert(false, "We should never get to this point.");
                                break;
                        }

                        chars = this.ps.chars;
                        continue;
                    default:
                        // end of buffer
                        if (pos == this.ps.charsUsed)
                        {
                            goto ReadData;
                        }

                        // surrogate chars
                        else
                        {
                            var ch = chars[pos];
                            if (ch >= SurHighStart && ch <= SurHighEnd)
                            {
                                if (pos + 1 == this.ps.charsUsed)
                                {
                                    goto ReadData;
                                }

                                pos++;
                                if (chars[pos] >= SurLowStart && chars[pos] <= SurLowEnd)
                                {
                                    pos++;
                                    continue;
                                }
                            }

                            this.ThrowInvalidChar(pos, ch);
                            break;
                        }
                }

            ReadData:
                if (pos - this.ps.charPos > 0)
                {
                    this.stringBuilder.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                    this.ps.charPos = pos;
                }

                // read new characters into the buffer
                if (this.ReadData() == 0)
                {
                    if (this.stringBuilder.Length > 0)
                    {
                        goto ReturnText;
                    }
                    else
                    {
                        Debug.Assert(false, "We should never get to this point.");
                    }
                }

                pos = this.ps.charPos;
                chars = this.ps.chars;
            }

        ReturnText:
            if (pos - this.ps.charPos > 0)
            {
                this.stringBuilder.Append(chars, this.ps.charPos, pos - this.ps.charPos);
                this.ps.charPos = pos;
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
            catch (XmlException e)
            {
                this.ReThrow(e, e.LineNumber, e.LinePosition - 6); // 6 == strlen( "<?xml " );
            }
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken) => this.ThrowUnexpectedToken(pos, expectedToken, null);

        private void ThrowUnexpectedToken(string expectedToken1) => this.ThrowUnexpectedToken(expectedToken1, null);

        private void ThrowUnexpectedToken(int pos, string expectedToken1, string expectedToken2)
        {
            this.ps.charPos = pos;
            this.ThrowUnexpectedToken(expectedToken1, expectedToken2);
        }

        private void ThrowUnexpectedToken(string expectedToken1, string expectedToken2)
        {
            var unexpectedToken = this.ParseUnexpectedToken();
            if (expectedToken2 != null)
            {
                this.Throw(Res.Xml_UnexpectedTokens2, new string[3] { unexpectedToken, expectedToken1, expectedToken2 });
            }
            else
            {
                this.Throw(Res.Xml_UnexpectedTokenEx, new string[2] { unexpectedToken, expectedToken1 });
            }
        }

        private string ParseUnexpectedToken(int pos)
        {
            this.ps.charPos = pos;
            return this.ParseUnexpectedToken();
        }

        private string ParseUnexpectedToken()
        {
            if (this.xmlCharType.IsNCNameChar(this.ps.chars[this.ps.charPos]))
            {
                var pos = this.ps.charPos + 1;
                while (this.xmlCharType.IsNCNameChar(this.ps.chars[pos]))
                {
                    pos++;
                }

                return new string(this.ps.chars, this.ps.charPos, pos - this.ps.charPos);
            }
            else
            {
                Debug.Assert(this.ps.charPos < this.ps.charsUsed);
                return new string(this.ps.chars, this.ps.charPos, 1);
            }
        }

        private int GetIndexOfAttributeWithoutPrefix(string name)
        {
            name = this.nameTable.Get(name);
            if (name == null)
            {
                return -1;
            }

            for (var i = this.index + 1; i < this.index + this.attrCount + 1; i++)
            {
                if (Ref.Equal(this.nodes[i].localName, name) && this.nodes[i].prefix.Length == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetIndexOfAttributeWithPrefix(string name)
        {
            name = this.nameTable.Add(name);
            if (name == null)
            {
                return -1;
            }

            for (var i = this.index + 1; i < this.index + this.attrCount + 1; i++)
            {
                if (Ref.Equal(this.nodes[i].GetNameWPrefix(this.nameTable), name))
                {
                    return i;
                }
            }

            return -1;
        }

        bool MoveToNextContentNode(bool moveIfOnContentNode)
        {
            do
            {
                switch (this.curNode.type)
                {
                    case XmlNodeType.Attribute:
                        return !moveIfOnContentNode;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.CDATA:
                        if (!moveIfOnContentNode)
                        {
                            return true;
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        // skip comments, pis and end entity nodes
                        break;
                    case XmlNodeType.EntityReference:
                        this.ResolveEntity();
                        break;
                    default:
                        return false;
                }

                moveIfOnContentNode = false;
            } while (this.Read());
            return false;
        }

#if NETCF_XVR_SUPPORT
        internal bool XmlValidatingReaderCompatibilityMode {
            set {
                validatingReaderCompatFlag = value;
            }
        }

#endif
#if SCHEMA_VALIDATION
        internal ValidationEventHandler ValidationEventHandler {
            set {
                validationEventHandler = value;
            }
        }

#endif //SCHEMA_VALIDATION

        internal XmlNodeType FragmentType => this.fragmentType;

        internal void ChangeCurrentNodeType(XmlNodeType newNodeType)
        {
            Debug.Assert(this.curNode.type == XmlNodeType.Whitespace && newNodeType == XmlNodeType.SignificantWhitespace, "Incorrect node type change!");
            this.curNode.type = newNodeType;
        }

        internal object InternalTypedValue {
            get => this.curNode.typedValue;

            set => this.curNode.typedValue = value;
        }

        internal bool StandAlone => this.standalone;

        internal ConformanceLevel V1ComformanceLevel => this.fragmentType == XmlNodeType.Element ? ConformanceLevel.Fragment : ConformanceLevel.Document;

        static internal void AdjustLineInfo(char[] chars, int startPos, int endPos, bool isNormalized, ref LineInfo lineInfo)
        {
            var lastNewLinePos = -1;
            var i = startPos;
            while (i < endPos)
            {
                switch (chars[i])
                {
                    case '\n':
                        lineInfo.lineNo++;
                        lastNewLinePos = i;
                        break;
                    case '\r':
                        if (isNormalized)
                        {
                            break;
                        }

                        lineInfo.lineNo++;
                        lastNewLinePos = i;
                        if (i + 1 < endPos && chars[i + 1] == '\n')
                        {
                            i++;
                            lastNewLinePos++;
                        }
                        break;
                }

                i++;
            }

            if (lastNewLinePos >= 0)
            {
                lineInfo.linePos = endPos - lastNewLinePos;
            }
        }

        //----------------------------------------------------------------------------------------------
        // included from "XmlTextReaderHelpers.cs" because partial class definitions not supported
        //----------------------------------------------------------------------------------------------

        //
        // ParsingState
        //
        // Parsing state (aka. scanner data) - holds parsing buffer and entity input data information
        private struct ParsingState
        {
            // character buffer
            internal char[] chars;
            internal int charPos;
            internal int charsUsed;
            internal Encoding encoding;
            internal bool appendMode;

            // input stream & byte buffer
            internal Stream stream;
            internal Decoder decoder;
            internal byte[] bytes;
            internal int bytePos;
            internal int bytesUsed;

            // text reader input
            internal TextReader textReader;

            // current line number & position
            internal int lineNo;
            internal int lineStartPos;

            // base uri of the current entity
            internal string baseUriStr;

            // eof flag of the entity
            internal bool isEof;
            internal bool isStreamEof;

            // normalization
            internal bool eolNormalized;

            internal void Clear()
            {
                this.chars = null;
                this.charPos = 0;
                this.charsUsed = 0;
                this.encoding = null;
                this.stream = null;
                this.decoder = null;
                this.bytes = null;
                this.bytePos = 0;
                this.bytesUsed = 0;
                this.textReader = null;
                this.lineNo = 1;
                this.lineStartPos = -1;
                this.baseUriStr = "";
                this.isEof = false;
                this.isStreamEof = false;
                this.eolNormalized = true;
            }

            internal void Close(bool closeInput)
            {
                if (closeInput)
                {
                    if (this.stream != null)
                    {
                        this.stream.Close();
                    }
                }
            }

            internal int LineNo => this.lineNo;

            internal int LinePos => this.charPos - this.lineStartPos;
        }

        //
        // XmlContext
        //
        private class XmlContext
        {
            internal XmlSpace xmlSpace;
            internal string xmlLang;
            // WsdModification - Add namesapce collection. We will maintain a namespace list per context.
            // Storing a namespace per context should provide a way to assend path of context when searching
            // for a namespace.
            internal XmlNamespaces xmlNamespaces;
            internal XmlContext previousContext;

            internal XmlContext()
            {
                this.xmlSpace = XmlSpace.None;
                this.xmlLang = "";
                this.previousContext = null;
                this.xmlNamespaces = new XmlNamespaces();
            }

            internal XmlContext(XmlContext previousContext)
            {
                this.xmlSpace = previousContext.xmlSpace;
                this.xmlLang = previousContext.xmlLang;
                this.previousContext = previousContext;
                this.xmlNamespaces = previousContext.xmlNamespaces;
            }
        }

        //
        // NodeData
        //
        private class NodeData : IComparable
        {
            // static instance with no data - is used when XmlTextReader is closed
            static NodeData s_None;

            // NOTE: Do not use this property for reference comparison. It may not be unique.
            internal static NodeData None
            {
                get
                {
                    if (s_None == null)
                    {
                        // no locking; s_None is immutable so it's not a problem that it may get initialized more than once
                        s_None = new NodeData();
                    }

                    return s_None;
                }
            }

            // type
            internal XmlNodeType type;

            // name
            internal string localName;
            internal string prefix;
            internal string ns;
            internal string nameWPrefix;

            // value:
            // value == null -> the value is kept in the 'chars' buffer starting at valueStartPos and valueLength long
            string value;
            char[] chars;
            int valueStartPos;
            int valueLength;

            // main line info
            internal LineInfo lineInfo;

            // second line info
            //ISSUE: rswaney - Haven't determined how to get rid of the "not initialized" warngin on this variable
            // so temporarily initializing it here. The CF versions disables the warning with a pragma
            internal LineInfo lineInfo2 = new LineInfo(0, 0);

            // quote char for attributes
            internal char quoteChar;

            // depth
            internal int depth;

            // empty element / default attribute
            bool isEmptyOrDefault;

            // helper members
            internal bool xmlContextPushed;

            // attribute value chunks
            internal NodeData nextAttrValueChunk;

            // type info
            internal object schemaType;
            internal object typedValue;

            internal NodeData()
            {
                this.Clear(XmlNodeType.None);
                this.xmlContextPushed = false;
            }

            internal int LineNo => this.lineInfo.lineNo;

            internal int LinePos => this.lineInfo.linePos;

            internal bool IsEmptyElement {
                get => this.type == XmlNodeType.Element && this.isEmptyOrDefault;

                set {
                    Debug.Assert(this.type == XmlNodeType.Element);
                    this.isEmptyOrDefault = value;
                }
            }

            internal bool IsDefaultAttribute {
                get => this.type == XmlNodeType.Attribute && this.isEmptyOrDefault;

                set {
                    Debug.Assert(this.type == XmlNodeType.Attribute);
                    this.isEmptyOrDefault = value;
                }
            }

            internal bool ValueBuffered => this.value == null;

            internal string StringValue
            {
                get
                {
                    Debug.Assert(this.valueStartPos >= 0 || this.value != null, "Value not ready.");

                    if (this.value == null)
                    {
                        this.value = new string(this.chars, this.valueStartPos, this.valueLength);
                    }

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
                this.schemaType = null;
                this.typedValue = null;
            }

            internal void ClearName()
            {
                this.localName = "";
                this.prefix = "";
                this.ns = "";
                this.nameWPrefix = "";
            }

            internal void SetLineInfo(int lineNo, int linePos) => this.lineInfo.Set(lineNo, linePos);

            internal void SetLineInfo2(int lineNo, int linePos) => this.lineInfo2.Set(lineNo, linePos);

            internal void SetValueNode(XmlNodeType type, string value)
            {
                Debug.Assert(value != null);

                this.type = type;
                this.ClearName();
                this.value = value;
                this.valueStartPos = -1;
            }

            internal void SetValueNode(XmlNodeType type, char[] chars, int startPos, int len)
            {
                this.type = type;
                this.ClearName();

                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void SetNamedNode(XmlNodeType type, string localName) => this.SetNamedNode(type, localName, "", localName);

            internal void SetNamedNode(XmlNodeType type, string localName, string prefix, string nameWPrefix)
            {
                Debug.Assert(localName != null);
                Debug.Assert(localName.Length > 0);

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
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void OnBufferInvalidated()
            {
                if (this.value == null)
                {
                    Debug.Assert(this.valueStartPos != -1);
                    Debug.Assert(this.chars != null);
                    this.value = new string(this.chars, this.valueStartPos, this.valueLength);
                }

                this.valueStartPos = -1;
            }

            internal string GetAtomizedValue(XmlNameTable nameTable)
            {
                if (this.value == null)
                {
                    Debug.Assert(this.valueStartPos != -1);
                    Debug.Assert(this.chars != null);
                    return nameTable.Add(this.chars, this.valueStartPos, this.valueLength);
                }
                else
                {
                    return nameTable.Add(this.value);
                }
            }

            internal void CopyTo(BufferBuilder sb) => this.CopyTo(0, sb);

            internal void CopyTo(int valueOffset, BufferBuilder sb)
            {
                if (this.value == null)
                {
                    Debug.Assert(this.valueStartPos != -1);
                    Debug.Assert(this.chars != null);
                    sb.Append(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
                }
                else
                {
                    if (valueOffset <= 0)
                    {
                        sb.Append(this.value);
                    }
                    else
                    {
                        sb.Append(this.value, valueOffset, this.value.Length - valueOffset);
                    }
                }
            }

            internal int CopyTo(int valueOffset, char[] buffer, int offset, int length)
            {
                if (this.value == null)
                {
                    Debug.Assert(this.valueStartPos != -1);
                    Debug.Assert(this.chars != null);
                    var copyCount = this.valueLength - valueOffset;
                    if (copyCount > length)
                    {
                        copyCount = length;
                    }

                    Array.Copy(this.chars, (this.valueStartPos + valueOffset), buffer, offset, copyCount);
                    return copyCount;
                }
                else
                {
                    var copyCount = this.value.Length - valueOffset;
                    if (copyCount > length)
                    {
                        copyCount = length;
                    }

                    for (var i = 0; i < copyCount; i++)
                    {
                        buffer[offset + i] = this.value[valueOffset + i];
                    }

                    return copyCount;
                }
            }

            internal int CopyToBinary(IncrementalReadDecoder decoder, int valueOffset)
            {
                if (this.value == null)
                {
                    Debug.Assert(this.valueStartPos != -1);
                    Debug.Assert(this.chars != null);
                    return decoder.Decode(this.chars, this.valueStartPos + valueOffset, this.valueLength - valueOffset);
                }
                else
                {
                    return decoder.Decode(this.value, valueOffset, this.value.Length - valueOffset);
                }
            }

            internal void AdjustLineInfo(int valueOffset, bool isNormalized, ref LineInfo lineInfo)
            {
                if (valueOffset == 0)
                {
                    return;
                }

                if (this.valueStartPos != -1)
                {
                    XmlTextReader.AdjustLineInfo(this.chars, this.valueStartPos, this.valueStartPos + valueOffset, isNormalized, ref lineInfo);
                }
                else
                {
                    var chars = this.value.Substring(0, valueOffset).ToCharArray();
                    XmlTextReader.AdjustLineInfo(chars, 0, chars.Length, isNormalized, ref lineInfo);
                }
            }

            // This should be inlined by JIT compiler
            internal string GetNameWPrefix(XmlNameTable nt)
            {
                if (this.nameWPrefix != null)
                {
                    return this.nameWPrefix;
                }
                else
                {
                    return this.CreateNameWPrefix(nt);
                }
            }

            // WsdModification - Adds support for namespace prefix

            // WsdModification - Adds support for namespaceUri

            internal string CreateNameWPrefix(XmlNameTable nt)
            {
                Debug.Assert(this.nameWPrefix == null);
                if (this.prefix.Length == 0)
                {
                    this.nameWPrefix = this.localName;
                }
                else
                {
                    this.nameWPrefix = nt.Add(string.Concat(this.prefix, ":", this.localName));
                }

                return this.nameWPrefix;
            }

            int IComparable.CompareTo(object obj)
            {
                if (obj is NodeData other) {
                    if (Ref.Equal(this.localName, other.localName)) {
                        if (Ref.Equal(this.ns, other.ns)) {
                            return 0;
                        }
                        else {
                            return string.Compare(this.ns, other.ns);
                        }
                    }
                    else {
                        return string.Compare(this.localName, other.localName);
                    }
                }
                else {
                    Debug.Assert(false, "We should never get to this point.");
                    return 1;
                }
            }
        }
    }
}


