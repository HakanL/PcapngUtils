using Haukcode.PcapngUtils.Common;
using Haukcode.PcapngUtils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haukcode.PcapngUtils.PcapNG.BlockTypes
{          
    public static class AbstractBlockFactory
    {
        #region method

        private static readonly Dictionary<int, long> EmptyTsresols = new Dictionary<int, long>();

        [Obsolete("Use the overload that accepts a tsresols dictionary so that per-interface timestamp resolution is applied correctly.")]
        public static AbstractBlock ReadNextBlock(BinaryReader binaryReader, bool bytesReorder, Action<Exception> ActionOnException)
        {
            return ReadNextBlock(binaryReader, bytesReorder, ActionOnException, EmptyTsresols);
        }

        public static AbstractBlock ReadNextBlock(BinaryReader binaryReader, bool bytesReorder, Action<Exception> ActionOnException, Dictionary<int, long> tsresols)
        {
            CustomContract.Requires<ArgumentNullException>(binaryReader != null, "binaryReader cannot be null");
            CustomContract.Requires<ArgumentNullException>(tsresols != null, "tsresols cannot be null");
            try
            {
                BaseBlock baseblock = new BaseBlock(binaryReader, bytesReorder);
                AbstractBlock block = null;
                switch (baseblock.BlockType)
                {
                    case BaseBlock.Types.SectionHeader:
                        block = SectionHeaderBlock.Parse(baseblock, ActionOnException);  
                        break;
                    case BaseBlock.Types.InterfaceDescription:
                        block = InterfaceDescriptionBlock.Parse(baseblock, ActionOnException);                        
                        break;
                    case BaseBlock.Types.Packet:
                        block = PacketBlock.Parse(baseblock, ActionOnException, tsresols);
                        break;
                    case BaseBlock.Types.SimplePacket:                             
                        block = SimplePacketBlock.Parse(baseblock, ActionOnException);   
                        break;
                    case BaseBlock.Types.NameResolution:
                        block = NameResolutionBlock.Parse(baseblock, ActionOnException);                         
                        break;
                    case BaseBlock.Types.InterfaceStatistics:
                        block = InterfaceStatisticsBlock.Parse(baseblock, ActionOnException, tsresols);
                        break;
                    case BaseBlock.Types.EnhancedPacket:
                        block = EnhancedPacketBlock.Parse(baseblock, ActionOnException, tsresols);
                        break;
                    default:                             
                        break;
                }
                return block;
            }
            catch(Exception exc)
            {
                ActionOnException(exc);
                return null;
            }

        }
        #endregion
    }
}
