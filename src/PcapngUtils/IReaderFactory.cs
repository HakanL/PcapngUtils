using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.Extensions;
using Haukcode.PcapngUtils.Pcap;
using Haukcode.PcapngUtils.PcapNG;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haukcode.PcapngUtils
{
    public sealed class IReaderFactory
    {
        #region fields && properties

        #endregion

        #region methods
        public static IReader GetReader(string path)
        {
            CustomContract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), $"{nameof(path)} cannot be null or empty");
            CustomContract.Requires<ArgumentException>(File.Exists(path), $"file for {nameof(path)} must exist");

            var stream = File.OpenRead(path);

            try
            {
                return GetReader(stream);
            }
            catch (ArgumentException ex)
            {
                stream.Dispose();
                throw new ArgumentException($"[IReaderFactory.GetReader] unable to open IReader from file {path}", ex);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public static IReader GetReader(Stream stream)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, $"{nameof(stream)} cannot be null");

            UInt32 mask = 0;
            using (var binaryReader = new BinaryReader(stream, encoding: Encoding.Default, leaveOpen: true))
            {
                if (binaryReader.BaseStream.Length < 12)
                    throw new ArgumentException("[IReaderFactory.GetHeaderMask] stream is too short");

                mask = binaryReader.ReadUInt32();
                if (mask == (uint)BaseBlock.Types.SectionHeader)
                {
                    binaryReader.ReadUInt32();
                    mask = binaryReader.ReadUInt32();
                }
            }

            stream.Position = 0;

            switch (mask)
            {
                case (uint)Pcap.SectionHeader.MagicNumbers.microsecondIdentical:
                case (uint)Pcap.SectionHeader.MagicNumbers.microsecondSwapped:
                case (uint)Pcap.SectionHeader.MagicNumbers.nanosecondSwapped:
                case (uint)Pcap.SectionHeader.MagicNumbers.nanosecondIdentical:
                    {
                        IReader reader = new PcapReader(stream);
                        return reader;
                    }
                case (uint)PcapNG.BlockTypes.SectionHeaderBlock.MagicNumbers.Identical:
                    {
                        IReader reader = new PcapNGReader(stream, false);
                        return reader;
                    }
                case (uint)PcapNG.BlockTypes.SectionHeaderBlock.MagicNumbers.Swapped:
                    {
                        IReader reader = new PcapNGReader(stream, true);
                        return reader;
                    }
                default:
                    throw new ArgumentException("[IReaderFactory.GetReader] stream is not PCAP/PCAPNG file");
            }
        }

        #endregion
    }
}
