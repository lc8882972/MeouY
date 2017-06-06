using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    public class HeartBeatServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            
            RequestBytes req = message as RequestBytes;

            string msg = System.Text.Encoding.UTF8.GetString(req.Bytes);

            Console.WriteLine(context.Channel.RemoteAddress + "->Server :" + msg);
            ResponseBytes resp = new ResponseBytes(req.InvokeId);
            resp.SetBytes(1,System.Text.Encoding.UTF8.GetBytes("response from server"));
            resp.Status = 1;
            context.WriteAndFlushAsync(resp).Wait();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine(exception.Message);
            context.CloseAsync().Wait();
        }

    }
}

