using System;
using System.Collections.Generic;
using System.IO;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using System.Diagnostics.Contracts;
using Haukcode.PcapngUtils;
using System.Linq;
using Haukcode.PcapngUtils.Common;
using NUnit.Framework;
using System.Runtime.ExceptionServices;
using Haukcode.PcapngUtils.PcapNG.OptionTypes;

namespace Haukcode.PcapngUtils.PcapNG
{
    [TestFixture]
    public static class PcapNGReader_Test
    {
        /// <summary>
        /// Builds a minimal pcapng stream containing one SHB, one IDB (with the given tsresol),
        /// and one EPB whose raw 64-bit timestamp is supplied as tsHigh:tsLow in the IDB's own
        /// resolution units (not in microseconds).
        /// </summary>
        private static MemoryStream BuildStreamWithTsresol(byte tsresol, uint tsHigh, uint tsLow)
        {
            var ms = new MemoryStream();
            Action<Exception> rethrow = ex => ExceptionDispatchInfo.Capture(ex).Throw();

            // Section Header Block
            var shb = SectionHeaderBlock.GetEmptyHeader(false);
            byte[] shbBytes = shb.ConvertToByte(false, rethrow);
            ms.Write(shbBytes, 0, shbBytes.Length);

            // Interface Description Block with custom if_tsresol
            var idbOptions = new InterfaceDescriptionOption(TimestampResolution: tsresol);
            var idb = new InterfaceDescriptionBlock(LinkTypes.Ethernet, 65535, idbOptions);
            byte[] idbBytes = idb.ConvertToByte(false, rethrow);
            ms.Write(idbBytes, 0, idbBytes.Length);

            // Enhanced Packet Block – timestamp encoded in the IDB's resolution units.
            // Build the EPB body manually so the raw timestamp bytes are not re-scaled.
            byte[] payload = { 1, 2, 3, 4 };
            var body = new List<byte>();
            body.AddRange(BitConverter.GetBytes((int)0));         // InterfaceID = 0
            body.AddRange(BitConverter.GetBytes(tsHigh));         // Timestamp High
            body.AddRange(BitConverter.GetBytes(tsLow));          // Timestamp Low
            body.AddRange(BitConverter.GetBytes(payload.Length)); // Captured Length
            body.AddRange(BitConverter.GetBytes(payload.Length)); // Original Length
            body.AddRange(payload);                                // Packet data
            body.AddRange(new byte[] { 0, 0, 0, 0 });            // Options End

            uint totalLength = (uint)(12 + body.Count);  // type(4) + totLen(4) + body + totLen(4)
            byte[] epbType = BitConverter.GetBytes((uint)BaseBlock.Types.EnhancedPacket);
            byte[] epbTotLen = BitConverter.GetBytes(totalLength);
            ms.Write(epbType, 0, 4);
            ms.Write(epbTotLen, 0, 4);
            ms.Write(body.ToArray(), 0, body.Count);
            ms.Write(epbTotLen, 0, 4);

            ms.Position = 0;
            return ms;
        }

        [Test]
        public static void PcapNgReader_IfTsresol_AffectsEnhancedPacketTimestamp_Test()
        {
            // if_tsresol = 9 → nanosecond resolution (base-10, 10^-9)
            // ts = 1,500,000,000 ns = 1 second + 500,000 microseconds
            using (var ms = BuildStreamWithTsresol(tsresol: 9, tsHigh: 0, tsLow: 1_500_000_000u))
            using (var reader = new PcapNGReader(ms, false))
            {
                var packet = reader.ReadNextPacket();
                Assert.IsNotNull(packet);
                Assert.AreEqual((uint)1, packet.Seconds, "Seconds should be 1 for a 1.5 s nanosecond-resolution timestamp");
                Assert.AreEqual((uint)500_000, packet.Microseconds, "Microseconds should be 500000 for a 1.5 s nanosecond-resolution timestamp");
            }
        }

        [Test]
        public static void PcapNgReader_IfTsresol_AffectsTimestampAfterRewind_Test()
        {
            // Verify that Rewind() preserves the tsresol mapping so packets are still
            // decoded at the correct resolution after rewinding to the start of the stream.
            using (var ms = BuildStreamWithTsresol(tsresol: 9, tsHigh: 0, tsLow: 1_500_000_000u))
            using (var reader = new PcapNGReader(ms, false))
            {
                // First read – consume the packet
                var firstPacket = reader.ReadNextPacket();
                Assert.IsNotNull(firstPacket);

                // Rewind and read again
                reader.Rewind();
                var secondPacket = reader.ReadNextPacket();
                Assert.IsNotNull(secondPacket);
                Assert.AreEqual((uint)1, secondPacket.Seconds, "Seconds should still be 1 after Rewind()");
                Assert.AreEqual((uint)500_000, secondPacket.Microseconds, "Microseconds should still be 500000 after Rewind()");
            }
        }

        [TestCase(50)]
        [TestCase(500)]
        public static void PcapNgReader_IncompletedFileStream_Test(int maxLength)
        {
            byte[] data = { 10, 13, 13, 10, 28, 0, 0, 0, 77, 60, 43, 26, 1, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 28, 0, 0, 0, 1, 0, 0, 0, 32, 0, 0, 0, 1, 0, 0, 0, 255, 255, 0, 0, 9, 0, 1, 0, 6, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 6, 0, 0, 0, 156, 0, 0, 0, 0, 0, 0, 0, 176, 18, 5, 0, 122, 254, 36, 0, 124, 0, 0, 0, 124, 0, 0, 0, 68, 109, 87, 125, 40, 18, 192, 74, 0, 154, 76, 44, 8, 0, 69, 0, 0, 110, 100, 55, 0, 0, 117, 17, 76, 144, 37, 157, 173, 13, 192, 168, 1, 101, 130, 165, 130, 165, 0, 90, 107, 107, 0, 25, 137, 153, 119, 253, 219, 183, 207, 74, 89, 213, 110, 239, 3, 75, 110, 227, 57, 128, 86, 105, 94, 91, 40, 2, 126, 2, 227, 250, 106, 221, 113, 98, 211, 229, 10, 134, 44, 193, 245, 77, 75, 238, 69, 78, 16, 195, 254, 113, 224, 43, 130, 205, 115, 131, 90, 245, 238, 164, 68, 27, 45, 26, 73, 234, 87, 155, 38, 207, 55, 185, 252, 116, 214, 9, 21, 191, 90, 47, 72, 237, 156, 0, 0, 0, 6, 0, 0, 0, 156, 0, 0, 0, 0, 0, 0, 0, 176, 18, 5, 0, 46, 5, 37, 0, 124, 0, 0, 0, 124, 0, 0, 0, 192, 74, 0, 154, 76, 44, 68, 109, 87, 125, 40, 18, 8, 0, 69, 0, 0, 110, 86, 139, 0, 0, 128, 17, 79, 60, 192, 168, 1, 101, 37, 157, 173, 13, 130, 165, 130, 165, 0, 90, 231, 47, 1, 58, 24, 184, 214, 196, 94, 75, 77, 220, 157, 176, 83, 89, 123, 27, 227, 4, 191, 49, 212, 210, 159, 242, 76, 107, 220, 255, 224, 49, 210, 91, 60, 123, 25, 25, 177, 182, 26, 207, 101, 44, 139, 21, 36, 187, 192, 158, 161, 12, 197, 7, 14, 227, 100, 74, 127, 93, 217, 215, 125, 71, 63, 0, 53, 68, 127, 44, 168, 214, 168, 23, 226, 50, 204, 25, 152, 57, 240, 212, 94, 223, 156, 0, 0, 0, 6, 0, 0, 0, 156, 0, 0, 0, 0, 0, 0, 0, 176, 18, 5, 0, 43, 45, 40, 0, 124, 0, 0, 0, 124, 0, 0, 0, 68, 109, 87, 125, 40, 18, 192, 74, 0, 154, 76, 44, 8, 0, 69, 0, 0, 110, 48, 136, 0, 0, 120, 17, 134, 122, 93, 193, 107, 174, 192, 168, 1, 101, 130, 165, 130, 165, 0, 90, 105, 33, 0, 200, 108, 212, 239, 124, 52, 18, 91, 157, 116, 129, 208, 179, 149, 94, 224, 221, 174, 167, 233, 167, 231, 45, 177, 240, 114, 56, 218, 205, 246, 228, 40, 64, 239, 25, 130, 125, 47, 206, 242, 0, 130, 81, 95, 174, 138, 87, 250, 242, 190, 183, 131, 163, 164, 85, 183, 158 };
            data = data.Take(maxLength).ToArray();
            using (var stream = new MemoryStream(data))
            {
                Assert.That(() =>
                {
                    using (var reader = new PcapNGReader(stream, false))
                    {
                        reader.OnReadPacketEvent += (context, packet) =>
                        {
                            IPacket ipacket = packet;
                        };
                        reader.OnExceptionEvent += (sender, exc) =>
                        {
                            ExceptionDispatchInfo.Capture(exc).Throw();
                        };
                        reader.ReadPackets(new System.Threading.CancellationToken());
                        var a = reader.HeadersWithInterfaceDescriptions.Last();
                    }
                }, Throws.TypeOf<EndOfStreamException>());
            }
        }
    }
}
