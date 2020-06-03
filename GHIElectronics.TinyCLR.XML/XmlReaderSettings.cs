////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  public class XmlReaderSettings
  {
    private XmlNameTable nameTable;
    private int lineNumberOffset;
    private int linePositionOffset;
    private ConformanceLevel conformanceLevel;
    private bool checkCharacters;
    private ValidationType validationType;
    private bool ignoreWhitespace;
    private bool ignorePIs;
    private bool ignoreComments;
    private bool closeInput;
    private bool isReadOnly;

    public XmlReaderSettings()
    {
      this.Reset();
    }

    public XmlNameTable NameTable
    {
      get
      {
        return this.nameTable;
      }
      set
      {
        this.CheckReadOnly(nameof (NameTable));
        this.nameTable = value;
      }
    }

    public int LineNumberOffset
    {
      get
      {
        return this.lineNumberOffset;
      }
      set
      {
        this.CheckReadOnly(nameof (LineNumberOffset));
        if (this.lineNumberOffset < 0)
          throw new ArgumentOutOfRangeException(nameof (value));
        this.lineNumberOffset = value;
      }
    }

    public int LinePositionOffset
    {
      get
      {
        return this.linePositionOffset;
      }
      set
      {
        this.CheckReadOnly(nameof (LinePositionOffset));
        if (this.linePositionOffset < 0)
          throw new ArgumentOutOfRangeException(nameof (value));
        this.linePositionOffset = value;
      }
    }

    public ConformanceLevel ConformanceLevel
    {
      get
      {
        return this.conformanceLevel;
      }
      set
      {
        this.CheckReadOnly(nameof (ConformanceLevel));
        if ((uint) value > 2U)
          throw new XmlException(44, "");
        this.conformanceLevel = value;
      }
    }

    public bool CheckCharacters
    {
      get
      {
        return this.checkCharacters;
      }
      set
      {
        this.CheckReadOnly(nameof (CheckCharacters));
        this.checkCharacters = value;
      }
    }

    [Obsolete("Use ValidationType property set to ValidationType.Schema")]
    public bool XsdValidate
    {
      get
      {
        return this.validationType == ValidationType.Schema;
      }
      set
      {
        this.CheckReadOnly(nameof (XsdValidate));
        if (value)
          this.validationType = ValidationType.Schema;
        else
          this.validationType = ValidationType.None;
      }
    }

    public ValidationType ValidationType
    {
      get
      {
        return this.validationType;
      }
      set
      {
        this.CheckReadOnly(nameof (ValidationType));
        this.validationType = value;
      }
    }

    public bool IgnoreWhitespace
    {
      get
      {
        return this.ignoreWhitespace;
      }
      set
      {
        this.CheckReadOnly(nameof (IgnoreWhitespace));
        this.ignoreWhitespace = value;
      }
    }

    public bool IgnoreProcessingInstructions
    {
      get
      {
        return this.ignorePIs;
      }
      set
      {
        this.CheckReadOnly(nameof (IgnoreProcessingInstructions));
        this.ignorePIs = value;
      }
    }

    public bool IgnoreComments
    {
      get
      {
        return this.ignoreComments;
      }
      set
      {
        this.CheckReadOnly(nameof (IgnoreComments));
        this.ignoreComments = value;
      }
    }

    public bool CloseInput
    {
      get
      {
        return this.closeInput;
      }
      set
      {
        this.CheckReadOnly(nameof (CloseInput));
        this.closeInput = value;
      }
    }

    public void Reset()
    {
      this.nameTable = (XmlNameTable) null;
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
      XmlReaderSettings xmlReaderSettings = this.MemberwiseClone() as XmlReaderSettings;
      xmlReaderSettings.isReadOnly = false;
      return xmlReaderSettings;
    }

    internal bool ReadOnly
    {
      get
      {
        return this.isReadOnly;
      }
      set
      {
        this.isReadOnly = value;
      }
    }

    private void CheckReadOnly(string propertyName)
    {
      if (this.isReadOnly)
        throw new XmlException(45, "XmlReaderSettings." + propertyName);
    }

    internal bool CanResolveExternals
    {
      get
      {
        return false;
      }
    }
  }
}
