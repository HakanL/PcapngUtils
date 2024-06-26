﻿using Haukcode.PcapngUtils.PcapNG.CommonTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haukcode.PcapngUtils.Extensions;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace Haukcode.PcapngUtils.PcapNG.OptionTypes
{
    public sealed class EnhancedPacketOption : AbstractOption
    {
        #region enum
        public enum EnhancedPacketOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            PacketFlagCode = 2,
            HashCode = 3,
            DropCountCode = 4
        };
        #endregion

        #region fields & properies
        /// <summary>
        /// A list of UTF-8 strings containing comments that are associated to the current block.
        /// </summary>
        public List<string> Comments
        {
            get;
            set;
        }

        /// <summary>
        /// A flags word containing link-layer information. 
        /// </summary>
        public PacketBlockFlags PacketFlag
        {
            get;
            set;
        }

        /// <summary>
        /// This option contains a hash of the packet. The first byte specifies the hashing algorithm, while the following bytes contain 
        /// the actual hash, whose size depends on the hashing algorithm, and hence from the value in the first bit. The hashing algorithm 
        /// can be: 2s complement (algorithm byte = 0, size=XXX), XOR (algorithm byte = 1, size=XXX), CRC32 (algorithm byte = 2, size = 4), 
        /// MD-5 (algorithm byte = 3, size=XXX), SHA-1 (algorithm byte = 4, size=XXX). The hash covers only the packet, not the header added 
        /// by the capture driver: this gives the possibility to calculate it inside the network card. The hash allows easier comparison/merging 
        /// of different capture files, and reliable data transfer between the data acquisition system and the capture library. (TODO: the text 
        /// above uses "first bit", but shouldn't this be "first byte"?!?)
        /// </summary>
        public HashBlock Hash
        {
            get;
            set;
        }

        /// <summary>
        /// A 64bit integer value specifying the number of packets lost (by the interface and the operating system) between this packet and the preceding one.
        /// </summary>
        public long? DropCount
        {
            get;
            set;
        }
        #endregion

        #region ctor
        public EnhancedPacketOption(List<string> Comments = null, PacketBlockFlags PacketFlag = null, long? DropCount = null, HashBlock Hash = null)
        {
            this.Comments = Comments ?? new List<string>();
            this.PacketFlag = PacketFlag;
            this.DropCount = DropCount;
            this.Hash = Hash;
        }
        #endregion

        #region methods
        public static EnhancedPacketOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception> ActionOnException)
        {
            CustomContract.Requires<ArgumentNullException>(binaryReader != null, "binaryReader cannot be null");
            EnhancedPacketOption option = new EnhancedPacketOption();
            List<KeyValuePair<ushort, byte[]>> optionsList = EkstractOptions(binaryReader, reverseByteOrder, ActionOnException);

            if (optionsList.Any())
            {
                foreach (var item in optionsList)
                {
                    try
                    {
                        switch (item.Key)
                        {
                            case (ushort)EnhancedPacketOptionCode.CommentCode:
                                option.Comments.Add(UTF8Encoding.UTF8.GetString(item.Value));
                                break;
                            case (ushort)EnhancedPacketOptionCode.PacketFlagCode:
                                if (item.Value.Length == 4)
                                {
                                    uint packetFlag = (BitConverter.ToUInt32(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                                    option.PacketFlag = new PacketBlockFlags(packetFlag);
                                }
                                else
                                    throw new ArgumentException(string.Format("[EnhancedPacketOptio.Parse] PacketFlagCode contains invalid length. Received: {0} bytes, expected: {1}", item.Value.Length, 4));
                                break;
                            case (ushort)EnhancedPacketOptionCode.HashCode:
                                option.Hash = new HashBlock(item.Value);
                                break;
                            case (ushort)EnhancedPacketOptionCode.DropCountCode:
                                if (item.Value.Length == 8)
                                    option.DropCount = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                                else
                                    throw new ArgumentException(string.Format("[EnhancedPacketOptio.Parse] HashCode contains invalid length. Received: {0} bytes, expected: {1}", item.Value.Length, 8));
                                break;
                            case (ushort)EnhancedPacketOptionCode.EndOfOptionsCode:
                            default:
                                break;
                        }
                    }
                    catch (Exception exc)
                    {
                        if (ActionOnException != null)
                            ActionOnException(exc);
                    }
                }
            }
            return option;
        }

        public override byte[] ConvertToByte(bool reverseByteOrder, Action<Exception> ActionOnException)
        {

            List<byte> ret = new List<byte>();

            if (Comments != null)
            {
                foreach (string comment in Comments)
                {
                    byte[] comentValue = UTF8Encoding.UTF8.GetBytes(comment);
                    if (comentValue.Length <= UInt16.MaxValue)
                        ret.AddRange(ConvertOptionFieldToByte((ushort)EnhancedPacketOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
                }
            }

            if (PacketFlag != null)
            {
                byte[] packetFlagValue = BitConverter.GetBytes(PacketFlag.Flag.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)EnhancedPacketOptionCode.PacketFlagCode, packetFlagValue, reverseByteOrder, ActionOnException));
            }

            if (Hash != null)
            {
                byte[] hashValue = Hash.ConvertToByte();
                if (hashValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)EnhancedPacketOptionCode.HashCode, hashValue, reverseByteOrder, ActionOnException));
            }

            if (DropCount.HasValue)
            {
                byte[] dropCountValue = BitConverter.GetBytes(DropCount.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)EnhancedPacketOptionCode.DropCountCode, dropCountValue, reverseByteOrder, ActionOnException));
            }

            ret.AddRange(ConvertOptionFieldToByte((ushort)EnhancedPacketOptionCode.EndOfOptionsCode, new byte[0], reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion


    }
}
