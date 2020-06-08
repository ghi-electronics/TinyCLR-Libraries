////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.Data.Xml;

#if SCHEMA_VALIDATION
using GHIElectronics.TinyCLR.Data.Xml.Schema;
#endif //SCHEMA_VALIDATION

namespace GHIElectronics.TinyCLR.Data.Xml
{

    // XmlReaderSettings class specifies features of an XmlReader.
    public class XmlReaderSettings
    {
        //
        // Fields
        //
        // Nametable
        XmlNameTable nameTable;

        // Text settings
        int lineNumberOffset;
        int linePositionOffset;

        // Conformance settings
        ConformanceLevel conformanceLevel;
        bool checkCharacters;

        // Validation settings
        ValidationType validationType;

        // Filtering settings
        bool ignoreWhitespace;
        bool ignorePIs;
        bool ignoreComments;
        // other settings
        bool closeInput;

        // read-only flag
        bool isReadOnly;

        //
        // Constructor
        //
        public XmlReaderSettings() => this.Reset();

        //
        // Properties
        //
        // Nametable
        public XmlNameTable NameTable {
            get => this.nameTable;

            set {
                this.CheckReadOnly("NameTable");
                this.nameTable = value;
            }
        }

        // Text settings
        public int LineNumberOffset {
            get => this.lineNumberOffset;

            set {
                this.CheckReadOnly("LineNumberOffset");
                if (this.lineNumberOffset < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.lineNumberOffset = value;
            }
        }

        public int LinePositionOffset {
            get => this.linePositionOffset;

            set {
                this.CheckReadOnly("LinePositionOffset");
                if (this.linePositionOffset < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.linePositionOffset = value;
            }
        }

        // Conformance settings
        public ConformanceLevel ConformanceLevel {
            get => this.conformanceLevel;

            set {
                this.CheckReadOnly("ConformanceLevel");

                if ((uint)value > (uint)ConformanceLevel.Document) {
                    throw new XmlException(Res.Xml_ConformanceLevel, "");
                }

                this.conformanceLevel = value;
            }
        }

        public bool CheckCharacters {
            get => this.checkCharacters;

            set {
                this.CheckReadOnly("CheckCharacters");
                this.checkCharacters = value;
            }
        }

        // Validation settings
        [Obsolete("Use ValidationType property set to ValidationType.Schema")]
        public bool XsdValidate {
            get => this.validationType == ValidationType.Schema;

            set {
                this.CheckReadOnly("XsdValidate");
                if (value) {
                    this.validationType = ValidationType.Schema;
                }
                else {
                    this.validationType = ValidationType.None;
                }
            }
        }

        public ValidationType ValidationType {
            get => this.validationType;

            set {
                this.CheckReadOnly("ValidationType");
                this.validationType = value;
            }
        }

        // Filtering settings
        public bool IgnoreWhitespace {
            get => this.ignoreWhitespace;

            set {
                this.CheckReadOnly("IgnoreWhitespace");
                this.ignoreWhitespace = value;
            }
        }

        public bool IgnoreProcessingInstructions {
            get => this.ignorePIs;

            set {
                this.CheckReadOnly("IgnoreProcessingInstructions");
                this.ignorePIs = value;
            }
        }

        public bool IgnoreComments {
            get => this.ignoreComments;

            set {
                this.CheckReadOnly("IgnoreComments");
                this.ignoreComments = value;
            }
        }

        public bool CloseInput {
            get => this.closeInput;

            set {
                this.CheckReadOnly("CloseInput");
                this.closeInput = value;
            }
        }

        //
        // Public methods
        //
        public void Reset()
        {
            this.nameTable = null;
            this.lineNumberOffset = 0;
            this.linePositionOffset = 0;
            this.checkCharacters = true;
            this.conformanceLevel = ConformanceLevel.Document;
            this.ignoreWhitespace = false;
            this.ignorePIs = false;
            this.ignoreComments = false;
            this.closeInput = false;
            this.isReadOnly = false;
        }

        public XmlReaderSettings Clone()
        {
            var clonedSettings = this.MemberwiseClone() as XmlReaderSettings;
            clonedSettings.isReadOnly = false;
            return clonedSettings;
        }

        //
        // Internal and private methods
        //
        internal bool ReadOnly {
            get => this.isReadOnly;

            set => this.isReadOnly = value;
        }

        private void CheckReadOnly(string propertyName)
        {
            if (this.isReadOnly)
            {
                throw new XmlException(Res.Xml_ReadOnlyProperty, "XmlReaderSettings." + propertyName);
            }
        }

        internal bool CanResolveExternals => false;

    }
}


