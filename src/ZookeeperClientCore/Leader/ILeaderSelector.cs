using System;
using System.Threading.Tasks;

namespace ZooKeeperClient.Leader
{
    public interface ILeaderSelector : IDisposable
    {
        /// <summary>
        /// 启动参与选举Leader
        /// </summary>
        void Start();

        /// <summary>
        /// 重新添加当前线程到选举队列
        /// </summary>
        void Requeue();

        /// <summary>
        /// 获得Leader ID
        /// </summary>
        /// <returns></returns>
        Task<string> GetLeaderAsync();

        /// <summary>
        /// 是否是Leader
        /// </summary>
        /// <returns></returns>
        Task<bool> IsLeaderAsync();

        /// <summary>
        /// 终止等待成为Leader
        /// </summary>
        void InterruptLeadership();

        /// <summary>
        /// 关闭Leader选举
        /// </summary>
        void Close();
    }
}
