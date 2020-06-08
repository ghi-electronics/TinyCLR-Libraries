////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
    public interface IXmlLineInfo
    {
        bool HasLineInfo();
        int LineNumber { get; }
        int LinePosition { get; }
    }

    internal class PositionInfo : IXmlLineInfo
    {
        public virtual bool HasLineInfo() => false;
        public virtual int LineNumber => 0;
        public virtual int LinePosition => 0;

        public static PositionInfo GetPositionInfo(object o)
        {
            if (o is IXmlLineInfo li) {
                return new ReaderPositionInfo(li);
            }
            else {
                return new PositionInfo();
            }
        }
    }

    internal class ReaderPositionInfo : PositionInfo
    {
        private IXmlLineInfo lineInfo;

        public ReaderPositionInfo(IXmlLineInfo lineInfo) => this.lineInfo = lineInfo;

        public override bool HasLineInfo() => this.lineInfo.HasLineInfo();

        public override int LineNumber => this.lineInfo.LineNumber;

        public override int LinePosition => this.lineInfo.LinePosition;

    }
}// namespace


