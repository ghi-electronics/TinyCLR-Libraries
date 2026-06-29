using System.Reflection;
using System.Runtime.InteropServices;

// Identity MUST match GHIElectronics.TinyCLR.Native\Properties\AssemblyInfo.cs.
// The Desktop runtime resolves type lookups via this shim's identity, so any
// drift breaks the dual-mode trick.
[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Native")]
[assembly: AssemblyDescription("TinyCLR OS native library.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("B602474A-AEB0-491C-832B-7EAF0B4511C0")]
[assembly: AssemblyVersion("3.0.0.3000")]
[assembly: AssemblyFileVersion("3.0.0.3000")]
[assembly: AssemblyInformationalVersion("3.0.0.3000-prerelease")]
