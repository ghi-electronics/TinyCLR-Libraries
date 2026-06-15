using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: forward HashSet<T> and ISet<T> to System.Core.dll so Desktop
// runs the framework's mature implementation instead of the TinyCLR copy. User
// code keeps the same typerefs (compiled against TinyCLR), the runtime resolves
// them through these forwards on Desktop.
[assembly: TypeForwardedTo(typeof(System.Collections.Generic.HashSet<>))]
[assembly: TypeForwardedTo(typeof(System.Collections.Generic.ISet<>))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Collections")]
[assembly: AssemblyDescription("TinyCLR OS Collections library (HashSet/ISet).")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2026")]
[assembly: ComVisible(false)]
[assembly: Guid("158c0ea7-71a7-421c-a13c-03c883ac0844")]
[assembly: AssemblyVersion("3.0.0.2000")]
[assembly: AssemblyFileVersion("3.0.0.2000")]
[assembly: AssemblyInformationalVersion("3.0.0.2000-prerelease")]
