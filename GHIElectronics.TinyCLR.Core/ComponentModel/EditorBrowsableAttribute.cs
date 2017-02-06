namespace System.ComponentModel {
    using System;

    public enum EditorBrowsableState {
        Always,
        Never,
        Advanced
    }

    /** Custom attribute to indicate that a specified object
     * should be hidden from the editor. (i.e Intellisence filtering)
     */
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public sealed class EditorBrowsableAttribute : Attribute
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {

        private EditorBrowsableState browsableState;

        public EditorBrowsableAttribute() : this(EditorBrowsableState.Always) { }

        public EditorBrowsableAttribute(EditorBrowsableState state) => this.browsableState = state;

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            if (obj is EditorBrowsableAttribute attribute1) {
                return (attribute1.browsableState == this.browsableState);
            }

            return false;
        }

        public EditorBrowsableState State => this.browsableState;

    }
}


