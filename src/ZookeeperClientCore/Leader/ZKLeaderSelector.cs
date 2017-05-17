using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperClient.Client;
using ZooKeeperClient.Listener;
using ZooKeeperClient.Lock;
using static org.apache.zookeeper.Watcher.Event;

namespace ZooKeeperClient.Leader
{
    /// <summary>
    /// 选举Leader
    /// </summary>
    public class ZKLeaderSelector : ILeaderSelector
    {

        private string id;
        private ZKClient _zkClient;
        private ZKDistributedLock _lock;
        private string leaderPath;
        private CancellationTokenSource cancellationTokenSource;
        private Task task;
        private IZKLeaderSelectorListener listener;
        private IZKStateListener stateListener;
        private volatile bool isInterrupted = false;
        private volatile bool autoRequeue = false;
        private int state = (int)State.LATENT;

        private enum State
        {
            LATENT = 0,
            STARTED = 1,
            CLOSED = 2
        }

        /// <summary>
        /// 创建Leader选举对象
        /// </summary>
        /// <param name="id"> 每个Leader选举的参与者都有一个ID标识，用于区分各个参与者。</param>
        /// <param name="autoRequeue"> 是否在由于网络问题造成与服务器断开连接后，自动参与到选举队列中。</param>
        /// <param name="client"> ZooKeeperClient</param>
        /// <param name="leaderPath"> 选举的路径</param>
        /// <param name="listener"> 成为Leader后执行的的监听器</param>
        public ZKLeaderSelector(string id, bool autoRequeue, ZKClient zkClient, string leaderPath, IZKLeaderSelectorListener listener)
        {
            this.id = id;
            this._zkClient = zkClient;
            this.autoRequeue = autoRequeue;
            this.leaderPath = leaderPath;
            this._lock = new ZKDistributedLock(_zkClient, leaderPath);
            this._lock.lockNodeData = id;
            this.listener = listener;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.stateListener = new ZKStateListener();
            stateListener.StateChangedHandler = async (state) =>
            {
                if (state == KeeperState.SyncConnected)
                {
                    //如果重新连接
                    if (!isInterrupted)
                    {
                        await Task.Run(() => Requeue());
                    }
                }
            };
        }



        /// <summary>
        /// 启动参与选举Leader
        /// </summary>
        public void Start()
        {
            if ((int)State.LATENT != Interlocked.CompareExchange(ref state, (int)State.STARTED, (int)State.LATENT))
            {
                throw new Exception("Cannot be started more than once");
            }
            _zkClient.SubscribeStateChanges(stateListener);
            Requeue();
        }

        /// <summary>
        /// 重新添加当前线程到选举队列
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void Requeue()
        {
            lock (this)
            {
                if (state != (int)State.STARTED)
                {
                    throw new Exception("close() has already been called");
                }

                isInterrupted = false;
                if (task != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = new CancellationTokenSource();
                }
                task = Task.Run(async () =>
                 {
                     if (!cancellationTokenSource.IsCancellationRequested)
                     {
                         await _lock.LockAsync();
                         listener.TakeLeadership(_zkClient, this);
                     }
                 }, cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// 获得
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetLeaderAsync()
        {
            if ((await _lock.GetParticipantNodes()).Count > 0)
            {
                return await _zkClient.GetDataAsync<string>(leaderPath + "/" + (await _lock.GetParticipantNodes())[0]);
            }
            return null;
        }

        public async Task<bool> IsLeaderAsync()
        {
            if (_zkClient.GetCurrentState() == KeeperState.SyncConnected)
            {
                if ((await _lock.GetParticipantNodes()).Count > 0)
                {
                    return id == await _zkClient.GetDataAsync<string>(leaderPath + "/" + (await _lock.GetParticipantNodes())[0]);
                }
            }
            return false;
        }


        /// <summary>
        /// 终止等待成为Leader
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void InterruptLeadership()
        {
            lock (this)
            {
                cancellationTokenSource.Cancel();
                isInterrupted = true;
            }
        }

        /// <summary>
        /// 关闭Leader选举
        /// </summary>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void Close()
        {
            lock (this)
            {
                if ((int)State.STARTED != Interlocked.CompareExchange(ref state, (int)State.CLOSED, (int)State.STARTED))
                {
                    throw new Exception("Already closed or has not been started");
                }
                Task.Run(async () =>
                {
                    await _lock.UnLockAsync().ConfigureAwait(false);
                }).ConfigureAwait(false).GetAwaiter().GetResult();

                _zkClient.UnSubscribeStateChanges(stateListener);
                cancellationTokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
