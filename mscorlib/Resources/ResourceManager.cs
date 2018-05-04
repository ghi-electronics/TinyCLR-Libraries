//-----------------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Resources {
    public class ResourceManager {
        internal const string s_fileExtension = ".tinyresources";
        internal const string s_resourcesExtension = ".resources";

        private readonly string origBaseName;
        private readonly Assembly origAssembly;
        private string currentUICultureName;

        private int m_resourceFileId;
        private Assembly m_assembly;
        private Assembly m_baseAssembly;
        private string m_baseName;
        internal string m_cultureName;
        private ResourceManager m_rmFallback;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static private int NativeFindResource(string baseName, Assembly assembly);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private object NativeGetObject(short id, int offset, int length, out uint buffer, out uint size, out uint assembly);

        public ResourceManager(string baseName, Assembly assembly)
            : this(baseName, assembly, CultureInfo.CurrentUICulture.Name, true) {
        }

        internal ResourceManager(string baseName, Assembly assembly, string cultureName, bool fThrowOnFailure) {
            if (!Initialize(baseName, assembly, cultureName)) {
                if (fThrowOnFailure) {
                    throw new ArgumentException();
                }
            }
            else {
                this.origBaseName = baseName;
                this.origAssembly = assembly;
                this.currentUICultureName = cultureName;
            }
        }

        internal ResourceManager(string baseName, string cultureName, int iResourceFileId, Assembly assemblyBase, Assembly assemblyResource) {
            //found resource
            this.m_baseAssembly = assemblyBase;
            this.m_assembly = assemblyResource;
            this.m_baseName = baseName;
            this.m_cultureName = cultureName;
            this.m_resourceFileId = iResourceFileId;

            this.origBaseName = baseName;
            this.origAssembly = assemblyBase;
            this.currentUICultureName = cultureName;
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

            this.m_resourceFileId = -1;
            this.m_assembly = null;
            this.m_baseAssembly = null;
            this.m_baseName = null;
            this.m_cultureName = null;
            this.m_rmFallback = null;

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

                var iResourceFileId = NativeFindResource(resourceName, assemblyResource);

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

        public object GetObject(short id) => this.GetObject(id, 0, -1);

        public object GetObject(short id, int offset, int length) {
            if (this.currentUICultureName != CultureInfo.CurrentUICulture.Name) {
                if (!this.Initialize(this.origBaseName, this.origAssembly, CultureInfo.CurrentUICulture.Name))
                    throw new ArgumentException();

                this.currentUICultureName = CultureInfo.CurrentUICulture.Name;
            }

            var rm = this;

            var data = 0U;
            var assembly = 0U;
            var size = 0U;

            while (rm != null) {
                var obj = rm.NativeGetObject(id, offset, length, out data, out size, out assembly);

                if (obj != null) {

                    var method = obj.GetType().GetMethod("CreateInstantFromResources", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (method != null)
                        method.Invoke(obj, new object[] { data, size, assembly });

                    if (obj.GetType().FullName != "System.Drawing.Internal.Bitmap")
                        return obj;

                    foreach (var assm in AppDomain.CurrentDomain.GetAssemblies())
                        if (assm.GetName().Name == "GHIElectronics.TinyCLR.Drawing")
                            foreach (var t in assm.GetTypes())
                                if (t.FullName == "System.Drawing.Bitmap")
                                    return t.GetConstructor(new[] { obj.GetType() }).Invoke(new[] { obj });

                    throw new InvalidOperationException("Can't find Graphics assembly.");
                }

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
    }
}


