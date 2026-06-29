using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: forward all BCL-shaped regex types to System.dll so
// Desktop runs the framework's mature Regex engine instead of the hand-rolled
// TinyCLR port. User code keeps the same typerefs (compiled against TinyCLR),
// the runtime resolves them through these forwards on Desktop.
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.Regex))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.Match))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.Group))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.Capture))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.MatchEvaluator))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.RegexOptions))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.CaptureCollection))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.GroupCollection))]
[assembly: TypeForwardedTo(typeof(System.Text.RegularExpressions.MatchCollection))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.RegularExpressions")]
[assembly: AssemblyDescription("TinyCLR OS RegularExpressions library.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("4F7F4E0D-676E-42AC-AE13-991EDD5121E7")]
[assembly: AssemblyVersion("3.0.0.3000")]
[assembly: AssemblyFileVersion("3.0.0.3000")]
[assembly: AssemblyInformationalVersion("3.0.0.3000-prerelease")]
