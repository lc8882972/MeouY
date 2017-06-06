using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class HeartBeatClientHandler : ChannelHandlerAdapter
    {
        public override bool IsSharable => true;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            RequestBytes req = new RequestBytes();
            req.SetBytes(1, Encoding.UTF8.GetBytes("hello"));
            context.WriteAsync(req);
            context.Flush();
            context.FireChannelActive();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            //Console.WriteLine("停止时间是：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            //Console.WriteLine("HeartBeatClientHandler channelInactive");
            context.CloseAsync().Wait();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            ResponseBytes resp = message as ResponseBytes;
            string msg = Encoding.UTF8.GetString(resp.Bytes);
            Console.WriteLine(msg);
            context.Flush();

            ReferenceCountUtil.Release(message);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync().Wait();
        }
    }
}
