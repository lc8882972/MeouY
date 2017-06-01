using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Meou.Registry.Abstractions;
using Rabbit.Rpc;
using Rabbit.Transport.DotNetty;
using Rabbit.Rpc.Runtime.Server;
using Echo.Service;

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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddServiceProvider()
                .AddZookeeperRegistry()
                .AddRpcCore()
                .AddServiceRuntime()
                .UseDotNettyTransport();

            serviceCollection.AddTransient<IUserService, UserService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            RegistryService registry = null;
            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole();

            var registerMetaDiscoveryProvider = serviceProvider.GetRequiredService<IRegisterMetaDiscoveryProvider>();
            var metas = registerMetaDiscoveryProvider.Builder();

            registry = serviceProvider.GetRequiredService<RegistryService>();

            foreach (var meta in metas)
            {
                meta.setHost("127.0.0.1");
                meta.setPort(9981);
                registry.register(meta);
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

            registry.Shutdown();
        }
    }
}