////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System;

namespace GHIElectronics.TinyCLR.Data.Xml
{

    // Represents a reader that provides fast, non-cached forward only stream access to XML data.
    public abstract class XmlReader : IDisposable
    {

        static private uint IsTextualNodeBitmap = 0x6018; // 00 0110 0000 0001 1000
        // 0 None,
        // 0 Element,
        // 0 Attribute,
        // 1 Text,
        // 1 CDATA,
        // 0 EntityReference,
        // 0 Entity,
        // 0 ProcessingInstruction,
        // 0 Comment,
        // 0 Document,
        // 0 DocumentType,
        // 0 DocumentFragment,
        // 0 Notation,
        // 1 Whitespace,
        // 1 SignificantWhitespace,
        // 0 EndElement,
        // 0 EndEntity,
        // 0 XmlDeclaration

        static private uint CanReadContentAsBitmap = 0x1E1BC; // 01 1110 0001 1011 1100
        // 0 None,
        // 0 Element,
        // 1 Attribute,
        // 1 Text,
        // 1 CDATA,
        // 1 EntityReference,
        // 0 Entity,
        // 1 ProcessingInstruction,
        // 1 Comment,
        // 0 Document,
        // 0 DocumentType,
        // 0 DocumentFragment,
        // 0 Notation,
        // 1 Whitespace,
        // 1 SignificantWhitespace,
        // 1 EndElement,
        // 1 EndEntity,
        // 0 XmlDeclaration

        static private uint HasValueBitmap = 0x2659C; // 10 0110 0101 1001 1100
        // 0 None,
        // 0 Element,
        // 1 Attribute,
        // 1 Text,
        // 1 CDATA,
        // 0 EntityReference,
        // 0 Entity,
        // 1 ProcessingInstruction,
        // 1 Comment,
        // 0 Document,
        // 1 DocumentType,
        // 0 DocumentFragment,
        // 0 Notation,
        // 1 Whitespace,
        // 1 SignificantWhitespace,
        // 0 EndElement,
        // 0 EndEntity,
        // 1 XmlDeclaration

        //
        // Constants
        //
        internal const int DefaultBufferSize = 4096;
        internal const int BiggerBufferSize = 8192;
        internal const int MaxStreamLengthForDefaultBufferSize = 64 * 1024; // 64kB

        // Settings
        public virtual XmlReaderSettings Settings => null;

        // Node Properties
        // Get the type of the current node.
        public abstract XmlNodeType NodeType { get; }

        // Gets the name of the current node, including the namespace prefix.
        public virtual string Name
        {
            get
            {
                if (this.Prefix.Length == 0)
                {
                    return this.LocalName;
                }
                else
                {
                    return this.NameTable.Add(string.Concat(this.Prefix, ":", this.LocalName));
                }
            }
        }

        //       Gets the name of the current node without the namespace prefix.
        public abstract string LocalName { get; }

        //       Gets the namespace URN (as defined in the W3C Namespace Specification) of the current namespace scope.
        public abstract string NamespaceURI { get; }

        //       Gets the namespace prefix associated with the current node.
        public abstract string Prefix { get; }

        //       Gets a value indicating whether
        public abstract bool HasValue { get; }

        //       Gets the text value of the current node.
        public abstract string Value { get; }

        // Gets the depth of the current node in the XML element stack.
        public abstract int Depth { get; }

        //       Gets the base URI of the current node.
        public abstract string BaseURI { get; }

        // Gets a value indicating whether the current node is an empty element (for example, <MyElement/>).
        public abstract bool IsEmptyElement { get; }

        // Gets a value indicating whether the current node is an attribute that was generated from the default value defined
        //       in the DTD or schema.
        public virtual bool IsDefault => false;

        // Gets the quotation mark character used to enclose the value of an attribute node.
        public virtual char QuoteChar => '"';

        // Gets the current xml:space scope.
        public virtual XmlSpace XmlSpace => XmlSpace.None;

        // Gets the current xml:lang scope.
        public virtual string XmlLang => "";

#if SCHEMA_VALIDATION
        // returns the schema info interface of the reader
        public virtual IXmlSchemaInfo SchemaInfo {
            get {
                return this as IXmlSchemaInfo;
            }
        }

#endif //SCHEMA_VALIDATION

        // returns the type of the current node
        public virtual System.Type ValueType => typeof(string);

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references,
        // and returns the content as the most appropriate type (by default as string). Stops at start tags and end tags.
        public virtual object ReadContentAsObject()
        {
            if (!CanReadContentAs(this.NodeType))
            {
                throw this.CreateReadContentAsException("ReadContentAsObject");
            }

            return this.InternalReadContentAsString();
        }

        // Attribute Accessors
        // The number of attributes on the current node.
        public abstract int AttributeCount { get; }

        // Gets the value of the attribute with the specified Name
        public abstract string GetAttribute(string name);

        // Gets the value of the attribute with the LocalName and NamespaceURI
        public abstract string GetAttribute(string name, string namespaceURI);

        // Gets the value of the attribute with the specified index.
        public abstract string GetAttribute(int i);

        // Gets the value of the attribute with the specified index.
        public virtual string this[int i] => this.GetAttribute(i);

        // Gets the value of the attribute with the specified Name.
        public virtual string this[string name] => this.GetAttribute(name);

        // Gets the value of the attribute with the LocalName and NamespaceURI
        public virtual string this[string name, string namespaceURI] => this.GetAttribute(name, namespaceURI);

        // Moves to the attribute with the specified Name.
        public abstract bool MoveToAttribute(string name);

        // Moves to the attribute with the specified LocalName and NamespaceURI.
        public abstract bool MoveToAttribute(string name, string ns);

        // Moves to the attribute with the specified index.
        public virtual void MoveToAttribute(int i)
        {
            if (i < 0 || i >= this.AttributeCount)
            {
                throw new ArgumentOutOfRangeException("i");
            }

            this.MoveToElement();
            this.MoveToFirstAttribute();
            var j = 0;
            while (j < i)
            {
                this.MoveToNextAttribute();
                j++;
            }
        }

        // Moves to the first attribute of the current node.
        public abstract bool MoveToFirstAttribute();

        //       Moves to the next attribute.
        public abstract bool MoveToNextAttribute();

        //       Moves to the element that contains the current attribute node.
        public abstract bool MoveToElement();

        // Parses the attribute value into one or more Text and/or EntityReference node types.
        public abstract bool ReadAttributeValue();

        // Moving through the Stream
        // Reads the next node from the stream.
        public abstract bool Read();

        // Returns true when the XmlReader is positioned at the end of the stream.
        public abstract bool EOF { get; }

        // Closes the stream (if CloseInput==true), changes the ReadState to Closed, and sets all the properties back to zero/empty string.
        public abstract void Close();

        // Returns the read state of the XmlReader.
        public abstract ReadState ReadState { get; }

        // Skips to the end tag of the current element.
        public virtual void Skip() => this.SkipSubtree();

        // Gets the XmlNameTable associated with the XmlReader.
        public abstract XmlNameTable NameTable { get; }

        //       Resolves a namespace prefix in the current element's scope.
        public abstract string LookupNamespace(string prefix);

        // Returns true if the XmlReader can expand general entities.
        public virtual bool CanResolveEntity => false;

        // Resolves the entity reference for nodes of NodeType EntityReference.
        public abstract void ResolveEntity();

        // Binary content access methods
        // Returns true if the reader supports call to ReadContentAsBase64, ReadElementContentAsBase64, ReadContentAsBinHex and ReadElementContentAsBinHex.
        public virtual bool CanReadBinaryContent => false;

        // Returns decoded bytes of the current base64 text content. Call this methods until it returns 0 to get all the data.
        public virtual int ReadContentAsBase64(byte[] buffer, int index, int count) => throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadContentAsBase64"));

        // Returns decoded bytes of the current base64 element content. Call this methods until it returns 0 to get all the data.
        public virtual int ReadElementContentAsBase64(byte[] buffer, int index, int count) => throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadElementContentAsBase64"));

        // Returns decoded bytes of the current binhex text content. Call this methods until it returns 0 to get all the data.
        public virtual int ReadContentAsBinHex(byte[] buffer, int index, int count) => throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadContentAsBinHex"));

        // Returns decoded bytes of the current binhex element content. Call this methods until it returns 0 to get all the data.
        public virtual int ReadElementContentAsBinHex(byte[] buffer, int index, int count) => throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadElementContentAsBinHex"));

        // Text streaming methods

        // Returns true if the XmlReader supports calls to ReadValueChunk.
        public virtual bool CanReadValueChunk => false;

        // Returns a chunk of the value of the current node. Call this method in a loop to get all the data.
        // Use this method to get a streaming access to the value of the current node.
        public virtual int ReadValueChunk(char[] buffer, int index, int count) => throw new NotSupportedException(Res.GetString(Res.Xml_ReadValueChunkNotSupported));

        // Virtual helper methods
        // Reads the contents of an element as a string. Stops of comments, PIs or entity references.
        public virtual string ReadString()
        {
            if (this.ReadState != ReadState.Interactive)
            {
                return "";
            }

            this.MoveToElement();
            if (this.NodeType == XmlNodeType.Element)
            {
                if (this.IsEmptyElement)
                {
                    return "";
                }
                else if (!this.Read())
                {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }

                if (this.NodeType == XmlNodeType.EndElement)
                {
                    return "";
                }
            }

            var result = "";
            while (IsTextualNode(this.NodeType))
            {
                result += this.Value;
                if (!this.Read())
                {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }
            }

            return result;
        }

        // Checks whether the current node is a content (non-whitespace text, CDATA, Element, EndElement, EntityReference
        // or EndEntity) node. If the node is not a content node, then the method skips ahead to the next content node or
        // end of file. Skips over nodes of type ProcessingInstruction, DocumentType, Comment, Whitespace and SignificantWhitespace.
        public virtual XmlNodeType MoveToContent()
        {
            do
            {
                switch (this.NodeType)
                {
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

        // Checks that the current node is an element and advances the reader to the next node.
        public virtual void ReadStartElement()
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            this.Read();
        }

        // Checks that the current content node is an element with the given Name and advances the reader to the next node.
        public virtual void ReadStartElement(string name)
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (this.Name == name)
            {
                this.Read();
            }
            else
            {
                throw new XmlException(Res.Xml_ElementNotFound, name, this as IXmlLineInfo);
            }
        }

        // Checks that the current content node is an element with the given LocalName and NamespaceURI
        // and advances the reader to the next node.
        public virtual void ReadStartElement(string localname, string ns)
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (this.LocalName == localname && this.NamespaceURI == ns)
            {
                this.Read();
            }
            else
            {
                throw new XmlException(Res.Xml_ElementNotFoundNs, new string[2] { localname, ns }, this as IXmlLineInfo);
            }
        }

        // Reads a text-only element.
        public virtual string ReadElementString()
        {
            var result = "";

            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (!this.IsEmptyElement)
            {
                this.Read();
                result = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
                }

                this.Read();
            }
            else
            {
                this.Read();
            }

            return result;
        }

        // Checks that the Name property of the element found matches the given string before reading a text-only element.
        public virtual string ReadElementString(string name)
        {
            var result = "";

            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (this.Name != name)
            {
                throw new XmlException(Res.Xml_ElementNotFound, name, this as IXmlLineInfo);
            }

            if (!this.IsEmptyElement)
            {
                //Read();
                result = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
                }

                this.Read();
            }
            else
            {
                this.Read();
            }

            return result;
        }

        // Checks that the LocalName and NamespaceURI properties of the element found matches the given strings
        // before reading a text-only element.
        public virtual string ReadElementString(string localname, string ns)
        {
            var result = "";
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (this.LocalName != localname || this.NamespaceURI != ns)
            {
                throw new XmlException(Res.Xml_ElementNotFoundNs, new string[2] { localname, ns }, this as IXmlLineInfo);
            }

            if (!this.IsEmptyElement)
            {
                //Read();
                result = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
                }

                this.Read();
            }
            else
            {
                this.Read();
            }

            return result;
        }

        // Checks that the current content node is an end tag and advances the reader to the next node.
        public virtual void ReadEndElement()
        {
            if (this.MoveToContent() != XmlNodeType.EndElement)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            this.Read();
        }

        // Calls MoveToContent and tests if the current content node is a start tag or empty element tag (XmlNodeType.Element).
        public virtual bool IsStartElement() => this.MoveToContent() == XmlNodeType.Element;

        // Calls MoveToContentand tests if the current content node is a start tag or empty element tag (XmlNodeType.Element) and if the
        // Name property of the element found matches the given argument.
        public virtual bool IsStartElement(string name) => (this.MoveToContent() == XmlNodeType.Element) &&
                (this.Name == name);

        // Calls MoveToContent and tests if the current content node is a start tag or empty element tag (XmlNodeType.Element) and if
        // the LocalName and NamespaceURI properties of the element found match the given strings.
        public virtual bool IsStartElement(string localname, string ns) => (this.MoveToContent() == XmlNodeType.Element) &&
                (this.LocalName == localname && this.NamespaceURI == ns);

        // Reads to the following element with the given Name.
        public virtual bool ReadToFollowing(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(name, "name");
            }

            // atomize name
            name = this.NameTable.Add(name);

            // find following element with that name
            while (this.Read())
            {
                if (this.NodeType == XmlNodeType.Element && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            }

            return false;
        }

        // Reads to the following element with the given LocalName and NamespaceURI.
        public virtual bool ReadToFollowing(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, "localName");
            }

            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            // atomize local name and namespace
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);

            // find following element with that name
            while (this.Read())
            {
                if (this.NodeType == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            }

            return false;
        }

        // Reads to the first descendant of the current element with the given Name.
        public virtual bool ReadToDescendant(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException(name, "name");
            }

            // save the element or root depth
            var parentDepth = this.Depth;
            if (this.NodeType != XmlNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (this.ReadState == ReadState.Initial)
                {
                    Debug.Assert(parentDepth == 0);
                    parentDepth--;
                }
                else
                {
                    return false;
                }
            }
            else if (this.IsEmptyElement)
            {
                return false;
            }

            // atomize name
            name = this.NameTable.Add(name);

            // find the descendant
            while (this.Read() && this.Depth > parentDepth)
            {
                if (this.NodeType == XmlNodeType.Element && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            }

            Debug.Assert(this.NodeType == XmlNodeType.EndElement || (parentDepth == -1 && this.ReadState == ReadState.EndOfFile));
            return false;
        }

        // Reads to the first descendant of the current element with the given LocalName and NamespaceURI.
        public virtual bool ReadToDescendant(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, "localName");
            }

            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            // save the element or root depth
            var parentDepth = this.Depth;
            if (this.NodeType != XmlNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (this.ReadState == ReadState.Initial)
                {
                    Debug.Assert(parentDepth == 0);
                    parentDepth--;
                }
                else
                {
                    return false;
                }
            }
            else if (this.IsEmptyElement)
            {
                return false;
            }

            // atomize local name and namespace
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);

            // find the descendant
            while (this.Read() && this.Depth > parentDepth)
            {
                if (this.NodeType == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            }

            Debug.Assert(this.NodeType == XmlNodeType.EndElement || (parentDepth == -1 && this.ReadState == ReadState.EndOfFile));
            return false;
        }

        // Reads to the next sibling of the current element with the given Name.
        public virtual bool ReadToNextSibling(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(name, "name");
            }

            // atomize name
            name = this.NameTable.Add(name);

            // find the next sibling
            XmlNodeType nt;
            do
            {
                this.SkipSubtree();
                nt = this.NodeType;
                if (nt == XmlNodeType.Element && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            } while (nt != XmlNodeType.EndElement && !this.EOF);
            return false;
        }

        // Reads to the next sibling of the current element with the given LocalName and NamespaceURI.
        public virtual bool ReadToNextSibling(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, "localName");
            }

            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            // atomize local name and namespace
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);

            // find the next sibling
            XmlNodeType nt;
            do
            {
                this.SkipSubtree();
                nt = this.NodeType;
                if (nt == XmlNodeType.Element && Ref.Equal(localName, this.LocalName) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            } while (nt != XmlNodeType.EndElement && !this.EOF);
            return false;
        }

        // Returns true if the given argument is a valid Name.
        public static bool IsName(string str) => XmlCharType.Instance.IsName(str);

        // Returns true if the given argument is a valid NmToken.
        public static bool IsNameToken(string str) => XmlCharType.Instance.IsNmToken(str);

        // Returns true when the current node has any attributes.
        public virtual bool HasAttributes => this.AttributeCount > 0;

        //
        // IDisposable interface
        //
        public virtual void Dispose()
        {
            if (this.ReadState != ReadState.Closed)
            {
                this.Close();
            }
        }

        static internal bool IsTextualNode(XmlNodeType nodeType) => 0 != (IsTextualNodeBitmap & (1 << (int)nodeType));

        static internal bool CanReadContentAs(XmlNodeType nodeType) => 0 != (CanReadContentAsBitmap & (1 << (int)nodeType));

        static internal bool HasValueInternal(XmlNodeType nodeType) => 0 != (HasValueBitmap & (1 << (int)nodeType));

        //
        // Private methods
        //
        //SkipSubTree is called whenever validation of the skipped subtree is required on a reader with XsdValidate=true
        private void SkipSubtree()
        {
            if (this.ReadState != ReadState.Interactive)
                return;

            this.MoveToElement();
            if (this.NodeType == XmlNodeType.Element && !this.IsEmptyElement)
            {
                var depth = this.Depth;

                while (this.Read() && depth < this.Depth)
                {
                    // Nothing, just read on
                }

                // consume end tag
                if (this.NodeType == XmlNodeType.EndElement)
                    this.Read();
            }
            else
            {
                this.Read();
            }
        }

        internal void CheckElement(string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw XmlExceptionHelper.CreateInvalidNameArgumentException(localName, "localName");
            }

            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            if (this.NodeType != XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            if (this.LocalName != localName || this.NamespaceURI != namespaceURI)
            {
                throw new XmlException(Res.Xml_ElementNotFoundNs, new string[2] { localName, namespaceURI }, this as IXmlLineInfo);
            }
        }

        internal Exception CreateReadContentAsException(string methodName) => CreateReadContentAsException(methodName, this.NodeType);

        internal Exception CreateReadElementContentAsException(string methodName) => CreateReadElementContentAsException(methodName, this.NodeType);

        static internal Exception CreateReadContentAsException(string methodName, XmlNodeType nodeType) => new InvalidOperationException(Res.GetString(Res.Xml_InvalidReadContentAs, new string[] { methodName, nodeType.ToString() }));

        static internal Exception CreateReadElementContentAsException(string methodName, XmlNodeType nodeType) => new InvalidOperationException(Res.GetString(Res.Xml_InvalidReadElementContentAs, new string[] { methodName, nodeType.ToString() }));

        internal string InternalReadContentAsString()
        {
            var value = "";
            BufferBuilder sb = null;
            do
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Attribute:
                        return this.Value;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.CDATA:
                        // merge text content
                        if (value.Length == 0)
                        {
                            value = this.Value;
                        }
                        else
                        {
                            if (sb == null)
                            {
                                sb = new BufferBuilder();
                                sb.Append(value);
                            }

                            sb.Append(this.Value);
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        // skip comments, pis and end entity nodes
                        break;
                    case XmlNodeType.EntityReference:
                        if (this.CanResolveEntity)
                        {
                            this.ResolveEntity();
                            break;
                        }

                        goto default;
                    case XmlNodeType.EndElement:
                    default:
                        goto ReturnContent;
                }
            } while ((this.AttributeCount != 0) ? this.ReadAttributeValue() : this.Read());

        ReturnContent:
            return (sb == null) ? value : sb.ToString();
        }

        private bool SetupReadElementContentAsXxx(string methodName)
        {
            if (this.NodeType != XmlNodeType.Element)
            {
                throw this.CreateReadElementContentAsException(methodName);
            }

            var isEmptyElement = this.IsEmptyElement;

            // move to content or beyond the empty element
            this.Read();

            if (isEmptyElement)
            {
                return false;
            }

            var nodeType = this.NodeType;
            if (nodeType == XmlNodeType.EndElement)
            {
                this.Read();
                return false;
            }
            else if (nodeType == XmlNodeType.Element)
            {
                throw new XmlException(Res.Xml_MixedReadElementContentAs, "", this as IXmlLineInfo);
            }

            return true;
        }

        private void FinishReadElementContentAsXxx()
        {
            if (this.NodeType != XmlNodeType.EndElement)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString());
            }

            this.Read();
        }

        internal static Encoding GetEncoding(XmlReader reader)
        {
            var tri = GetXmlTextReader(reader);
            return tri?.Encoding;
        }

        internal static ConformanceLevel GetV1ConformanceLevel(XmlReader reader)
        {
            var tri = GetXmlTextReader(reader);
            return tri != null ? tri.V1ComformanceLevel : ConformanceLevel.Document;
        }

        private static XmlTextReader GetXmlTextReader(XmlReader reader)
        {
            if (reader is XmlTextReader tri) {
                return tri;
            }

            return null;
        }

        // Creates an XmlReader according for parsing XML from the given stream.
        public static XmlReader Create(Stream input)
        {
            var settings = new XmlReaderSettings();
            return CreateReaderImpl(input, settings, "", settings.CloseInput);
        }

        // Creates an XmlReader according to the settings for parsing XML from the given stream.
        public static XmlReader Create(Stream input, XmlReaderSettings settings) => Create(input, settings, "");

        // Creates an XmlReader according to the settings and base Uri for parsing XML from the given stream.
        public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }

            return CreateReaderImpl(input, settings, (string)baseUri, settings.CloseInput);
        }

        private static XmlReader CreateReaderImpl(Stream input, XmlReaderSettings settings, string baseUriStr, bool closeInput)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (baseUriStr == null)
            {
                baseUriStr = "";
            }

            XmlReader reader = new XmlTextReader(input, null, 0, settings, baseUriStr, closeInput);

            return reader;
        }

        internal static int CalcBufferSize(Stream input)
        {
            // determine the size of byte buffer
            var bufferSize = DefaultBufferSize;
            if (input.CanSeek)
            {
                var len = input.Length;
                if (len < bufferSize)
                {
                    bufferSize = unchecked((int)len);
                }
                else if (len > MaxStreamLengthForDefaultBufferSize)
                {
                    bufferSize = BiggerBufferSize;
                }
            }

            // return the byte buffer size
            return bufferSize;
        }

    }
}


