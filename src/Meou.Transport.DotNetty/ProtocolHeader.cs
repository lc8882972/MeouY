using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class ProtocolHeader
    {

        /** 协议头长度 */
        public static int HEAD_LENGTH = 16;
        /** Magic */
        public static ushort MAGIC = (ushort)0xbabe;

        /** Message Code: 0x01 ~ 0x0f =================================================================================== */
        public const  byte REQUEST = 0x01;     // Request
        public const byte RESPONSE = 0x02;     // Response
        public const byte PUBLISH_SERVICE = 0x03;     // 发布服务
        public const byte PUBLISH_CANCEL_SERVICE = 0x04;     // 取消发布服务
        public const byte SUBSCRIBE_SERVICE = 0x05;     // 订阅服务
        public const byte OFFLINE_NOTICE = 0x06;     // 通知下线
        public const byte ACK = 0x07;     // Acknowledge
        public const byte HEARTBEAT = 0x0f;     // Heartbeat

        private byte _messageCode;       // sign 低地址4位

        /** Serializer Code: 0x01 ~ 0x0f ================================================================================ */
        // 位数限制最多支持15种不同的序列化/反序列化方式
        // protostuff   = 0x01
        // hessian      = 0x02
        // kryo         = 0x03
        // java         = 0x04
        // ...
        // XX1          = 0x0e
        // XX2          = 0x0f
        private byte _serializerCode;    // sign 高地址4位
        private byte _status;            // 响应状态码
        private long _id;                // request.invokeId, 用于映射 <id, request, response> 三元组
        private int _bodyLength;         // 消息体长度

        public static byte toSign(byte serializerCode, byte messageCode)
        {
            return (byte)((serializerCode << 4) + messageCode);
        }

        public void sign(byte sign)
        {
            // sign 低地址4位
            this._messageCode = (byte)(sign & 0x0f);
  
            // sign 高地址4位, 先转成无符号int再右移4位
            this._serializerCode = (byte)((((int)sign) & 0xff) >> 4);
        }

        public byte messageCode()
        {
            return this._messageCode;
        }

        public byte serializerCode()
        {
            return this._serializerCode;
        }

        public byte status()
        {
            return this._status;
        }

        public void status(byte status)
        {
            this._status = status;
        }

        public long id()
        {
            return this._id;
        }

        public void id(long id)
        {
            this._id = id;
        }

        public int bodyLength()
        {
            return this._bodyLength;
        }

        public void bodyLength(int bodyLength)
        {
            this._bodyLength = bodyLength;
        }

       
        public override string ToString()
        {
            return "ProtocolHeader{" +  "messageCode=" + this._messageCode +", serializerCode=" + this._serializerCode +
                    ", status=" + this._status +
                    ", id=" + this._id +
                    ", bodyLength=" + this._bodyLength +
                    '}';
        }
    }
}
