﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Meou.Registry.Abstractions;
using org.apache.zookeeper;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

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

        private string address = "127.0.0.1:2181";
        private int sessionTimeoutMs = 60 * 1000;
        private int connectionTimeoutMs = 15 * 1000;
        //private  ConcurrentMap<RegisterMeta.ServiceMeta, PathChildrenCache> pathChildrenCaches = Maps.newConcurrentMap();
        private ConcurrentDictionary<RegisterMeta.Address, HashSet<RegisterMeta.ServiceMeta>> serviceMetaMap = new ConcurrentDictionary<RegisterMeta.Address, HashSet<RegisterMeta.ServiceMeta>>();
        private ZooKeeper configClient;

        public override void connectToRegistryServer(String connectString)
        {
            configClient = new ZooKeeper(connectString, sessionTimeoutMs, null);
        }

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
                var result = configClient.existsAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (result == null)
                {
                    configClient.createAsync(directory, System.Text.Encoding.UTF8.GetBytes(""), ZooDefs.Ids.CREATOR_ALL_ACL, CreateMode.EPHEMERAL).GetAwaiter().GetResult();
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
                meta.setHost(address);
                string tempPath = $"{directory}/{meta.getHost()}:{meta.getPort()}:{meta.getWeight()}:{meta.getConnCount()}";

                // The znode will be deleted upon the client's disconnect.
                configClient.createAsync(tempPath, System.Text.Encoding.UTF8.GetBytes(""), ZooDefs.Ids.CREATOR_ALL_ACL, CreateMode.EPHEMERAL);
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
           
            configClient.getChildrenAsync(directory, new ChildWatcher());
        }

        protected override void doUnregister(RegisterMeta meta)
        {
            string directory = $"/jupiter/provider/{meta.getGroup()}/{meta.getServiceProviderName()}/{meta.getVersion()}";

            try
            {
                var result = configClient.existsAsync(directory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (result == null)
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
                configClient.deleteAsync(tempPath).ConfigureAwait(false).GetAwaiter().GetResult();
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

        public override void destroy()
        {
            configClient.closeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
