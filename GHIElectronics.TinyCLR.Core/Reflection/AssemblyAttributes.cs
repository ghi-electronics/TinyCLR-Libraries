namespace System.Reflection {

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCultureAttribute : Attribute {
        private string m_culture;

        public AssemblyCultureAttribute(string culture) => this.m_culture = culture;

        public string Culture => this.m_culture;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyVersionAttribute : Attribute {
        private string m_version;

        public AssemblyVersionAttribute(string version) => this.m_version = version;

    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyKeyFileAttribute : Attribute {
        private string m_keyFile;

        public AssemblyKeyFileAttribute(string keyFile) => this.m_keyFile = keyFile;

        public string KeyFile => this.m_keyFile;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyKeyNameAttribute : Attribute {
        private string m_keyName;

        public AssemblyKeyNameAttribute(string keyName) => this.m_keyName = keyName;

        public string KeyName => this.m_keyName;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDelaySignAttribute : Attribute {
        private bool m_delaySign;

        public AssemblyDelaySignAttribute(bool delaySign) => this.m_delaySign = delaySign;

        public bool DelaySign => this.m_delaySign;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyFlagsAttribute : Attribute {
        private AssemblyNameFlags m_flags;

        [CLSCompliant(false)]
        public AssemblyFlagsAttribute(uint flags) => this.m_flags = (AssemblyNameFlags)flags;

        [CLSCompliant(false)]
        public uint Flags => (uint)this.m_flags;
        public AssemblyFlagsAttribute(AssemblyNameFlags assemblyFlags) => this.m_flags = assemblyFlags;
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblyFileVersionAttribute : Attribute {
        private string _version;

        public AssemblyFileVersionAttribute(string version) => this._version = version ?? throw new ArgumentNullException("version");

        public string Version => this._version;
    }
}


