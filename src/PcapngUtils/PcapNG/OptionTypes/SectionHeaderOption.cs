using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.IO;
using Haukcode.PcapngUtils.Extensions;

namespace Haukcode.PcapngUtils.PcapNG.OptionTypes
{
    public sealed class SectionHeaderOption : AbstractOption
    {
        #region enum
        public enum SectionHeaderOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            HardwareCode = 2,
            OperatingSystemCode = 3,
            UserApplicationCode = 4
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
        /// An UTF-8 string containing the description of the hardware used to create this section.
        /// </summary>
        public string Hardware
        {
            get;
            set;
        }

        /// <summary>
        /// An UTF-8 string containing the name of the operating system used to create this section
        /// </summary>
        public string OperatingSystem
        {
            get;
            set;
        }

        /// <summary>
        /// An UTF-8 string containing the name of the application used to create this section.
        /// </summary>
        public string UserApplication
        {
            get;
            set;
        }
        #endregion

        #region ctor
        public SectionHeaderOption(List<string> Comments = null, string Hardware = null, string OperatingSystem = null, string UserApplication = null)
        {
            this.Comments = Comments ?? new List<string>();
            this.Hardware = Hardware;
            this.OperatingSystem = OperatingSystem;
            this.UserApplication = UserApplication;
        }
        #endregion

        #region method
        public static SectionHeaderOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception> ActionOnException)
        {
            CustomContract.Requires<ArgumentNullException>(binaryReader != null, "binaryReader cannot be null");

            SectionHeaderOption option = new SectionHeaderOption();
            List<KeyValuePair<ushort, byte[]>> optionsList = EkstractOptions(binaryReader, reverseByteOrder, ActionOnException);
            if (optionsList.Any())
            {
                foreach (var item in optionsList)
                {
                    try
                    {
                        switch (item.Key)
                        {
                            case (ushort)SectionHeaderOptionCode.CommentCode:
                                option.Comments.Add(UTF8Encoding.UTF8.GetString(item.Value));
                                break;
                            case (ushort)SectionHeaderOptionCode.HardwareCode:
                                option.Hardware = UTF8Encoding.UTF8.GetString(item.Value);
                                break;
                            case (ushort)SectionHeaderOptionCode.OperatingSystemCode:
                                option.OperatingSystem = UTF8Encoding.UTF8.GetString(item.Value);
                                break;
                            case (ushort)SectionHeaderOptionCode.UserApplicationCode:
                                option.UserApplication = UTF8Encoding.UTF8.GetString(item.Value);
                                break;
                            case (ushort)SectionHeaderOptionCode.EndOfOptionsCode:
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
                foreach (var comment in Comments)
                {
                    byte[] comentValue = UTF8Encoding.UTF8.GetBytes(comment);
                    if (comentValue.Length <= UInt16.MaxValue)
                        ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
                }
            }

            if (Hardware != null)
            {
                byte[] hardwareValue = UTF8Encoding.UTF8.GetBytes(Hardware);
                if (hardwareValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.HardwareCode, hardwareValue, reverseByteOrder, ActionOnException));
            }

            if (OperatingSystem != null)
            {
                byte[] systemValue = UTF8Encoding.UTF8.GetBytes(OperatingSystem);
                if (systemValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.OperatingSystemCode, systemValue, reverseByteOrder, ActionOnException));
            }

            if (UserApplication != null)
            {
                byte[] userAppValue = UTF8Encoding.UTF8.GetBytes(UserApplication);
                if (userAppValue.Length <= UInt16.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.UserApplicationCode, userAppValue, reverseByteOrder, ActionOnException));
            }


            ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.EndOfOptionsCode, new byte[0], reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion
    }
}
