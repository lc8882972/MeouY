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
            
        }

        public override Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            List<ServiceRoute> routes = new List<ServiceRoute>();
            routes.Add(new ServiceRoute() { ServiceDescriptor = new Rabbit.Rpc.ServiceDescriptor() { Id = "TestGroup.IUserService.0.0.0=>GetUserName_id" } });

            return Task.FromResult<IEnumerable<ServiceRoute>>(routes);
        }

        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            var result =await this._serviceRouteFactory.CreateServiceRoutesAsync(routes);
            this._routes = result.ToArray(); 
        }
    }
}
