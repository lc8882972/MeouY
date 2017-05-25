using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using static Meou.Registry.Abstractions.RegisterMeta;
using Microsoft.Extensions.Logging;

namespace Meou.Registry.Abstractions
{
    public abstract class AbstractRegistryService : RegistryService
    {
        public ILogger logger;

        public AbstractRegistryService(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<AbstractRegistryService>();
        }

        private bool shutdown = false;

        private BlockingCollection<RegisterMeta> queue = new BlockingCollection<RegisterMeta>();
        private Dictionary<RegisterMeta.ServiceMeta, KeyValuePair<long, List<RegisterMeta>>> registries = new Dictionary<RegisterMeta.ServiceMeta, KeyValuePair<long, List<RegisterMeta>>>();

        private ConcurrentDictionary<RegisterMeta.ServiceMeta, BlockingCollection<INotifyListener>> subscribeListeners = new ConcurrentDictionary<RegisterMeta.ServiceMeta, BlockingCollection<INotifyListener>>();
        private ConcurrentDictionary<RegisterMeta.Address, BlockingCollection<OfflineListener>> offlineListeners = new ConcurrentDictionary<RegisterMeta.Address, BlockingCollection<OfflineListener>>();

        // Consumer已订阅的信息
        private HashSet<ServiceMeta> subscribeSet = new HashSet<ServiceMeta>();
        // Provider已发布的注册信息
        private HashSet<RegisterMeta> registerMetaSet = new HashSet<RegisterMeta>();

        private Thread thread;
        public virtual void connectToRegistryServer(string connectString)
        {
            System.Collections.Concurrent.BlockingCollection<OfflineListener> c = new BlockingCollection<OfflineListener>();
            
            this.thread = new Thread(() =>
            {
                while (!shutdown)
                {
                    RegisterMeta meta = null;
                    try
                    {
                        queue.TryTake(out meta);
                        //doRegister(meta);
                    }
                    catch (Exception e)
                    {
                        if (meta != null)
                        {
                            if (logger.IsEnabled(LogLevel.Warning))
                            {
                                logger.LogWarning($"Register [{meta.getServiceMeta()}] fail: { e}, will try again...");
                            }

                            queue.Add(meta);
                        }
                    }
                }
            });
        }

        public bool isShutdown()
        {
            return shutdown;
        }

        public void shutdownGracefully()
        {
            //Interlocked.CompareExchange(ref shutdown, true, false);
            if (!shutdown)
            {
                destroy();
            }
            shutdown = true;
        }

        public virtual Collection<RegisterMeta> lookup(RegisterMeta.ServiceMeta serviceMeta)
        {
            KeyValuePair<long, List<RegisterMeta>> data;

            registries.TryGetValue(serviceMeta, out data);

            if (data.Key != 0)
            {
                return new Collection<RegisterMeta>(data.Value);
            }
            return new Collection<RegisterMeta>();
        }

        public void register(RegisterMeta meta)
        {
            queue.Add(meta);
            doRegister(meta);
        }

        public void unregister(RegisterMeta meta)
        {            
            //if (!queue.(item))
            //{
            //    doUnregister(meta);
            //}
            doUnregister(meta);
        }

        public void subscribe(ServiceMeta serviceMeta, INotifyListener listener)
        {
            BlockingCollection<INotifyListener> listeners;
            subscribeListeners.TryGetValue(serviceMeta, out listeners);

            if (listeners == null)
            {
                BlockingCollection<INotifyListener> newListeners = new BlockingCollection<INotifyListener>();
                subscribeListeners.TryAdd(serviceMeta, newListeners);
                if (listeners == null)
                {
                    listeners = newListeners;
                }
            }
            listeners.Add(listener);

            doSubscribe(serviceMeta);
        }

        protected void notify(ServiceMeta serviceMeta, List<RegisterMeta> array)
        {
            BlockingCollection<INotifyListener> listeners;
            subscribeListeners.TryGetValue(serviceMeta, out listeners);
            if (listeners != null)
            {
                foreach (var item in listeners)
                {
                    item.Notify(array);
                }
            }
        }

        public void offlineListening(RegisterMeta.Address address, OfflineListener listener)
        {
            BlockingCollection<OfflineListener> listeners;
            offlineListeners.TryGetValue(address,out listeners);
            if (listeners == null)
            {
                BlockingCollection<OfflineListener> newListeners = new BlockingCollection<OfflineListener>();
                offlineListeners.TryAdd(address, newListeners);

                if (listeners == null)
                {
                    listeners = newListeners;
                }
            }
            listeners.TryAdd(listener);
        }
        public void offline(RegisterMeta.Address address)
        {
            // remove & notify
            BlockingCollection<OfflineListener> listeners;
            offlineListeners.TryGetValue(address,out listeners);
            if (listeners != null)
            {
                foreach (OfflineListener item in  listeners)
                {
                    item.Offline();
                }
            }
        }

        protected abstract void doRegister(RegisterMeta meta);
        protected abstract void doSubscribe(RegisterMeta.ServiceMeta serviceMeta);
        protected abstract void doUnregister(RegisterMeta meta);
        public abstract void destroy();
    }
}
