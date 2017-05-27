using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ZooKeeperClient.Client;
using ZooKeeperClient.Listener;
using Microsoft.Extensions.Logging;
using Meou.Registry.Abstractions;
using Microsoft.Extensions.Options;
using Meou.Common;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperRegistryService : AbstractRegistryService
    {
        public ZookeeperOption Options { get; set; }

        private ConcurrentDictionary<Address, HashSet<ServiceMeta>> serviceMetaMap = new ConcurrentDictionary<Address, HashSet<ServiceMeta>>();
        private ZKClient configClient;

        public ZookeeperRegistryService(IOptions<ZookeeperOption> options,ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            this.Options = options.Value;

            configClient = ZKClientBuilder.NewZKClient(this.Options.ConnectString)
                           .SessionTimeout(this.Options.SessionTimeout)//可选  
                           .ConnectionTimeout(this.Options.ConnectionTimeout)//可选  
                           .Build(); //创建实例
        }

        public override Collection<RegisterMeta> lookup(ServiceMeta serviceMeta)
        {
            string directory = $"/meou/provider/{serviceMeta.getGroup()}/{serviceMeta.getName()}/{serviceMeta.getVersion()}";
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
            string directory = $"/meou/provider/{meta.getGroup()}/{meta.getName()}/{meta.getVersion()}";
            try
            {
                var result = configClient.ExistsAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (!result)
                {
                    configClient.CreateRecursiveAsync(directory, null, org.apache.zookeeper.CreateMode.PERSISTENT).ConfigureAwait(false).GetAwaiter().GetResult();
                }
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
                string tempPath = $"{directory}/{meta.getHost()}:{meta.getPort()}:{meta.getWeight()}:{meta.getConnCount()}";

                // The znode will be deleted upon the client's disconnect.
                configClient.CreateEphemeralAsync(tempPath).Wait();
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"Create register meta: {meta} path failed, {e}.");
                }
            }
        }

        protected override void doSubscribe(ServiceMeta serviceMeta)
        {
            string directory = $"/meou/provider/{serviceMeta.getGroup()}/{serviceMeta.getName()}/{serviceMeta.getVersion()}";
            IZKChildListener childListener = new ZKChildListener();
            childListener.ChildChangeHandler =  (parentPath, currentChilds) =>
            {
                var list = new List<RegisterMeta>(currentChilds.Count);
                RegisterMeta temp =null;
                foreach (var child in currentChilds)
                {
                    temp = parseRegisterMeta($"{parentPath}/{child}");
                    list.Add(temp);
                }
      
                 notify(serviceMeta,list);

                return Task.CompletedTask;
            };

            //childListener.ChildCountChangedHandler = async (parentPath, currentChilds) =>
            //{
            //    await notify(serviceMeta, NotifyEvent.CHILD_ADDED, 1L, new List<RegisterMeta>());
            //};
            configClient.SubscribeChildChanges(directory, childListener);

        }

        protected override void doUnregister(RegisterMeta meta)
        {
            string directory = $"/meou/provider/{meta.getGroup()}/{meta.getName()}/{meta.getVersion()}";

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
                //meta.setHost(address);
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
            meta.setName(array_0[4]);
            meta.setVersion(array_0[5]);

            String[] array_1 = array_0[6].Split(':');
            meta.setHost(array_1[0]);
            meta.setPort(Convert.ToInt32(array_1[1]));
            meta.setWeight(Convert.ToInt32(array_1[2]));
            meta.setConnCount(Convert.ToInt32(array_1[3]));

            return meta;
        }

        private HashSet<ServiceMeta> getServiceMeta(Address address)
        {
            HashSet<ServiceMeta> serviceMetaSet;
            serviceMetaMap.TryGetValue(address,out serviceMetaSet);
            if (serviceMetaSet == null)
            {
                HashSet<ServiceMeta> newServiceMetaSet = new HashSet<ServiceMeta>();
                serviceMetaMap.TryAdd(address, newServiceMetaSet);
                if (serviceMetaSet == null)
                {
                    serviceMetaSet = newServiceMetaSet;
                }
            }
            return serviceMetaSet;
        }

        public override void destroy()
        {
            configClient.Close();
        }
    }
}
