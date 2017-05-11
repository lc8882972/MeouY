using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Meou.Registry.Abstractions;
using org.apache.zookeeper;
using System.Collections.ObjectModel;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperRegistryService : AbstractRegistryService
    {
        private readonly string address = "127.0.0.1:2181";
        private int sessionTimeoutMs = 60 * 1000;
        private int connectionTimeoutMs =15 * 1000;
        //private  ConcurrentMap<RegisterMeta.ServiceMeta, PathChildrenCache> pathChildrenCaches = Maps.newConcurrentMap();
        private ConcurrentDictionary<RegisterMeta.Address, HashSet<RegisterMeta.ServiceMeta>> serviceMetaMap = new ConcurrentDictionary<Address, HashSet<ServiceMeta>>();
        private  ZooKeeper configClient;

        public override Collection<RegisterMeta> lookup(RegisterMeta.ServiceMeta serviceMeta)
        {
            string directory = $"/jupiter/provider/{serviceMeta.getGroup()}/{serviceMeta.getVersion()}/{serviceMeta.getServiceProviderName()}";
            List<RegisterMeta> registerMetaList = new List<RegisterMeta>();
            try
            {
                var children = configClient.getChildrenAsync(directory, false).ConfigureAwait(false).GetAwaiter().GetResult();
                List<String> paths = children.Children;
                foreach (string p in paths)
                {
                    registerMetaList.Add(parseRegisterMeta($"{directory}/{p}"));
                }
            }
            catch (Exception e)
            {
                //if (logger.isWarnEnabled())
                //{
                //    logger.warn("Lookup service meta: {} path failed, {}.", serviceMeta, stackTrace(e));
                //}
            }
            return new Collection<RegisterMeta>(registerMetaList);
        }

        protected override void doRegister(RegisterMeta meta)
        {
           
            throw new NotImplementedException();
        }

        protected override void doSubscribe(RegisterMeta.ServiceMeta serviceMeta)
        {
            throw new NotImplementedException();
        }

        protected override void doUnregister(RegisterMeta meta)
        {
            throw new NotImplementedException();
        }

        private RegisterMeta parseRegisterMeta(String data)
        {
            String[] array_0 = data.Split('/');
            RegisterMeta meta = new RegisterMeta();
            meta.setGroup(array_0[2]);
            meta.setServiceProviderName(array_0[3]);
            meta.setVersion(array_0[4]);

            String[] array_1 = array_0[5].Split(':');
            meta.setHost(array_1[0]);
            meta.setPort(Convert.ToInt32(array_1[1]));
            meta.setWeight(Convert.ToInt32(array_1[2]));
            meta.setConnCount(Convert.ToInt32(array_1[3]));

            return meta;
        }
    }
}
