# Haukcode.PcapngUtils [![NuGet Version](http://img.shields.io/nuget/v/Haukcode.PcapngUtils.svg?style=flat)](https://www.nuget.org/packages/Haukcode.PcapngUtils/)

A fully managed C# implementation for reading and writing Pcap and PcapNG network capture files.

## Table of Contents
- [What is Pcap/PcapNG?](#what-is-pcappcapng)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
  - [Read packets one at a time](#read-packets-one-at-a-time)
  - [Read packets via event callback](#read-packets-via-event-callback)
  - [Auto-detect file format](#auto-detect-file-format)
  - [Write packets to Pcap file](#write-packets-to-pcap-file)
  - [Write packets to PcapNG file](#write-packets-to-pcapng-file)
  - [Clone a capture file](#clone-a-capture-file)
- [API Reference](#api-reference)
- [License](#license)

## What is Pcap/PcapNG?

Pcap and PcapNG are file formats used to store dumps of network traffic:

- **Pcap** – the classic packet capture format, widely supported by tools like Wireshark and tcpdump. See the [Pcap file format specification](https://wiki.wireshark.org/Development/LibpcapFileFormat).
- **PcapNG** – the next-generation format with richer metadata (multiple interfaces, timestamps, comments, etc.). See the [PcapNG specification](https://www.winpcap.org/ntar/draft/PCAP-DumpFileFormat.html).

This library provides a pure managed implementation — no native WinPcap or libpcap dependency required.

## Features

✅ **Pure Managed .NET**
- No native WinPcap or libpcap dependency
- Targets .NET Standard 2.0 — works on .NET Framework, .NET Core, and .NET 5+

✅ **Read & Write Both Formats**
- Read and write Pcap and PcapNG files
- Auto-detect file format from file or stream

✅ **Flexible Reading API**
- Read packets one at a time with `ReadNextPacket()` for full control
- Or stream all packets via the `OnReadPacketEvent` callback with `ReadPackets()`
- Random access via `Position`/`Rewind()`

✅ **Cross-Platform**
- Works on Windows, Linux, and macOS

## Installation

Install via .NET CLI:

```bash
dotnet add package Haukcode.PcapngUtils
```

Or via Package Manager Console:

```powershell
Install-Package Haukcode.PcapngUtils
```

Or add directly to your `.csproj` file:

```xml
<PackageReference Include="Haukcode.PcapngUtils" Version="1.3.14" />
```

## Quick Start

```csharp
using Haukcode.PcapngUtils;

// Auto-detect format and read packets one at a time
using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

## Usage Examples

### Read packets one at a time

Use `ReadNextPacket()` to iterate through packets at your own pace. This is the recommended approach when you need full control over iteration, want to process packets synchronously, or need to inspect `Position`/`MoreAvailable`.

#### Pcap file

```csharp
using Haukcode.PcapngUtils.Pcap;

using var reader = new PcapReader("capture.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

#### PcapNG file

```csharp
using Haukcode.PcapngUtils.PcapNG;

using var reader = new PcapNGReader("capture.pcapng", swapBytes: false);

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

#### Auto-detect format

```csharp
using Haukcode.PcapngUtils;

using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

#### Allocation-free reading

`ReadNextPacket(out PacketMemory packet)` reads into a reader-owned reusable buffer instead of allocating a fresh `byte[]` per packet — useful on hot paths where per-packet garbage matters. The packet's `Data` is only valid until the next read or rewind on that reader, so copy any bytes that must live longer. Because the buffer is shared, this overload does not support interleaved reads from multiple threads. (On `PcapNGReader` the overload satisfies the same contract but block parsing still allocates internally.)

```csharp
using Haukcode.PcapngUtils;
using Haukcode.PcapngUtils.Common;

using var reader = IReaderFactory.GetReader("capture.pcap");

while (reader.ReadNextPacket(out PacketMemory packet))
{
    // packet.Data is valid until the next ReadNextPacket/Rewind call
    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
}
```

### Read packets via event callback

Use `ReadPackets()` with the `OnReadPacketEvent` callback when you prefer an event-driven model. The call blocks until all packets have been read or the cancellation token is triggered.

#### Pcap file

```csharp
using System.Threading;
using Haukcode.PcapngUtils.Pcap;

using var reader = new PcapReader("capture.pcap");
reader.OnReadPacketEvent += (context, packet) =>
{
    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
};
reader.ReadPackets(CancellationToken.None);
```

#### PcapNG file

```csharp
using System.Threading;
using Haukcode.PcapngUtils.PcapNG;

using var reader = new PcapNGReader("capture.pcapng", swapBytes: false);
reader.OnReadPacketEvent += (context, packet) =>
{
    Console.WriteLine($"Packet: {packet.Seconds}.{packet.Microseconds:D6}  ({packet.Data.Length} bytes)");
};
reader.ReadPackets(CancellationToken.None);
```

### Auto-detect file format

`IReaderFactory.GetReader()` inspects the file header and returns the appropriate `PcapReader` or `PcapNGReader` automatically. It accepts either a file path or a `Stream`.

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

### Write packets to Pcap file

```csharp
using System.Threading;
using Haukcode.PcapngUtils;
using Haukcode.PcapngUtils.Pcap;

using var reader = IReaderFactory.GetReader("input.pcap");
using var writer = new PcapWriter("output.pcap");

while (reader.MoreAvailable)
{
    var packet = reader.ReadNextPacket();
    if (packet == null)
        break;

    writer.WritePacket(packet);
}
```

### Write packets to PcapNG file

```csharp
using System.Threading;
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

### Clone a capture file

Using the event-callback approach to clone a file:

```csharp
using System.Threading;
using Haukcode.PcapngUtils;
using Haukcode.PcapngUtils.PcapNG;

using var reader = IReaderFactory.GetReader("input.pcap");
using var writer = new PcapNGWriter("output.pcapng");

reader.OnReadPacketEvent += (context, packet) => writer.WritePacket(packet);
reader.ReadPackets(CancellationToken.None);
```

## API Reference

### `IReaderFactory`

| Method | Description |
|---|---|
| `GetReader(string path)` | Opens a Pcap or PcapNG file from a path; format is auto-detected. |
| `GetReader(Stream stream)` | Opens a Pcap or PcapNG file from a stream; format is auto-detected. |

### `IReader`

| Member | Description |
|---|---|
| `ReadNextPacket()` | Reads and returns the next packet, or `null` at end-of-file. |
| `ReadNextPacket(out PacketMemory)` | Reads the next packet into a reader-owned reusable buffer (no per-packet allocation); returns `false` at end-of-file. The packet's `Data` is only valid until the next read or rewind. |
| `ReadPackets(CancellationToken)` | Reads all packets, raising `OnReadPacketEvent` for each. Blocks until complete or cancelled. |
| `OnReadPacketEvent` | Event raised for each packet read by `ReadPackets()`. |
| `OnExceptionEvent` | Event raised when a non-fatal read error occurs. If not subscribed, the exception is rethrown. |
| `MoreAvailable` | `true` if there are more packets to read. |
| `Position` | Current byte position in the stream (get/set for random access). |
| `Length` | Total length of the stream in bytes. |
| `Rewind()` | Resets the reader to the beginning of the packet data. |

### `IWriter`

| Member | Description |
|---|---|
| `WritePacket(IPacket)` | Writes a packet to the output file. |
| `OnExceptionEvent` | Event raised when a write error occurs. |

### `IPacket`

| Property | Description |
|---|---|
| `Seconds` | Timestamp seconds component. |
| `Microseconds` | Timestamp microseconds component. |
| `Data` | Raw packet bytes. |

For pcapng files, `Seconds`/`Microseconds` are computed using the `if_tsresol` timestamp
resolution declared by the packet's Interface Description Block (both base-10 and base-2
resolutions are supported), falling back to the pcapng default of microseconds when the
option is absent. Sub-microsecond values are rounded half-up to the nearest microsecond.

## License

This project is licensed under the [MIT License](LICENSE).
