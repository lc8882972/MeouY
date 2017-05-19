using Meou.Registry.Abstractions;
using Rabbit.Rpc.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Meou.RPC.Abstractions.Routing.Implementation
{
    public class RouteNotifyListener : NotifyListener
    {
        private ServiceRoute _routes;
        private RegistryService _registryService;

        public RouteNotifyListener(RegistryService registryService, ServiceRoute routes)
        {
            _registryService = registryService;
            _routes = routes;
        }
        public Task Notify(RegisterMeta registerMeta)
        {
            

            return Task.CompletedTask;
        }
    }
}
