using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class HeartBeatClientHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine("激活时间是：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            Console.WriteLine("HeartBeatClientHandler channelActive");
            context.FireChannelActive();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine("停止时间是：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            Console.WriteLine("HeartBeatClientHandler channelInactive");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            String msg = (String)message;
            Console.WriteLine(message);
            if (message.Equals("Heartbeat"))
            {
                context.WriteAsync("has read message from server");
                context.Flush();
            }
            ReferenceCountUtil.Release(msg);
        }
    }
}
