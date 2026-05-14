namespace System.Runtime.CompilerServices {
    // C# 13. Tiebreaker for overload resolution: when two candidates are
    // otherwise equally applicable, the one with higher Priority wins.
    // Useful for ship-then-deprecate API evolution. Pure marker.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
                    AllowMultiple = false, Inherited = false)]
    public sealed class OverloadResolutionPriorityAttribute : Attribute {
        public OverloadResolutionPriorityAttribute(int priority) {
            this.Priority = priority;
        }
        public int Priority { get; }
    }
}
