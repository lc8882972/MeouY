using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Meou.Registry.Abstractions;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using ZooKeeperClient.Client;
using ZooKeeperClient.Listener;
using System.Threading.Tasks;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperRegistryService : AbstractRegistryService
    {
        public ZookeeperRegistryService(ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        public ZookeeperRegistryService(string connectString, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            address = connectString;
        }
        private string serviceAddr = "127.0.0.1:8022";
        private string address = "127.0.0.1:2181";
        private int sessionTimeout = 60 * 1000;
        private int connectionTimeout = 15 * 1000;
        //private  ConcurrentMap<RegisterMeta.ServiceMeta, PathChildrenCache> pathChildrenCaches = Maps.newConcurrentMap();
        private ConcurrentDictionary<RegisterMeta.Address, HashSet<RegisterMeta.ServiceMeta>> serviceMetaMap = new ConcurrentDictionary<RegisterMeta.Address, HashSet<RegisterMeta.ServiceMeta>>();
        private ZKClient configClient;

        public override void connectToRegistryServer(String connectString)
        {
            configClient = ZKClientBuilder.NewZKClient(connectString)
                                       .SessionTimeout(sessionTimeout)//可选  
                                       .ConnectionTimeout(connectionTimeout)//可选  
                                       .Build(); //创建实例
        }

        public override Collection<RegisterMeta> lookup(RegisterMeta.ServiceMeta serviceMeta)
        {
            string directory = $"/jupiter/provider/{serviceMeta.getGroup()}/{serviceMeta.getVersion()}/{serviceMeta.getServiceProviderName()}";
            List<RegisterMeta> registerMetaList = new List<RegisterMeta>();
            try
            {
                var children = configClient.GetChildrenAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
             
                foreach (string p in children)
                {
                    registerMetaList.Add(parseRegisterMeta($"{directory}/{p}"));
                }
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Lookup service meta: {serviceMeta} path failed, {e}.");
                }
            }
            return new Collection<RegisterMeta>(registerMetaList);
        }

        protected override void doRegister(RegisterMeta meta)
        {
            string directory = $"/jupiter/provider/{meta.getGroup()}/{meta.getServiceProviderName()}/{meta.getVersion()}";
            try
            {
                var result = configClient.ExistsAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (!result)
                {
                    configClient.CreateRecursiveAsync(directory, null, org.apache.zookeeper.CreateMode.EPHEMERAL).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                //configClient.CreateEphemeralAsync(directory, System.Text.Encoding.UTF8.GetBytes("")).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Create parent path failed, directory: {directory}, {e}.");
                }
            }

            try
            {
                //meta.setAddress(RegisterMeta.Address.Parse(serviceAddr));

                string tempPath = $"{directory}/{meta.getHost()}:{meta.getPort()}:{meta.getWeight()}:{meta.getConnCount()}";

                // The znode will be deleted upon the client's disconnect.
                configClient.CreateEphemeralAsync(tempPath, System.Text.Encoding.UTF8.GetBytes("")).Wait();
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Create register meta: {meta} path failed, {e}.");
                }
            }
        }

        protected override void doSubscribe(RegisterMeta.ServiceMeta serviceMeta)
        {
            string directory = $"/jupiter/provider/{serviceMeta.getGroup()}/{serviceMeta.getServiceProviderName()}/{serviceMeta.getVersion()}";
            IZKChildListener childListener = new ZKChildListener();
            childListener.ChildChangeHandler = async (parentPath, currentChilds) =>
            {
                var list = new List<RegisterMeta>(currentChilds.Count);
                RegisterMeta temp =null;
                foreach (var child in currentChilds)
                {
                    temp = parseRegisterMeta($"{parentPath}/{child}");
                    list.Add(temp);
                }
      
                await notify(serviceMeta,NotifyEvent.CHILD_ADDED,1L,list);
            };

            //childListener.ChildCountChangedHandler = async (parentPath, currentChilds) =>
            //{
            //    await notify(serviceMeta, NotifyEvent.CHILD_ADDED, 1L, new List<RegisterMeta>());
            //};
            configClient.SubscribeChildChanges(directory, childListener);

        }

        protected override void doUnregister(RegisterMeta meta)
        {
            string directory = $"/jupiter/provider/{meta.getGroup()}/{meta.getServiceProviderName()}/{meta.getVersion()}";

            try
            {
                var result = configClient.ExistsAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (result)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Check exists with parent path failed, directory: { directory},{e}.");
                }
            }

            try
            {
                meta.setHost(address);
                string tempPath = $"{directory}/{meta.getHost()}:{meta.getPort()}:{meta.getWeight()}:{meta.getConnCount()}";

                // The znode will be deleted upon the client's disconnect.
                configClient.DeleteAsync(tempPath).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Delete register meta: {meta} path failed, {e}.");
                }
            }
        }

        private RegisterMeta parseRegisterMeta(String data)
        {
            String[] array_0 = data.Split('/');
            RegisterMeta meta = new RegisterMeta();
            meta.setGroup(array_0[3]);
            meta.setServiceProviderName(array_0[4]);
            meta.setVersion(array_0[5]);

            String[] array_1 = array_0[6].Split(':');
            meta.setHost(array_1[0]);
            meta.setPort(Convert.ToInt32(array_1[1]));
            meta.setWeight(Convert.ToInt32(array_1[2]));
            meta.setConnCount(Convert.ToInt32(array_1[3]));

            return meta;
        }

        public override void destroy()
        {
            configClient.Close();
        }
    }
}
