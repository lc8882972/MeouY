using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System.Collections.Generic;

namespace ZooKeeperClient.Client
{
    public class ZKNode
    {
        public ZKNode()
        { }

        public ZKNode(string path, object data, CreateMode createMode)
        {
            this.path = path;
            this.data = data;
            this.createMode = createMode;
        }

        public ZKNode(string path, object data, List<ACL> acl, CreateMode createMode)
        {
            this.path = path;
            this.data = data;
            this.acl = acl;
            this.createMode = createMode;
        }

        public string path { set; get; }

        public object data { set; get; }

        public List<ACL> acl { set; get; }

        public CreateMode createMode { set; get; }
    }
}
