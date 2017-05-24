namespace System.Reflection {

    using System;
    using System.Runtime.CompilerServices;
    using CultureInfo = System.Globalization.CultureInfo;

    public sealed class AssemblyName {
        private Assembly _assembly;

        //--//

        internal AssemblyName(Assembly assm) => this._assembly = assm;

        public string Name => this._assembly.FullName.Substring(0, this._assembly.FullName.IndexOf(','));

        public string FullName => this._assembly.FullName;

        public Version Version {
            get {
                int major = -1, minor = -1, build = -1, revision = -1;

                this._assembly.GetVersion(ref major, ref minor, ref build, ref revision);

                return new Version(major, minor, build, revision);
            }
        }
    }

    [Serializable()]
    public class Assembly {
        public extern virtual string FullName {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static Assembly GetExecutingAssembly();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern void GetVersion(ref int major, ref int minor, ref int build, ref int revision);

        public AssemblyName GetName() => new AssemblyName(this);

        public static Assembly GetAssembly(Type type) => type.Assembly;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual Type GetType(string name);
        public virtual Type GetType(string name, bool throwOnError) {
            var type = GetType(name);

            if (type == null) {
                throw new ArgumentException();
            }

            return type;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual Type[] GetTypes();
        public Assembly GetSatelliteAssembly(CultureInfo culture) {
            if (culture == null) {
                throw new ArgumentNullException("culture");
            }

            Assembly assm = null;
            var baseName = this.FullName;
            string cultureName;

            while (assm == null && (cultureName = culture.Name) != "") {
                var assmName = baseName + "." + cultureName;

                assm = Assembly.Load(assmName, false);

                culture = culture.Parent;
            }

            if (assm == null) {
                throw new ArgumentException();
                // FIXME -- throw new FileNotFoundException();
            }

            return assm;
        }

        public static Assembly Load(string assemblyString) {
            if (assemblyString == null) {
                throw new ArgumentNullException("assemblyString");
            }

            return Load(assemblyString, true);
        }

        internal static string ParseAssemblyName(string assemblyString, ref bool fVersion, ref int[] ver) {
            // valid names are in the forms:
            // 1) "Microsoft.SPOT.Native" or
            // 2) "Microsoft.SPOT.Native, Version=1.2.3.4" or
            // 3) "Microsoft.SPOT.Native.resources, Version=1.2.3.4" or
            // 4) "Microsoft.SPOT.Native.tinyresources, Version=1.2.3.4"
            // 5) (FROM THE DEBUGGER) "Microsoft.SPOT.Native, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null[, ...]

            int versionIdx, commaIdx;
            string name;

            fVersion = false;

            // if there is no comma then we have an assembly name in the form with no version
            if ((commaIdx = assemblyString.IndexOf(',')) != -1) {
                name = assemblyString.Substring(0, commaIdx);

                const string c_versionTag = "version=";

                // verify that the format with the version is correct and skip the ", Version=" part of the string
                if ((versionIdx = assemblyString.ToLower().IndexOf(c_versionTag)) != 0) {
                    fVersion = true;

                    // the "version=" string must come right after the ' ,'
                    if (versionIdx == commaIdx + 2) {
                        var startIdx = versionIdx + c_versionTag.Length;
                        int endIdx;

                        // trim off the Culture, PublicKeyToken, etc for now
                        if (-1 != (endIdx = assemblyString.IndexOf(',', startIdx))) {
                            assemblyString = assemblyString.Substring(startIdx, endIdx - startIdx);
                        }
                        else {
                            assemblyString = assemblyString.Substring(startIdx);
                        }

                        // at this point we have assemblyString = "1.2.3.4"
                        var version = assemblyString.Split('.');

                        if (version.Length > 0)
                            ver[0] = ushort.Parse(version[0]);
                        if (version.Length > 1)
                            ver[1] = ushort.Parse(version[1]);
                        // build and revision versions may be -1 (which means "don't care")
                        if (version.Length > 2)
                            ver[2] = int.Parse(version[2]);
                        if (version.Length > 3)
                            ver[3] = int.Parse(version[3]);
                    }
                    else {
                        throw new ArgumentException();
                    }
                }
                else {
                    throw new ArgumentException();
                }
            }
            else {
                name = assemblyString;
            }

            return name;
        }

        internal static Assembly Load(string assemblyString, bool fThrowOnError) {
            var fVersion = false;
            var ver = new int[4];

            var name = ParseAssemblyName(assemblyString, ref fVersion, ref ver);

            var assm = LoadInternal(name, fVersion, ver[0], ver[1], ver[2], ver[3]);

            if (assm == null) {
                if (fThrowOnError) {
                    // FIXME -- should be FileNotFoundException, per spec.
                    throw new ArgumentException();
                }
            }

            return assm;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static Assembly LoadInternal(string assemblyString, bool fVersion, int maj, int min, int build, int rev);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern public Assembly Load(byte[] rawAssembly);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern string[] GetManifestResourceNames();
    }
}


