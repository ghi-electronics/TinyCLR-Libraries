#define ENABLE_CROSS_APPDOMAIN
namespace System.Globalization {
    using System;
    using System.Collections;
    using System.Resources;
    using System.Runtime.CompilerServices;
    public class CultureInfo : IFormatProvider /* ICloneable */ {
        internal NumberFormatInfo numInfo = null;
        internal DateTimeFormatInfo dateTimeInfo = null;
        internal string m_name = null;
        internal ResourceManager m_rm;
        [NonSerialized]
        private CultureInfo m_parent;
        const string c_ResourceBase = "System.Globalization.Resources.CultureInfo";
        internal string EnsureStringResource(ref string str, System.Globalization.Resources.CultureInfo.StringResources id) {
            if (str == null) {
                str = (string)this.m_rm.GetObject((short)id);
            }

            return str;
        }

        internal string[] EnsureStringArrayResource(ref string[] strArray, System.Globalization.Resources.CultureInfo.StringResources id) {
            if (strArray == null) {
                var str = (string)this.m_rm.GetObject((short)id);
                strArray = str.Split('|');
            }

            return (string[])strArray.Clone();
        }

        public CultureInfo(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            this.m_rm = new ResourceManager(c_ResourceBase, typeof(CultureInfo).Assembly, name, true);
            this.m_name = this.m_rm.m_cultureName;
        }

        internal CultureInfo(ResourceManager resourceManager) {
            this.m_rm = resourceManager;
            this.m_name = resourceManager.m_cultureName;
        }

        public static CultureInfo InvariantCulture { get; } = new CultureInfo("");
        public static CultureInfo CurrentCulture => CultureInfo.CurrentUICulture;

        public static CultureInfo CurrentUICulture {
            get {
                //only one system-wide culture.  We do not currently support per-thread cultures
                var culture = CurrentUICultureInternal;
                if (culture == null) {
                    culture = new CultureInfo("");
                    CurrentUICultureInternal = culture;
                }

                return culture;
            }
            set => CurrentUICultureInternal = value ?? throw new ArgumentNullException();
        }

        private extern static CultureInfo CurrentUICultureInternal {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        public virtual CultureInfo Parent {
            get {
                if (this.m_parent == null) {
                    if (this.m_name == "") //Invariant culture
                    {
                        this.m_parent = this;
                    }
                    else {
                        var parentName = this.m_name;
                        var iDash = this.m_name.LastIndexOf('-');
                        if (iDash >= 0) {
                            parentName = parentName.Substring(0, iDash);
                        }
                        else {
                            parentName = "";
                        }

                        this.m_parent = new CultureInfo(parentName);
                    }
                }

                return this.m_parent;
            }
        }

        public static CultureInfo[] GetCultures(CultureTypes types) {
            var listCultures = new ArrayList();
            //Look for all assemblies/satellite assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var iAssembly = 0; iAssembly < assemblies.Length; iAssembly++) {
                var assembly = assemblies[iAssembly];
                var mscorlib = "mscorlib";
                var fullName = assembly.FullName;
                // consider adding startswith ?
                if ((mscorlib.Length <= fullName.Length) && (fullName.Substring(0, mscorlib.Length) == mscorlib)) {
                    var resources = assembly.GetManifestResourceNames();
                    for (var iResource = 0; iResource < resources.Length; iResource++) {
                        var resource = resources[iResource];
                        var ciResource = c_ResourceBase;
                        if (ciResource.Length < resource.Length && resource.Substring(0, ciResource.Length) == ciResource) {
                            //System.Globalization.Resources.CultureInfo.<culture>.tinyresources
                            var cultureName = resource.Substring(ciResource.Length, resource.Length - ciResource.Length - System.Resources.ResourceManager.s_fileExtension.Length);
                            // remove the leading "."
                            if (cultureName != "") {
                                cultureName = cultureName.Substring(1, cultureName.Length - 1);
                            }

                            // if GetManifestResourceNames() changes, we need to change this code to ensure the index is the same.
                            listCultures.Add(new CultureInfo(new ResourceManager(c_ResourceBase, cultureName, iResource, typeof(CultureInfo).Assembly, assembly)));
                        }
                    }
                }
            }

            return (CultureInfo[])listCultures.ToArray(typeof(CultureInfo));
        }

        public virtual string Name => this.m_name;

        public override string ToString() => this.m_name;

        public virtual object GetFormat(Type formatType) {
            if (formatType == typeof(NumberFormatInfo)) {
                return this.NumberFormat;
            }
            if (formatType == typeof(DateTimeFormatInfo)) {
                return this.DateTimeFormat;
            }
            return null;
        }

        //        public virtual Object GetFormat(Type formatType) {
        //            if (formatType == typeof(NumberFormatInfo)) {
        //                return (NumberFormat);
        //            }
        //            if (formatType == typeof(DateTimeFormatInfo)) {
        //                return (DateTimeFormat);
        //            }
        //            return (null);
        //        }

        //        internal static void CheckNeutral(CultureInfo culture) {
        //            if (culture.IsNeutralCulture) {
        //                    BCLDebug.Assert(culture.m_name != null, "[CultureInfo.CheckNeutral]Always expect m_name to be set");
        //                    throw new NotSupportedException(
        //                                    Environment.GetResourceString("Argument_CultureInvalidFormat",
        //                                    culture.m_name));
        //            }
        //        }

        //        [System.Runtime.InteropServices.ComVisible(false)]
        //        public CultureTypes CultureTypes
        //        {
        //            get
        //            {
        //                CultureTypes types = 0;

        //                if (m_cultureTableRecord.IsNeutralCulture)
        //                    types |= CultureTypes.NeutralCultures;
        //                else
        //                    types |= CultureTypes.SpecificCultures;

        //                if (m_cultureTableRecord.IsSynthetic)
        //                    types |= CultureTypes.WindowsOnlyCultures | CultureTypes.InstalledWin32Cultures; // Synthetic is installed culture too.
        //                else
        //                {
        //                    // Not Synthetic
        //                    if (CultureTable.IsInstalledLCID(cultureID))
        //                        types |= CultureTypes.InstalledWin32Cultures;

        //                    if (!m_cultureTableRecord.IsCustomCulture || m_cultureTableRecord.IsReplacementCulture)
        //                        types |= CultureTypes.FrameworkCultures;
        //                }

        //                if (m_cultureTableRecord.IsCustomCulture)
        //                {
        //                    types |= CultureTypes.UserCustomCulture;

        //                    if (m_cultureTableRecord.IsReplacementCulture)
        //                        types |= CultureTypes.ReplacementCultures;
        //                }


        //                return types;
        //            }
        //        }

        public virtual NumberFormatInfo NumberFormat {
            get {

                if (this.numInfo == null) {
                    this.numInfo = new NumberFormatInfo(this);
                }

                return this.numInfo;
            }
        }

        public virtual DateTimeFormatInfo DateTimeFormat {
            get {
                if (this.dateTimeInfo == null) {
                    this.dateTimeInfo = new DateTimeFormatInfo(this);
                }

                return this.dateTimeInfo;
            }
        }
    }
}


