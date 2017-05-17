using System;
using System.Collections.Generic;
using log4net;
using org.apache.zookeeper.data;
using org.apache.zookeeper;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;

namespace ZooKeeperClient.Connection
{
    public class ZKConnection : IZKConnection
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ZKConnection));

        private static TimeSpan DEFAULT_SESSION_TIMEOUT = new TimeSpan(0, 0, 0, 0, 3000);

        private ZooKeeper _zooKeeper { set; get; } = null;
   
        private TimeSpan _sessionTimeOut { set; get; }

        private object _zookeeperLock = new object();

        public string servers { set; get; }

        public ZKConnection(string zkServers) : this(zkServers, DEFAULT_SESSION_TIMEOUT)
        {
            servers = zkServers;
        }

        public ZKConnection(string zkServers, TimeSpan sessionTimeOut)
        {
            servers = zkServers;
            _sessionTimeOut = sessionTimeOut;
        }

        public void Connect(Watcher watcher)
        {
            lock (_zookeeperLock)
            {
                if (_zooKeeper != null)
                {
                    throw new Exception("zk client has already been started");
                }
                LOG.Debug($"Creating new ZookKeeper instance to connect to {servers}");
                _zooKeeper = new ZooKeeper(servers, (int)_sessionTimeOut.TotalMilliseconds, watcher);

            }
        }

        public void ReConnect(Watcher watcher)
        {
            Close();
            Connect(watcher);
        }

        public void Close()
        {
            lock (_zookeeperLock)
            {
                if (_zooKeeper != null)
                {
                    LOG.Debug($"Closing ZooKeeper connected to {servers}");
                    Task.Run(async () =>
                    {
                        await _zooKeeper.closeAsync().ConfigureAwait(false);
                    }).ConfigureAwait(false).GetAwaiter().GetResult();

                    _zooKeeper = null;
                }
            }
        }

        public async Task<string> CreateAsync(string path, byte[] data, CreateMode mode)
        {
            return await _zooKeeper.createAsync(path, data, Ids.OPEN_ACL_UNSAFE, mode);
        }

        public async Task<string> CreateAsync(string path, byte[] data, List<ACL> acl, CreateMode mode)
        {
            return await _zooKeeper.createAsync(path, data, acl, mode);
        }

        public async Task DeleteAsync(string path)
        {
            await _zooKeeper.deleteAsync(path, -1);
        }

        public async Task DeleteAsync(string path, int version)
        {
            await _zooKeeper.deleteAsync(path, version);
        }

        public async Task<bool> ExistsAsync(string path, bool watch)
        {
            return await _zooKeeper.existsAsync(path, watch) != null;
        }

        public async Task<List<string>> GetChildrenAsync(string path, bool watch)
        {
            return (await _zooKeeper.getChildrenAsync(path, watch)).Children;
        }

        public async Task<DataResult> GetDataAsync(string path, bool watch)
        {
            return await _zooKeeper.getDataAsync(path, watch);
        }

        public async Task SetDataAsync(string path, byte[] data)
        {
            await SetDataAsync(path, data, -1);
        }

        public async Task SetDataAsync(string path, byte[] data, int version)
        {
            await _zooKeeper.setDataAsync(path, data, version);
        }

        public async Task<Stat> SetDataReturnStatAsync(string path, byte[] data, int expectedVersion)
        {
            return await _zooKeeper.setDataAsync(path, data, expectedVersion);
        }

        public ZooKeeper.States GetZookeeperState()
        {
            return _zooKeeper.getState();
        }

        public async Task<long> GetCreateTimeAsync(string path)
        {
            Stat stat = await _zooKeeper.existsAsync(path, false);
            if (stat != null)
            {
                return stat.getCtime();
            }
            return -1;
        }

        public void AddAuthInfo(string scheme, byte[] auth)
        {
            _zooKeeper.addAuthInfo(scheme, auth);
        }

        public async Task SetACLAsync(string path, List<ACL> acl, int version)
        {
            await _zooKeeper.setACLAsync(path, acl, version);
        }

        public async Task<ACLResult> GetACLAsync(string path)
        {
            return await _zooKeeper.getACLAsync(path);
        }   
    }
}
