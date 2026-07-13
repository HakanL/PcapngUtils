using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haukcode.PcapngUtils.Extensions;
using System.Diagnostics.Contracts;

namespace Haukcode.PcapngUtils.PcapNG.CommonTypes
{
    public sealed class TimestampHelper
    {
        #region fields && properties
        private static readonly DateTime epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public uint TimestampHigh
        {
            get;
            private set;
        }

        public uint TimestampLow
        {
            get;
            private set;
        }
        public uint Seconds
        {
            get;
            private set;
        }

        public uint Microseconds
        {
            get;
            private set;
        }

        #endregion

        #region ctor
        public TimestampHelper(byte[] timestampAsByte, bool reverseByteOrder) : this(timestampAsByte, reverseByteOrder, (byte)6)
        {
        }

        /// <summary>
        /// Constructs a TimestampHelper using the pcapng if_tsresol encoding.
        /// </summary>
        /// <param name="timestampAsByte">8-byte raw timestamp from the block.</param>
        /// <param name="reverseByteOrder">Whether to reverse byte order when reading the timestamp words.</param>
        /// <param name="tsresol">The raw if_tsresol byte value from the Interface Description Block.
        /// Bit 7 selects the base: 0 = base-10 (10^-n), 1 = base-2 (2^-n). Bits 0..6 are the exponent n.
        /// The default pcapng resolution is 6 (microseconds, i.e. 10^-6).</param>
        public TimestampHelper(byte[] timestampAsByte, bool reverseByteOrder, byte tsresol)
        {
            CustomContract.Requires<ArgumentNullException>(timestampAsByte != null, "timestampAsByte cannot be null");
            CustomContract.Requires<ArgumentException>(timestampAsByte.Length == 8, "timestamp must have length = 8");

            TimestampHigh = (BitConverter.ToUInt32(timestampAsByte.Take(4).ToArray(), 0)).ReverseByteOrder(reverseByteOrder);
            TimestampLow = (BitConverter.ToUInt32(timestampAsByte.Skip(4).Take(4).ToArray(), 0)).ReverseByteOrder(reverseByteOrder);

            ulong ts = ((ulong)TimestampHigh << 32) | TimestampLow;
            bool isPwr2 = (tsresol & 0b10000000) != 0;
            int exponent = tsresol & 0b01111111;

            if (isPwr2)
            {
                CustomContract.Requires<ArgumentOutOfRangeException>(exponent <= 95, "base-2 tsresol exponent must be <= 95 to prevent overflow when converting timestamps to microseconds");
            }
            else
            {
                CustomContract.Requires<ArgumentOutOfRangeException>(exponent <= 28, "base-10 tsresol exponent must be <= 28 to prevent overflow when converting timestamps to microseconds");
            }

            // Iterative decimal multiplication is intentional: Math.Pow returns double which
            // loses precision for large exponents, defeating the purpose of decimal arithmetic.
            decimal scale = isPwr2 ? Pow2(exponent) : Pow10(exponent);
            decimal totalMicrosDecimal = decimal.Round(((decimal)ts * 1_000_000m) / scale, MidpointRounding.AwayFromZero);

            CustomContract.Requires<OverflowException>(totalMicrosDecimal >= 0 && totalMicrosDecimal <= (decimal)long.MaxValue, "Timestamp value is out of range for microsecond representation");

long totalMicros = (long)totalMicrosDecimal;

long seconds = totalMicros / 1_000_000;
CustomContract.Requires<OverflowException>(seconds >= 0 && seconds <= uint.MaxValue, "Timestamp value is out of range for Seconds/Microseconds representation");

Seconds = (uint)seconds;
Microseconds = (uint)(totalMicros % 1_000_000);
        }

        public TimestampHelper(uint seconds, uint microseconds)
        {
            Seconds = seconds;
            Microseconds = microseconds;

            long timestamp = seconds * 1000000 + microseconds;
            TimestampHigh = (uint)(timestamp / 4294967296);
            TimestampLow = (uint)(timestamp % 4294967296);
        }
        #endregion

        #region method
        public DateTime ToUtc()
        {
            long ticks = (Microseconds * (TimeSpan.TicksPerMillisecond / 1000)) +
                         (Seconds * TimeSpan.TicksPerSecond);
            return epochDateTime.AddTicks(ticks);
        }

        public byte[] ConvertToByte(bool reverseByteOrder)
        {
            long timestamp = ((long)Seconds * 1000000) + Microseconds;
            uint timestampHigh = (uint)(timestamp / 4294967296);
            uint timestampLow = (uint)(timestamp % 4294967296);

            var ret = new List<byte>(8);
            ret.AddRange(BitConverter.GetBytes(timestampHigh.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(timestampLow.ReverseByteOrder(reverseByteOrder)));

            return ret.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (TimestampHelper)obj;
            return (Seconds == p.Seconds) && (Microseconds == p.Microseconds);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static decimal Pow10(int exponent)
        {
            decimal result = 1m;
            for (int i = 0; i < exponent; i++)
                result *= 10m;
            return result;
        }

        private static decimal Pow2(int exponent)
        {
            decimal result = 1m;
            for (int i = 0; i < exponent; i++)
                result *= 2m;
            return result;
        }
        #endregion
    }
}
