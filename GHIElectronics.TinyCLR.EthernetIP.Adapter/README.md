# GHIElectronics.TinyCLR.EthernetIP.Adapter

Run a TinyCLR device as an EtherNet/IP **Adapter** (the *server* side — what
a PLC scanner connects to). Native implementation based on the
[OpENer](https://github.com/EIPStackGroup/OpENer) stack with a TinyCLR-friendly
managed C# API on top.

## Capabilities

- Encapsulation: List Identity / List Services / List Interfaces /
  Register Session / Unregister Session / Send RR Data / Send Unit Data
- Class 3 explicit messaging (UCMM + connected)
- Class 1 implicit (cyclic) I/O via UDP 2222
- Forward Open / Large Forward Open / Forward Close
- Standard CIP objects auto-initialized: Identity (1), Message Router (2),
  Connection Manager (6), Assembly (4), TCP/IP Interface (0xF5),
  Ethernet Link (0xF6), QoS (0x48)
- Custom CIP classes / attributes / services from C# user code
- Run/Idle header runtime toggles (`EnableHeaderO2T`, `EnableHeaderT2O`)
- Lifecycle events: `RegisterSessionDetected`, `UnregisterSessionDetected`,
  `ForwardOpenDetected`, `ForwardCloseDetected`,
  `AfterAssemblyDataReceived`, `BeforeAssemblyDataSend`,
  `ReceivedExplicitTcpData`, `ReceivedExplicitUdpData`, `NotifyClass`

## Minimal example

```csharp
using GHIElectronics.TinyCLR.EthernetIP.Adapter;
using static GHIElectronics.TinyCLR.EthernetIP.Adapter.AdapterController;

// Keep assembly buffers as long-lived fields so the GC doesn't move them
// while the native side holds raw pointers (Phase 4 will copy these
// internally; until then, lifetime is the user's responsibility).
static readonly byte[] inputData  = new byte[32];   // T->O (target produces, scanner reads)
static readonly byte[] outputData = new byte[32];   // O->T (scanner produces, target reads)
static readonly byte[] configData = new byte[10];   // configuration

using (var adapter = new AdapterController(
        deviceName:         "MyDevice",
        deviceVendorID:     0x1234,    // ODVA-assigned (1-99 are reserved; use a real vendor ID)
        deviceType:         0x000C,    // CIP Vol 1 device type (0x0C = Generic Device)
        deviceProductCode:  100,
        deviceSerialNumber: 0x01020304,
        deviceMajorRevision: 1,
        deviceMinorRevision: 0)) {

    // CRITICAL: enable Run/Idle on O->T if you're talking to Allen-Bradley
    // ControlLogix/CompactLogix scanners. They always prepend a 4-byte Run/Idle
    // header on outputs. Without this, your first 4 bytes of outputData are
    // wrong. See "Common pitfalls" below.
    adapter.EnableHeaderO2T(true);

    adapter.AddAssemblyObject(new AssemblyObject(100, inputData,  (ushort)inputData.Length));
    adapter.AddAssemblyObject(new AssemblyObject(150, outputData, (ushort)outputData.Length));
    adapter.AddAssemblyObject(new AssemblyObject(151, configData, (ushort)configData.Length));

    // connection_number=0, output=150, input=100, configuration=151
    adapter.ConfigureExclusiveOwnerConnectionPoint(0, 150, 100, 151);

    adapter.Enable();    // Opens sockets, spawns OpENer thread, auto-inits CIP classes

    // Your application loop: update inputData[..] to produce values,
    // read outputData[..] to consume scanner writes.
    while (true) { Thread.Sleep(10); }
}
// Dispose() at end of `using` signals the OpENer thread to stop,
// waits up to 1 s for it to terminate, frees its 8 KB stack,
// and resets the singleton flags so a new adapter can be constructed.
```

## Common pitfalls

### 1. Run/Idle header — biggest interop trap

Rockwell Automation scanners (ControlLogix, CompactLogix, MicroLogix family)
**always prepend a 32-bit Run/Idle header** on the O→T (output) cyclic data
they send. Other scanner brands (HMS Anybus, Codesys, etc.) may not.

If your O→T data starts with garbage that changes between 0x00000000 and
0x00000001 every cycle, the scanner is sending Run/Idle and your adapter
isn't decoding it. Fix:

```csharp
adapter.EnableHeaderO2T(true);   // strip the 4-byte header from O->T before assembly
```

T→O (input) Run/Idle is less common but some scanners require it; toggle via
`EnableHeaderT2O(true)`.

### 2. Forward Open size mismatch

If the scanner reports a Forward Open failure with extended status
`0x0109 "Invalid connection size"`, the negotiated size differs from your
assembly length plus the per-direction overhead:
- Each direction: +2 bytes for the 16-bit CIP sequence count
- O→T with Run/Idle enabled: +4 more bytes

So a 32-byte assembly with Class 1 + Run/Idle ON appears as 38 bytes on the
wire. The scanner's connection-size field in Forward Open must match.

### 3. Vendor IDs

Vendor IDs 1-99 are assigned to specific companies by ODVA. Don't ship a
product with vendor ID 0x1234 to customers without registering with ODVA
(https://www.odva.org/). For lab work it doesn't matter.

### 4. Assembly byte[] lifetime

The C# `byte[]` you pass to `AssemblyObject` is held as a raw pointer by
the native side and accessed every RPI. If your application drops the only
managed reference, the GC may free it and the next Class-1 send reads
garbage. Keep assembly buffers as long-lived `static` or instance fields.

### 5. Singleton

Only one `AdapterController` can exist per process at a time. `Dispose()`
or `Disable()` resets the singleton flags so you can reconstruct.

### 6. Loopback (127.0.0.1) doesn't work

Self-loopback is disabled in lwIP on device. Use a separate PC partner for
testing.

## See also

- Sample app: `Test\TinyCLRApplication_EthernetIP\Program.cs`
- ODVA EtherNet/IP specification (Vol 1 + Vol 2): https://www.odva.org/
- EDS file template: `<lib-folder>\example.eds` (customize for your device)
