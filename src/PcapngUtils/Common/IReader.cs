﻿using System;
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
        /// Current file position
        /// </summary>
        long Position { get; set; }

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
