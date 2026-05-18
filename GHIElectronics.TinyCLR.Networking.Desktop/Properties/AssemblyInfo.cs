using System.Reflection;
using System.Runtime.InteropServices;

// IDENTITY MUST MATCH the TinyCLR-side GHIElectronics.TinyCLR.Networking
// assembly. The .NET runtime resolves IL refs by assembly identity
// (AssemblyName + Version + PublicKeyToken). If these drift apart between
// the impl DLL (TinyCLR-side) and this shim (Desktop-side), Desktop binding
// fails and the trick collapses.
[assembly: AssemblyTitle("GHIElectronics.TinyCLR.Networking")]
[assembly: AssemblyDescription("Desktop-side shim for TinyCLR Networking. Type forwards to .NET Framework's System.dll so the same compiled .exe runs on both TinyCLR and Desktop.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2022")]
[assembly: ComVisible(false)]
[assembly: Guid("1AFA3955-B36D-4A8A-8B7F-86F39FC2FDE3")]
[assembly: AssemblyVersion("3.0.0.1000")]
[assembly: AssemblyFileVersion("3.0.0.1000")]
[assembly: AssemblyInformationalVersion("3.0.0.1000-prerelease")]
