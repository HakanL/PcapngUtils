﻿using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using Haukcode.PcapngUtils.PcapNG.OptionTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haukcode.PcapngUtils.Extensions;

namespace Haukcode.PcapngUtils.PcapNG
{
    public class HeaderWithInterfacesDescriptions
    {
        #region fields && properties 
        private SectionHeaderBlock header;
        public SectionHeaderBlock Header
        {
            get
            {
                return header;
            }
            set
            {
                CustomContract.Requires<ArgumentNullException>(value != null, "Header cannot be null");
                header = value;
            }
        }

        private readonly List<InterfaceDescriptionBlock> interfaceDescriptions = new List<InterfaceDescriptionBlock>();
        public IList<InterfaceDescriptionBlock> InterfaceDescriptions
        {
            get { return interfaceDescriptions.AsReadOnly(); }
        }

        #endregion

        #region ctor
        public HeaderWithInterfacesDescriptions(SectionHeaderBlock header, List<InterfaceDescriptionBlock> interfaceDescriptions)
        {
            CustomContract.Requires<ArgumentNullException>(header != null, "Header cannot be null");
            CustomContract.Requires<ArgumentNullException>(interfaceDescriptions != null, "Interface description list cannot be null");

            CustomContract.Requires<ArgumentException>(interfaceDescriptions.Count >= 1, "Interface description list is empty");
            
            this.Header = header;
            this.interfaceDescriptions = interfaceDescriptions;
        }
        #endregion


        #region method
        public byte[] ConvertToByte(bool reverseBytesOrder, Action<Exception> ActionOnException )
        {
            CustomContract.Requires<ArgumentNullException>(Header != null, "Header cannot be null");
            List<byte> ret = new List<byte>();
            try
            {
                ret.AddRange(Header.ConvertToByte(reverseBytesOrder, ActionOnException));
                
                foreach (var item in interfaceDescriptions)
                {
                    ret.AddRange(item.ConvertToByte(reverseBytesOrder, ActionOnException));                   
                }
            }
            catch (Exception exc)
            {
                if (ActionOnException != null)
                    ActionOnException(exc);
            }
            return ret.ToArray();
        }
        #endregion

        public static HeaderWithInterfacesDescriptions CreateEmptyHeadeWithInterfacesDescriptions(bool reverseBytesOrder)
        {
            SectionHeaderBlock header = SectionHeaderBlock.GetEmptyHeader(reverseBytesOrder);
            InterfaceDescriptionBlock emptyInterface = InterfaceDescriptionBlock.GetEmptyInterfaceDescription(reverseBytesOrder);
            return new HeaderWithInterfacesDescriptions(header, new List<InterfaceDescriptionBlock>() { emptyInterface });
        }
    }
}
