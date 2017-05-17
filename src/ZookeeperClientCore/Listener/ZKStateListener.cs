using System;
using System.Threading.Tasks;
using static org.apache.zookeeper.Watcher.Event;

namespace ZooKeeperClient.Listener
{
    public class ZKStateListener : IZKStateListener
    {
        public Func<KeeperState, Task> StateChangedHandler { set; get; }

        public Func<string, Task> SessionExpiredHandler { set; get; }

        public Func<Task> NewSessionHandler { set; get; }

        public Func<Exception, Task> SessionEstablishmentErrorHandler { set; get; }
    }
}
