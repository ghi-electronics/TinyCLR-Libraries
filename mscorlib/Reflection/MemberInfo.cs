namespace System.Reflection {

    using System;

    [Serializable()]
    public abstract class MemberInfo {
        public abstract MemberTypes MemberType {
            get;
        }

        public abstract string Name {
            get;
        }

        public abstract Type DeclaringType {
            get;
        }
    }
}


