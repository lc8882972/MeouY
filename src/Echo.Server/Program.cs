﻿using Echo.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Server;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Transport.DotNetty;
using Meou.Registry.Abstractions;

namespace Echo.Server
{
    public class Program
    {
        static Program()
        {
            //因为没有引用Echo.Common中的任何类型
            //所以强制加载Echo.Common程序集以保证Echo.Common在AppDomain中被加载。
            Assembly.Load(new AssemblyName("Echo.Service"));
        }

        public static void Main(string[] args)
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddServiceProvider()
                .AddZookeeperRegistry("localhost:2181")
                .AddRpcCore()
                .AddServiceRuntime()
                .UseRegistryRouteManager("localhost:2181")
                .UseDotNettyTransport();

            serviceCollection.AddTransient<IUserService, UserService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            RegistryService registry = null;
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole((c, l) => (int)l >= 3);

            //自动生成服务路由（这边的文件与Echo.Client为强制约束）
            {
                var registerMetaDiscoveryProvider = serviceProvider.GetRequiredService<IRegisterMetaDiscoveryProvider>();

                var metas = registerMetaDiscoveryProvider.Builder();

                IRegistryServiceBuilder registryServiceBuilder = serviceProvider.GetRequiredService<IRegistryServiceBuilder>();
                registry = registryServiceBuilder.Builder();

                foreach (var meta in metas)
                {
                    meta.setHost("127.0.0.1");
                    meta.setPort(9981);
                    registry.register(meta);
                } 

                var serviceEntryManager = serviceProvider.GetRequiredService<IServiceEntryManager>();
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i => new ServiceRoute
                {
                    ServiceDescriptor = i.Descriptor
                });

                var serviceRouteManager = serviceProvider.GetRequiredService<IServiceRouteManager>();
                serviceRouteManager.SetRoutesAsync(addressDescriptors).Wait();
            }

            var serviceHost = serviceProvider.GetRequiredService<IServiceHost>();

            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981));
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            });
            var key = Console.ReadKey();

            while (key.KeyChar != 'c' && key.Modifiers != ConsoleModifiers.Control)
            {
                key = Console.ReadKey();
            }

            registry.shutdownGracefully();
        }
    }
}