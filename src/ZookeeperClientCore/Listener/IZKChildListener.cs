using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZooKeeperClient.Listener
{
    public interface IZKChildListener
    {
        /// <summary>
        /// 子节点数量变化
        /// parentPath
        /// currentChilds
        /// </summary>
        Func<string, List<string>, Task> ChildCountChangedHandler { set; get; }

        /// <summary>
        /// 子节点内容变化
        /// parentPath
        /// currentChilds
        /// </summary>
        Func<string, List<string>, Task> ChildChangeHandler { set; get; }
    }
}
