using System;

namespace Haukcode.PcapngUtils.Common
{
    /// <summary>
    /// A packet returned by the buffer-reusing read API (see IReader.ReadNextPacket(out PacketMemory)).
    /// Data may be backed by a buffer owned by the reader that produced it, and is then only valid
    /// until the next read or rewind on that reader — copy the bytes if they must live longer.
    /// </summary>
    public readonly struct PacketMemory
    {
        public uint Seconds { get; }

        public uint Microseconds { get; }

        /// <summary>
        /// Packet data. Only valid until the next read or rewind on the reader that produced it.
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Packet position in the stream
        /// </summary>
        public long PositionInStream { get; }

        public PacketMemory(uint seconds, uint microseconds, ReadOnlyMemory<byte> data, long positionInStream)
        {
            Seconds = seconds;
            Microseconds = microseconds;
            Data = data;
            PositionInStream = positionInStream;
        }
    }
}
