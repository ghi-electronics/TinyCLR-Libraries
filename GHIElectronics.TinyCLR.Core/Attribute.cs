namespace System {
    [Serializable, AttributeUsageAttribute(AttributeTargets.All, Inherited = true, AllowMultiple = false)] // Base class for all attributes
    public abstract class Attribute {
        protected Attribute() { }
    }
}


