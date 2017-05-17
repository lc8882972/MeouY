using log4net;
using org.apache.zookeeper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ZooKeeperClient.Connection;
using ZooKeeperClient.Listener;
using ZooKeeperClient.Serialize;
using ZooKeeperClient.Util;
using System.Threading.Tasks;
using static org.apache.zookeeper.Watcher.Event;
using static org.apache.zookeeper.ZooDefs;
using org.apache.zookeeper.data;
using static org.apache.zookeeper.KeeperException;

namespace ZooKeeperClient.Client
{
    /// <summary>
    /// ZooKeeper客户端
    /// 断线重连，会话过期处理，永久监听，子节点数据变化监听
    /// </summary>
    public class ZKClient : Watcher, IDisposable
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ZKClient));

        private ConcurrentDictionary<string, ConcurrentHashSet<IZKChildListener>> _childListeners = new ConcurrentDictionary<string, ConcurrentHashSet<IZKChildListener>>();
        private ConcurrentDictionary<string, ConcurrentHashSet<IZKDataListener>> _dataListeners = new ConcurrentDictionary<string, ConcurrentHashSet<IZKDataListener>>();
        private ConcurrentHashSet<IZKStateListener> _stateListeners = new ConcurrentHashSet<IZKStateListener>();
        //保存Ephemeral类型的节点，用于在断开重连，以及会话失效后的自动创建节点
        private ConcurrentDictionary<string, ZKNode> ephemeralNodeMap = new ConcurrentDictionary<string, ZKNode>();

        private object _zkEventLock = new object();
        protected TimeSpan _operationRetryTimeout;
        protected IZKConnection _connection;
        private KeeperState _currentState;
        private IZKSerializer _zkSerializer;
        /// <summary>
        /// 触发关闭的标记，如果为true证明正在关闭客户端及连接   
        /// </summary>
        private bool _shutdownTriggered;
        /// <summary>
        /// 是否已关闭客户端的标记
        /// </summary>
        private volatile bool _closed;
        private ZKTask _eventTask;

        #region ZooKeeperClient
        public ZKClient(string serverstring) : this(serverstring, TimeSpan.FromMilliseconds(10000))
        {
        }

        public ZKClient(string zkServers, TimeSpan connectionTimeout) : this(new ZKConnection(zkServers), connectionTimeout)
        {
        }

        public ZKClient(string zkServers, TimeSpan sessionTimeout, TimeSpan connectionTimeout) : this(new ZKConnection(zkServers, sessionTimeout), connectionTimeout)
        {
        }

        public ZKClient(string zkServers, TimeSpan sessionTimeout, TimeSpan connectionTimeout, IZKSerializer zkSerializer) : this(new ZKConnection(zkServers, sessionTimeout), connectionTimeout, zkSerializer)
        {
        }

        /// <summary>
        /// Most operations done through this {@link org.I0Itec.zkclient.ZooKeeperClient}
        /// are retried in cases like
        /// connection loss with the Zookeeper servers.During such failures, this
        /// <code>operationRetryTimeout</code> decides the maximum amount of time, in milli seconds, each
        ///operation is retried.A value lesser than 0 is considered as
        ///"retry forever until a connection has been reestablished".
        /// </summary>
        /// <param name="zkServers">  The Zookeeper servers</param>
        /// <param name="sessionTimeout">The session timeout in milli seconds</param>
        /// <param name="connectionTimeout">The connection timeout in milli seconds</param>
        /// <param name="IZKSerializer"> The Zookeeper data serializer</param>
        /// <param name="operationRetryTimeout"> operationRetryTimeout</param>

        public ZKClient(string zkServers, TimeSpan sessionTimeout, TimeSpan connectionTimeout, IZKSerializer zkSerializer, TimeSpan operationRetryTimeout) : this(new ZKConnection(zkServers, sessionTimeout), connectionTimeout, zkSerializer, operationRetryTimeout)
        {
        }

        public ZKClient(IZKConnection connection) : this(connection, TimeSpan.FromMilliseconds(10000))
        {
        }

        public ZKClient(IZKConnection connection, TimeSpan connectionTimeout) : this(connection, connectionTimeout, new SerializableSerializer())
        {
        }

        public ZKClient(IZKConnection zkConnection, TimeSpan connectionTimeout, IZKSerializer zkSerializer) : this(zkConnection, connectionTimeout, zkSerializer, TimeSpan.FromSeconds(-1))
        {
        }

        /// <summary>
        /// Most operations done through this {@link org.I0Itec.zkclient.ZooKeeperClient} are retried in cases like
        /// connection loss with the Zookeeper servers.During such failures, this
        /// <code>operationRetryTimeout</code> decides the maximum amount of time, in milli seconds, each
        /// operation is retried.A value lesser than 0 is considered as
        /// retry forever until a connection has been reestablished".
        /// </summary>
        /// <param name="zkConnection"> The Zookeeper servers</param>
        /// <param name="connectionTimeout"> The connection timeout in milli seconds</param>
        /// <param name="IZKSerializer"></param>
        /// <param name="operationRetryTimeout"> The Zookeeper data serializer</param>
        public ZKClient(IZKConnection zkConnection, TimeSpan connectionTimeout, IZKSerializer zkSerializer, TimeSpan operationRetryTimeout)
        {
            _connection = zkConnection ?? throw new ArgumentNullException("Zookeeper connection is null!");
            _zkSerializer = zkSerializer;
            _operationRetryTimeout = operationRetryTimeout;
            Connect(connectionTimeout, this);
        }
        #endregion

        public void SetZKSerializer(IZKSerializer zkSerializer)
        {
            _zkSerializer = zkSerializer;
        }

        public List<string> SubscribeChildChanges(string path, IZKChildListener listener)
        {
            ConcurrentHashSet<IZKChildListener> listeners;
            _childListeners.TryGetValue(path, out listeners);
            if (listeners == null)
            {
                listeners = new ConcurrentHashSet<IZKChildListener>();
                _childListeners.TryAdd(path, listeners);
            }
            listeners.Add(listener);
            return Task.Run(async () =>
            {
                return await WatchForChildsAsync(path);
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void UnSubscribeChildChanges(string path, IZKChildListener childListener)
        {
            ConcurrentHashSet<IZKChildListener> listeners;
            _childListeners.TryGetValue(path, out listeners);
            if (listeners != null)
            {
                listeners.Remove(childListener);
            }
        }

        public void SubscribeDataChanges(string path, IZKDataListener listener)
        {
            ConcurrentHashSet<IZKDataListener> listeners;
            _dataListeners.TryGetValue(path, out listeners);
            if (listeners == null)
            {
                listeners = new ConcurrentHashSet<IZKDataListener>();
                _dataListeners.TryAdd(path, listeners);
            }
            listeners.Add(listener);
            Task.Run(async () =>
            {
                await WatchForDataAsync(path);
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            LOG.Debug($"Subscribed data changes for {path}");
        }

        public void UnSubscribeDataChanges(string path, IZKDataListener dataListener)
        {
            ConcurrentHashSet<IZKDataListener> listeners;
            _dataListeners.TryGetValue(path, out listeners);
            if (listeners != null)
            {
                listeners.Remove(dataListener);
            }
            if (listeners == null || listeners.IsEmpty)
            {
                ConcurrentHashSet<IZKDataListener> _listeners;
                _dataListeners.TryRemove(path, out _listeners);
            }
        }

        public void SubscribeStateChanges(IZKStateListener listener)
        {
            _stateListeners.Add(listener);
        }

        public void UnSubscribeStateChanges(IZKStateListener stateListener)
        {
            _stateListeners.Remove(stateListener);
        }

        public void UnSubscribeAll()
        {
            _childListeners.Clear();
            _dataListeners.Clear();
            _stateListeners.Clear();
        }

        /// <summary>
        /// 递归创建节点,如果节点的上层节点不存在，则自动创建
        ///上层节点都被创建为PERSISTENT类型的节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="createMode"></param>
        public async Task CreateRecursiveAsync(string path, object data, CreateMode createMode)
        {
            await CreateRecursiveAsync(path, data, Ids.OPEN_ACL_UNSAFE, createMode);
        }

        /// <summary>
        /// 递归创建节点,如果节点的上层节点不存在，则自动创建
        ///上层节点都被创建为PERSISTENT类型的节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="acl"></param>
        /// <param name="createMode"></param>
        public async Task CreateRecursiveAsync(string path, object data, List<ACL> acl, CreateMode createMode)
        {
            try
            {
                await CreateAsync(path, data, acl, createMode);
            }
            catch (NodeExistsException)
            {
                LOG.Error($"{path} not exists");
            }
            catch (NoNodeException)
            {
                string parentDir = path.Substring(0, path.LastIndexOf('/'));
                await CreateRecursiveAsync(parentDir, null, acl, CreateMode.PERSISTENT);
                await CreateAsync(path, data, acl, createMode);
            }
        }

        public async Task CreatePersistentAsync(string path)
        {
            await CreatePersistentAsync(path, false);
        }

        private async Task CreatePersistentAsync(string path, bool createParents)
        {
            await CreatePersistentAsync(path, createParents, Ids.OPEN_ACL_UNSAFE);
        }

        private async Task CreatePersistentAsync(string path, bool createParents, List<ACL> acl)
        {
            try
            {
                await CreateAsync(path, null, acl, CreateMode.PERSISTENT);
            }
            catch (NodeExistsException e)
            {
                if (!createParents)
                {
                    throw e;
                }
            }
            catch (NoNodeException e)
            {
                if (!createParents)
                {
                    throw e;
                }
                string parentDir = path.Substring(0, path.LastIndexOf('/'));
                await CreatePersistentAsync(parentDir, createParents, acl);
                await CreatePersistentAsync(path, createParents, acl);
            }
        }

        /// <summary>
        /// 设置访问权限
        /// </summary>
        /// <param name="path"></param>
        /// <param name="acl"></param>
        public async Task SetACLAsync(string path, List<ACL> acl)
        {
            if (path == null)
            {
                throw new ArgumentNullException("Missing value for path");
            }

            if (acl == null || acl.Count == 0)
            {
                throw new ArgumentNullException("Missing value for ACL");
            }

            if (!(await ExistsAsync(path)))
            {
                throw new Exception("trying to set acls on non existing node " + path);
            }

            await RetryUntilConnected(async () =>
            {
                Stat stat = new Stat();
                stat = (await _connection.GetDataAsync(path, false)).Stat;
                await _connection.SetACLAsync(path, acl, stat.getAversion()); ;
            });
        }

        public async Task<ACLResult> GetACLAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("Missing value for path");
            }

            if (!(await ExistsAsync(path)))
            {
                throw new Exception("trying to get acls on non existing node " + path);
            }
            return await RetryUntilConnected(async () => await _connection.GetACLAsync(path));
        }

        public async Task CreatePersistentAsync(string path, object data)
        {
            await CreateAsync(path, data, CreateMode.PERSISTENT);
        }

        public async Task CreatePersistentAsync(string path, object data, List<ACL> acl)
        {
            await CreateAsync(path, data, acl, CreateMode.PERSISTENT);
        }

        public async Task<string> CreatePersistentSequentialAsync(string path, object data)
        {
            return await CreateAsync(path, data, CreateMode.PERSISTENT_SEQUENTIAL);
        }

        public async Task<string> CreatePersistentSequentialAsync(string path, object data, List<ACL> acl)
        {
            return await CreateAsync(path, data, acl, CreateMode.PERSISTENT_SEQUENTIAL);
        }

        /// <summary>
        /// 会话失效后临时节点会被删除，并且在重新连接后并不会重新创建。
        /// </summary>
        /// <param name="path"></param>
        public async Task CreateEphemeralAsync(string path)
        {
            await CreateAsync(path, null, CreateMode.EPHEMERAL);
            ephemeralNodeMap.TryAdd(path, new ZKNode(path, null, CreateMode.EPHEMERAL));
        }

        public async Task CreateEphemeralAsync(string path, List<ACL> acl)
        {
            await CreateAsync(path, null, acl, CreateMode.EPHEMERAL);
            ephemeralNodeMap.TryAdd(path, new ZKNode(path, null, acl, CreateMode.EPHEMERAL));
        }

        public async Task CreateEphemeralAsync(string path, object data)
        {
            await CreateAsync(path, data, CreateMode.EPHEMERAL);
            ephemeralNodeMap.TryAdd(path, new ZKNode(path, data, CreateMode.EPHEMERAL));
        }

        public async Task<string> CreateEphemeralSequentialAsync(string path, object data)
        {
            string retPath = await CreateAsync(path, data, CreateMode.EPHEMERAL_SEQUENTIAL);
            ephemeralNodeMap.TryAdd(path, new ZKNode(path, data, CreateMode.EPHEMERAL_SEQUENTIAL));
            return retPath;
        }

        public async Task<string> CreateEphemeralSequentialAsync(string path, object data, List<ACL> acl)
        {
            string retPath = await CreateAsync(path, data, acl, CreateMode.EPHEMERAL_SEQUENTIAL);
            ephemeralNodeMap.TryAdd(path, new ZKNode(path, data, acl, CreateMode.EPHEMERAL_SEQUENTIAL));
            return retPath;
        }

        public async Task<string> CreateAsync(string path, object data, CreateMode mode)
        {
            return await CreateAsync(path, data, Ids.OPEN_ACL_UNSAFE, mode);
        }

        public async Task<string> CreateAsync(string path, object data, List<ACL> acl, CreateMode mode)
        {
            if (path == null)
            {
                throw new ArgumentNullException("Missing value for path");
            }
            if (acl == null || acl.Count == 0)
            {
                throw new ArgumentNullException("Missing value for ACL");
            }

            byte[] bytes = data == null ? null : Serialize(data);

            return await RetryUntilConnected(async () => await _connection.CreateAsync(path, bytes, acl, mode));
        }

        public async Task<List<string>> GetChildrenAsync(string path)
        {
            return await GetChildrenAsync(path, HasListeners(path));
        }

        protected async Task<List<string>> GetChildrenAsync(string path, bool watch)
        {
            return await RetryUntilConnected(async () => await _connection.GetChildrenAsync(path, watch));
        }

        public async Task<int> CountChildrenAsync(string path)
        {
            try
            {
                return (await GetChildrenAsync(path)).Count;
            }
            catch (NoNodeException)
            {
                return 0;
            }
        }

        public async Task<bool> ExistsAsync(string path, bool watch)
        {
            return await RetryUntilConnected(async () => await _connection.ExistsAsync(path, watch));
        }

        public async Task<bool> ExistsAsync(string path)
        {
            return await ExistsAsync(path, HasListeners(path));
        }

        public async Task<bool> DeleteRecursiveAsync(string path)
        {
            List<string> children;
            try
            {
                children = await GetChildrenAsync(path, false);
            }
            catch (NoNodeException)
            {
                return true;
            }

            foreach (string subPath in children)
            {
                var result = await DeleteRecursiveAsync(path + "/" + subPath);
                if (!result)
                {
                    return false;
                }
            }
            return await DeleteAsync(path);
        }

        public async Task<bool> DeleteAsync(string path)
        {
            return await DeleteAsync(path, -1);
        }

        public async Task<bool> DeleteAsync(string path, int version)
        {
            try
            {
                return await RetryUntilConnected(async () =>
                 {
                     await _connection.DeleteAsync(path, version);
                     return true;
                 });
            }
            catch (NoNodeException)
            {
                return false;
            }
        }

        private byte[] Serialize<T>(T data)
        {
            return _zkSerializer.Serialize(data);
        }

        private T Derializable<T>(byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }
            return _zkSerializer.Deserialize<T>(data);
        }

        public async Task<T> GetDataAsync<T>(string path)
        {
            return (await GetZKDataAsync<T>(path, returnNullIfPathNotExists: false)).data;
        }

        public async Task<T> GetDataAsync<T>(string path, bool returnNullIfPathNotExists)
        {
            return (await GetZKDataAsync<T>(path, returnNullIfPathNotExists: returnNullIfPathNotExists)).data;
        }

        public async Task<ZKData<T>> GetZKDataAsync<T>(string path)
        {
            return await GetZKDataAsync<T>(path, returnNullIfPathNotExists: false);
        }

        public async Task<ZKData<T>> GetZKDataAsync<T>(string path, bool returnNullIfPathNotExists)
        {
            ZKData<T> zkData = null;
            try
            {
                DataResult dataResult = await RetryUntilConnected(async () =>
                 {
                     return (await _connection.GetDataAsync(path, HasListeners(path)));
                 });
                zkData = new ZKData<T>();
                zkData.data = Derializable<T>(dataResult.Data);
                zkData.stat = dataResult.Stat;
            }
            catch (NoNodeException e)
            {
                if (!returnNullIfPathNotExists)
                {
                    throw e;
                }
            }
            return zkData;
        }

        public async Task SetDataAsync<T>(string path, T data)
        {
            await SetDataAsync(path, data, -1);
        }

        public async Task SetDataAsync<T>(string path, T data, int version)
        {
            await SetDataReturnStatAsync<T>(path, data, version);
        }

        public async Task<Stat> SetDataReturnStatAsync<T>(string path, T datat, int expectedVersion)
        {
            byte[] data = Serialize(datat);
            return await RetryUntilConnected(async () =>
            {
                Stat stat = await _connection.SetDataReturnStatAsync(path, data, expectedVersion);
                return stat;
            });
        }

        /// <summary>
        /// 监听数据变化
        /// </summary>
        /// <param name="path"></param>
        private async Task WatchForDataAsync(string path)
        {
            await RetryUntilConnected(async () =>
           {
               await _connection.ExistsAsync(path, true);
               return true;
           });
        }

        /// <summary>
        /// 监听子节点变化
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<List<string>> WatchForChildsAsync(string path)
        {
            return await RetryUntilConnected(async () =>
            {
                await ExistsAsync(path, true);
                try
                {
                    return await GetChildrenAsync(path, true);
                }
                catch (NoNodeException) { }
                return null;
            });
        }

        /// <summary>
        /// 添加认证信息，用于访问被ACL保护的节点
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="auth"></param>
        public void AddAuthInfo(string scheme, byte[] auth)
        {
            RetryUntilConnected(() =>
             {
                 _connection.AddAuthInfo(scheme, auth);
                 return true;
             });
        }

        public void Connect(TimeSpan connectionTimeout, Watcher watcher)
        {
            bool started = false;
            lock (_zkEventLock)
            {
                try
                {
                    _shutdownTriggered = false;
                    _eventTask = new ZKTask(_connection.servers);
                    _eventTask.Start();
                    _connection.Connect(watcher);

                    LOG.Debug("Awaiting connection to Zookeeper server");
                    bool waitSuccessful = WaitUntilConnected(connectionTimeout);
                    if (!waitSuccessful)
                    {
                        throw new TimeoutException("Unable to connect to zookeeper server within timeout: " + connectionTimeout.Seconds);
                    }
                    started = true;
                }
                finally
                {
                    // we should close the zookeeper instance, otherwise it would keep
                    // on trying to connect
                    if (!started)
                    {
                        Close();
                    }
                }
            }
        }

        public void ReConnect(bool recreate)
        {
            lock (_zkEventLock)
            {

                _connection.Close();
                _connection.Connect(this);
                Task.Run(async () =>
                {
                    await RecreateEphemeraleNodeAsync(recreate).ConfigureAwait(false);
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 重连后重新创建临时节点
        /// </summary>
        /// <param name="recreate"></param>
        public async Task RecreateEphemeraleNodeAsync(bool recreate)
        {
            if (recreate)
            {
                foreach (var path in ephemeralNodeMap.Keys)
                {
                    var node = ephemeralNodeMap[path];
                    if (node.acl == null)
                        await CreateAsync(path, node.data, node.createMode);
                    else
                        await CreateAsync(path, node.data, node.acl, node.createMode);
                }
            }
            else
            {
                ephemeralNodeMap.Clear();
            }
        }

        public async Task<long> GetCreateTimeAsync(string path)
        {
            return await _connection.GetCreateTimeAsync(path);
        }

        public void Close()
        {
            if (_closed)
            {
                return;
            }
            LOG.Debug("Closing ZooKeeperClient...");

            lock (_zkEventLock)
            {
                _shutdownTriggered = true;
                _eventTask.Cancel();
                _connection.Close();
                _closed = true;
            }
            LOG.Debug("Closing ZooKeeperClient...done");
        }

        /// <summary>
        /// 重试直到zk连接上。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="callable">执行的zk操作。</param>
        /// <returns>执行结果。</returns>
        public async Task<T> RetryUntilConnected<T>(Func<Task<T>> callable)
        {
            var operationStartTime = DateTime.Now;
            while (true)
            {
                if (_closed)
                {
                    throw new Exception("ZooKeeperClient already closed!");
                }
                try
                {
                    return await callable();
                }
                catch (ConnectionLossException)
                {
                    // we give the event thread some time to update the status to 'Disconnected'
                    await Task.Yield();
                    WaitForRetry();
                }
                catch (SessionExpiredException)
                {
                    // we give the event thread some time to update the status to 'Expired'
                    await Task.Yield();
                    WaitForRetry();
                }

                // before attempting a retry, check whether retry timeout has elapsed
                if (_operationRetryTimeout.TotalMilliseconds > 0 &&
                    (DateTime.Now - operationStartTime) >= _operationRetryTimeout)
                {
                    throw new TimeoutException($"Operation cannot be retried because of retry timeout ({_operationRetryTimeout.TotalMilliseconds} milli seconds)");
                }
            }
        }

        public T RetryUntilConnected<T>(Func<T> callable)
        {
            var operationStartTime = DateTime.Now;
            while (true)
            {
                if (_closed)
                {
                    throw new Exception("ZooKeeperClient already closed!");
                }
                try
                {
                    return callable();
                }
                catch (ConnectionLossException)
                {
                    // we give the event thread some time to update the status to 'Disconnected'
                    Task.Yield();
                    WaitForRetry();
                }
                catch (SessionExpiredException)
                {
                    // we give the event thread some time to update the status to 'Expired'
                    Task.Yield();
                    WaitForRetry();
                }

                // before attempting a retry, check whether retry timeout has elapsed
                if (_operationRetryTimeout.TotalMilliseconds > 0 &&
                    (DateTime.Now - operationStartTime) >= _operationRetryTimeout)
                {
                    throw new TimeoutException($"Operation cannot be retried because of retry timeout ({_operationRetryTimeout.TotalMilliseconds} milli seconds)");
                }
            }
        }


        /// <summary>
        /// 等待直到数据被创建
        /// </summary>
        /// <param name="path"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public async Task<bool> WaitUntilExistsAsync(string path, TimeSpan timeOut)
        {
            if ((await ExistsAsync(path)))
            {
                return true;
            }
            while (!(await ExistsAsync(path, true)))
            {
                bool gotSignal = _nodeEventCondition.WaitOne(timeOut);
                if (!gotSignal)
                {
                    return false;
                }
            }
            return true;
        }

        public void WaitUntilConnected()
        {
            WaitUntilConnected(TimeSpan.FromSeconds(10));
        }

        public bool WaitUntilConnected(TimeSpan timeOut)
        {
            return WaitForKeeperState(KeeperState.SyncConnected, timeOut);
        }

        public bool WaitForKeeperState(KeeperState keeperState, TimeSpan timeOut)
        {
            LOG.Info($"Waiting for keeper state {Convert.ToString(keeperState)}");
            bool stillWaiting = true;
            while (_currentState != keeperState)
            {
                if (!stillWaiting)
                {
                    return false;
                }
                stillWaiting = _stateChangedCondition.WaitOne(timeOut);
                // Throw an exception in the case authorization fails
            }
            LOG.Debug($"State is {Convert.ToString(_currentState)}");
            return true;
        }

        private void WaitForRetry()
        {
            if (_operationRetryTimeout.TotalMilliseconds < 0)
            {
                WaitUntilConnected();
                return;
            }
            WaitUntilConnected(_operationRetryTimeout);
        }

       //MethodImpl(MethodImplOptions.Synchronized)]
        private void SetCurrentState(KeeperState state)
        {
            lock (this)
            {
                _currentState = state;
            }        
        }


        //MethodImpl(MethodImplOptions.Synchronized)]
        public KeeperState GetCurrentState()
        {
            lock (this)
            {
                return _currentState;
            }
        }

        private AutoResetEvent _stateChangedCondition = new AutoResetEvent(false);

        private AutoResetEvent _nodeEventCondition = new AutoResetEvent(false);

        private AutoResetEvent _dataChangedCondition = new AutoResetEvent(false);

        public override async Task process(WatchedEvent @event)
        {
            LOG.Debug($"Received event: {@event.getPath()}");

            bool stateChanged = @event.getPath() == null;
            bool nodeChanged = @event.getPath() != null;
            bool dataChanged = @event.get_Type() == EventType.NodeDataChanged ||
                                             @event.get_Type() == EventType.NodeDeleted ||
                                             @event.get_Type() == EventType.NodeCreated ||
                                             @event.get_Type() == EventType.NodeChildrenChanged;

            try
            {
                // We might have to install child change event listener if a new node was created
                if (_shutdownTriggered)
                {
                    LOG.Debug($"ignoring event '[{@event.get_Type()} | {@event.getPath()}]' since shutdown triggered");
                    return;
                }
                if (stateChanged)
                {
                    await Task.Run(() => ProcessStateChanged(@event));
                }
                if (nodeChanged)
                {
                }
                if (dataChanged)
                {
                    await Task.Run(() => ProcessDataOrChildChange(@event));
                }
            }
            finally
            {
                if (stateChanged)
                {
                    _stateChangedCondition.Set();

                    // If the session expired we have to signal all conditions, because watches might have been removed and
                    // there is no guarantee that those
                    // conditions will be signaled at all after an Expired event
                    // TODO PVo write a test for this
                    if (@event.getState() == KeeperState.Expired)
                    {
                        _nodeEventCondition.Set();

                        _dataChangedCondition.Set();

                        // We also have to notify all listeners that something might have changed
                        await Task.Run(() => FireAllEvents(@event.get_Type()));
                    }
                }
                if (nodeChanged)
                {
                    _nodeEventCondition.Set();
                }
                if (dataChanged)
                {
                    _dataChangedCondition.Set();
                }
                LOG.Debug("Leaving process event");
            }
        }

        private void ProcessDataOrChildChange(WatchedEvent @event)
        {
            string path = @event.getPath();

            if (@event.get_Type() == EventType.NodeChildrenChanged 
                || @event.get_Type() == EventType.NodeCreated 
                || @event.get_Type() == EventType.NodeDeleted)
            {
                ConcurrentHashSet<IZKChildListener> childListeners;
                _childListeners.TryGetValue(path, out childListeners);
                if (childListeners != null && !childListeners.IsEmpty)
                {
                    FireChildChangedEvents(path, childListeners, @event.get_Type());
                }
            }

            if (@event.get_Type() == EventType.NodeDataChanged 
                || @event.get_Type() == EventType.NodeDeleted 
                || @event.get_Type() == EventType.NodeCreated)
            {
                ConcurrentHashSet<IZKDataListener> listeners;
                _dataListeners.TryGetValue(path, out listeners);
                if (listeners != null && !listeners.IsEmpty)
                {
                    FireDataChangedEvents(@event.getPath(), listeners, @event.get_Type());
                }
            }
        }

        private void ProcessStateChanged(WatchedEvent @event)
        {
            LOG.Info($"zookeeper state changed ({Convert.ToString(@event.getState()) })");
            SetCurrentState(@event.getState());
            if (_shutdownTriggered)
            {
                return;
            }
            FireStateChangedEvent(@event.getState());
            if (@event.getState() == KeeperState.Expired)
            {
                try
                {
                    FireSessionExpiredEvents(@event.getPath());
                    ReConnect(true);
                    FireNewSessionEvents();
                }
                catch (Exception e)
                {
                    LOG.Info("Unable to re-establish connection. Notifying consumer of the following exception: ", e);
                    FireSessionEstablishmentError(e);
                }
            }
        }

        private void FireAllEvents(EventType eventType)
        {
            foreach (var _key in _childListeners.Keys)
            {
                var key = _key;
                ConcurrentHashSet<IZKChildListener> childListenes;
                if (_childListeners.TryGetValue(key, out childListenes))
                {
                    FireChildChangedEvents(key, childListenes, eventType);
                }
            }
            foreach (var _key in _dataListeners.Keys)
            {
                var key = _key;
                ConcurrentHashSet<IZKDataListener> dataListeners;
                if (_dataListeners.TryGetValue(key, out dataListeners))
                {
                    FireDataChangedEvents(key, dataListeners, eventType);
                }
            }
        }

        private void FireDataChangedEvents(string path, ConcurrentHashSet<IZKDataListener> dataListeners, EventType eventType)
        {
            foreach (var dataListener in dataListeners)
            {
                _eventTask.Send(new ZKTask.ZKEvent()
                {
                    Run = async () =>
                    {
                        // reinstall watch
                        await ExistsAsync(path, true);
                        try
                        {
                            var data = await GetDataAsync<object>(path,  true);
                            if (eventType == EventType.NodeCreated)
                            {
                                if (dataListener.DataCreatedHandler != null)
                                    await dataListener.DataCreatedHandler(path, data);
                            }
                            else
                            {
                                if (dataListener.DataChangeHandler != null)
                                    await dataListener.DataChangeHandler(path, data);
                            }
                            if (dataListener.DataCreatedOrChangeHandler != null)
                                await dataListener.DataCreatedOrChangeHandler(path, data);
                        }
                        catch (NoNodeException)
                        {
                            if (dataListener.DataDeletedHandler != null)
                                await dataListener.DataDeletedHandler(path);
                        }
                    }
                });
            }
        }

        private void FireChildChangedEvents(string path, ConcurrentHashSet<IZKChildListener> childListeners, EventType eventType)
        {
            try
            {
                // reinstall the watch
                foreach (var childListener in childListeners)
                {
                    _eventTask.Send(new ZKTask.ZKEvent()
                    {
                        Run = async () =>
                        {
                            try
                            {
                                // if the node doesn't exist we should listen for the root node to reappear
                                await ExistsAsync(path);
                                var children = await GetChildrenAsync(path);
                                if (childListener.ChildChangeHandler != null)
                                    await childListener.ChildChangeHandler(path, children);
                                //子节点个数变化
                                if (eventType == EventType.NodeChildrenChanged
                                   || eventType == EventType.NodeCreated
                                   || eventType == EventType.NodeDeleted)
                                {
                                    if (childListener.ChildCountChangedHandler != null)
                                        await childListener.ChildCountChangedHandler(path, children);
                                }
                            }
                            catch (NoNodeException)
                            {
                                if (childListener.ChildChangeHandler != null)
                                    await childListener.ChildChangeHandler(path, null);
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                LOG.Error("Failed to fire child changed event. Unable to getChildren.  ", e);
            }
        }

        private void FireSessionExpiredEvents(string path)
        {
            foreach (var stateListener in _stateListeners)
            {
                _eventTask.Send(new ZKTask.ZKEvent()
                {
                    Run = async () =>
                    {
                        if (stateListener.SessionExpiredHandler != null)
                            await stateListener.SessionExpiredHandler(path);
                    }
                });
            }
        }

        private void FireNewSessionEvents()
        {
            foreach (var stateListener in _stateListeners)
            {
                _eventTask.Send(new ZKTask.ZKEvent()
                {
                    Run = async () =>
                    {
                        if (stateListener.NewSessionHandler != null)
                            await stateListener.NewSessionHandler();
                    }
                });
            }
        }

        private void FireStateChangedEvent(KeeperState state)
        {
            foreach (var stateListener in _stateListeners)
            {
                _eventTask.Send(new ZKTask.ZKEvent()
                {
                    Run = async () =>
                    {
                        if (stateListener.StateChangedHandler != null)
                            await stateListener.StateChangedHandler(state);
                    }
                });
            }
        }

        private void FireSessionEstablishmentError(Exception error)
        {
            foreach (var stateListener in _stateListeners)
            {
                _eventTask.Send(new ZKTask.ZKEvent()
                {
                    Run = async () =>
                     {
                         if (stateListener.SessionEstablishmentErrorHandler != null)
                             await stateListener.SessionEstablishmentErrorHandler(error);
                     }
                });
            }
        }

        private bool HasListeners(string path)
        {
            ConcurrentHashSet<IZKDataListener> dataListeners = null;
            _dataListeners.TryGetValue(path, out dataListeners);
            if (dataListeners != null && dataListeners.Count > 0)
            {
                return true;
            }
            ConcurrentHashSet<IZKChildListener> childListeners = null;
            _childListeners.TryGetValue(path, out childListeners);
            if (childListeners != null && childListeners.Count > 0)
            {
                return true;
            }
            return false;
        }

        public int NumberOfListeners()
        {
            int listeners = 0;
            foreach (var childListeners in _childListeners.Values)
            {
                listeners += childListeners.Count;
            }
            foreach (var dataListeners in _dataListeners.Values)
            {
                listeners += dataListeners.Count;
            }
            listeners += _stateListeners.Count;
            return listeners;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
