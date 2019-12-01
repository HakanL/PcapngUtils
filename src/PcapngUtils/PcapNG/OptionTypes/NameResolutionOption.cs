﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haukcode.PcapngUtils.Extensions;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;

namespace Haukcode.PcapngUtils.PcapNG.OptionTypes
{
    public sealed class NameResolutionOption : AbstractOption
    {
        #region enum
        public enum NameResolutionOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            DnsNameCode = 2,
            DnsIp4AddrCode = 3,
            DnsIp6AddrCode = 4
        }
        #endregion

        #region fields & properies
        /// <summary>
        /// A UTF-8 string containing a comment that is associated to the current block.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }

        /// <summary>
        /// A UTF-8 string containing the name of the machine (DNS server) used to perform the name resolution.
        /// </summary>
        public string DnsName
        {
            get;
            set;
        }

        private IPAddress dnsIp4Addr;
        /// <summary>
        /// Specifies an IPv4 address (contained in the first 4 bytes), followed by one or more zero-terminated strings containing 
        /// the DNS entries for that address.
        /// </summary>
        public IPAddress DnsIp4Addr
        {
            get
            {
                return dnsIp4Addr;
            }
            set
            {
                CustomContract.Requires<ArgumentException>(value == null || value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork, "dnsIp4Addr is not AddressFamily.InterNetwork");
                dnsIp4Addr = value;
            }
        }

        private IPAddress dnsIp6Addr;
        /// <summary>
        /// Specifies an IPv6 address (contained in the first 16 bytes), followed by one or more zero-terminated strings containing 
        /// the DNS entries for that address.
        /// </summary>
        public IPAddress DnsIp6Addr
        {
            get
            {
                return dnsIp6Addr;
            }
            set
            {
                CustomContract.Requires<ArgumentException>(value == null || value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6, "dnsIp6Addr is not AddressFamily.InterNetworkV6");
                dnsIp6Addr = value;
            }
        }
        #endregion

        #region ctor
        public NameResolutionOption(string Comment = null, string DnsName = null, IPAddress DnsIp4Addr = null, IPAddress DnsIp6Addr = null)
        {
            CustomContract.Requires<ArgumentException>(DnsIp4Addr == null || DnsIp4Addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork,   "dnsIp4Addr is not AddressFamily.InterNetwork");
            CustomContract.Requires<ArgumentException>(DnsIp6Addr == null || DnsIp6Addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6, "dnsIp6Addr is not AddressFamily.InterNetworkV6");
                    
            this.Comment = Comment;
            this.DnsName = DnsName;
            this.dnsIp4Addr = DnsIp4Addr;
            this.dnsIp6Addr = DnsIp6Addr;
        }
        #endregion

        #region method
        public static NameResolutionOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception> ActionOnException)
        {
            CustomContract.Requires<ArgumentNullException>(binaryReader != null, "binaryReader cannot be null");

            NameResolutionOption option = new NameResolutionOption();
            List<KeyValuePair<ushort, byte[]>> optionsList = EkstractOptions(binaryReader, reverseByteOrder, ActionOnException);
            if (optionsList.Any())
            {
                foreach (var item in optionsList)
                {
                    try
                    {
                        switch (item.Key)
                        {
                            case (ushort)NameResolutionOptionCode.CommentCode:
                                option.Comment = UTF8Encoding.UTF8.GetString(item.Value);
                                break;
                            case (ushort)NameResolutionOptionCode.DnsNameCode:
                                option.DnsName = UTF8Encoding.UTF8.GetString(item.Value);
                                break;
                            case (ushort)NameResolutionOptionCode.DnsIp4AddrCode:
                                if (item.Value.Length == 4)
                                    option.DnsIp4Addr = new IPAddress(item.Value);
                                else
                                    throw new ArgumentException(string.Format("[NameResolutionOption.Parse] DnsIp4AddrCode contains invalid length. Received: {0} bytes, expected: {1}", item.Value.Length, 4));
                                break;
                            case (ushort)NameResolutionOptionCode.DnsIp6AddrCode:                                
                                if (item.Value.Length == 16)
                                    option.DnsIp6Addr = new IPAddress(item.Value);
                                else
                                    throw new ArgumentException(string.Format("[NameResolutionOption.Parse] DnsIp6AddrCode contains invalid length. Received: {0} bytes, expected: {1}", item.Value.Length, 16));
                                break;
                            case (ushort)NameResolutionOptionCode.EndOfOptionsCode:
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

            if (Comment != null)
            {
                byte[] comentValue = UTF8Encoding.UTF8.GetBytes(Comment);
                if (comentValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
            }

            if (DnsName != null)
            {
                byte[] nameValue = UTF8Encoding.UTF8.GetBytes(DnsName);
                if (nameValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsNameCode, nameValue, reverseByteOrder, ActionOnException));
            }

            if (DnsIp4Addr != null)
            {
                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsIp4AddrCode, DnsIp4Addr.GetAddressBytes(), reverseByteOrder, ActionOnException));
            }

            if (DnsIp6Addr != null)
            {
                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsIp6AddrCode, DnsIp6Addr.GetAddressBytes(), reverseByteOrder, ActionOnException));
            }

            ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.EndOfOptionsCode, new byte[0], reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion
    }
}
