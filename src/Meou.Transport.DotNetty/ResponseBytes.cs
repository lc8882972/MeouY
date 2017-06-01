using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class ResponseBytes : BytesHolder
    {
        // 用于映射 <id, request, response> 三元组
        private long _id; // request.invokeId
        private byte _status;

        public ResponseBytes(long id)
        {
            this._id = id;
        }

        public long Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public byte Status
        {
            get { return this._status; }
            set { this._status = value; }
        }
    }
}
