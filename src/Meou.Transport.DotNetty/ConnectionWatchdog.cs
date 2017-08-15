using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Meou.Transport.DotNetty;
using DotNetty.Common.Utilities;
using System.Net;

namespace Meou.Transport
{
    public abstract class ConnectionWatchdog : ChannelHandlerAdapter, ChannelHandlerHolder, ITimerTask
    {
        private readonly Bootstrap bootstrap;
        private IChannel channel;
        private readonly ITimer timer;
        private readonly EndPoint endPoint;
        private volatile int reconnect = 6;
        private int attempts;
        private static object lookobj = new object();

        public override bool IsSharable
        {
            get { return true; }
        }

        public ConnectionWatchdog(Bootstrap bootstrap, ITimer timer, EndPoint endPoint, int reconnect)
        {
            this.bootstrap = bootstrap;
            this.timer = timer;
            this.endPoint = endPoint;
            this.reconnect = reconnect;
        }

        public abstract IChannelHandler[] handlers();


        public override void ChannelActive(IChannelHandlerContext context)
        {
            //Console.WriteLine("当前链路已经激活了，重连尝试次数重新置为0");
            channel = context.Channel;
            attempts = 0;
            context.FireChannelActive();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            channel = context.Channel;
            Console.WriteLine(reconnect);
            if (reconnect > 0)
            {
                if (attempts <= reconnect)
                {
                    attempts++;
                }
                Console.WriteLine(attempts);
                //重连的间隔时间会越来越长  
                int timeout = 2 << attempts;
                timer.NewTimeout(this, TimeSpan.FromMilliseconds(timeout));
            }
            context.FireChannelInactive();
        }

        public void Run(ITimeout timeout)
        {
            IChannel future = null;
            try
            {
                lock (lookobj)
                {
                    bootstrap.Handler(new DefaultChannelInitializer(handlers()));
                    future = bootstrap.ConnectAsync(this.endPoint).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //如果重连失败，则调用ChannelInactive方法，再次出发重连事件，一直尝试12次，如果失败则不再重连  
            if (future == null)
            {
                if (attempts <= reconnect)
                {
                    attempts++;
                    timer.NewTimeout(this, TimeSpan.FromMilliseconds(2 << attempts));
                }
            }
            else
            {
                Console.WriteLine("重连成功");
            }
        }
    }

    internal class DefaultChannelInitializer : ChannelInitializer<IChannel>
    {
        private IChannelHandler[] handlers;

        public DefaultChannelInitializer(IChannelHandler[] handlers)
        {
            this.handlers = handlers;
        }

        protected override void InitChannel(IChannel channel)
        {
            channel.Pipeline.AddLast(handlers);
        }
    }
}
