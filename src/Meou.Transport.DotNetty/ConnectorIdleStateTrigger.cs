using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using DotNetty.Handlers.Timeout;
using DotNetty.Buffers;

namespace Meou.Transport.DotNetty
{
    public class ConnectorIdleStateTrigger : ChannelHandlerAdapter
    {
        private static IByteBuffer buffer = Unpooled.WrappedBuffer(UTF8Encoding.UTF8.GetBytes("Heartbeat"));
        public override bool IsSharable => true;

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent) {
                IdleState state = ((IdleStateEvent)evt).State;
                if (state == IdleState.WriterIdle)
                {
                    // write heartbeat to server  
                    //context.WriteAndFlushAsync(Heartbeats.HeartbeatContent());
                    context.WriteAndFlushAsync(buffer);
                }
            } else {
                base.UserEventTriggered(context, evt);
            }
        }
    }
}
