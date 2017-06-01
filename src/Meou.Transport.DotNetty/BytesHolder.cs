using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class BytesHolder
    {
        private byte[] _bytes;
        private byte _serializerCode;

        public byte SerializerCode
        {
            get { return this._serializerCode; }
            set { this._serializerCode = value; }
        }
        public byte[] Bytes
        {
            get { return this._bytes; }
        }

        public void SetBytes(byte serializerCode,byte[] bytes)
        {
            this._bytes = bytes;
        }

        public void nullBytes()
        {
            _bytes = null; // help gc
        }

        public int size()
        {
            return _bytes == null ? 0 : _bytes.Length;
        }
    }
}
