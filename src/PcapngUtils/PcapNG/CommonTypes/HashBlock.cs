﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haukcode.PcapngUtils.Extensions;

namespace Haukcode.PcapngUtils.PcapNG.CommonTypes
{
    public sealed class HashBlock
    {
        #region enum
        public enum HashAlgorithm : byte
        {
            TwoSComplement = 0,
            Xor = 1,
            Crc32 = 2,
            Md5 = 3,
            Sha1 = 4,
            Invalid = 7
        }
        #endregion

        #region fields && properties
        public HashAlgorithm Algorithm
        {
            get;
            private set;
        }

        public byte[] Value
        {
            get;
            private set;
        }

        public string StringValue
        {
            get
            {
                CustomContract.Requires<ArgumentNullException>(Value != null, "Value cannot be null");
                try
                {
                    string ret = UTF8Encoding.UTF8.GetString(this.Value);
                    return ret;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        #endregion
        
        #region ctor
        public HashBlock(byte[] inputArray)
        {
            CustomContract.Requires<ArgumentNullException>(inputArray != null, "inputArray cannot be null");
            CustomContract.Requires<ArgumentException>(inputArray.Length >= 2, "HashBlock, inputArray length < 2");           

            byte tempAlgorithm = inputArray[0];
            Algorithm = Enum.IsDefined(typeof(HashAlgorithm), tempAlgorithm) ? (HashAlgorithm)tempAlgorithm : HashAlgorithm.Invalid;
            Value = inputArray.Skip(1).Take(inputArray.Length - 1).ToArray();
        }   
        #endregion       

        #region method
        public byte[] ConvertToByte()
        {
            CustomContract.Requires<ArgumentNullException>(Value != null, "Value cannot be null");
            List<byte> ret = new List<byte>();
            ret.Add((byte)Algorithm);
            ret.AddRange(Value);
            return ret.ToArray();
        }
        
        
        public override bool Equals(Object obj)
        {     
            if (obj == null || GetType() != obj.GetType())
                return false;

            HashBlock p = (HashBlock)obj;
            return (this.Algorithm == p.Algorithm) && (this.Value == p.Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
