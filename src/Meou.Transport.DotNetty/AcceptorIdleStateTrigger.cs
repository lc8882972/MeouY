using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using DotNetty.Handlers.Timeout;

namespace Meou.Transport.DotNetty
{
    public class AcceptorIdleStateTrigger : ChannelHandlerAdapter
    {
        public override bool IsSharable => true;

        public override void UserEventTriggered(IChannelHandlerContext context, object @event)
        {
            if (@event is IdleStateEvent) {

                if (IdleState.ReaderIdle == ((IdleStateEvent)@event).State)
                {
                    throw new Exception("idle exception");
                }
            } else {
                base.UserEventTriggered(context, @event);
            }
        }
    }
}
