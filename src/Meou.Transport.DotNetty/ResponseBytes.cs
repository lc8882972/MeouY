using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class ResponseBytes : BytesHolder
    {
        // 用于映射 <id, request, response> 三元组
        private long id; // request.invokeId
        private byte status;

        public ResponseBytes(long id)
        {
            this.id = id;
        }

        public long Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public byte Status
        {
            get { return this.status; }
            set { this.status = value; }
        }
    }
}
