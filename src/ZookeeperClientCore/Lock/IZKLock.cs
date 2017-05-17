using System;
using System.Threading.Tasks;

namespace ZooKeeperClient.Lock
{
    public interface IZKLock
    {
        /// <summary>
        /// 获得锁
        /// </summary>
        /// <returns></returns>
        Task<bool> LockAsync();

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <returns></returns>
        Task<bool> UnLockAsync();
    }
}
