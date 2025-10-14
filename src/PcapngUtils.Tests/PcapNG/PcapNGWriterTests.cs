using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.PcapNG;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using NUnit.Framework;

namespace Haukcode.PcapngUtils.PcapNG
{
    [TestFixture]
    public static class PcapNGWriter_Test
    {
        [Test]
        public static void PcapNgWriter_Close_StreamClosedBug_Test()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "single_packet.pcapng");
            Assert.That(File.Exists(testFile), $"Test file not found: {testFile}");

            var tempFile = Path.GetTempFileName();
            try
            {
                // Read packets from the test file
                var packets = new List<IPacket>();
                List<HeaderWithInterfacesDescriptions> headers = null;
                using (var input = File.OpenRead(testFile))
                using (var reader = new PcapNGReader(input, false))
                {
                    reader.OnReadPacketEvent += (context, packet) => packets.Add(packet);
                    reader.OnExceptionEvent += (sender, exc) => ExceptionDispatchInfo.Capture(exc).Throw();
                    reader.ReadPackets(new System.Threading.CancellationToken());
                    headers = reader.HeadersWithInterfaceDescriptions.ToList();
                }

                // Write packets to a temp file
                using (var output = File.Open(tempFile, FileMode.Create, FileAccess.Write))
                using (var writer = new PcapNGWriter(output, headers))
                {
                    foreach (var packet in packets)
                    {
                        writer.WritePacket(packet);
                    }
                    // Call Close twice, should not throw
                    writer.Close();
                    writer.Close();
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
