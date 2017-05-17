using System;
using System.Threading.Tasks;

namespace ZooKeeperClient.Listener
{
    public interface IZKDataListener
    {
        /// <summary>
        /// 节点创建和节点内容变化
        /// dataPath
        /// data
        /// </summary>
        Func<string, object, Task> DataCreatedOrChangeHandler { set; get; }

        /// <summary>
        /// 节点创建
        /// dataPath
        /// data
        /// </summary>
        Func<string, object, Task> DataCreatedHandler { set; get; }

        /// <summary>
        /// 节点内容变化
        /// dataPath
        /// data
        /// </summary>
        Func<string, object, Task> DataChangeHandler { set; get; }

        /// <summary>
        /// 节点删除
        /// dataPath
        /// </summary>
        Func<string, Task> DataDeletedHandler { set; get; }
    }
}
