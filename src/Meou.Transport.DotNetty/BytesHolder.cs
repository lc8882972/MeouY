using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class BytesHolder
    {
        private byte[] bytes;
        private byte serializerCode;

        public byte SerializerCode
        {
            get { return this.serializerCode; }
            //set { this.serializerCode = value; }
        }
        public byte[] Bytes
        {
            get { return this.bytes; }
        }

        public void SetBytes(byte serializerCode,byte[] bytes)
        {
            this.bytes = bytes;
        }

        public void NullBytes()
        {
            bytes = null; // help gc
        }


        public int Size
        {
            get { return bytes == null ? 0 : bytes.Length; }
        }
    }
}
