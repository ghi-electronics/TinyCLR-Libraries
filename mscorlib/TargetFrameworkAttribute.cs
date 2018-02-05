/*============================================================
**
** Class:  TargetFrameworkAttribute
**
**
** Purpose: Identifies which SKU and version of the .NET
**   Framework that a particular library was compiled against.
**   Emitted by VS, and can help catch deployment problems.
**
===========================================================*/

namespace System.Runtime.Versioning {

    [AttributeUsageAttribute(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class TargetFrameworkAttribute : Attribute {
        private string _frameworkName;  // A target framework moniker
        private string _frameworkDisplayName;

        // The frameworkName parameter is intended to be the string form of a FrameworkName instance.
        public TargetFrameworkAttribute(string frameworkName) => this._frameworkName = frameworkName ?? throw new ArgumentNullException();

        // The target framework moniker that this assembly was compiled against.
        // Use the FrameworkName class to interpret target framework monikers.
        public string FrameworkName => this._frameworkName;
        public string FrameworkDisplayName {
            get => this._frameworkDisplayName; set => this._frameworkDisplayName = value;
        }
    }
}
