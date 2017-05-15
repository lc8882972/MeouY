using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public static class RegistryServiceProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceProvider(this IServiceCollection services)
        {

            return services;
        }
    }
}
