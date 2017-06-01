using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Transport.DotNetty;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.ProxyGenerator;


namespace Echo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddServiceConsumer()
                .AddZookeeperRegistry()
                .AddClient()
                .UseRegistryRouteManager()
                .UseDotNettyTransport();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole();

            var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
            var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                        watch.Stop();
                        logger.LogInformation($"执行耗时：{watch.ElapsedMilliseconds}/ms");
                    }
                    catch (RpcRemoteException remoteException)
                    {
                        logger.LogError(remoteException.Message);
                    }
                    catch
                    {
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}