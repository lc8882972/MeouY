using DotNetty.Codecs;
using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Meou.Transport.DotNetty
{
    public class ProtocolDecoder : ReplayingDecoder<ProtocolDecoder.StateEnum>
    {
        //static StateEnum state;
        //public override bool IsSharable => true;
        public ProtocolDecoder() : base(StateEnum.HEADER_MAGIC)
        {

        }
        private ProtocolHeader header = new ProtocolHeader();
        private int MAX_BODY_SIZE = 1024 * 1024 * 4;

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            switch (State)
            {
                case StateEnum.HEADER_MAGIC:
                    checkMagic(input.ReadShort());         // MAGIC
                    Checkpoint(StateEnum.HEADER_SIGN);
                    break;
                case StateEnum.HEADER_SIGN:
                    header.sign(input.ReadByte());         // 消息标志位
                    Checkpoint(StateEnum.HEADER_STATUS);
                    break;
                case StateEnum.HEADER_STATUS:
                    header.status(input.ReadByte());       // 状态位
                    Checkpoint(StateEnum.HEADER_ID);
                    break;
                case StateEnum.HEADER_ID:
                    header.id(input.ReadLong());           // 消息id
                    Checkpoint(StateEnum.HEADER_BODY_LENGTH);

                    break;
                case StateEnum.HEADER_BODY_LENGTH:
                    header.bodyLength(input.ReadInt());    // 消息体长度
                    Checkpoint(StateEnum.BODY);
                    break;
                case StateEnum.BODY:
                    switch (header.messageCode())
                    {
                        case ProtocolHeader.HEARTBEAT:
                            break;

                        case ProtocolHeader.REQUEST:

                            int length = checkBodyLength(header.bodyLength());
                            byte[] bytes = new byte[length];
                            input.ReadBytes(bytes);

                            RequestBytes request = new RequestBytes(header.id());
                            //request.timestamp(SystemClock.millisClock().now());
                            request.SetBytes(header.serializerCode(), bytes);
                            output.Add(request);

                            break;

                        case ProtocolHeader.RESPONSE:
                            int resplength = checkBodyLength(header.bodyLength());
                            byte[] respbytes = new byte[resplength];
                            input.ReadBytes(respbytes);

                            ResponseBytes response = new ResponseBytes(header.id());
                            response.Status = header.status();
                            response.SetBytes(header.serializerCode(), respbytes);
                            output.Add(response);
                            break;

                    }
                    Checkpoint(StateEnum.HEADER_MAGIC);
                    break;
            }
        }
        private void checkMagic(short magic)
        {

        }

        private int checkBodyLength(int size)
        {
            //if (size > MAX_BODY_SIZE)
            //{
            //    throw IoSignals.BODY_TOO_LARGE;
            //}
            return size;
        }
        public enum StateEnum
        {
            HEADER_MAGIC,
            HEADER_SIGN,
            HEADER_STATUS,
            HEADER_ID,
            HEADER_BODY_LENGTH,
            BODY
        }
    }

}
