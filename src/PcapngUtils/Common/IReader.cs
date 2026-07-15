using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haukcode.PcapngUtils.Common
{
    public interface IReader : IDisposable
    {
        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        void Close();
        event CommonDelegates.ExceptionEventDelegate OnExceptionEvent;
        event CommonDelegates.ReadPacketEventDelegate OnReadPacketEvent;

        /// <summary>
        /// Read all packet from a stream. After read any packet event OnReadPacketEvent is called.
        /// Function is NOT asynchronous! (blocking thread). If you want abort it, use CancellationToken
        /// </summary>
        /// <param name="cancellationToken"></param>
        void ReadPackets(System.Threading.CancellationToken cancellationToken);

        /// <summary>
        /// Read one packet
        /// </summary>
        /// <returns>Next packet, or null at EOF</returns>
        IPacket ReadNextPacket();

        /// <summary>
        /// Read one packet into a reader-owned reusable buffer, avoiding the per-packet
        /// allocation of ReadNextPacket(). The packet's Data is only valid until the next
        /// read or rewind on this reader — copy the bytes if they must live longer. Because
        /// the buffer is shared, interleaving reads from multiple threads is not supported
        /// with this overload.
        /// </summary>
        /// <param name="packet">The packet that was read; valid until the next read or rewind</param>
        /// <returns>false at EOF</returns>
        bool ReadNextPacket(out PacketMemory packet);

        /// <summary>
        /// Current file position
        /// </summary>
        long Position { get; set; }

        /// <summary>
        /// Length of file
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Rewind to read from the beginning again
        /// </summary>
        void Rewind();

        /// <summary>
        /// More data is available
        /// </summary>
        bool MoreAvailable { get; }
    }
}
