﻿using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class HeartBeatServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            //var isR = message is RequestBytes;
            Console.WriteLine(context.Channel.RemoteAddress + "->Server :" + message.ToString());
           
            context.WriteAndFlushAsync("Heartbeat").Wait();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine(exception.Message);
            context.CloseAsync().Wait();
        }

    }
}

