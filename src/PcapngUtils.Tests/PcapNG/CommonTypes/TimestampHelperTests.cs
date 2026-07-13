using System;
using NUnit.Framework;
using Haukcode.PcapngUtils.PcapNG.CommonTypes;

namespace Haukcode.PcapngUtils.PcapNG.CommonTypes
{
    [TestFixture]
    public static class TimestampHelperTests
    {
        private static byte[] MakeTimestampBytes(ulong ts)
        {
            byte[] bytes = new byte[8];
            Array.Copy(BitConverter.GetBytes((uint)(ts >> 32)), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes((uint)ts), 0, bytes, 4, 4);
            return bytes;
        }

        [Test]
        public static void TimestampHelper_Base10_MicrosecondResolution_Test()
        {
            // ts = 1,500,000 units at 10^-6 s/unit = 1.5 seconds
            ulong ts = 1_500_000;
            var helper = new TimestampHelper(MakeTimestampBytes(ts), false, 6);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500_000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Base10_NanosecondResolution_Test()
        {
            // ts = 1,500,000,000 units at 10^-9 s/unit = 1.5 seconds
            ulong ts = 1_500_000_000;
            var helper = new TimestampHelper(MakeTimestampBytes(ts), false, 9);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500_000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Base2_Resolution_Test()
        {
            // tsresol = 0x86 = base-2 exponent 6 => 2^-6 s/unit = 1/64 s/unit
            // ts = 96 units => 96 / 64 = 1.5 seconds
            ulong ts = 96;
            var helper = new TimestampHelper(MakeTimestampBytes(ts), false, 0x86);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)500_000, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Rounding_HalfUp_Test()
        {
            // 500 ns -> 0.5 µs -> rounds to 1 µs
            ulong ts = 500;
            var helper = new TimestampHelper(MakeTimestampBytes(ts), false, 9);
            Assert.AreEqual((uint)0, helper.Seconds);
            Assert.AreEqual((uint)1, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_Rounding_Carry_Test()
        {
            // 999,999,500 ns = 999,999.5 µs -> rounds to 1,000,000 µs = 1 second
            ulong ts = 999_999_500;
            var helper = new TimestampHelper(MakeTimestampBytes(ts), false, 9);
            Assert.AreEqual((uint)1, helper.Seconds);
            Assert.AreEqual((uint)0, helper.Microseconds);
        }

        [Test]
        public static void TimestampHelper_DefaultResolution_BackwardCompat_Test()
        {
            // The no-tsresol constructor should default to tsresol=6 (microseconds)
            ulong ts = 2_000_000;
            byte[] bytes = MakeTimestampBytes(ts);
            var helperDefault = new TimestampHelper(bytes, false);
            var helperExplicit = new TimestampHelper(bytes, false, 6);
            Assert.AreEqual(helperExplicit.Seconds, helperDefault.Seconds);
            Assert.AreEqual(helperExplicit.Microseconds, helperDefault.Microseconds);
        }
    }
}
