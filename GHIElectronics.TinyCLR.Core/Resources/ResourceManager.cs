//-----------------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Resources {
    public class ResourceManager {
        internal const string s_fileExtension = ".tinyresources";
        internal const string s_resourcesExtension = ".resources";

        private int m_resourceFileId;
        private Assembly m_assembly;
        private Assembly m_baseAssembly;
        private string m_baseName;
        internal string m_cultureName;
        private ResourceManager m_rmFallback;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static private int FindResource(string baseName, Assembly assembly);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private object GetObjectInternal(short id);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private object GetObjectInternal(short id, int offset, int length);

        public ResourceManager(string baseName, Assembly assembly)
            : this(baseName, assembly, CultureInfo.CurrentUICulture.Name, true) {
        }

        internal ResourceManager(string baseName, Assembly assembly, string cultureName, bool fThrowOnFailure) {
            if (!Initialize(baseName, assembly, cultureName)) {
                if (fThrowOnFailure) {
                    throw new ArgumentException();
                }
            }
        }

        internal ResourceManager(string baseName, string cultureName, int iResourceFileId, Assembly assemblyBase, Assembly assemblyResource) {
            //found resource
            this.m_baseAssembly = assemblyBase;
            this.m_assembly = assemblyResource;
            this.m_baseName = baseName;
            this.m_cultureName = cultureName;
            this.m_resourceFileId = iResourceFileId;
        }

        private bool IsValid => this.m_resourceFileId >= 0;
        private string GetParentCultureName(string cultureName) {
            var iDash = cultureName.LastIndexOf('-');
            if (iDash < 0)
                cultureName = "";
            else
                cultureName = cultureName.Substring(0, iDash);

            return cultureName;
        }

        internal bool Initialize(string baseName, Assembly assembly, string cultureName) {
            var cultureNameSav = cultureName;
            var assemblySav = assembly;

            this.m_resourceFileId = -1;  //set to invalid state

            var fTryBaseAssembly = false;

            while (true) {
                var fInvariantCulture = (cultureName == "");

                var splitName = assemblySav.FullName.Split(',');

                var assemblyName = splitName[0];

                if (!fInvariantCulture) {
                    assemblyName = assemblyName + "." + cultureName;
                }
                else if (!fTryBaseAssembly) {
                    assemblyName = assemblyName + s_resourcesExtension;
                }

                // append version
                if (splitName.Length >= 1 && splitName[1] != null) {
                    assemblyName += ", " + splitName[1].Trim();
                }

                assembly = Assembly.Load(assemblyName, false);

                if (assembly != null) {
                    if (Initialize(baseName, assemblySav, cultureNameSav, assembly))
                        return true;
                }

                if (!fInvariantCulture) {
                    cultureName = GetParentCultureName(cultureName);
                }
                else if (!fTryBaseAssembly) {
                    fTryBaseAssembly = true;
                }
                else {
                    break;
                }
            }

            return false;
        }

        internal bool Initialize(string baseName, Assembly assemblyBase, string cultureName, Assembly assemblyResource) {
            while (true) {
                var resourceName = baseName;
                var fInvariantCulture = (cultureName == "");

                if (!fInvariantCulture) {
                    resourceName = baseName + "." + cultureName;
                }

                resourceName = resourceName + s_fileExtension;

                var iResourceFileId = FindResource(resourceName, assemblyResource);

                if (iResourceFileId >= 0) {
                    //found resource
                    this.m_baseAssembly = assemblyBase;
                    this.m_assembly = assemblyResource;
                    this.m_baseName = baseName;
                    this.m_cultureName = cultureName;
                    this.m_resourceFileId = iResourceFileId;

                    break;
                }
                else if (fInvariantCulture) {
                    break;
                }

                cultureName = GetParentCultureName(cultureName);
            }

            return this.IsValid;
        }

        private object GetObjectFromId(short id) {
            var rm = this;

            while (rm != null) {
                var obj = rm.GetObjectInternal(id);

                if (obj != null)
                    return obj;

                if (rm.m_rmFallback == null) {
                    if (rm.m_cultureName != "") {
                        var cultureNameParent = GetParentCultureName(rm.m_cultureName);
                        var rmFallback = new ResourceManager(this.m_baseName, this.m_baseAssembly, cultureNameParent, false);

                        if (rmFallback.IsValid)
                            rm.m_rmFallback = rmFallback;
                    }
                }

                rm = rm.m_rmFallback;
            }

            throw new ArgumentException();
        }

        private object GetObjectChunkFromId(short id, int offset, int length) {
            var rm = this;

            while (rm != null) {
                var obj = rm.GetObjectInternal(id, offset, length);

                if (obj != null)
                    return obj;

                if (rm.m_rmFallback == null) {
                    if (rm.m_cultureName != "") {
                        var cultureNameParent = GetParentCultureName(rm.m_cultureName);
                        var rmFallback = new ResourceManager(this.m_baseName, this.m_baseAssembly, cultureNameParent, false);

                        if (rmFallback.IsValid)
                            rm.m_rmFallback = rmFallback;
                    }
                }

                rm = rm.m_rmFallback;
            }

            throw new ArgumentException();
        }

        public static object GetObject(ResourceManager rm, Enum id) {
            var obj = ResourceManager.NativeGetObject(rm, id);

            if (obj.GetType().FullName != "System.Drawing.Internal.Bitmap")
                return obj;

            foreach (var assm in AppDomain.CurrentDomain.GetAssemblies())
                if (assm.GetName().Name == "GHIElectronics.TinyCLR.Graphics")
                    foreach (var t in assm.GetTypes())
                        if (t.FullName == "System.Drawing.Bitmap")
                            return t.GetConstructor(new[] { obj.GetType() }).Invoke(new[] { obj });

            throw new InvalidOperationException("Can't find Graphics assembly.");
        }

        public static object GetObject(ResourceManager rm, Enum id, int offset, int length) => ResourceManager.NativeGetObject(rm, id, offset, length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static object NativeGetObject(ResourceManager rm, Enum id);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static object NativeGetObject(ResourceManager rm, Enum id, int offset, int length);
    }
}


