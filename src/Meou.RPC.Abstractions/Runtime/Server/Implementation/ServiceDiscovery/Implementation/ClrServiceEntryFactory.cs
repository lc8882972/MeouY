using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Diagnostics;
using Meou.Common;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr服务条目工厂。
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly ILogger _logger;

        #endregion Field

        #region Constructor

        public ClrServiceEntryFactory(IServiceProvider serviceProvider, IServiceIdGenerator serviceIdGenerator, ITypeConvertibleService typeConvertibleService, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _serviceIdGenerator = serviceIdGenerator;
            _typeConvertibleService = typeConvertibleService;
            _logger = loggerFactory.CreateLogger<ClrServiceEntryFactory>();
        }

        #endregion Constructor

        #region Implementation of IClrServiceEntryFactory

        /// <summary>
        /// 创建服务条目。
        /// </summary>
        /// <param name="service">服务类型。</param>
        /// <param name="serviceImplementation">服务实现类型。</param>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> CreateServiceEntry(Type service, Type serviceImplementation)
        {
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var implementationMethodInfo = serviceImplementation.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                yield return Create(methodInfo, implementationMethodInfo);
            }
        }

        #endregion Implementation of IClrServiceEntryFactory

        #region Private Method

        private ServiceEntry Create(MethodInfo method, MethodBase implementationMethod)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);

            var serviceDescriptor = new ServiceDescriptor
            {
                Id = serviceId
            };

            var descriptorAttributes = method.GetCustomAttributes<RpcServiceDescriptorAttribute>();
            foreach (var descriptorAttribute in descriptorAttributes)
            {
                descriptorAttribute.Apply(serviceDescriptor);
            }

            return new ServiceEntry
            {
                Descriptor = serviceDescriptor,
                Func = async (parameters) =>
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var instance = scope.ServiceProvider.GetService(method.DeclaringType);

                        var list = new List<object>(implementationMethod.GetParameters().Length);
                        foreach (var parameterInfo in implementationMethod.GetParameters())
                        {
                            var value = parameters[parameterInfo.Name];
                            var parameterType = parameterInfo.ParameterType;

                            var parameter = _typeConvertibleService.Convert(value, parameterType);
                            list.Add(parameter);
                        }

                        var result = implementationMethod.Invoke(instance, list.ToArray());
                        watch.Stop();
                        _logger.LogInformation($"执行耗时：{watch.ElapsedMilliseconds}/ms");
                        Task<ActionResult> task = result as Task<ActionResult>;

                        return await task;
                    }
                }
            };
        }

        #endregion Private Method
    }
}