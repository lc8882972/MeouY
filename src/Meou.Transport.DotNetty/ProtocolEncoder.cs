using DotNetty.Codecs;
using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Meou.Transport.DotNetty
{
    /**
    * **************************************************************************************************
    *                                          Protocol
    *  ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐
    *       2   │   1   │    1   │     8     │      4      │
    *  ├ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┤
    *           │       │        │           │             │
    *  │  MAGIC   Sign    Status   Invoke Id   Body Length                   Body Content              │
    *           │       │        │           │             │
    *  └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘
    *
    * 消息头16个字节定长
    * = 2 // magic = (short) 0xbabe
    * + 1 // 消息标志位, 低地址4位用来表示消息类型request/response/heartbeat等, 高地址4位用来表示序列化类型
    * + 1 // 状态位, 设置请求响应状态
    * + 8 // 消息 id, long 类型, 未来jupiter可能将id限制在48位, 留出高地址的16位作为扩展字段
    * + 4 // 消息体 body 长度, int 类型
    *
    * jupiter
    * org.jupiter.transport.netty.handler
    *
    * @author jiachun.fjc
    */
    public class ProtocolEncoder : MessageToByteEncoder<BytesHolder>
    {
        public override bool IsSharable => true;
        protected override void Encode(IChannelHandlerContext context, BytesHolder message, IByteBuffer output)
        {
            if (message is RequestBytes)
            {
                EncodeRequest((RequestBytes)message, output);
            }
            else if (message is ResponseBytes)
            {
                EncodeResponse((ResponseBytes)message, output);
            }
            else
            {
                throw new ArgumentException("无法对消息编码");
            }

        }

        private void EncodeRequest(RequestBytes request, IByteBuffer output)
        {
            byte sign = ProtocolHeader.toSign(request.SerializerCode, ProtocolHeader.REQUEST);
            long invokeId = request.InvokeId;
            byte[] bytes = request.Bytes;
            int length = bytes.Length;
            output.WriteShort(ProtocolHeader.MAGIC)
                .WriteByte(sign)
                .WriteByte(0x00)
                .WriteLong(invokeId)
                .WriteInt(length)
                .WriteBytes(bytes);
        }

        private void EncodeResponse(ResponseBytes response, IByteBuffer output)
        {
            byte sign = ProtocolHeader.toSign(response.SerializerCode, ProtocolHeader.RESPONSE);
            byte status = response.Status;
            long invokeId = response.Id;
            byte[] bytes = response.Bytes;
            int length = bytes.Length;

            output.WriteShort(ProtocolHeader.MAGIC)
                .WriteByte(sign)
                .WriteByte(status)
                .WriteLong(invokeId)
                .WriteInt(length)
                .WriteBytes(bytes);
        }
    }
}
