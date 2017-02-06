namespace System {
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Serializable()]
    internal sealed class RuntimeType : Type {

        public override MemberTypes MemberType => (this.DeclaringType != null) ? MemberTypes.NestedType : MemberTypes.TypeInfo;

        public extern override Assembly Assembly {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern override string Name {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern override string FullName {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public override string AssemblyQualifiedName => this.FullName + ", " + this.Assembly.FullName;

        public extern override Type BaseType {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override MethodInfo[] GetMethods(BindingFlags bindingAttr);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override FieldInfo GetField(string name, BindingFlags bindingAttr);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override FieldInfo[] GetFields(BindingFlags bindingAttr);

        // GetInterfaces
        // This method will return all of the interfaces implemented by a
        //  class
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern override Type[] GetInterfaces();
        ////////////////////////////////////////////////////////////////////////////////////
        //////
        ////// Attributes
        //////
        //////   The attributes are all treated as read-only properties on a class.  Most of
        //////  these boolean properties have flag values defined in this class and act like
        //////  a bit mask of attributes.  There are also a set of boolean properties that
        //////  relate to the classes relationship to other classes and to the state of the
        //////  class inside the runtime.
        //////
        ////////////////////////////////////////////////////////////////////////////////////
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override Type GetElementType();

    }
}


