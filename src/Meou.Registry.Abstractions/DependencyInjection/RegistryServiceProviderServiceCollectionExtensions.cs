using System;
using System.Collections.Generic;
using System.Text;
using Meou.Registry.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RegistryServiceProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceProvider(this IServiceCollection services)
        {
            services.AddSingleton<IRegisterMetaDiscoveryProvider>(new DefaultRegisterMetaDiscoveryProvider());
            return services;
        }

        public static IServiceCollection AddServiceConsumer(this IServiceCollection services)
        {
            services.AddSingleton<IRegisterMetaDiscoveryProvider>(new DefaultRegisterMetaDiscoveryProvider());
            return services;
        }
    }
}
