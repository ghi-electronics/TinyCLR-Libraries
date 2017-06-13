namespace System.Runtime.CompilerServices {
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AccessedThroughPropertyAttribute : Attribute {
        private readonly string propertyName;

        public AccessedThroughPropertyAttribute(string propertyName) => this.propertyName = propertyName;

        public string PropertyName => this.propertyName;
    }
}


