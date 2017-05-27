using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meou.Common;
using Meou.Registry.Abstractions;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperNotifyListener : INotifyListener
    {
        Action<List<RegisterMeta>> act;
        public ZookeeperNotifyListener(Action<List<RegisterMeta>> notify)
        {
            act = notify;
        }

        public Task Notify(List<RegisterMeta> registerMeta)
        {
            if (act != null)
                act.Invoke(registerMeta);

            return Task.CompletedTask;
        }
    }
}
