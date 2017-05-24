using Meou.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperNotifyListener : NotifyListener
    {
        Action<RegisterMeta> act;
        public ZookeeperNotifyListener(Action<RegisterMeta> notify)
        {
            act = notify;
        }
        public Task Notify(RegisterMeta registerMeta)
        {
            if (act != null)
                act.Invoke(registerMeta);
            
            return Task.CompletedTask;

        }

        protected virtual void NotifyFunc(Action<RegisterMeta> act)
        {
           
        }
    }
}
