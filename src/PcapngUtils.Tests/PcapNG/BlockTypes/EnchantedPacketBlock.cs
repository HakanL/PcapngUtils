using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Haukcode.PcapngUtils.PcapNG;
using Haukcode.PcapngUtils.PcapNG.CommonTypes;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using Haukcode.PcapngUtils.Extensions;
using System.Diagnostics.Contracts;
using Haukcode.PcapngUtils.PcapNG.OptionTypes;
using NUnit.Framework;
using Haukcode.PcapngUtils.Common;

namespace Haukcode.PcapngUtils.PcapNG.BlockTypes
{
    [TestFixture]
    public static class EnhancedPacketBlock_Test
    {
        [TestCase(true)]
        [TestCase(false)]
        public static void EnhancedPacketBlock_ConvertToByte_Test(bool reorder)
        {
            EnhancedPacketBlock prePacketBlock, postPacketBlock;
            byte[] byteblock = { 6, 0, 0, 0, 132, 0, 0, 0, 0, 0, 0, 0, 12, 191, 4, 0, 118, 176, 176, 8, 98, 0, 0, 0, 98, 0, 0, 0, 0, 0, 94, 0, 1, 177, 0, 33, 40, 5, 41, 186, 8, 0, 69, 0, 0, 84, 48, 167, 0, 0, 255, 1, 3, 72, 192, 168, 177, 160, 10, 64, 11, 49, 8, 0, 10, 251, 67, 168, 0, 0, 79, 161, 27, 41, 0, 2, 83, 141, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 0, 0, 132, 0, 0, 0 };
            using (MemoryStream stream = new MemoryStream(byteblock))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    AbstractBlock block = AbstractBlockFactory.ReadNextBlock(binaryReader, false, null);
                    Assert.IsNotNull(block);
                    prePacketBlock = block as EnhancedPacketBlock;
                    Assert.IsNotNull(prePacketBlock);
                    byteblock = prePacketBlock.ConvertToByte(reorder, null);
                }
            }
            using (MemoryStream stream = new MemoryStream(byteblock))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    AbstractBlock block = AbstractBlockFactory.ReadNextBlock(binaryReader, reorder, null);
                    Assert.IsNotNull(block);
                    postPacketBlock = block as EnhancedPacketBlock;
                    Assert.IsNotNull(postPacketBlock);

                    Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock.BlockType);
                    Assert.AreEqual(prePacketBlock.Data, postPacketBlock.Data);
                    Assert.AreEqual(prePacketBlock.InterfaceID, postPacketBlock.InterfaceID);
                    Assert.AreEqual(prePacketBlock.Microseconds, postPacketBlock.Microseconds);
                    Assert.AreEqual(prePacketBlock.PacketLength, postPacketBlock.PacketLength);
                    Assert.AreEqual(prePacketBlock.PositionInStream, postPacketBlock.PositionInStream);
                    Assert.AreEqual(prePacketBlock.Seconds, postPacketBlock.Seconds);
                    Assert.AreEqual(prePacketBlock.Options.Comments, postPacketBlock.Options.Comments);
                    Assert.AreEqual(prePacketBlock.Options.DropCount, postPacketBlock.Options.DropCount);
                    Assert.AreEqual(prePacketBlock.Options.Hash, postPacketBlock.Options.Hash);
                    Assert.AreEqual(prePacketBlock.Options.PacketFlag, postPacketBlock.Options.PacketFlag);
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public static void EnhancedPacketBlock_ConvertToByteWithTwoComments_Test(bool reorder)
        {
            EnhancedPacketBlock prePacketBlock, postPacketBlock;
            byte[] byteblock = {
                0x06,0x00,0x00,0x00,0x94,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x38,0xE7,0x05,0x00,
                0x78,0xE5,0x9C,0xAF,0x3E,0x00,0x00,0x00,0x3E,0x00,0x00,0x00,0x50,0x6F,0x0C,0x03,
                0xC7,0x07,0xA8,0x47,0x4A,0x6E,0xCC,0xC3,0x08,0x00,0x45,0x00,0x00,0x30,0x9F,0x35,
                0x40,0x00,0x80,0x06,0x00,0x00,0x0A,0x64,0x66,0x0C,0xAC,0x1C,0xC3,0x01,0x70,0x80,
                0x11,0x67,0x53,0x67,0x60,0x3E,0x00,0x00,0x00,0x00,0x70,0x02,0xFA,0xF0,0xDF,0xB0,
                0x00,0x00,0x02,0x04,0x05,0xB4,0x01,0x01,0x04,0x02,0x00,0x00,0x01,0x00,0x11,0x00,
                0x54,0x68,0x65,0x20,0x46,0x69,0x72,0x73,0x74,0x20,0x43,0x6F,0x6D,0x6D,0x65,0x6E,
                0x74,0x00,0x00,0x00,0x01,0x00,0x12,0x00,0x54,0x68,0x65,0x20,0x53,0x65,0x63,0x6F,
                0x6E,0x64,0x20,0x43,0x6F,0x6D,0x6D,0x65,0x6E,0x74,0x00,0x00,0x00,0x00,0x00,0x00,
                0x94,0x00,0x00,0x00,0x05,0x00,0x00,0x00,0x6C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x38,0xE7,0x05,0x00,0x94,0x95,0xA2,0xAF,0x01,0x00,0x1C,0x00,0x43,0x6F,0x75,0x6E,
                0x74,0x65,0x72,0x73,0x20,0x70,0x72,0x6F,0x76,0x69,0x64,0x65,0x64,0x20,0x62,0x79,
                0x20,0x64,0x75,0x6D,0x70,0x63,0x61,0x70,0x02,0x00,0x08,0x00,0x38,0xE7,0x05,0x00,
                0xDD,0xD8,0x96,0xAF,0x03,0x00,0x08,0x00,0x38,0xE7,0x05,0x00,0x94,0x95,0xA2,0xAF,
                0x04,0x00,0x08,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x05,0x00,0x08,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x6C,0x00,0x00,0x00
            };
            using (MemoryStream stream = new MemoryStream(byteblock))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    AbstractBlock block = AbstractBlockFactory.ReadNextBlock(binaryReader, false, null);
                    Assert.IsNotNull(block);
                    prePacketBlock = block as EnhancedPacketBlock;
                    Assert.IsNotNull(prePacketBlock);
                    byteblock = prePacketBlock.ConvertToByte(reorder, null);
                }
            }
            using (MemoryStream stream = new MemoryStream(byteblock))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    AbstractBlock block = AbstractBlockFactory.ReadNextBlock(binaryReader, reorder, null);
                    Assert.IsNotNull(block);
                    postPacketBlock = block as EnhancedPacketBlock;
                    Assert.IsNotNull(postPacketBlock);

                    Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock.BlockType);
                    Assert.AreEqual(prePacketBlock.Data, postPacketBlock.Data);
                    Assert.AreEqual(prePacketBlock.InterfaceID, postPacketBlock.InterfaceID);
                    Assert.AreEqual(prePacketBlock.Microseconds, postPacketBlock.Microseconds);
                    Assert.AreEqual(prePacketBlock.PacketLength, postPacketBlock.PacketLength);
                    Assert.AreEqual(prePacketBlock.PositionInStream, postPacketBlock.PositionInStream);
                    Assert.AreEqual(prePacketBlock.Seconds, postPacketBlock.Seconds);
                    Assert.AreEqual(prePacketBlock.Options.Comments, postPacketBlock.Options.Comments);
                    Assert.AreEqual(prePacketBlock.Options.Comments.Count, 2);
                    Assert.AreEqual(prePacketBlock.Options.Comments[0], "The First Comment");
                    Assert.AreEqual(prePacketBlock.Options.Comments[1], "The Second Comment");
                    Assert.AreEqual(prePacketBlock.Options.DropCount, postPacketBlock.Options.DropCount);
                    Assert.AreEqual(prePacketBlock.Options.Hash, postPacketBlock.Options.Hash);
                    Assert.AreEqual(prePacketBlock.Options.PacketFlag, postPacketBlock.Options.PacketFlag);
                }
            }
        }
    }
}
