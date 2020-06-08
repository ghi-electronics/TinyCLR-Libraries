////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Diagnostics;
#if SCHEMA_VALIDATION
using System.Xml.Schema;
#endif //SCHEMA_VALIDATION
using System.Collections;
#if XML_SECURITY
using System.Security.Policy;
#endif

namespace System.Xml
{

    internal class XmlWrappingReader : XmlReader, IXmlLineInfo 
    {
        //
        // Fields
        //
        protected XmlReader reader;
        protected IXmlLineInfo readerAsIXmlLineInfo;

        //
        // Constructor
        //
        internal XmlWrappingReader(XmlReader baseReader)
        {
            Debug.Assert(baseReader != null);
            this.Reader = baseReader;
        }

        //
        // XmlReader implementation
        //
        public override XmlReaderSettings Settings => this.reader.Settings;
        public override XmlNodeType NodeType => this.reader.NodeType;
        public override string Name => this.reader.Name;
        public override string LocalName => this.reader.LocalName;
        public override string NamespaceURI => this.reader.NamespaceURI;
        public override string Prefix => this.reader.Prefix;
        public override bool HasValue => this.reader.HasValue;
        public override string Value => this.reader.Value;
        public override int Depth => this.reader.Depth;
        public override string BaseURI => this.reader.BaseURI;
        public override bool IsEmptyElement => this.reader.IsEmptyElement;
        public override bool IsDefault => this.reader.IsDefault;
        public override char QuoteChar => this.reader.QuoteChar;
        public override XmlSpace XmlSpace => this.reader.XmlSpace;
        public override string XmlLang => this.reader.XmlLang;
        public override System.Type ValueType => this.reader.ValueType;
        public override int AttributeCount => this.reader.AttributeCount;
        public override string this[int i] => this[i];
        public override string this[string name] => this[name];
        public override string this[string name, string namespaceURI] => this[name, namespaceURI];
        public override bool CanResolveEntity => this.reader.CanResolveEntity;
        public override bool EOF => this.reader.EOF;
        public override ReadState ReadState => this.reader.ReadState;
        public override bool HasAttributes => this.reader.HasAttributes;
        public override XmlNameTable NameTable => this.reader.NameTable;

        public override string GetAttribute(string name) => this.reader.GetAttribute(name);

        public override string GetAttribute(string name, string namespaceURI) => this.reader.GetAttribute(name, namespaceURI);

        public override string GetAttribute(int i) => this.reader.GetAttribute(i);

        public override bool MoveToAttribute(string name) => this.reader.MoveToAttribute(name);

        public override bool MoveToAttribute(string name, string ns) => this.reader.MoveToAttribute(name, ns);

        public override void MoveToAttribute(int i) => this.reader.MoveToAttribute(i);

        public override bool MoveToFirstAttribute() => this.reader.MoveToFirstAttribute();

        public override bool MoveToNextAttribute() => this.reader.MoveToNextAttribute();

        public override bool MoveToElement() => this.reader.MoveToElement();

        public override bool Read() => this.reader.Read();

        public override void Close() => this.reader.Close();

        public override void Skip() => this.reader.Skip();

        public override string LookupNamespace(string prefix) => this.reader.LookupNamespace(prefix);

        public override void ResolveEntity() => this.reader.ResolveEntity();

        public override bool ReadAttributeValue() => this.reader.ReadAttributeValue();

        //
        // IDisposable interface
        //
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.reader != null)
                {
                    this.reader.Dispose();
                    this.reader = null;
                }
            }
        }

        //
        // IXmlLineInfo members
        //
        public virtual bool HasLineInfo() => (this.readerAsIXmlLineInfo == null) ? false : this.readerAsIXmlLineInfo.HasLineInfo();

        public virtual int LineNumber => (this.readerAsIXmlLineInfo == null) ? 0 : this.readerAsIXmlLineInfo.LineNumber;

        public virtual int LinePosition => (this.readerAsIXmlLineInfo == null) ? 0 : this.readerAsIXmlLineInfo.LinePosition;

        //
        //  Protected methods
        //
        protected XmlReader Reader {
            get => this.reader;

            set {
                this.reader = value;
                this.readerAsIXmlLineInfo = value as IXmlLineInfo;
            }
        }
    }
}


