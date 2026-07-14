using System;
using System.Collections.Generic;
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

        // Largest total-microseconds value that still fits the uint Seconds / uint Microseconds representation.
        private const decimal MaxTotalMicroseconds = (decimal)uint.MaxValue * 1_000_000m + 999_999m;

        // Units-per-second scale factor for every possible if_tsresol value, precomputed once so the
        // per-packet constructor does no repeated multiplication. Entries left at 0 have scale factors
        // that are not representable as decimal (base-10 exponent > 28, base-2 exponent > 95) and are
        // rejected when parsing.
        private static readonly decimal[] unitsPerSecond = BuildUnitsPerSecondTable();

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

            TimestampHigh = BitConverter.ToUInt32(timestampAsByte, 0).ReverseByteOrder(reverseByteOrder);
            TimestampLow = BitConverter.ToUInt32(timestampAsByte, 4).ReverseByteOrder(reverseByteOrder);

            ulong ts = ((ulong)TimestampHigh << 32) | TimestampLow;

            if (tsresol == 6)
            {
                // Fast path for the default microsecond resolution: the value already is in
                // microseconds, so exact integer math suffices.
                ulong seconds = ts / 1_000_000;
                if (seconds > uint.MaxValue)
                    throw new OverflowException("Timestamp does not fit the Seconds/Microseconds representation (seconds exceed uint.MaxValue)");

                Seconds = (uint)seconds;
                Microseconds = (uint)(ts % 1_000_000);
                return;
            }

            decimal scale = unitsPerSecond[tsresol];
            if (scale == 0m)
                throw new ArgumentOutOfRangeException(nameof(tsresol), tsresol, "tsresol scale factor is not representable as decimal (base-10 exponent must be <= 28, base-2 exponent must be <= 95)");

            decimal totalMicrosDecimal = decimal.Round((decimal)ts * 1_000_000m / scale, MidpointRounding.AwayFromZero);
            if (totalMicrosDecimal > MaxTotalMicroseconds)
                throw new OverflowException("Timestamp does not fit the Seconds/Microseconds representation (seconds exceed uint.MaxValue)");

            long totalMicros = (long)totalMicrosDecimal;
            Seconds = (uint)(totalMicros / 1_000_000);
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
            // Write the raw timestamp words back unchanged. For parsed blocks this preserves the
            // original units (whatever if_tsresol the source interface declared), so re-serializing
            // a block into a section with the same interface description is lossless. Helpers
            // constructed from (seconds, microseconds) hold microsecond-based words, matching the
            // default resolution of interface descriptions the writer creates.
            var ret = new List<byte>(8);
            ret.AddRange(BitConverter.GetBytes(TimestampHigh.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(TimestampLow.ReverseByteOrder(reverseByteOrder)));

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

        private static decimal[] BuildUnitsPerSecondTable()
        {
            var table = new decimal[256];
            for (int tsresol = 0; tsresol < 256; tsresol++)
            {
                bool isPwr2 = (tsresol & 0b10000000) != 0;
                int exponent = tsresol & 0b01111111;
                if (isPwr2 ? exponent <= 95 : exponent <= 28)
                    table[tsresol] = isPwr2 ? Pow2(exponent) : Pow10(exponent);
            }
            return table;
        }

        // Iterative decimal multiplication is intentional: Math.Pow returns double which
        // loses precision for large exponents, defeating the purpose of decimal arithmetic.
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
