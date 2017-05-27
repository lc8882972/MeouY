using Meou.Registry.Abstractions;
using Meou.Registry.Zookeeper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ZookeeperRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddZookeeperRegistry(this IServiceCollection services)
        {
            return AddZookeeperRegistry(services, (option) => { });
        }
        public static IServiceCollection AddZookeeperRegistry(this IServiceCollection services, Action<ZookeeperOption> configureOptions)
        {
            services.AddOptions();
            var options = new ZookeeperOption();
            if (configureOptions != null)
            {
                configureOptions(options);
            }
            services.Configure(configureOptions);
            services.AddSingleton<RegistryService, ZookeeperRegistryService>();
            return services;
        }
    }
}
