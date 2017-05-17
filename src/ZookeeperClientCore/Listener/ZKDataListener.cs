using System;
using System.Threading.Tasks;

namespace ZooKeeperClient.Listener
{
    public class ZKDataListener : IZKDataListener
    {
        public Func<string, object, Task> DataCreatedOrChangeHandler { set; get; }

        public Func<string, object, Task> DataCreatedHandler { set; get; }

        public Func<string, object, Task> DataChangeHandler { set; get; }

        public Func<string, Task> DataDeletedHandler { set; get; }
    }
}