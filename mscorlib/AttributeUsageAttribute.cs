namespace System {
    /* By default, attributes are inherited and multiple attributes are not allowed */
    [AttributeUsage(AttributeTargets.Class, Inherited = true), Serializable()]
    public sealed class AttributeUsageAttribute : Attribute {
        internal AttributeTargets m_attributeTarget = AttributeTargets.All; // Defaults to all
        internal bool m_allowMultiple = false; // Defaults to false
        internal bool m_inherited = true; // Defaults to true

        internal static AttributeUsageAttribute Default = new AttributeUsageAttribute(AttributeTargets.All);

        //Constructors
        public AttributeUsageAttribute(AttributeTargets validOn) => this.m_attributeTarget = validOn;

        //Properties
        public AttributeTargets ValidOn => this.m_attributeTarget;
        public bool AllowMultiple {
            get => this.m_allowMultiple; set => this.m_allowMultiple = value;
        }

        public bool Inherited {
            get => this.m_inherited; set => this.m_inherited = value;
        }
    }
}


