using org.apache.zookeeper.data;

namespace ZooKeeperClient.Client
{
    public class ZKData<T>
    {
        public T data { set; get; }

        public Stat stat { set; get; }
    }
}
