using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Meou.Registry.Zookeeper
{
    public class ChildWatcher : org.apache.zookeeper.Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            
        }
    }
}
