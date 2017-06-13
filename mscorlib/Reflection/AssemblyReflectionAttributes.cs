namespace System.Reflection {

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCopyrightAttribute : Attribute {
        private string m_copyright;

        public AssemblyCopyrightAttribute(string copyright) => this.m_copyright = copyright;

        public string Copyright => this.m_copyright;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyTrademarkAttribute : Attribute {
        private string m_trademark;

        public AssemblyTrademarkAttribute(string trademark) => this.m_trademark = trademark;

        public string Trademark => this.m_trademark;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyProductAttribute : Attribute {
        private string m_product;

        public AssemblyProductAttribute(string product) => this.m_product = product;

        public string Product => this.m_product;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyCompanyAttribute : Attribute {
        private string m_company;

        public AssemblyCompanyAttribute(string company) => this.m_company = company;

        public string Company => this.m_company;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDescriptionAttribute : Attribute {
        private string m_description;

        public AssemblyDescriptionAttribute(string description) => this.m_description = description;

        public string Description => this.m_description;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyTitleAttribute : Attribute {
        private string m_title;

        public AssemblyTitleAttribute(string title) => this.m_title = title;

        public string Title => this.m_title;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyConfigurationAttribute : Attribute {
        private string m_configuration;

        public AssemblyConfigurationAttribute(string configuration) => this.m_configuration = configuration;

        public string Configuration => this.m_configuration;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDefaultAliasAttribute : Attribute {
        private string m_defaultAlias;

        public AssemblyDefaultAliasAttribute(string defaultAlias) => this.m_defaultAlias = defaultAlias;

        public string DefaultAlias => this.m_defaultAlias;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyInformationalVersionAttribute : Attribute {
        private string m_informationalVersion;

        public AssemblyInformationalVersionAttribute(string informationalVersion) => this.m_informationalVersion = informationalVersion;

        public string InformationalVersion => this.m_informationalVersion;
    }
}


