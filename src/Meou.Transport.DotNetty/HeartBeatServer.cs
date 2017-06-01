using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Transport.DotNetty
{
    public class HeartBeatServer
    {
        private AcceptorIdleStateTrigger idleStateTrigger = new AcceptorIdleStateTrigger();
        private int port;
        private string host = "0.0.0.0";

        public HeartBeatServer(int port)
        {
            this.port = port;
        }

        public HeartBeatServer(string host,int port)
        {
            this.host = host;
            this.port = port;
        }

        public async Task StartAsync()
        {
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Handler(new LoggingHandler(LogLevel.INFO))
                    .LocalAddress(port)
                    .ChildHandler(new ActionChannelInitializer(this.idleStateTrigger))
                    .Option(ChannelOption.SoBacklog, 128)
                    .ChildOption(ChannelOption.SoKeepalive, true);

                IChannel channel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(this.host), port));
                Console.WriteLine("Server start listen at " + port);

                Console.ReadLine();

                Console.WriteLine("Stop listen at " + port);
                await channel.CloseAsync();
            }
            catch (Exception e)
            {
                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        private class ActionChannelInitializer : ChannelInitializer<ISocketChannel>
        {
            private AcceptorIdleStateTrigger idleStateTrigger;
            public ActionChannelInitializer(AcceptorIdleStateTrigger idleStateTrigger)
            {
                this.idleStateTrigger = idleStateTrigger;
            }
            protected override void InitChannel(ISocketChannel channel)
            {
                channel.Pipeline.AddLast(new IdleStateHandler(8, 0, 0));
                channel.Pipeline.AddLast(idleStateTrigger);
                channel.Pipeline.AddLast("decoder", new StringDecoder());
                channel.Pipeline.AddLast("encoder", new StringEncoder());
                channel.Pipeline.AddLast(new HeartBeatServerHandler());
            }
        }
    }
}
