using System;
using System.IO;
using TinyXml = GHIElectronics.TinyCLR.Data.Xml;

namespace System.Xml {

    // ---- Enums (copy values verbatim from TinyXml; matches .NET BCL) ----

    /// <summary>Node kinds in an XML document. Same values as .NET's <c>System.Xml.XmlNodeType</c>.</summary>
    public enum XmlNodeType {
        /// <summary>No node returned, or the reader has not yet read anything.</summary>
        None = 0,
        /// <summary>An element start tag.</summary>
        Element = 1,
        /// <summary>An attribute.</summary>
        Attribute = 2,
        /// <summary>The text content of a node.</summary>
        Text = 3,
        /// <summary>A CDATA section.</summary>
        CDATA = 4,
        /// <summary>A reference to an entity.</summary>
        EntityReference = 5,
        /// <summary>An entity declaration.</summary>
        Entity = 6,
        /// <summary>A processing instruction.</summary>
        ProcessingInstruction = 7,
        /// <summary>A comment.</summary>
        Comment = 8,
        /// <summary>The document root that contains the whole tree.</summary>
        Document = 9,
        /// <summary>A document type declaration.</summary>
        DocumentType = 10,
        /// <summary>A document fragment.</summary>
        DocumentFragment = 11,
        /// <summary>A notation in a document type declaration.</summary>
        Notation = 12,
        /// <summary>Whitespace between markup.</summary>
        Whitespace = 13,
        /// <summary>Whitespace between markup in a mixed content model, or within an xml:space="preserve" scope.</summary>
        SignificantWhitespace = 14,
        /// <summary>An element end tag.</summary>
        EndElement = 15,
        /// <summary>The end of an included entity.</summary>
        EndEntity = 16,
        /// <summary>The XML declaration.</summary>
        XmlDeclaration = 17,
    }

    /// <summary>Reader lifecycle state.</summary>
    public enum ReadState {
        /// <summary>The Read method has not been called yet.</summary>
        Initial = 0,
        /// <summary>The Read method has been called and more can be read.</summary>
        Interactive = 1,
        /// <summary>An error occurred that prevents further reading.</summary>
        Error = 2,
        /// <summary>The end of the file has been reached.</summary>
        EndOfFile = 3,
        /// <summary>The reader has been closed.</summary>
        Closed = 4,
    }

    /// <summary>XML conformance level (auto-detect, fragment, or full document).</summary>
    public enum ConformanceLevel {
        /// <summary>The conformance level is detected automatically from the input.</summary>
        Auto = 0,
        /// <summary>The input conforms to the rules for a well-formed XML fragment.</summary>
        Fragment = 1,
        /// <summary>The input conforms to the rules for a well-formed XML document.</summary>
        Document = 2,
    }

    /// <summary>How the reader treats whitespace nodes.</summary>
    public enum WhitespaceHandling {
        /// <summary>Return all whitespace and significant whitespace nodes.</summary>
        All = 0,
        /// <summary>Return only significant whitespace nodes.</summary>
        Significant = 1,
        /// <summary>Return no whitespace nodes.</summary>
        None = 2,
    }

    /// <summary>How the writer represents line breaks.</summary>
    public enum NewLineHandling {
        /// <summary>Replace new line characters with the configured new line string.</summary>
        Replace = 0,
        /// <summary>Replace new line characters with character entities.</summary>
        Entitize = 1,
        /// <summary>Leave new line characters unchanged.</summary>
        None = 2,
    }

    /// <summary>Value of an in-scope <c>xml:space</c> attribute.</summary>
    public enum XmlSpace {
        /// <summary>No xml:space scope is in effect.</summary>
        None = 0,
        /// <summary>The xml:space scope equals "default".</summary>
        Default = 1,
        /// <summary>The xml:space scope equals "preserve".</summary>
        Preserve = 2,
    }

    /// <summary>XML validation policy (none or DTD/Schema).</summary>
    public enum ValidationType {
        /// <summary>No validation is performed.</summary>
        None = 0,
        /// <summary>The validation type is detected automatically.</summary>
        Auto = 1,
        /// <summary>Validate according to a DTD.</summary>
        DTD = 2,
        /// <summary>Validate according to XML-Data Reduced (XDR) schemas.</summary>
        XDR = 3,
        /// <summary>Validate according to XSD schemas.</summary>
        Schema = 4,
    }

    // ---- Interface ----

    /// <summary>Provides line and position information for the current node.</summary>
    public interface IXmlLineInfo {
        /// <summary>Gets a value indicating whether line information is available.</summary>
        bool HasLineInfo();
        /// <summary>Gets the line number of the current node.</summary>
        int LineNumber { get; }
        /// <summary>Gets the line position of the current node.</summary>
        int LinePosition { get; }
    }

    // ---- Exception ----

    /// <summary>Thrown for XML parse errors; carries line/position info.</summary>
    public class XmlException : Exception {
        /// <summary>Initializes a new instance of the <see cref="XmlException"/> class.</summary>
        public XmlException() : base() { }
        /// <summary>Initializes a new instance of the <see cref="XmlException"/> class with the specified message.</summary>
        public XmlException(string message) : base(message) { }
        /// <summary>Initializes a new instance of the <see cref="XmlException"/> class with the specified message and inner exception.</summary>
        public XmlException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>The line number where the error occurred.</summary>
        public int LineNumber { get; }
        /// <summary>The line position where the error occurred.</summary>
        public int LinePosition { get; }
        /// <summary>Initializes a new instance of the <see cref="XmlException"/> class with the specified message, inner exception, and line information.</summary>
        public XmlException(string message, Exception innerException, int lineNumber, int linePosition)
            : base(message, innerException) {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }
    }

    // ---- Name table (abstract base) ----

    /// <summary>Atomized-string table shared between readers and writers.</summary>
    public abstract class XmlNameTable {
        /// <summary>Atomizes the specified string and adds it to the table.</summary>
        public abstract string Add(string array);
        /// <summary>Atomizes the specified character range and adds it to the table.</summary>
        public abstract string Add(char[] array, int offset, int length);
        /// <summary>Gets the atomized string equal to the specified string, or null if it is not in the table.</summary>
        public abstract string Get(string array);
        /// <summary>Gets the atomized string equal to the specified character range, or null if it is not in the table.</summary>
        public abstract string Get(char[] array, int offset, int length);
    }

    /// <summary>Hashtable-backed <see cref="XmlNameTable"/>.</summary>
    public class NameTable : XmlNameTable {
        private readonly TinyXml.NameTable inner;

        /// <summary>Initializes a new instance of the <see cref="NameTable"/> class.</summary>
        public NameTable() => this.inner = new TinyXml.NameTable();

        internal NameTable(TinyXml.NameTable existing) => this.inner = existing;

        internal TinyXml.XmlNameTable Inner => this.inner;

        /// <inheritdoc/>
        public override string Add(string array) => this.inner.Add(array);
        /// <inheritdoc/>
        public override string Add(char[] array, int offset, int length) => this.inner.Add(array, offset, length);
        /// <inheritdoc/>
        public override string Get(string array) => this.inner.Get(array);
        /// <inheritdoc/>
        public override string Get(char[] array, int offset, int length) => this.inner.Get(array, offset, length);
    }

    // ---- XmlReaderSettings (data wrapper) ----

    /// <summary>Settings bag passed to <see cref="XmlReader.Create(Stream, XmlReaderSettings)"/>. Mirrors the .NET BCL type.</summary>
    public class XmlReaderSettings {
        private readonly TinyXml.XmlReaderSettings inner;

        /// <summary>Initializes a new instance of the <see cref="XmlReaderSettings"/> class.</summary>
        public XmlReaderSettings() => this.inner = new TinyXml.XmlReaderSettings();

        internal TinyXml.XmlReaderSettings Inner => this.inner;

        /// <summary>The name table used by the reader. Not wrapped; always returns null.</summary>
        public XmlNameTable NameTable {
            get => null; // not wrapping the name table here; user-level access is rare
            set { }
        }

        /// <summary>The starting line number reported by the reader.</summary>
        public int LineNumberOffset {
            get => this.inner.LineNumberOffset;
            set => this.inner.LineNumberOffset = value;
        }

        /// <summary>The starting line position reported by the reader.</summary>
        public int LinePositionOffset {
            get => this.inner.LinePositionOffset;
            set => this.inner.LinePositionOffset = value;
        }

        /// <summary>The level of conformance the reader enforces.</summary>
        public ConformanceLevel ConformanceLevel {
            get => (ConformanceLevel)(int)this.inner.ConformanceLevel;
            set => this.inner.ConformanceLevel = (TinyXml.ConformanceLevel)(int)value;
        }

        /// <summary>Indicates whether the reader checks that characters are legal XML.</summary>
        public bool CheckCharacters {
            get => this.inner.CheckCharacters;
            set => this.inner.CheckCharacters = value;
        }

        /// <summary>The type of validation the reader performs.</summary>
        public ValidationType ValidationType {
            get => (ValidationType)(int)this.inner.ValidationType;
            set => this.inner.ValidationType = (TinyXml.ValidationType)(int)value;
        }

        /// <summary>Indicates whether to ignore insignificant whitespace.</summary>
        public bool IgnoreWhitespace {
            get => this.inner.IgnoreWhitespace;
            set => this.inner.IgnoreWhitespace = value;
        }

        /// <summary>Indicates whether to ignore processing instructions.</summary>
        public bool IgnoreProcessingInstructions {
            get => this.inner.IgnoreProcessingInstructions;
            set => this.inner.IgnoreProcessingInstructions = value;
        }

        /// <summary>Indicates whether to ignore comments.</summary>
        public bool IgnoreComments {
            get => this.inner.IgnoreComments;
            set => this.inner.IgnoreComments = value;
        }

        /// <summary>Indicates whether to close the underlying input when the reader is closed.</summary>
        public bool CloseInput {
            get => this.inner.CloseInput;
            set => this.inner.CloseInput = value;
        }

        /// <summary>Resets the settings to their default values.</summary>
        public void Reset() => this.inner.Reset();

        /// <summary>Returns a copy of this settings object.</summary>
        public XmlReaderSettings Clone() {
            var copy = new XmlReaderSettings();
            copy.inner.LineNumberOffset = this.inner.LineNumberOffset;
            copy.inner.LinePositionOffset = this.inner.LinePositionOffset;
            copy.inner.ConformanceLevel = this.inner.ConformanceLevel;
            copy.inner.CheckCharacters = this.inner.CheckCharacters;
            copy.inner.ValidationType = this.inner.ValidationType;
            copy.inner.IgnoreWhitespace = this.inner.IgnoreWhitespace;
            copy.inner.IgnoreProcessingInstructions = this.inner.IgnoreProcessingInstructions;
            copy.inner.IgnoreComments = this.inner.IgnoreComments;
            copy.inner.CloseInput = this.inner.CloseInput;
            return copy;
        }
    }

    // ---- XmlReader (abstract; .NET-shape facade backed by an inner TinyXml reader) ----

    /// <summary>Forward-only XML reader. Same surface as .NET's <c>System.Xml.XmlReader</c>.</summary>
    public abstract class XmlReader : IDisposable {
        /// <summary>The type of the current node.</summary>
        public abstract XmlNodeType NodeType { get; }
        /// <summary>The local name of the current node.</summary>
        public abstract string LocalName { get; }
        /// <summary>The namespace URI of the current node.</summary>
        public abstract string NamespaceURI { get; }
        /// <summary>The namespace prefix of the current node.</summary>
        public abstract string Prefix { get; }
        /// <summary>Indicates whether the current node has a value.</summary>
        public abstract bool HasValue { get; }
        /// <summary>The text value of the current node.</summary>
        public abstract string Value { get; }
        /// <summary>The depth of the current node in the element tree.</summary>
        public abstract int Depth { get; }
        /// <summary>The base URI of the current node.</summary>
        public abstract string BaseURI { get; }
        /// <summary>Indicates whether the current element is empty.</summary>
        public abstract bool IsEmptyElement { get; }
        /// <summary>The number of attributes on the current node.</summary>
        public abstract int AttributeCount { get; }
        /// <summary>Indicates whether the reader is positioned at the end of the stream.</summary>
        public abstract bool EOF { get; }
        /// <summary>The current state of the reader.</summary>
        public abstract ReadState ReadState { get; }

        /// <summary>The qualified name of the current node.</summary>
        public virtual string Name {
            get {
                var prefix = this.Prefix;
                var local = this.LocalName;
                return string.IsNullOrEmpty(prefix) ? local : prefix + ":" + local;
            }
        }
        /// <summary>Indicates whether the current node has any attributes.</summary>
        public virtual bool HasAttributes => this.AttributeCount > 0;

        /// <summary>Gets the value of the attribute with the specified name.</summary>
        public abstract string GetAttribute(string name);
        /// <summary>Gets the value of the attribute with the specified local name and namespace URI.</summary>
        public abstract string GetAttribute(string name, string namespaceURI);
        /// <summary>Gets the value of the attribute at the specified index.</summary>
        public abstract string GetAttribute(int i);
        /// <summary>Moves to the attribute with the specified name.</summary>
        public abstract bool MoveToAttribute(string name);
        /// <summary>Moves to the attribute with the specified local name and namespace URI.</summary>
        public abstract bool MoveToAttribute(string name, string ns);
        /// <summary>Moves to the first attribute of the current node.</summary>
        public abstract bool MoveToFirstAttribute();
        /// <summary>Moves to the next attribute of the current node.</summary>
        public abstract bool MoveToNextAttribute();
        /// <summary>Moves to the element that contains the current attribute node.</summary>
        public abstract bool MoveToElement();
        /// <summary>Parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.</summary>
        public abstract bool ReadAttributeValue();
        /// <summary>Reads the next node from the stream.</summary>
        public abstract bool Read();
        /// <summary>Closes the reader and the underlying stream.</summary>
        public abstract void Close();
        /// <summary>Resolves a namespace prefix in the scope of the current node.</summary>
        public abstract string LookupNamespace(string prefix);
        /// <summary>Resolves the entity reference for EntityReference nodes.</summary>
        public abstract void ResolveEntity();

        /// <summary>Skips the children of the current node.</summary>
        public virtual void Skip() { while (this.Read()) { /* drain to next sibling */ } }
        /// <summary>Reads the contents of the current text or element node as a string.</summary>
        public virtual string ReadString() => this.NodeType == XmlNodeType.Element || this.NodeType == XmlNodeType.Text ? this.Value : "";
        /// <summary>Indicates whether the current content node is a start tag.</summary>
        public virtual bool IsStartElement() => this.MoveToContent() == XmlNodeType.Element;
        /// <summary>Indicates whether the current content node is a start tag with the specified name.</summary>
        public virtual bool IsStartElement(string name) => this.IsStartElement() && this.Name == name;
        /// <summary>Advances past non-content nodes to the next content node and returns its type.</summary>
        public virtual XmlNodeType MoveToContent() {
            do {
                switch (this.NodeType) {
                    case XmlNodeType.Attribute:
                        this.MoveToElement();
                        goto case XmlNodeType.Element;
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.EndEntity:
                        return this.NodeType;
                }
            } while (this.Read());
            return this.NodeType;
        }
        /// <summary>Verifies the current content node is a start tag and advances the reader past it.</summary>
        public virtual void ReadStartElement() {
            if (this.MoveToContent() != XmlNodeType.Element)
                throw new XmlException("Expected element");
            this.Read();
        }
        /// <summary>Verifies the current content node is a start tag with the specified name and advances the reader past it.</summary>
        public virtual void ReadStartElement(string name) {
            if (!this.IsStartElement(name))
                throw new XmlException("Expected start element <" + name + ">");
            this.Read();
        }
        /// <summary>Verifies the current content node is an end tag and advances the reader past it.</summary>
        public virtual void ReadEndElement() {
            if (this.MoveToContent() != XmlNodeType.EndElement)
                throw new XmlException("Expected end element");
            this.Read();
        }
        /// <summary>Reads a text-only element and returns its content as a string.</summary>
        public virtual string ReadElementString() {
            this.ReadStartElement();
            var v = this.NodeType == XmlNodeType.Text ? this.Value : "";
            if (this.NodeType == XmlNodeType.Text) this.Read();
            this.ReadEndElement();
            return v;
        }

        /// <summary>Releases the resources used by the reader.</summary>
        public virtual void Dispose() => this.Close();

        // ---- Static factories — return facade-typed readers ----

        /// <summary>Creates a new reader over the specified stream.</summary>
        public static XmlReader Create(Stream input) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return new WrappingXmlReader(TinyXml.XmlReader.Create(input));
        }

        /// <summary>Creates a new reader over the specified stream using the specified settings.</summary>
        public static XmlReader Create(Stream input, XmlReaderSettings settings) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var inner = settings == null
                ? TinyXml.XmlReader.Create(input)
                : TinyXml.XmlReader.Create(input, settings.Inner);
            return new WrappingXmlReader(inner);
        }

        /// <summary>Creates a new reader over the specified stream using the specified settings and base URI.</summary>
        public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var inner = TinyXml.XmlReader.Create(input, settings == null ? null : settings.Inner, baseUri);
            return new WrappingXmlReader(inner);
        }

        /// <summary>Indicates whether the specified string is a valid XML name.</summary>
        public static bool IsName(string str) => TinyXml.XmlReader.IsName(str);
        /// <summary>Indicates whether the specified string is a valid XML name token.</summary>
        public static bool IsNameToken(string str) => TinyXml.XmlReader.IsNameToken(str);
    }

    // Concrete forwarder. Holds an inner TinyXml.XmlReader and delegates every abstract
    // member to it. Keeps the .NET-shape facade decoupled from the impl's concrete subclasses.
    internal sealed class WrappingXmlReader : XmlReader {
        private readonly TinyXml.XmlReader inner;
        public WrappingXmlReader(TinyXml.XmlReader inner) => this.inner = inner;

        public override XmlNodeType NodeType => (XmlNodeType)(int)this.inner.NodeType;
        public override string LocalName => this.inner.LocalName;
        public override string NamespaceURI => this.inner.NamespaceURI;
        public override string Prefix => this.inner.Prefix;
        public override bool HasValue => this.inner.HasValue;
        public override string Value => this.inner.Value;
        public override int Depth => this.inner.Depth;
        public override string BaseURI => this.inner.BaseURI;
        public override bool IsEmptyElement => this.inner.IsEmptyElement;
        public override int AttributeCount => this.inner.AttributeCount;
        public override bool EOF => this.inner.EOF;
        public override ReadState ReadState => (ReadState)(int)this.inner.ReadState;

        public override string GetAttribute(string name) => this.inner.GetAttribute(name);
        public override string GetAttribute(string name, string namespaceURI) => this.inner.GetAttribute(name, namespaceURI);
        public override string GetAttribute(int i) => this.inner.GetAttribute(i);
        public override bool MoveToAttribute(string name) => this.inner.MoveToAttribute(name);
        public override bool MoveToAttribute(string name, string ns) => this.inner.MoveToAttribute(name, ns);
        public override bool MoveToFirstAttribute() => this.inner.MoveToFirstAttribute();
        public override bool MoveToNextAttribute() => this.inner.MoveToNextAttribute();
        public override bool MoveToElement() => this.inner.MoveToElement();
        public override bool ReadAttributeValue() => this.inner.ReadAttributeValue();

        // Parsing errors bubble up here as TinyXml.XmlException. Translate to the
        // BCL-shape System.Xml.XmlException so user code's `catch (XmlException)`
        // catches on both sides — on Desktop the typeref forwards to the BCL
        // type, which the BCL reader naturally throws. Match that here.
        public override bool Read() {
            try { return this.inner.Read(); }
            catch (TinyXml.XmlException ex) {
                throw new XmlException(ex.Message, ex, ex.LineNumber, ex.LinePosition);
            }
        }

        public override void Close() => this.inner.Close();
        public override string LookupNamespace(string prefix) => this.inner.LookupNamespace(prefix);
        public override void ResolveEntity() => this.inner.ResolveEntity();
    }

    // ---- XmlWriter (abstract, .NET-shape; backed by TinyXml.XmlWriter which is concrete) ----

    /// <summary>Forward-only XML writer. Same surface as .NET's <c>System.Xml.XmlWriter</c>.</summary>
    public abstract class XmlWriter : IDisposable {
        /// <summary>Writes the XML declaration.</summary>
        public abstract void WriteStartDocument();
        /// <summary>Writes the XML declaration with the specified standalone attribute.</summary>
        public abstract void WriteStartDocument(bool standalone);
        /// <summary>Closes any open elements and the document.</summary>
        public abstract void WriteEndDocument();
        /// <summary>Writes a start tag with the specified local name.</summary>
        public abstract void WriteStartElement(string localName);
        /// <summary>Writes a start tag with the specified local name and namespace URI.</summary>
        public abstract void WriteStartElement(string localName, string ns);
        /// <summary>Writes a start tag with the specified prefix, local name, and namespace URI.</summary>
        public abstract void WriteStartElement(string prefix, string localName, string ns);
        /// <summary>Closes the most recently opened element.</summary>
        public abstract void WriteEndElement();
        /// <summary>Closes the most recently opened element, always writing a full end tag.</summary>
        public abstract void WriteFullEndElement();
        /// <summary>Writes an attribute with the specified local name and value.</summary>
        public abstract void WriteAttributeString(string localName, string value);
        /// <summary>Writes an attribute with the specified local name, namespace URI, and value.</summary>
        public abstract void WriteAttributeString(string localName, string ns, string value);
        /// <summary>Writes an attribute with the specified prefix, local name, namespace URI, and value.</summary>
        public abstract void WriteAttributeString(string prefix, string localName, string ns, string value);
        /// <summary>Writes the specified text content.</summary>
        public abstract void WriteString(string text);
        /// <summary>Writes the specified markup verbatim without escaping.</summary>
        public abstract void WriteRaw(string data);
        /// <summary>Writes the specified text inside a CDATA section.</summary>
        public abstract void WriteCData(string text);
        /// <summary>Writes the specified text inside a comment.</summary>
        public abstract void WriteComment(string text);
        /// <summary>Writes a processing instruction with the specified name and text.</summary>
        public abstract void WriteProcessingInstruction(string name, string text);
        /// <summary>Flushes buffered output to the underlying stream.</summary>
        public abstract void Flush();
        /// <summary>Closes the writer and flushes any buffered output.</summary>
        public abstract void Close();
        /// <summary>Returns the closest prefix in scope for the specified namespace URI.</summary>
        public abstract string LookupPrefix(string ns);

        /// <summary>Writes an element with the specified local name and text content.</summary>
        public void WriteElementString(string localName, string value) {
            this.WriteStartElement(localName);
            this.WriteString(value);
            this.WriteEndElement();
        }

        /// <summary>Releases the resources used by the writer.</summary>
        public virtual void Dispose() => this.Close();

        /// <summary>Creates a new writer over the specified stream.</summary>
        public static XmlWriter Create(Stream output) {
            if (output == null) throw new ArgumentNullException(nameof(output));
            return new WrappingXmlWriter(TinyXml.XmlWriter.Create(output));
        }
    }

    internal sealed class WrappingXmlWriter : XmlWriter {
        private readonly TinyXml.XmlWriter inner;
        private bool closed;
        public WrappingXmlWriter(TinyXml.XmlWriter inner) => this.inner = inner;

        public override void WriteStartDocument() => this.inner.WriteStartDocument();
        public override void WriteStartDocument(bool standalone) => this.inner.WriteStartDocument(standalone);
        public override void WriteEndDocument() => this.inner.WriteEndDocument();
        public override void WriteStartElement(string localName) => this.inner.WriteStartElement(localName);
        public override void WriteStartElement(string localName, string ns) { this.inner.WriteStartElement(localName, ns); }
        public override void WriteStartElement(string prefix, string localName, string ns) { this.inner.WriteStartElement(prefix, localName, ns); }
        public override void WriteEndElement() => this.inner.WriteEndElement();
        public override void WriteFullEndElement() => this.inner.WriteFullEndElement();
        public override void WriteAttributeString(string localName, string value) => this.inner.WriteAttributeString(localName, value);
        public override void WriteAttributeString(string localName, string ns, string value) => this.inner.WriteAttributeString(localName, ns, value);
        public override void WriteAttributeString(string prefix, string localName, string ns, string value) => this.inner.WriteAttributeString(prefix, localName, ns, value);
        public override void WriteString(string text) => this.inner.WriteString(text);
        public override void WriteRaw(string data) => this.inner.WriteRaw(data);
        public override void WriteCData(string text) => this.inner.WriteCData(text);
        public override void WriteComment(string text) => this.inner.WriteComment(text);
        public override void WriteProcessingInstruction(string name, string text) => this.inner.WriteProcessingInstruction(name, text);
        public override void Flush() => this.inner.Flush();

        // BCL XmlWriter.Close (with default XmlWriterSettings.CloseOutput == false)
        // flushes the buffered XML but LEAVES the underlying stream open. The
        // user owns the stream lifecycle. TinyXml.XmlWriter.Close eagerly closes
        // the inner stream — that mismatches BCL and breaks the common pattern
        //   var ms = new MemoryStream();
        //   using (var w = XmlWriter.Create(ms)) { ... }
        //   ms.Position = 0;   // would throw ObjectDisposedException
        //   var bytes = ms.ToArray();
        // So we flush only — the inner XmlWriter's internal state is unreachable
        // after our wrapper is disposed; GC will reclaim it. The user's stream
        // stays open and seekable.
        public override void Close() {
            if (this.closed) return;
            this.closed = true;
            this.inner.Flush();
        }

        public override string LookupPrefix(string ns) => this.inner.LookupPrefix(ns);
    }
}
