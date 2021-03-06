﻿using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class Heartbeats
    {
        private static readonly IByteBuffer HEARTBEAT_BUF;

        static Heartbeats()
        {
            IByteBuffer buf = Unpooled.Buffer(ProtocolHeader.HEAD_LENGTH);
            buf.WriteShort(ProtocolHeader.MAGIC);
            buf.WriteByte(ProtocolHeader.HEARTBEAT); // 心跳包这里可忽略高地址的4位序列化/反序列化标志
            buf.WriteByte(0);
            buf.WriteLong(0);
            buf.WriteInt(0);
            HEARTBEAT_BUF = Unpooled.UnreleasableBuffer(buf);
        }
        /**
         * Returns the shared heartbeat content.
         */
        public static IByteBuffer HeartbeatContent()
        {
            return HEARTBEAT_BUF.Duplicate();
        }
    }
}
