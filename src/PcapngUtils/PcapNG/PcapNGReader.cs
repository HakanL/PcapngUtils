using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.Extensions;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;

namespace Haukcode.PcapngUtils.PcapNG
{
    public sealed class PcapNGReader : Disposable, IReader
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
            CustomContract.Requires<ArgumentNullException>(packet != null, "packet cannot be null");
            OnReadPacketEvent?.Invoke(this.headersWithInterface.Last(), packet);
        }

        private BinaryReader binaryReader;
        private Stream stream;
        private long basePosition = 0;
        private bool ReverseByteOrder = false;
        private int interfaceId = 0;
        private int initialInterfaceId = 0;
        private readonly Dictionary<int, byte> tsresols = new Dictionary<int, byte>();
        private readonly Dictionary<int, byte> initialTsresols = new Dictionary<int, byte>();

        private List<HeaderWithInterfacesDescriptions> headersWithInterface = new List<HeaderWithInterfacesDescriptions>();

        public IList<HeaderWithInterfacesDescriptions> HeadersWithInterfaceDescriptions
        {
            get { return this.headersWithInterface.AsReadOnly(); }
        }

        public long Position
        {
            get => this.binaryReader.BaseStream.Position;
            set => this.binaryReader.BaseStream.Position = value;
        }

        public long Length => this.binaryReader.BaseStream.Length;

        public bool MoreAvailable => this.binaryReader.BaseStream.Position < this.binaryReader.BaseStream.Length;

        private readonly object syncRoot = new object();

        public PcapNGReader(string path, bool swapBytes)
        {
            CustomContract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), "path cannot be null or empty");
            CustomContract.Requires<ArgumentException>(File.Exists(path), "file must exists");

            Initialize(File.OpenRead(path), swapBytes);
        }

        public PcapNGReader(Stream stream, bool reverseByteOrder)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");

            Initialize(stream, reverseByteOrder);
        }

        private void Initialize(Stream stream, bool reverseByteOrder)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");
            CustomContract.Requires<Exception>(stream.CanRead == true, "cannot read stream");
            Action<Exception> ReThrowException = (exc) =>
            {
                ExceptionDispatchInfo.Capture(exc).Throw();
            };
            this.ReverseByteOrder = reverseByteOrder;
            this.stream = stream;
            this.binaryReader = new BinaryReader(stream);
            var preHeadersWithInterface = new List<KeyValuePair<SectionHeaderBlock, List<InterfaceDescriptionBlock>>>();
            while (this.binaryReader.BaseStream.Position < this.binaryReader.BaseStream.Length && this.basePosition == 0)
            {
                AbstractBlock block = AbstractBlockFactory.ReadNextBlock(binaryReader, this.ReverseByteOrder, ReThrowException, tsresols);
                if (block == null)
                    break;

                switch (block.BlockType)
                {
                    case BaseBlock.Types.SectionHeader:
                        tsresols.Clear();
                        interfaceId = 0;
                        if (block is SectionHeaderBlock headerBlock)
                        {
                            preHeadersWithInterface.Add(new KeyValuePair<SectionHeaderBlock, List<InterfaceDescriptionBlock>>(headerBlock, new List<InterfaceDescriptionBlock>()));
                        }
                        break;

                    case BaseBlock.Types.InterfaceDescription:
                        if (block is InterfaceDescriptionBlock idb)
                        {
                            RegisterInterface(idb);
                            if (preHeadersWithInterface.Any())
                            {
                                preHeadersWithInterface.Last().Value.Add(idb);
                            }
                            else
                            {
                                throw new Exception(string.Format("[PcapNgReader.Initialize] stream must contains SectionHeaderBlock before any InterfaceDescriptionBlock"));
                            }
                        }
                        break;

                    default:
                        this.basePosition = block.PositionInStream;
                        break;
                }
            }
            if (this.basePosition <= 0)
                this.basePosition = this.binaryReader.BaseStream.Position;

            if (!preHeadersWithInterface.Any())
                throw new ArgumentException(string.Format("[PcapNgReader.Initialize] Stream don't contains any SectionHeaderBlock"));

            if (!(from item in preHeadersWithInterface where (item.Value.Any()) select item).Any())
                throw new ArgumentException(string.Format("[PcapNgReader.Initialize] Stream don't contains any InterfaceDescriptionBlock"));

            this.headersWithInterface = (from item in preHeadersWithInterface
                                         where (item.Value.Any())
                                         select item)
                                                .Select(x => new HeaderWithInterfacesDescriptions(x.Key, x.Value))
                                                .ToList();

            initialTsresols.Clear();
            foreach (KeyValuePair<int, byte> item in tsresols)
            {
                initialTsresols[item.Key] = item.Value;
            }
            initialInterfaceId = interfaceId;

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
        /// rewind to the beginning of the stream
        /// </summary>
        public void Rewind()
        {
            lock (this.syncRoot)
            {
                this.binaryReader.BaseStream.Position = this.basePosition;
                tsresols.Clear();
                foreach (KeyValuePair<int, byte> item in initialTsresols)
                {
                    tsresols[item.Key] = item.Value;
                }
                interfaceId = initialInterfaceId;
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

        public void ReadPackets(System.Threading.CancellationToken cancellationToken)
        {
            long prevPosition = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    prevPosition = this.binaryReader.BaseStream.Position;

                    var packet = ReadNextPacket();
                    if (packet == null)
                        // EOF
                        return;

                    OnReadPacket(packet);
                }
                catch (Exception exc)
                {
                    OnException(exc);

                    lock (this.syncRoot)
                    {
                        if (prevPosition == this.binaryReader.BaseStream.Position)
                            // Nothing more to read
                            break;
                    }
                    continue;
                }
            }
        }

        public IPacket ReadNextPacket()
        {
            AbstractBlock block;
            long prevPosition = 0;
            while (this.binaryReader.BaseStream.Position < this.binaryReader.BaseStream.Length)
            {
                IPacket result = null;
                lock (this.syncRoot)
                {
                    prevPosition = this.binaryReader.BaseStream.Position;
                    block = AbstractBlockFactory.ReadNextBlock(this.binaryReader, this.ReverseByteOrder, OnException, tsresols);

                    if (block == null)
                    {
                        throw new Exception($"[ReadPackets] AbstractBlockFactory cannot read packet on position {prevPosition}");
                    }

                    switch (block.BlockType)
                    {
                        case BaseBlock.Types.SectionHeader:
                            tsresols.Clear();
                            interfaceId = 0;
                            break;

                        case BaseBlock.Types.InterfaceDescription:
                            if (block is InterfaceDescriptionBlock idb)
                                RegisterInterface(idb);
                            break;

                        case BaseBlock.Types.EnhancedPacket:
                            if (!(block is EnhancedPacketBlock enhancedBlock))
                                throw new Exception($"[ReadPackets] system cannot cast block to EnhancedPacketBlock. Block start on position: {prevPosition}.");
                            result = enhancedBlock;
                            break;

                        case BaseBlock.Types.Packet:
                            if (!(block is PacketBlock packetBlock))
                                throw new Exception($"[ReadPackets] system cannot cast block to PacketBlock. Block start on position: {prevPosition}.");
                            result = packetBlock;
                            break;

                        case BaseBlock.Types.SimplePacket:
                            if (!(block is SimplePacketBlock simpleBlock))
                                throw new Exception($"[ReadPackets] system cannot cast block to SimplePacketBlock. Block start on position: {prevPosition}.");
                            result = simpleBlock;
                            break;

                        default:
                            break;
                    }
                }

                if (result != null)
                    return result;
            }

            return null;
        }

        private void RegisterInterface(InterfaceDescriptionBlock idb)
        {
            tsresols[interfaceId] = idb.Options.TimestampResolution ?? 6;
            interfaceId++;
        }
    }
}

