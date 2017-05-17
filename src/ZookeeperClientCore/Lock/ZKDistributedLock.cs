using log4net;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperClient.Client;
using ZooKeeperClient.Listener;
using static org.apache.zookeeper.KeeperException;

namespace ZooKeeperClient.Lock
{
    /// <summary>
    /// 分布式锁
    /// 非线程安全，每个线程请单独创建实例
    /// </summary>
    public class ZKDistributedLock : IZKLock
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ZKDistributedLock));
        private IZKChildListener countListener;
        private ZKClient _zkClient;
        private string _lockPath;
        private string currentSeq;
        private Semaphore semaphore;
        public string lockNodeData;

        public ZKDistributedLock(ZKClient zkClient, string lockPach)
        {
            if (!(zkClient.ExistsAsync(lockPach).GetAwaiter().GetResult()))
            {
                throw new NoNodeException($"The lockPath is not exists!,please create the node.[path:{_lockPath}]");
            }
            this._zkClient = zkClient;
            this._lockPath = lockPach;
            this.countListener = new ZKChildListener();
            this.countListener.ChildChangeHandler = async (parentPath, currentChilds) =>
              {
                  var check = await Task.Run(() => Check(currentSeq, currentChilds));
                  if (check)
                  {
                      semaphore.Release();
                  }
              };
            this._zkClient.SubscribeChildChanges(_lockPath, countListener);
        }

        /// <summary>
        /// 判断路径是否可以获得锁，如果checkPath 对应的序列是所有子节点中最小的，则可以获得锁。
        /// </summary>
        /// <param name="checkPath"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        private bool Check(string checkPath, List<string> children)
        {
            if (children == null || !children.Contains(checkPath))
            {
                return false;
            }
            //判断checkPath 是否是children中的最小值，如果是返回true，不是返回false
            var chePathSeq = long.Parse(checkPath);
            var isLock = true;
            foreach (var path in children)
            {
                var pathSeq = long.Parse(path);
                if (chePathSeq > pathSeq)
                {
                    isLock = false;
                    break;
                }
            }
            return isLock;
        }


        public Task<bool> LockAsync()
        {
            return LockAsync(0);
        }

        /// <summary>
        /// 获得锁
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// 如果超时间大于0，则会在超时后直接返回false。
        /// 如果超时时间小于等于0，则会等待直到获取锁为止。
        /// </param>
        /// <returns>成功获得锁返回true，否则返回false</returns>
        public async Task<bool> LockAsync(int millisecondsTimeout)
        {          
            semaphore = new Semaphore(1, 1);
            var newPath = await _zkClient.CreateAsync(_lockPath + "/1", lockNodeData, CreateMode.EPHEMERAL_SEQUENTIAL);
            var paths = newPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            currentSeq = paths[paths.Length - 1];
            bool getLock = false;

            if (millisecondsTimeout > 0)
            {
                getLock = semaphore.WaitOne(millisecondsTimeout);
            }
            else
            {
                semaphore.WaitOne();
                getLock = true;
            }
            if (getLock)
            {
                LOG.Debug("get lock successful.");
            }
            else
            {
                LOG.Debug("failed to get lock.");
            }
            return getLock;
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <returns>
        /// 如果释放锁成功返回true，否则返回false
        ///释放锁失败只有一种情况，就是线程正好获得锁，在释放之前，
        /// 与服务器断开连接，这时候服务器会自动删除EPHEMERAL_SEQUENTIAL节点。
        ///在会话过期之后再删除节点就会删除失败，因为路径已经不存在了。
        /// </returns>
        public async Task<bool> UnLockAsync()
        {
            _zkClient.UnSubscribeChildChanges(_lockPath, countListener);
            return await _zkClient.DeleteAsync(_lockPath + "/" + currentSeq);
        }

        /// <summary>
        /// 获得所有参与者的节点名称
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetParticipantNodes()
        {
            var children = await _zkClient.GetChildrenAsync(_lockPath);
            children.Sort(CompareRow);
            return children;
        }

        private int CompareRow(string lhs, string rhs)
        {
            return lhs.CompareTo(rhs);
        }

    }
}
