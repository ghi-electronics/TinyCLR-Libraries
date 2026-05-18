using System;
using System.IO;
using TinyXml = GHIElectronics.TinyCLR.Data.Xml;

namespace System.Xml {

    // ---- Enums (copy values verbatim from TinyXml; matches .NET BCL) ----

    /// <summary>Node kinds in an XML document. Same values as .NET's <c>System.Xml.XmlNodeType</c>.</summary>
    public enum XmlNodeType {
        None = 0,
        Element = 1,
        Attribute = 2,
        Text = 3,
        CDATA = 4,
        EntityReference = 5,
        Entity = 6,
        ProcessingInstruction = 7,
        Comment = 8,
        Document = 9,
        DocumentType = 10,
        DocumentFragment = 11,
        Notation = 12,
        Whitespace = 13,
        SignificantWhitespace = 14,
        EndElement = 15,
        EndEntity = 16,
        XmlDeclaration = 17,
    }

    /// <summary>Reader lifecycle state.</summary>
    public enum ReadState {
        Initial = 0,
        Interactive = 1,
        Error = 2,
        EndOfFile = 3,
        Closed = 4,
    }

    /// <summary>XML conformance level (auto-detect, fragment, or full document).</summary>
    public enum ConformanceLevel {
        Auto = 0,
        Fragment = 1,
        Document = 2,
    }

    /// <summary>How the reader treats whitespace nodes.</summary>
    public enum WhitespaceHandling {
        All = 0,
        Significant = 1,
        None = 2,
    }

    /// <summary>How the writer represents line breaks.</summary>
    public enum NewLineHandling {
        Replace = 0,
        Entitize = 1,
        None = 2,
    }

    /// <summary>Value of an in-scope <c>xml:space</c> attribute.</summary>
    public enum XmlSpace {
        None = 0,
        Default = 1,
        Preserve = 2,
    }

    /// <summary>XML validation policy (none or DTD/Schema).</summary>
    public enum ValidationType {
        None = 0,
        Auto = 1,
        DTD = 2,
        XDR = 3,
        Schema = 4,
    }

    // ---- Interface ----

    public interface IXmlLineInfo {
        bool HasLineInfo();
        int LineNumber { get; }
        int LinePosition { get; }
    }

    // ---- Exception ----

    /// <summary>Thrown for XML parse errors; carries line/position info.</summary>
    public class XmlException : Exception {
        public XmlException() : base() { }
        public XmlException(string message) : base(message) { }
        public XmlException(string message, Exception innerException) : base(message, innerException) { }
        public int LineNumber { get; }
        public int LinePosition { get; }
        public XmlException(string message, Exception innerException, int lineNumber, int linePosition)
            : base(message, innerException) {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }
    }

    // ---- Name table (abstract base) ----

    /// <summary>Atomized-string table shared between readers and writers.</summary>
    public abstract class XmlNameTable {
        public abstract string Add(string array);
        public abstract string Add(char[] array, int offset, int length);
        public abstract string Get(string array);
        public abstract string Get(char[] array, int offset, int length);
    }

    /// <summary>Hashtable-backed <see cref="XmlNameTable"/>.</summary>
    public class NameTable : XmlNameTable {
        private readonly TinyXml.NameTable inner;

        public NameTable() => this.inner = new TinyXml.NameTable();

        internal NameTable(TinyXml.NameTable existing) => this.inner = existing;

        internal TinyXml.XmlNameTable Inner => this.inner;

        public override string Add(string array) => this.inner.Add(array);
        public override string Add(char[] array, int offset, int length) => this.inner.Add(array, offset, length);
        public override string Get(string array) => this.inner.Get(array);
        public override string Get(char[] array, int offset, int length) => this.inner.Get(array, offset, length);
    }

    // ---- XmlReaderSettings (data wrapper) ----

    /// <summary>Settings bag passed to <see cref="XmlReader.Create(Stream, XmlReaderSettings)"/>. Mirrors the .NET BCL type.</summary>
    public class XmlReaderSettings {
        private readonly TinyXml.XmlReaderSettings inner;

        public XmlReaderSettings() => this.inner = new TinyXml.XmlReaderSettings();

        internal TinyXml.XmlReaderSettings Inner => this.inner;

        public XmlNameTable NameTable {
            get => null; // not wrapping the name table here; user-level access is rare
            set { }
        }

        public int LineNumberOffset {
            get => this.inner.LineNumberOffset;
            set => this.inner.LineNumberOffset = value;
        }

        public int LinePositionOffset {
            get => this.inner.LinePositionOffset;
            set => this.inner.LinePositionOffset = value;
        }

        public ConformanceLevel ConformanceLevel {
            get => (ConformanceLevel)(int)this.inner.ConformanceLevel;
            set => this.inner.ConformanceLevel = (TinyXml.ConformanceLevel)(int)value;
        }

        public bool CheckCharacters {
            get => this.inner.CheckCharacters;
            set => this.inner.CheckCharacters = value;
        }

        public ValidationType ValidationType {
            get => (ValidationType)(int)this.inner.ValidationType;
            set => this.inner.ValidationType = (TinyXml.ValidationType)(int)value;
        }

        public bool IgnoreWhitespace {
            get => this.inner.IgnoreWhitespace;
            set => this.inner.IgnoreWhitespace = value;
        }

        public bool IgnoreProcessingInstructions {
            get => this.inner.IgnoreProcessingInstructions;
            set => this.inner.IgnoreProcessingInstructions = value;
        }

        public bool IgnoreComments {
            get => this.inner.IgnoreComments;
            set => this.inner.IgnoreComments = value;
        }

        public bool CloseInput {
            get => this.inner.CloseInput;
            set => this.inner.CloseInput = value;
        }

        public void Reset() => this.inner.Reset();

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
        public abstract XmlNodeType NodeType { get; }
        public abstract string LocalName { get; }
        public abstract string NamespaceURI { get; }
        public abstract string Prefix { get; }
        public abstract bool HasValue { get; }
        public abstract string Value { get; }
        public abstract int Depth { get; }
        public abstract string BaseURI { get; }
        public abstract bool IsEmptyElement { get; }
        public abstract int AttributeCount { get; }
        public abstract bool EOF { get; }
        public abstract ReadState ReadState { get; }

        public virtual string Name {
            get {
                var prefix = this.Prefix;
                var local = this.LocalName;
                return string.IsNullOrEmpty(prefix) ? local : prefix + ":" + local;
            }
        }
        public virtual bool HasAttributes => this.AttributeCount > 0;

        public abstract string GetAttribute(string name);
        public abstract string GetAttribute(string name, string namespaceURI);
        public abstract string GetAttribute(int i);
        public abstract bool MoveToAttribute(string name);
        public abstract bool MoveToAttribute(string name, string ns);
        public abstract bool MoveToFirstAttribute();
        public abstract bool MoveToNextAttribute();
        public abstract bool MoveToElement();
        public abstract bool ReadAttributeValue();
        public abstract bool Read();
        public abstract void Close();
        public abstract string LookupNamespace(string prefix);
        public abstract void ResolveEntity();

        public virtual void Skip() { while (this.Read()) { /* drain to next sibling */ } }
        public virtual string ReadString() => this.NodeType == XmlNodeType.Element || this.NodeType == XmlNodeType.Text ? this.Value : "";
        public virtual bool IsStartElement() => this.MoveToContent() == XmlNodeType.Element;
        public virtual bool IsStartElement(string name) => this.IsStartElement() && this.Name == name;
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
        public virtual void ReadStartElement() {
            if (this.MoveToContent() != XmlNodeType.Element)
                throw new XmlException("Expected element");
            this.Read();
        }
        public virtual void ReadStartElement(string name) {
            if (!this.IsStartElement(name))
                throw new XmlException("Expected start element <" + name + ">");
            this.Read();
        }
        public virtual void ReadEndElement() {
            if (this.MoveToContent() != XmlNodeType.EndElement)
                throw new XmlException("Expected end element");
            this.Read();
        }
        public virtual string ReadElementString() {
            this.ReadStartElement();
            var v = this.NodeType == XmlNodeType.Text ? this.Value : "";
            if (this.NodeType == XmlNodeType.Text) this.Read();
            this.ReadEndElement();
            return v;
        }

        public virtual void Dispose() => this.Close();

        // ---- Static factories — return facade-typed readers ----

        public static XmlReader Create(Stream input) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return new WrappingXmlReader(TinyXml.XmlReader.Create(input));
        }

        public static XmlReader Create(Stream input, XmlReaderSettings settings) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var inner = settings == null
                ? TinyXml.XmlReader.Create(input)
                : TinyXml.XmlReader.Create(input, settings.Inner);
            return new WrappingXmlReader(inner);
        }

        public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri) {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var inner = TinyXml.XmlReader.Create(input, settings == null ? null : settings.Inner, baseUri);
            return new WrappingXmlReader(inner);
        }

        public static bool IsName(string str) => TinyXml.XmlReader.IsName(str);
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
        public override bool Read() => this.inner.Read();
        public override void Close() => this.inner.Close();
        public override string LookupNamespace(string prefix) => this.inner.LookupNamespace(prefix);
        public override void ResolveEntity() => this.inner.ResolveEntity();
    }

    // ---- XmlWriter (abstract, .NET-shape; backed by TinyXml.XmlWriter which is concrete) ----

    /// <summary>Forward-only XML writer. Same surface as .NET's <c>System.Xml.XmlWriter</c>.</summary>
    public abstract class XmlWriter : IDisposable {
        public abstract void WriteStartDocument();
        public abstract void WriteStartDocument(bool standalone);
        public abstract void WriteEndDocument();
        public abstract void WriteStartElement(string localName);
        public abstract void WriteStartElement(string localName, string ns);
        public abstract void WriteStartElement(string prefix, string localName, string ns);
        public abstract void WriteEndElement();
        public abstract void WriteFullEndElement();
        public abstract void WriteAttributeString(string localName, string value);
        public abstract void WriteAttributeString(string localName, string ns, string value);
        public abstract void WriteAttributeString(string prefix, string localName, string ns, string value);
        public abstract void WriteString(string text);
        public abstract void WriteRaw(string data);
        public abstract void WriteCData(string text);
        public abstract void WriteComment(string text);
        public abstract void WriteProcessingInstruction(string name, string text);
        public abstract void Flush();
        public abstract void Close();
        public abstract string LookupPrefix(string ns);

        public void WriteElementString(string localName, string value) {
            this.WriteStartElement(localName);
            this.WriteString(value);
            this.WriteEndElement();
        }

        public virtual void Dispose() => this.Close();

        public static XmlWriter Create(Stream output) {
            if (output == null) throw new ArgumentNullException(nameof(output));
            return new WrappingXmlWriter(TinyXml.XmlWriter.Create(output));
        }
    }

    internal sealed class WrappingXmlWriter : XmlWriter {
        private readonly TinyXml.XmlWriter inner;
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
        public override void Close() => this.inner.Close();
        public override string LookupPrefix(string ns) => this.inner.LookupPrefix(ns);
    }
}
