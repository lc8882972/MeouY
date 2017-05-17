using System;
using System.Threading.Tasks;
using static org.apache.zookeeper.Watcher.Event;

namespace ZooKeeperClient.Listener
{
    public interface IZKStateListener
    {
        /// <summary>
        /// 状态改变
        /// state
        /// </summary>
        Func<KeeperState, Task> StateChangedHandler { set; get; }

        /// <summary>
        /// 会话失效
        /// path
        /// </summary>
        Func<string,Task> SessionExpiredHandler { set; get; }

        /// <summary>
        /// 会话创建
        /// any ephemeral nodes here.
        /// </summary>
        Func<Task> NewSessionHandler { set; get; }

        /// <summary>
        ///会话出错
        /// failure handling e.g.retry to connect or pass the error up
        /// error
        /// </summary>
        Func<Exception, Task> SessionEstablishmentErrorHandler { set; get; }
    }
}
