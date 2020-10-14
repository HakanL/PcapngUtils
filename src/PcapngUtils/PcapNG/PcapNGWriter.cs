﻿using System;
using System.Collections.Generic;
using System.IO;
using Haukcode.PcapngUtils.Extensions;
using System.Diagnostics.Contracts;
using Haukcode.PcapngUtils.PcapNG.BlockTypes;
using System.Linq;
using Haukcode.PcapngUtils.Common;
using System.Runtime.ExceptionServices;

namespace Haukcode.PcapngUtils.PcapNG
{
    public sealed class PcapNGWriter : Disposable, IWriter
    {
        #region event & delegate
        public event CommonDelegates.ExceptionEventDelegate OnExceptionEvent;

        private void OnException(Exception exception)
        {
            CustomContract.Requires<ArgumentNullException>(exception != null, "exception cannot be null or empty");
            CommonDelegates.ExceptionEventDelegate handler = OnExceptionEvent;
            if (handler != null)
                handler(this, exception);
            else
                ExceptionDispatchInfo.Capture(exception).Throw();
        }

        #endregion

        #region fields & properties
        private Stream stream;
        private BinaryWriter binaryWriter;

        private List<HeaderWithInterfacesDescriptions> headersWithInterface = new List<HeaderWithInterfacesDescriptions>();
        public IList<HeaderWithInterfacesDescriptions> HeadersWithInterfaces
        {
            get { return headersWithInterface.AsReadOnly(); }
        }

        private object syncRoot = new object();
        #endregion

        #region ctor
        public PcapNGWriter(string path, bool reverseByteOrder = false)
        {
            CustomContract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), "path cannot be null or empty");
            CustomContract.Requires<ArgumentException>(!File.Exists(path), "file exists");
            HeaderWithInterfacesDescriptions header = HeaderWithInterfacesDescriptions.CreateEmptyHeadeWithInterfacesDescriptions(false);
            Initialize(new FileStream(path, FileMode.Create), new List<HeaderWithInterfacesDescriptions>(){header}) ;
        }  

        public PcapNGWriter(Stream stream, bool reverseByteOrder = false)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");
            HeaderWithInterfacesDescriptions header = HeaderWithInterfacesDescriptions.CreateEmptyHeadeWithInterfacesDescriptions(false);
            Initialize(stream, new List<HeaderWithInterfacesDescriptions>() { header });
        }
        public PcapNGWriter(string path, List<HeaderWithInterfacesDescriptions> headersWithInterface)
        {
            CustomContract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), "path cannot be null or empty");
            CustomContract.Requires<ArgumentException>(!File.Exists(path), "file exists");

            CustomContract.Requires<ArgumentNullException>(headersWithInterface != null, "headersWithInterface list cannot be null");

            CustomContract.Requires<ArgumentException>(headersWithInterface.Count >= 1, "headersWithInterface list is empty");

            Initialize(new FileStream(path, FileMode.Create), headersWithInterface);
        }
        public PcapNGWriter(Stream stream, List<HeaderWithInterfacesDescriptions> headersWithInterface)
        {
            CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");
            CustomContract.Requires<ArgumentNullException>(headersWithInterface != null, "headersWithInterface list cannot be null");

            CustomContract.Requires<ArgumentException>(headersWithInterface.Count >= 1, "headersWithInterface list is empty");

            Initialize(stream,  headersWithInterface);
        }

        private void Initialize(Stream stream, List<HeaderWithInterfacesDescriptions> headersWithInterface)
         {                     
             CustomContract.Requires<ArgumentNullException>(stream != null, "stream cannot be null");
             CustomContract.Requires<Exception>(stream.CanWrite == true, "Cannot write to stream");
             CustomContract.Requires<ArgumentNullException>(headersWithInterface != null, "headersWithInterface list cannot be null");

             CustomContract.Requires<ArgumentException>(headersWithInterface.Count >= 1, "headersWithInterface list is empty");

             this.headersWithInterface = headersWithInterface;
             this.stream = stream;
             binaryWriter = new BinaryWriter(stream);
             Action<Exception> ReThrowException = (exc) =>
             {
                 ExceptionDispatchInfo.Capture(exc).Throw();
             };
             foreach (var header in headersWithInterface)
             {
                 binaryWriter.Write(header.ConvertToByte(header.Header.ReverseByteOrder, ReThrowException));          
             }
               
         }           
        #endregion
        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        public void WritePacket(IPacket packet)
        {
            try
            {
                AbstractBlock abstractBlock =null;
                if (packet is AbstractBlock)
                {
                    abstractBlock = packet as AbstractBlock;                     
                }
                else
                {
                    abstractBlock = EnchantedPacketBlock.CreateEnchantedPacketFromIPacket(packet, OnException);
                }

                HeaderWithInterfacesDescriptions header = this.HeadersWithInterfaces.Last();
                byte[] data = abstractBlock.ConvertToByte(header.Header.ReverseByteOrder, OnException);

                if (abstractBlock.AssociatedInterfaceID.HasValue)
                {
                    if (abstractBlock.AssociatedInterfaceID.Value >= header.InterfaceDescriptions.Count)
                    {
                        throw new ArgumentOutOfRangeException(string.Format("[PcapNGWriter.WritePacket] Packet interface ID: {0} is greater than InterfaceDescriptions count: {1}", abstractBlock.AssociatedInterfaceID.Value, header.InterfaceDescriptions.Count));
                    }
                    int maxLength = header.InterfaceDescriptions[abstractBlock.AssociatedInterfaceID.Value].SnapLength;
                    if (data.Length > maxLength)
                    {
                        throw new ArgumentOutOfRangeException(string.Format("[PcapNGWriter.WritePacket] block length: {0} is greater than MaximumCaptureLength: {1}",data.Length,maxLength));
                            
                    }
                }
                lock (syncRoot)
                {
                    binaryWriter.Write(data);
                }
            }
            catch (Exception exc)
            {
                OnException(exc);
            }
        }

        public void WriteHeaderWithInterfacesDescriptions(HeaderWithInterfacesDescriptions headersWithInterface)
        {
            CustomContract.Requires<ArgumentNullException>(headersWithInterface != null, "headersWithInterface  cannot be null");
            CustomContract.Requires<ArgumentNullException>(headersWithInterface.Header != null, "headersWithInterface.Header  cannot be null");

            byte [] data = headersWithInterface.ConvertToByte(headersWithInterface.Header.ReverseByteOrder, OnException);
            try
            {
                lock (syncRoot)
                {
                    binaryWriter.Write(data);
                }
            }
            catch (Exception exc)
            {
                OnException(exc);
            }
        }
        #region IDisposable Members
        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (binaryWriter != null)
                binaryWriter.Close();
            if (stream != null)
                stream.Close();
        }

        #endregion      
    }  
}
