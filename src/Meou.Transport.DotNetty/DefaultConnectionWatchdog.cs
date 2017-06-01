using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Transport.DotNetty
{
    internal class DefaultConnectionWatchdog : ConnectionWatchdog
    {
        private ConnectorIdleStateTrigger idleStateTrigger = new ConnectorIdleStateTrigger();
        private Bootstrap boot;
        private ITimer timer;
        private int port;
        private string host;
        private int reconnect;
        private ProtocolEncoder encoder = new ProtocolEncoder();

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
