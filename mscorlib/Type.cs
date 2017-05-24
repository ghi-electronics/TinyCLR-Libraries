namespace System {
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Serializable()]
    public abstract class Type : MemberInfo, IReflect {

        public extern override Type DeclaringType {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public static Type GetType(string typeName) {
            var fVersion = false;
            var ver = new int[4];
            var assemblyString = string.Empty;
            var assemblyName = "";

            var name = ParseTypeName(typeName, ref assemblyString);

            if (assemblyString.Length > 0) {
                assemblyName = Assembly.ParseAssemblyName(assemblyString, ref fVersion, ref ver);
            }

            return GetTypeInternal(name, assemblyName, fVersion, ver);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern Type GetTypeInternal(string typeName, string assemblyName, bool fVersion, int[] ver);

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args);

        public abstract Assembly Assembly {
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static Type GetTypeFromHandle(RuntimeTypeHandle handle);

        public abstract string FullName {
            get;
        }

        public abstract string AssemblyQualifiedName {
            get;
        }

        public abstract Type BaseType {
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern ConstructorInfo GetConstructor(Type[] types);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern MethodInfo GetMethod(string name, Type[] types);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern MethodInfo GetMethod(string name, BindingFlags bindingAttr);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern MethodInfo GetMethod(string name);

        // GetMethods
        // This routine will return all the methods implemented by the class
        public MethodInfo[] GetMethods() => GetMethods(Type.DefaultLookup);

        abstract public MethodInfo[] GetMethods(BindingFlags bindingAttr);

        abstract public FieldInfo GetField(string name, BindingFlags bindingAttr);

        public FieldInfo GetField(string name) => GetField(name, Type.DefaultLookup);

        public FieldInfo[] GetFields() => GetFields(Type.DefaultLookup);

        abstract public FieldInfo[] GetFields(BindingFlags bindingAttr);

        // GetInterfaces
        // This method will return all of the interfaces implemented by a
        //  class
        abstract public Type[] GetInterfaces();
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
        public extern bool IsNotPublic {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsPublic {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsClass {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsInterface {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsValueType {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsAbstract {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsEnum {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsSerializable {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsArray {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        abstract public Type GetElementType();

        public virtual bool IsSubclassOf(Type c) {
            var p = this;
            if (p == c)
                return false;
            while (p != null) {
                if (p == c)
                    return true;
                p = p.BaseType;
            }

            return false;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual bool IsInstanceOfType(object o);

        public override string ToString() => this.FullName;

        // private convenience data
        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        //--//

        private static string ParseTypeName(string typeName, ref string assemblyString) {
            // valid names are in the forms:
            // 1) "Microsoft.SPOT.Hardware.Cpu.Pin" or
            // 2) "Microsoft.SPOT.Hardware.Cpu.Pin, Microsoft.SPOT.Hardware" or
            // 3) "Microsoft.SPOT.Hardware.Cpu.Pin, Microsoft.SPOT.Hardware, Version=1.2.3.4" 
            // 4) (FROM THE DEBUGGER) "Microsoft.SPOT.Hardware.Cpu.Pin, Microsoft.SPOT.Hardware, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null[, ...]

            int commaIdx;
            string name;

            // if there is no comma then we have an assembly name in the form with no version
            if ((commaIdx = typeName.IndexOf(',')) != -1) {
                // we grab the type name, but we already know there is more
                name = typeName.Substring(0, commaIdx);

                // after the comma we need ONE (1) space only and then the assembly name
                if (typeName.Length <= commaIdx + 2) {
                    throw new ArgumentException();
                }

                // now we can grab the assemblyName 
                // at this point there could be also the Version appended to it
                assemblyString = typeName.Substring(commaIdx + 2);
            }
            else {
                name = typeName;
                assemblyString = "";
            }

            return name;
        }
    }
}


