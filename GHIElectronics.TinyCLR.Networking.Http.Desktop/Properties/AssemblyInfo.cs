using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: when running on Desktop, anyone holding a typeref to
// [GHIElectronics.TinyCLR.Networking.Http]System.Uri (or UriKind/UriHostNameType)
// gets transparently redirected to the BCL types in System.dll.
// The TinyCLR-impl assembly defines these types locally for device runtime.
[assembly: TypeForwardedTo(typeof(System.Uri))]
[assembly: TypeForwardedTo(typeof(System.UriKind))]
[assembly: TypeForwardedTo(typeof(System.UriHostNameType))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Networking.Http")]
[assembly: AssemblyDescription("TinyCLR OS HTTP library.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("D89B51CA-0A2C-4A50-8312-D3B3F57F1096")]
[assembly: AssemblyVersion("3.0.0.1000")]
[assembly: AssemblyFileVersion("3.0.0.1000")]
[assembly: AssemblyInformationalVersion("3.0.0.1000-prerelease")]
