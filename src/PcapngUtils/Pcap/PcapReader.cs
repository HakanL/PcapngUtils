using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.Extensions;

namespace Haukcode.PcapngUtils.Pcap
{
    public sealed class PcapReader : Disposable, IReader
    {
        public event CommonDelegates.ExceptionEventDelegate OnExceptionEvent;

        private void OnException(Exception exception)
        {
            CustomContract.Requires<ArgumentNullException>(exception != null, "exception cannot be null or empty");
            CommonDelegates.ExceptionEventDelegate handler = OnExceptionEvent;
            if (handler != null)
                handler(this, exception);
            else
                ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public event CommonDelegates.ReadPacketEventDelegate OnReadPacketEvent;

        private void OnReadPacket(IPacket packet)
        {
            CustomContract.Requires<ArgumentNullException>(Header != null, "Header cannot be null");
            CustomContract.Requires<ArgumentNullException>(packet != null, "packet cannot be null");
            OnReadPacketEvent?.Invoke(Header, packet);
        }

        private Stream stream;
        private BinaryReader binaryReader;
        private readonly object syncRoot = new object();
        private long startPosition = 0;

        public SectionHeader Header { get; private set; }

        public long Position
        {
            get => this.binaryReader.BaseStream.Position;
            set => this.binaryReader.BaseStream.Position = value;
        }
        
        public long Length => this.binaryReader.BaseStream.Length;

        public bool MoreAvailable => this.binaryReader.BaseStream.Position < this.binaryReader.BaseStream.Length;

        public PcapReader(string path)
        {
            CustomContract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), "path cannot be null or empty");
            CustomContract.Requires<ArgumentException>(File.Exists(path), "file must exists");

            // 256 KB buffer with SequentialScan instead of the 4 KB default: playback reads the
            // capture front-to-back, so fewer, larger reads cut syscalls on the hot read path.
            Initialize(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.SequentialScan));
        }

        public PcapReader(Stream s)
        {
            CustomContract.Requires<ArgumentNullException>(s != null, "stream cannot be null");

            Initialize(s);
        }

        private void Initialize(Stream stream)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");
            CustomContract.Requires<Exception>(stream.CanRead == true, "cannot read stream");

            this.stream = stream;
            this.binaryReader = new BinaryReader(stream);
            Header = SectionHeader.Parse(this.binaryReader);
            this.startPosition = this.binaryReader.BaseStream.Position;
            Rewind();
        }

        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Read all packet from a stream. After read any packet event OnReadPacketEvent is called.
        /// Function is NOT asynchronous! (blocking thread). If you want abort it, use CancellationToken
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void ReadPackets(System.Threading.CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var packet = ReadNextPacket();

                    if (packet == null)
                    {
                        return;
                    }
                    OnReadPacket(packet);
                }
                catch (Exception exc)
                {
                    OnException(exc);
                }
            }
        }

        /// <inheritdoc/>
        public IPacket ReadNextPacket()
        {
            if (!ReadNextPacketCore(reuseBuffer: false, out uint secs, out uint usecs, out ArraySegment<byte> data, out long position))
                return null;

            return new PcapPacket(secs, usecs, data.Array, position);
        }

        /// <inheritdoc/>
        public bool ReadNextPacket(out PacketMemory packet)
        {
            if (!ReadNextPacketCore(reuseBuffer: true, out uint secs, out uint usecs, out ArraySegment<byte> data, out long position))
            {
                packet = default;
                return false;
            }

            packet = new PacketMemory(secs, usecs, new ReadOnlyMemory<byte>(data.Array, data.Offset, data.Count), position);
            return true;
        }

        // Grow-only scratch buffer for the allocation-free read path; sized so typical
        // Ethernet frames fit without growing.
        private byte[] reusableBuffer = new byte[2048];

        private bool ReadNextPacketCore(bool reuseBuffer, out uint seconds, out uint microseconds, out ArraySegment<byte> data, out long position)
        {
            if (this.binaryReader.BaseStream.Position >= this.binaryReader.BaseStream.Length)
            {
                seconds = 0;
                microseconds = 0;
                data = default;
                position = 0;

                return false;
            }

            lock (this.syncRoot)
            {
                position = this.binaryReader.BaseStream.Position;
                seconds = this.binaryReader.ReadUInt32().ReverseByteOrder(Header.ReverseByteOrder);
                microseconds = this.binaryReader.ReadUInt32().ReverseByteOrder(Header.ReverseByteOrder);
                if (Header.NanoSecondResolution)
                    microseconds = microseconds / 1000;
                uint caplen = this.binaryReader.ReadUInt32().ReverseByteOrder(Header.ReverseByteOrder);
                uint len = this.binaryReader.ReadUInt32().ReverseByteOrder(Header.ReverseByteOrder);

                byte[] buffer;
                if (reuseBuffer)
                {
                    if (this.reusableBuffer.Length < caplen)
                        this.reusableBuffer = new byte[Math.Max((int)caplen, this.reusableBuffer.Length * 2)];
                    buffer = this.reusableBuffer;
                }
                else
                {
                    buffer = new byte[caplen];
                }

                int totalRead = 0;
                while (totalRead < (int)caplen)
                {
                    int read = this.binaryReader.Read(buffer, totalRead, (int)caplen - totalRead);
                    if (read <= 0)
                        throw new EndOfStreamException("Unable to read beyond the end of the stream");
                    totalRead += read;
                }

                data = new ArraySegment<byte>(buffer, 0, (int)caplen);
            }

            return true;
        }

        /// <summary>
        /// rewind to the beginning of the stream
        /// </summary>
        public void Rewind()
        {
            CustomContract.Requires<ArgumentNullException>(Header != null, "Header cannot be null");
            lock (this.syncRoot)
            {
                this.binaryReader.BaseStream.Position = this.startPosition;
            }
        }

        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.binaryReader != null)
                this.binaryReader.Close();
            if (this.stream != null)
                this.stream.Close();
        }
    }
}
