using Meou.Registry.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Zookeeper
{
    public static class ZookeeperRegistryServiceCollectionExtensions
    {
        public static IServiceCollection AddZookeeperRegistry(this IServiceCollection services,string connstr)
        {
            services.AddSingleton<IRegistryServiceBuilder>(new ZookeeperRegistryServiceBuilder(connstr));
            return services;
        }
    }
}
