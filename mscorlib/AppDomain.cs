using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System {

    public sealed class AppDomain : MarshalByRefObject {
        [System.Reflection.FieldNoReflection]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0169 // The field is never used
        private object m_appDomain;
#pragma warning restore CS0169 // The field is never used
        private string m_friendlyName;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private AppDomain() => throw new Exception();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static AppDomain CreateDomain(string friendlyName);

        public object CreateInstanceAndUnwrap(string assemblyName, string typeName) {
            var assembly = Assembly.Load(assemblyName);
            var type = assembly.GetType(typeName);

            var ci = type.GetConstructor(new Type[0]);
            var obj = ci.Invoke(null);

            return obj;
        }

        public static AppDomain CurrentDomain => Thread.GetDomain();
        public string FriendlyName => this.m_friendlyName;

        public Assembly Load(string assemblyString) {
            var fVersion = false;
            var ver = new int[4];

            var name = Assembly.ParseAssemblyName(assemblyString, ref fVersion, ref ver);

            return LoadInternal(name, fVersion, ver[0], ver[1], ver[2], ver[3]);

        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Assembly[] GetAssemblies();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern Assembly LoadInternal(string assemblyString, bool fVersion, int maj, int min, int build, int rev);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Unload(AppDomain domain);
    }
}


