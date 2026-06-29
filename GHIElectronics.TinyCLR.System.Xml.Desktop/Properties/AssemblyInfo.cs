using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Dual-mode bridge: forward every public type from the TinyCLR System.Xml facade
// to the framework's System.Xml.dll so Desktop runs the real BCL XmlReader /
// XmlWriter (full conformance, validation, XPath capability). User code keeps
// the same `using System.Xml;` and `XmlReader.Create(...)` it would write
// against TinyCLR — the runtime follows these forwards on Desktop.
//
// Only PUBLIC types from GHIElectronics.TinyCLR.System.Xml need forwarding.
// WrappingXmlReader / WrappingXmlWriter are internal sealed and unreferenceable
// by user code.
[assembly: TypeForwardedTo(typeof(System.Xml.XmlReader))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlWriter))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlReaderSettings))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlNameTable))]
[assembly: TypeForwardedTo(typeof(System.Xml.NameTable))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlException))]
[assembly: TypeForwardedTo(typeof(System.Xml.IXmlLineInfo))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlNodeType))]
[assembly: TypeForwardedTo(typeof(System.Xml.ReadState))]
[assembly: TypeForwardedTo(typeof(System.Xml.ConformanceLevel))]
[assembly: TypeForwardedTo(typeof(System.Xml.WhitespaceHandling))]
[assembly: TypeForwardedTo(typeof(System.Xml.NewLineHandling))]
[assembly: TypeForwardedTo(typeof(System.Xml.XmlSpace))]
[assembly: TypeForwardedTo(typeof(System.Xml.ValidationType))]

[assembly: AssemblyTitle("GHIElectronics.TinyCLR.System.Xml")]
[assembly: AssemblyDescription("System.Xml compatibility facade for TinyCLR OS — Desktop shim forwarding to BCL System.Xml.")]
[assembly: AssemblyCompany("GHI Electronics, LLC")]
[assembly: AssemblyProduct("TinyCLR OS")]
[assembly: AssemblyCopyright("Copyright © GHI Electronics, LLC 2026")]
[assembly: ComVisible(false)]
[assembly: Guid("b8d0f2e3-4c5a-4f9b-ae6d-2e3f4a5b6c7e")]
[assembly: AssemblyVersion("3.0.0.3000")]
[assembly: AssemblyFileVersion("3.0.0.3000")]
[assembly: AssemblyInformationalVersion("3.0.0.3000-prerelease")]
