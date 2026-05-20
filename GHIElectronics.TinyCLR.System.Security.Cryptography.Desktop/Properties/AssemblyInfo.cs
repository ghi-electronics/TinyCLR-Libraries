using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: forward all BCL-shaped crypto types to BCL System.dll /
// mscorlib / System.Core. The impl assembly is named "System.Security.Cryptography"
// and contains hand-written wrappers around GHIElectronics.TinyCLR.Cryptography
// for use on the device. On Desktop we forward to the framework's real impls.
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.HashAlgorithmName))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSAEncryptionPaddingMode))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSAEncryptionPadding))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSASignaturePaddingMode))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSASignaturePadding))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSAParameters))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.HashAlgorithm))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.KeyedHashAlgorithm))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.HMAC))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.AsymmetricAlgorithm))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSA))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.SHA1))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.SHA256))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.MD5))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.HMACSHA1))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.HMACSHA256))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RSACryptoServiceProvider))]
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.RandomNumberGenerator))]

[assembly: AssemblyTitle("System.Security.Cryptography")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("System.Security.Cryptography")]
[assembly: AssemblyCopyright("Copyright GHI Electronics, LLC")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("ea2e917b-c1cb-4f53-8f7a-7ad7f9e8a3f8")]

[assembly: AssemblyVersion("3.0.0.1000")]
[assembly: AssemblyFileVersion("3.0.0.1000")]
[assembly: AssemblyInformationalVersion("3.0.0.1000-prerelease")]
