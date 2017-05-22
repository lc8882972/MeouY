using Meou.Registry.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RegistryServiceProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceProvider(this IServiceCollection services)
        {
            services.AddSingleton<IRegisterMetaDiscoveryProvider>(new DefaultRegisterMetaDiscoveryProvider());
            return services;
        }
    }
}
