////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System
{

    public sealed class AppDomain : MarshalByRefObject
    {
        [System.Reflection.FieldNoReflection]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0169 // The field is never used
        private object m_appDomain;
#pragma warning disable CS0169 // The field is never used
        private string m_friendlyName;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private AppDomain()
        {
            throw new Exception();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static AppDomain CreateDomain(String friendlyName);

        public Object CreateInstanceAndUnwrap(String assemblyName, String typeName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            Type type = assembly.GetType(typeName);

            ConstructorInfo ci = type.GetConstructor(new Type[0]);
            object obj = ci.Invoke(null);

            return obj;
        }

        public static AppDomain CurrentDomain
        {
            get { return Thread.GetDomain(); }
        }

        public String FriendlyName
        {
            get
            {
                return m_friendlyName;
            }
        }

        public Assembly Load(String assemblyString)
        {
            bool fVersion = false;
            int[] ver = new int[4];

            string name = Assembly.ParseAssemblyName(assemblyString, ref fVersion, ref ver);

            return LoadInternal(name, fVersion, ver[0], ver[1], ver[2], ver[3]);

        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Assembly[] GetAssemblies();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern Assembly LoadInternal(String assemblyString, bool fVersion, int maj, int min, int build, int rev);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Unload(AppDomain domain);
    }
}


