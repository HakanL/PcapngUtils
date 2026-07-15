using System;
using System.IO;
using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.PcapNG;
using NUnit.Framework;

namespace Haukcode.PcapngUtils.Pcap
{
    [TestFixture]
    public static class PcapReaderBufferedRead_Test
    {
        /// <summary>
        /// Builds a minimal little-endian microsecond pcap file (Ethernet link type) with two
        /// 4-byte packets of distinct content.
        /// </summary>
        private static byte[] BuildTwoPacketFile()
        {
            return new byte[]
            {
                // Global header
                0xd4, 0xc3, 0xb2, 0xa1, // magic (microsecond, little-endian)
                0x02, 0x00, 0x04, 0x00, // version 2.4
                0x00, 0x00, 0x00, 0x00, // thiszone
                0x00, 0x00, 0x00, 0x00, // sigfigs
                0xff, 0xff, 0x00, 0x00, // snaplen 65535
                0x01, 0x00, 0x00, 0x00, // LINKTYPE_ETHERNET
                // Packet 1: ts 1.000002, caplen 4, len 4
                0x01, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x01, 0x02, 0x03, 0x04,
                // Packet 2: ts 3.000004, caplen 4, len 4
                0x03, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,
                0x0a, 0x0b, 0x0c, 0x0d,
            };
        }

        [Test]
        public static void PcapReader_BufferedRead_MatchesAllocatingRead_Test()
        {
            byte[] file = BuildTwoPacketFile();

            using (var allocatingReader = new PcapReader(new MemoryStream(file)))
            using (var bufferedReader = new PcapReader(new MemoryStream(file)))
            {
                int packetCount = 0;
                while (true)
                {
                    IPacket expected = allocatingReader.ReadNextPacket();
                    bool more = bufferedReader.ReadNextPacket(out PacketMemory actual);

                    if (expected == null)
                    {
                        Assert.That(more, Is.False, "buffered read should hit EOF together with the allocating read");
                        break;
                    }

                    Assert.That(more, Is.True);
                    Assert.That(actual.Seconds, Is.EqualTo(expected.Seconds));
                    Assert.That(actual.Microseconds, Is.EqualTo(expected.Microseconds));
                    Assert.That(actual.Data.ToArray(), Is.EqualTo(expected.Data));
                    packetCount++;
                }

                Assert.That(packetCount, Is.EqualTo(2));
            }
        }

        [Test]
        public static void PcapReader_BufferedRead_ReusesBuffer_Test()
        {
            byte[] file = BuildTwoPacketFile();

            using (var reader = new PcapReader(new MemoryStream(file)))
            {
                Assert.That(reader.ReadNextPacket(out PacketMemory first), Is.True);
                ReadOnlyMemory<byte> firstData = first.Data;
                byte[] firstCopy = firstData.ToArray();

                Assert.That(reader.ReadNextPacket(out PacketMemory second), Is.True);

                // The documented contract: a packet's Data is only valid until the next read.
                // Both packets are the same size, so the first packet's memory now exposes the
                // second packet's bytes from the shared scratch buffer.
                Assert.That(firstData.ToArray(), Is.EqualTo(second.Data.ToArray()));
                Assert.That(firstCopy, Is.Not.EqualTo(second.Data.ToArray()));
            }
        }

        [Test]
        public static void PcapReader_BufferedRead_RewindRereadsSamePackets_Test()
        {
            byte[] file = BuildTwoPacketFile();

            using (var reader = new PcapReader(new MemoryStream(file)))
            {
                Assert.That(reader.ReadNextPacket(out PacketMemory first), Is.True);
                byte[] firstCopy = first.Data.ToArray();

                reader.Rewind();

                Assert.That(reader.ReadNextPacket(out PacketMemory reread), Is.True);
                Assert.That(reread.Data.ToArray(), Is.EqualTo(firstCopy));
            }
        }

        [Test]
        public static void PcapNGReader_BufferedRead_MatchesAllocatingRead_Test()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "single_packet.pcapng");

            using (var allocatingReader = new PcapNGReader(testFile, false))
            using (var bufferedReader = new PcapNGReader(testFile, false))
            {
                int packetCount = 0;
                while (true)
                {
                    IPacket expected = allocatingReader.ReadNextPacket();
                    bool more = bufferedReader.ReadNextPacket(out PacketMemory actual);

                    if (expected == null)
                    {
                        Assert.That(more, Is.False, "buffered read should hit EOF together with the allocating read");
                        break;
                    }

                    Assert.That(more, Is.True);
                    Assert.That(actual.Seconds, Is.EqualTo(expected.Seconds));
                    Assert.That(actual.Microseconds, Is.EqualTo(expected.Microseconds));
                    Assert.That(actual.Data.ToArray(), Is.EqualTo(expected.Data));
                    packetCount++;
                }

                Assert.That(packetCount, Is.GreaterThan(0));
            }
        }
    }
}
