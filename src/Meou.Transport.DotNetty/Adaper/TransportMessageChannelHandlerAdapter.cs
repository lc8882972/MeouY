using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Meou.Transport.DotNetty;
using Rabbit.Rpc.Transport.Codec;

namespace Rabbit.Transport.DotNetty.Adaper
{
    internal class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            BytesHolder buffer = message as BytesHolder;
            var data = buffer.Bytes;
            var transportMessage = _transportMessageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}