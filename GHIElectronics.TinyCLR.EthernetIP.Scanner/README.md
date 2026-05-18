# GHIElectronics.TinyCLR.EthernetIP.Scanner

Run a TinyCLR device as an EtherNet/IP **Scanner** (the *client* / *originator*
side — what talks to PLCs, motor drives, or other EIP adapters). Pure C#
implementation, no native interop. Based on the
[EEIP.NET](https://github.com/rossmann-engineering/EEIP.NET) library by
Rossmann Engineering with TinyCLR-specific fixes and a richer event surface.

## Capabilities

- Broadcast-discover EIP devices via `ListIdentity`
- Explicit messaging: `GetAttributeSingle`, `GetAttributeAll`, `SetAttributeSingle`
- Class 1 implicit (cyclic) I/O via Forward Open + UDP 2222
- Class 3 explicit messaging (unconnected)
- Standard CIP object proxies: `IdentityObject`, `MessageRouterObject`,
  `AssemblyObject` (remote), `TcpIpInterfaceObject`
- Lifecycle events: `ConnectionEstablished`, `ConnectionLost`,
  `ImplicitDataReceived`, `RpiViolated`
- `IDisposable` — clean teardown via `using` block

## Minimal example

```csharp
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.EthernetIP.Scanner;

var network = NetworkController.GetDefault();

using (var scanner = new ScannerController()) {
    // 1. Discover.
    var devices = scanner.ListIdentity(network, TimeSpan.FromSeconds(2));
    foreach (var d in devices) {
        Console.WriteLine(d.ProductName1 + " @ "
            + Encapsulation.CIPIdentityItem.GetIPAddress(d.SocketAddress.SIN_Address));
    }

    // 2. Configure the connection (property-bag API — set ALL parameters
    //    BEFORE calling ForwardOpen).
    scanner.IPAddress              = "192.168.1.100";
    scanner.O_T_InstanceID         = 150;          // target's output assembly (we write here)
    scanner.T_O_InstanceID         = 100;          // target's input assembly  (we read here)
    scanner.ConfigurationAssemblyInstanceID = 151;
    scanner.RequestedPacketRate_O_T = 10_000;      // 10 ms RPI in microseconds
    scanner.RequestedPacketRate_T_O = 10_000;
    scanner.O_T_IOData = new byte[32];
    scanner.T_O_IOData = new byte[32];
    scanner.O_T_RealTimeFormat = RealTimeFormat.Header32Bit;
    scanner.T_O_RealTimeFormat = RealTimeFormat.Modeless;

    // 3. Subscribe to events.
    scanner.ConnectionEstablished += (s, e) => Console.WriteLine("up");
    scanner.ConnectionLost        += (s, e) => Console.WriteLine("lost");
    scanner.ImplicitDataReceived  += (s, snapshot) => {
        // snapshot is a fresh byte[] — safe to read off-thread.
        // T_O_IOData is also updated for backwards compat but is racy.
    };

    // 4. Open the session, then the Class 1 connection.
    scanner.RegisterSession();
    scanner.ForwardOpen();

    // Application loop — write outgoing values.
    while (true) {
        scanner.O_T_IOData[0]++;
        Thread.Sleep(100);
    }
}
// Dispose() runs ForwardClose + UnRegisterSession + socket cleanup automatically.
```

## Common pitfalls

### 1. `T_O_IOData` is racy — prefer the `ImplicitDataReceived` event

The receive thread writes `T_O_IOData[i] = ...` byte-by-byte while your
application reads it. You can observe a half-updated buffer between writes.
For robust code:

```csharp
byte[] safeT_O = null;
scanner.ImplicitDataReceived += (s, snapshot) => {
    safeT_O = snapshot;   // fresh byte[], no race
};
```

`T_O_IOData` is kept for backwards compatibility but new code should use the
event-delivered snapshot.

### 2. Configure properties BEFORE `ForwardOpen`

`ScannerController` is a property-bag API. Changing
`RequestedPacketRate_O_T`, `O_T_IOData`, `T_O_RealTimeFormat`, etc. after
`ForwardOpen` has no effect on the live connection — close and reopen if
you need to change parameters.

### 3. Connection-size mismatch is the most common Forward Open failure

If `ForwardOpen()` throws a `CIPException` with status code "Connection
failure" + extended status `0x0109 "Invalid connection size"`, the connection
size you negotiated doesn't match what the target expects. Per direction:
- `O_T_Length` + 2 (CIP sequence count) + 4 if `O_T_RealTimeFormat = Header32Bit`
- `T_O_Length` + 2 (CIP sequence count) + 4 if `T_O_RealTimeFormat = Header32Bit`

`Detect_O_T_Length()` and `Detect_T_O_Length()` can auto-discover the target's
assembly sizes via explicit Get_Attribute_Single, but you still need to know
whether the target uses Run/Idle headers.

### 4. `LastReceivedImplicitMessage` uses wall-clock — prefer `LastReceivedImplicitMessageTickCount`

The DateTime version breaks if the system clock jumps (NTP, DST, manual set).
The tick-count version is monotonic:

```csharp
if (Environment.TickCount - scanner.LastReceivedImplicitMessageTickCount > 5000) {
    // 5 s since last implicit message — target probably offline
}
```

### 5. `ListIdentity` broadcasts to a directed subnet address

The current implementation computes a *directed broadcast* (e.g. 192.168.1.255
for a /24 subnet), not limited broadcast (255.255.255.255). Routers and some
managed switches block directed broadcast — discovery may miss devices behind
network segments. Run scanner on the same subnet as targets for now.

### 6. `TcpClient` connect has no timeout

`RegisterSession()` calls `new TcpClient(ipAddress, port)` which blocks the
calling thread until the OS-level TCP connect timeout (typically 60-120 s).
If the target is unreachable, your scanner thread is stuck. Mitigate by
pinging the target first, or run RegisterSession on a separate thread you
can abandon.

### 7. Class 1 RPI is rounded to TinyCLR's tick granularity

`Thread.Sleep((int)RequestedPacketRate_O_T / 1000)` in the producer thread
gives roughly ±10 ms jitter on TinyCLR. RPIs below ~10 ms aren't reliable;
RPIs ≥ 50 ms are well within tolerance.

## See also

- Sample app: `Test\TinyCLRApplication_EthernetIP\Program.cs`
- ODVA EtherNet/IP specification (Vol 1 + Vol 2): https://www.odva.org/
- Upstream EEIP.NET: https://github.com/rossmann-engineering/EEIP.NET
