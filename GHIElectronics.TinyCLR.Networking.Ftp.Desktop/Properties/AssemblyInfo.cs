extern alias bcl;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// FtpStatusCode and WebRequestMethods exist in BCL System.dll with the same shape
// (BCL is a strict superset of our enum/constants). The impl side compiles its own
// copy of WebRequestMethods.cs for device; on Desktop we forward to BCL so server-
// side code that references FtpStatusCode/WebRequestMethods.Ftp.* resolves to BCL.
[assembly: TypeForwardedTo(typeof(bcl::System.Net.FtpStatusCode))]
[assembly: TypeForwardedTo(typeof(bcl::System.Net.WebRequestMethods))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Networking.Ftp")]
[assembly: AssemblyDescription("TinyCLR OS FTP library.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("7D5D449A-E1A6-4232-8132-C7C982187560")]
[assembly: AssemblyVersion("3.0.0.3000")]
[assembly: AssemblyFileVersion("3.0.0.3000")]
[assembly: AssemblyInformationalVersion("3.0.0.3000-prerelease")]
