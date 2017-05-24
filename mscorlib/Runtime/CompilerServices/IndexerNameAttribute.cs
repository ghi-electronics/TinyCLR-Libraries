namespace System.Runtime.CompilerServices {
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class IndexerNameAttribute : Attribute {
        public IndexerNameAttribute(string indexerName) { }
    }
}


