using System;
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
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            return Connect(ipEndPoint, reconnect);
        }

        public IChannel Connect(EndPoint endPoint, int reconnect)
        {
            IEventLoopGroup group = new MultithreadEventLoopGroup();
            IChannel channel;
            IPEndPoint ip = (IPEndPoint)endPoint;
            boot = new Bootstrap();
            watchdog = new DefaultConnectionWatchdog(boot, timer, endPoint, reconnect);
            boot.Group(group)
                .Channel<TcpSocketChannel>()
                .Handler(new LoggingHandler(LogLevel.INFO))
                .Handler(new DefaultChannelInitializer(watchdog)); ;
            try
            {
                channel = boot.ConnectAsync(endPoint).ConfigureAwait(false).GetAwaiter().GetResult();
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
}
