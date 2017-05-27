//using System;
//using System.Collections.Generic;
//using System.Text;
//using Meou.Registry.Abstractions;
//using Microsoft.Extensions.Logging;

//namespace Meou.Registry.Zookeeper
//{
//    public class ZookeeperRegistryServiceBuilder : IRegistryServiceBuilder
//    {
//        private LoggerFactory loggerFactory;
//        public ZookeeperRegistryServiceBuilder(LoggerFactory loggerFactory)
//        {
//            this.loggerFactory = loggerFactory;
//        }
//        public RegistryService Builder()
//        {
//            return new ZookeeperRegistryService(this.loggerFactory);
//        }
//    }
//}
