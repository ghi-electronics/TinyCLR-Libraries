// On Desktop, this assembly is loaded from bin\Debug\ when user code references
// GHIElectronics.TinyCLR.Networking. The TypeForwardedTo entries below tell the
// .NET runtime "the real type lives in System.dll / System.Security.dll" — the
// runtime follows the forward and resolves the type from the framework BCL.
//
// The TinyCLR-side library provides the same types via its own implementation
// (deployed to the device as .pe). Same compiled .exe, two runtimes, two
// resolution paths — but only one set of source code.

using System.Runtime.CompilerServices;

// System.Net (in System.dll on .NET Framework 4.8)
[assembly: TypeForwardedTo(typeof(System.Net.Dns))]
[assembly: TypeForwardedTo(typeof(System.Net.IPAddress))]
[assembly: TypeForwardedTo(typeof(System.Net.IPEndPoint))]
[assembly: TypeForwardedTo(typeof(System.Net.IPHostEntry))]
[assembly: TypeForwardedTo(typeof(System.Net.EndPoint))]
[assembly: TypeForwardedTo(typeof(System.Net.SocketAddress))]
[assembly: TypeForwardedTo(typeof(System.Net.WebUtility))]

// System.Net.Sockets (in System.dll on .NET Framework 4.8)
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.Socket))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.AddressFamily))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.ProtocolType))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.ProtocolFamily))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SocketType))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SocketException))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SocketFlags))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SocketOptionLevel))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SocketOptionName))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.SelectMode))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.MulticastOption))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.NetworkStream))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.TcpClient))]
[assembly: TypeForwardedTo(typeof(System.Net.Sockets.UdpClient))]

// System.Net.Security (in System.dll on .NET Framework 4.8)
[assembly: TypeForwardedTo(typeof(System.Net.Security.SslStream))]

// System.Security.Authentication (in System.dll on .NET Framework 4.8)
[assembly: TypeForwardedTo(typeof(System.Security.Authentication.SslProtocols))]

// System.Security.Cryptography.X509Certificates (in System.dll / System.Security.dll)
[assembly: TypeForwardedTo(typeof(System.Security.Cryptography.X509Certificates.X509Certificate))]
