﻿using Meou.Registry.Abstractions;
using Meou.Registry.Zookeeper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
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