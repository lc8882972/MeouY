using System;
using System.Collections.Generic;
using System.Text;
using Meou.Registry.Abstractions;
using Microsoft.Extensions.Logging;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperRegistryServiceBuilder : IRegistryServiceBuilder
    {
        private string _connectString;
        public ZookeeperRegistryServiceBuilder(string connectString)
        {
            _connectString = connectString;
        }
        public RegistryService Builder()
        {
            return new ZookeeperRegistryService(new LoggerFactory().AddDebug());
        }
    }
}
