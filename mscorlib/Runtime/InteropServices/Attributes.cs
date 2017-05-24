namespace System.Runtime.InteropServices {

    using System;
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
    public sealed class ComVisibleAttribute : Attribute {
        internal bool _val;
        public ComVisibleAttribute(bool visibility) => this._val = visibility;

        public bool Value => this._val;
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Delegate, Inherited = false)]
    public sealed class GuidAttribute : Attribute {
        internal string _val;
        public GuidAttribute(string guid) => this._val = guid;

        public string Value => this._val;
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class OutAttribute : Attribute {
        public OutAttribute() {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    [ComVisible(true)]
    public sealed class InAttribute : Attribute {
        public InAttribute() {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class StructLayoutAttribute : Attribute {
        internal LayoutKind _val;
        public StructLayoutAttribute(LayoutKind layoutKind) => this._val = layoutKind;

        public StructLayoutAttribute(short layoutKind) => this._val = (LayoutKind)layoutKind;

        public LayoutKind Value => this._val; public int Pack;
        public int Size;
        public CharSet CharSet;
    }
}


