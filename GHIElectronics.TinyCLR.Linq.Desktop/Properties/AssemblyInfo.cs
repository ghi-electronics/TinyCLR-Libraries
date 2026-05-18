using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: forward all public System.Linq types to System.Core.dll so
// Desktop runs the framework's mature LINQ-to-Objects implementation instead of
// the TinyCLR embedded subset. User code keeps the same typerefs (compiled
// against TinyCLR), the runtime resolves them through these forwards on Desktop.
//
// Only PUBLIC types from GHIElectronics.TinyCLR.Linq need forwarding; internal
// helpers (Grouping<,>, OrderedEnumerable<>, SortKey) are unreferenceable by
// user code, so the BCL's own internals stay invisible to the forward chain.
[assembly: TypeForwardedTo(typeof(System.Linq.Enumerable))]
[assembly: TypeForwardedTo(typeof(System.Linq.IGrouping<,>))]
[assembly: TypeForwardedTo(typeof(System.Linq.IOrderedEnumerable<>))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Linq")]
[assembly: AssemblyDescription("TinyCLR OS LINQ library (System.Linq.Enumerable subset).")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("8B3C4D5E-6F7A-4B8C-9D0E-1F2A3B4C5D6E")]
[assembly: AssemblyVersion("3.0.0.1000")]
[assembly: AssemblyFileVersion("3.0.0.1000")]
[assembly: AssemblyInformationalVersion("3.0.0.1000-prerelease")]
