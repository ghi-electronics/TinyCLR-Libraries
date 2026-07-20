using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// .NET-overlap algorithms (SHA1/SHA256/MD5/HMACSHA1/HMACSHA256/RSACryptoServiceProvider/RSAParameters)
// are internal here; their public surface lives in the System.Security.Cryptography compat assembly,
// which delegates back via friend-assembly access.
[assembly: InternalsVisibleTo("System.Security.Cryptography")]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Cryptography")]
[assembly: AssemblyDescription("TinyCLR OS Cryptography library.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2026")]
[assembly: ComVisible(false)]
[assembly: Guid("7082C1DA-1E6C-4DDB-B54A-9609BB79F60F")]
[assembly: AssemblyVersion("3.0.1.1000")]
[assembly: AssemblyFileVersion("3.0.1.1000")]
[assembly: AssemblyInformationalVersion("3.0.1.1000-prerelease")]
