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
using Haukcode.PcapngUtils.PcapNG.CommonTypes;

namespace Haukcode.PcapngUtils.PcapNG
{
    [TestFixture]
    public static class TimestampHelper_Test
    {
        [Test]
        public static void TimestampHelper_Base10_MicrosecondResolution_Test()
        {
            // ts = 1,500,000 (1.5 seconds when tsresol = 6)
            ulong ts = 1500000;
            byte[] timestampBytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, timestampBytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, timestampBytes, 4, 4);
            TimestampHelper helper = new TimestampHelper(timestampBytes, false, 6);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Base2_Resolution_Test()
        {
            ulong ts = 96; // 96 * 2^-6 = 1.5 sec
            byte[] timestampBytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, timestampBytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, timestampBytes, 4, 4);
            TimestampHelper helper = new TimestampHelper(timestampBytes, false, 0x86);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_NanosecondResolution_Test()
        {
            ulong ts = 1500000000; // 1.5 sec in ns
            byte[] timestampBytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, timestampBytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, timestampBytes, 4, 4);
            TimestampHelper helper = new TimestampHelper(timestampBytes, false, 9);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Rounding_Test()
        {
            ulong ts = 500; // 500 ns
            byte[] timestampBytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, timestampBytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, timestampBytes, 4, 4);
            TimestampHelper helper = new TimestampHelper(timestampBytes, false, 9);
            Assert.AreEqual((uint)0, helper.Seconds);
            Assert.AreEqual((uint)1, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_RoundingCarry_Test()
        {
            // 999,999.5 microseconds
            // should round to exactly 1,000,000 microseconds
            ulong ts = 999999500; // ns
            byte[] timestampBytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, timestampBytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, timestampBytes, 4, 4);
            TimestampHelper helper = new TimestampHelper(timestampBytes, false, 9);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)0, helper.Microseconds);
        }
    }
}
