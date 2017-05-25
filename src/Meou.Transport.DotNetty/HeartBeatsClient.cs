﻿using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Handlers.Logging;
using System.Net;
using DotNetty.Handlers.Timeout;
using DotNetty.Codecs;
using System.Threading.Tasks;

namespace Meou.Transport.DotNetty
{

    public class HeartBeatsClient
    {
        protected HashedWheelTimer timer = new HashedWheelTimer();
        private Bootstrap boot;
        private ConnectorIdleStateTrigger idleStateTrigger = new ConnectorIdleStateTrigger();
        private DefaultConnectionWatchdog watchdog;

        public IChannel Connect(string host, int port ,int reconnect)
        {
            IEventLoopGroup group = new MultithreadEventLoopGroup();
            IChannel channel;

            boot = new Bootstrap();
            watchdog = new DefaultConnectionWatchdog(boot, timer, port, host, reconnect);
            boot.Group(group)
                .Channel<TcpSocketChannel>()
                .Handler(new LoggingHandler(LogLevel.INFO));
            try
            {
                boot.Handler(new DefaultChannelInitializer(watchdog));
                channel = boot.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port)).ConfigureAwait(false).GetAwaiter().GetResult();

                return channel;
            }
            catch (Exception e)
            {
                Task.WhenAll(group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
                return null;
            }

        }
    }

    class DefaultChannelInitializer : ChannelInitializer<IChannel>
    {
        ConnectionWatchdog watchdog;
        public DefaultChannelInitializer(ConnectionWatchdog watchdog)
        {
            this.watchdog = watchdog;
        }

        protected override void InitChannel(IChannel channel)
        {
            channel.Pipeline.AddLast(watchdog.handlers());
        }
    }

    class DefaultConnectionWatchdog : ConnectionWatchdog
    {
        private ConnectorIdleStateTrigger idleStateTrigger = new ConnectorIdleStateTrigger();
        private Bootstrap boot;
        private ITimer timer;
        private int port;
        private string host;
        private int reconnect;


        public DefaultConnectionWatchdog(Bootstrap bootstrap, ITimer timer, int port, string host, int reconnect) : base(bootstrap, timer, port, host, reconnect)
        {
            this.boot = bootstrap;
            this.timer = timer;
            this.host = host;
            this.port = port;
            this.reconnect = reconnect;
        }

        public override IChannelHandler[] handlers()
        {
            return new IChannelHandler[] {
                this,
                new IdleStateHandler(0, 4, 0),
                this.idleStateTrigger,
                new StringDecoder(),
                new StringEncoder(),
                new HeartBeatClientHandler()
             };
        }
    }
}
