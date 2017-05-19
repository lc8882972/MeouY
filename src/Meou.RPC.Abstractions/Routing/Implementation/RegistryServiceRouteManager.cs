using Rabbit.Rpc.Routing.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rabbit.Rpc.Routing;
using System.Threading.Tasks;
using Rabbit.Rpc.Serialization;
using Microsoft.Extensions.Logging;
using Meou.Registry.Abstractions;

namespace Meou.RPC.Abstractions.Routing.Implementation
{
    public class RegistryServiceRouteManager : ServiceRouteManagerBase, IDisposable
    {
        #region Field

        private readonly ISerializer<string> _serializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<RegistryServiceRouteManager> _logger;
        private ServiceRoute[] _routes;
        private RegistryService _registryService;
        #endregion Field

        public RegistryServiceRouteManager(
            IRegistryServiceBuilder registryServiceBuilder,
            ISerializer<string> serializer,
            IServiceRouteFactory serviceRouteFactory,
            ILogger<RegistryServiceRouteManager> logger) : base(serializer)
        {
            _registryService = registryServiceBuilder.Builder();
            _serializer = serializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;
        }
        public override Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _registryService.shutdownGracefully();
        }

        public override Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            if (_routes == null || _routes.Length == 0)
            {
               
            }
            return Task.FromResult<IEnumerable<ServiceRoute>>(_routes);
        }

        protected override Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            RegisterMeta meta =null;
            foreach (var item in routes)
            {
                meta = new RegisterMeta();
                foreach (var addr in item.AddressDescriptors)
                {
                    meta.setServiceProviderName(item.ServiceDescriptor.Name);
                    meta.setGroup(item.ServiceDescriptor.Group);
                    meta.setVersion(item.ServiceDescriptor.Version);
                    //_registryService.subscribe(meta, new RouteNotifyListener());
                    _registryService.register(meta);
                }

            }

            return Task.CompletedTask;
        }

        private ServiceRoute RegisterMetaPareRoute(RegisterMeta meta)
        {
            ServiceRoute temp = new ServiceRoute();
            return temp;
        }

        //private async Task EntryRoutes(RegisterMeta file)
        //{
        //    var oldRoutes = _routes?.ToArray();
        //    var newRoutes = _registryService.lookup()
        //    _routes = newRoutes;
        //    if (oldRoutes == null)
        //    {
        //        //触发服务路由创建事件。
        //        OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());
        //    }
        //    else
        //    {
        //        //旧的服务Id集合。
        //        var oldServiceIds = oldRoutes.Select(i => i.ServiceDescriptor.Id).ToArray();
        //        //新的服务Id集合。
        //        var newServiceIds = newRoutes.Select(i => i.ServiceDescriptor.Id).ToArray();

        //        //被删除的服务Id集合
        //        var removeServiceIds = oldServiceIds.Except(newServiceIds).ToArray();
        //        //新增的服务Id集合。
        //        var addServiceIds = newServiceIds.Except(oldServiceIds).ToArray();
        //        //可能被修改的服务Id集合。
        //        var mayModifyServiceIds = newServiceIds.Except(removeServiceIds).ToArray();

        //        //触发服务路由创建事件。
        //        OnCreated(
        //            newRoutes.Where(i => addServiceIds.Contains(i.ServiceDescriptor.Id))
        //                .Select(route => new ServiceRouteEventArgs(route))
        //                .ToArray());

        //        //触发服务路由删除事件。
        //        OnRemoved(
        //            oldRoutes.Where(i => removeServiceIds.Contains(i.ServiceDescriptor.Id))
        //                .Select(route => new ServiceRouteEventArgs(route))
        //                .ToArray());

        //        //触发服务路由变更事件。
        //        var currentMayModifyRoutes =
        //            newRoutes.Where(i => mayModifyServiceIds.Contains(i.ServiceDescriptor.Id)).ToArray();
        //        var oldMayModifyRoutes =
        //            oldRoutes.Where(i => mayModifyServiceIds.Contains(i.ServiceDescriptor.Id)).ToArray();

        //        foreach (var oldMayModifyRoute in oldMayModifyRoutes)
        //        {
        //            if (!currentMayModifyRoutes.Contains(oldMayModifyRoute))
        //                OnChanged(
        //                    new ServiceRouteChangedEventArgs(
        //                        currentMayModifyRoutes.First(
        //                            i => i.ServiceDescriptor.Id == oldMayModifyRoute.ServiceDescriptor.Id),
        //                        oldMayModifyRoute));
        //        }
        //    }
        //}
    }
}
