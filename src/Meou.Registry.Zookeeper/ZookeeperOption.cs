using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperOption
    {
        public string ConnectString { get; set; } = "127.0.0.1";
        public int SessionTimeout { get; set; } = 60 * 1000;
        public int ConnectionTimeout { get; set; } = 60 * 1000;
    }
}
