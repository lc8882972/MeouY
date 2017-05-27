using Meou.Registry.Abstractions;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.HealthChecks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Meou.Registry.Zookeeper;
using Meou.Common;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 一个人默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region Field
        private readonly IRegisterMetaDiscoveryProvider _serviceProvider;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly ILogger<DefaultAddressResolver> _logger;
        private readonly IAddressSelector _addressSelector;
        private readonly IHealthCheckService _healthCheckService;
        private readonly RegistryService _registryService;
        private ConcurrentDictionary<ServiceMeta, Collection<RegisterMeta>> _registerMetaList =new ConcurrentDictionary<ServiceMeta, Collection<RegisterMeta>>();
        #endregion Field

        #region Constructor

        public DefaultAddressResolver(RegistryService registryService, IRegisterMetaDiscoveryProvider serviceProvider, IServiceRouteManager serviceRouteManager, ILogger<DefaultAddressResolver> logger, IAddressSelector addressSelector, IHealthCheckService healthCheckService)
        {
            _registryService = registryService;
            _serviceProvider = serviceProvider;
            _serviceRouteManager = serviceRouteManager;
            _logger = logger;
            _addressSelector = addressSelector;
            _healthCheckService = healthCheckService;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        public async Task<AddressModel> Resolver(string serviceId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务id：{serviceId}，解析可用地址。");
            //var descriptors = await _serviceRouteManager.GetRoutesAsync();
            //var descriptor = descriptors.FirstOrDefault(i => i.ServiceDescriptor.Id == serviceId);

            //if (descriptor == null)
            //{
            //    if (_logger.IsEnabled(LogLevel.Warning))
            //        _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
            //    return null;
            //}

            var address = Parse(serviceId);

            var hasAddress = address.Any();
            if (!hasAddress)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务id：{serviceId}，找不到可用的地址。");
                return null;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"根据服务id：{serviceId}，找到以下可用地址：{string.Join(",", address.Select(i => i.ToString()))}。");

            return await _addressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = new ServiceDescriptor() {  Id = serviceId},
                Address = address
            });
        }

        private List<AddressModel> Parse(string serviceId)
        {
            var address = new List<AddressModel>();
            string[] array = serviceId.Split('.');
            Collection<RegisterMeta> registerMeta = null;
            ServiceMeta temp = new ServiceMeta();
            temp.setName(array[1]);
            temp.setGroup(array[0]);
            temp.setVersion("0.0.0");
            if (!_registerMetaList.ContainsKey(temp)) {
                registerMeta = _registryService.lookup(temp);
                _registerMetaList.GetOrAdd(temp, registerMeta);
            }else {
                registerMeta = _registerMetaList[temp];
            }
            _registryService.subscribe(temp, new ZookeeperNotifyListener((meta) =>
            {
                if (!_registerMetaList.ContainsKey(temp))
                {
                    _registerMetaList.TryAdd(temp, registerMeta);
                }
                else
                {
                    _registerMetaList.TryUpdate(temp, registerMeta , _registerMetaList[temp]);
                    registerMeta = _registerMetaList[temp];
                }
            }));

            foreach (var item in registerMeta)
            {
                address.Add(new IpAddressModel()
                {
                    Ip = item.getHost(),
                    Port = item.getPort()
                });
            }

            return address;
        }
        #endregion Implementation of IAddressResolver
    }
}