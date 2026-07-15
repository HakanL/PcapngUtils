# Haukcode.PcapngUtils

A fully managed C# library for reading and writing **Pcap** and **PcapNG** network capture files — no native WinPcap or libpcap dependency required.

## Installation

```bash
dotnet add package Haukcode.PcapngUtils
```

## Quick Start

```csharp
using Haukcode.PcapngUtils;

// Auto-detect format (Pcap or PcapNG) and read packets one at a time
using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

## Reading Packets

### One at a time (recommended)

```csharp
using Haukcode.PcapngUtils;

using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    // Process packet.Data, packet.Seconds, packet.Microseconds
}
```

### Allocation-free (reusable buffer)

```csharp
using Haukcode.PcapngUtils;
using Haukcode.PcapngUtils.Common;

using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.ReadNextPacket(out PacketMemory packet))
{
    // packet.Data is valid until the next ReadNextPacket/Rewind call — copy if it must live longer
    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

### Via event callback

```csharp
using System.Threading;
using Haukcode.PcapngUtils;

using var reader = IReaderFactory.GetReader("capture.pcap");
reader.OnReadPacketEvent += (context, packet) =>
{
    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
};
reader.ReadPackets(CancellationToken.None);
```

## Writing Packets

```csharp
using Haukcode.PcapngUtils;
using Haukcode.PcapngUtils.PcapNG;

using var reader = IReaderFactory.GetReader("input.pcap");
using var writer = new PcapNGWriter("output.pcapng");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    writer.WritePacket(packet);
}
```

## Key Types

| Type | Description |
|---|---|
| `IReaderFactory` | Opens Pcap or PcapNG files/streams with auto-detection. |
| `PcapReader` | Reads Pcap format files. |
| `PcapNGReader` | Reads PcapNG format files. |
| `PcapWriter` | Writes Pcap format files. |
| `PcapNGWriter` | Writes PcapNG format files. |
| `IPacket` | Packet interface: `Seconds`, `Microseconds`, `Data`. |

For full documentation and more examples, see the [GitHub repository](https://github.com/HakanL/PcapngUtils).
