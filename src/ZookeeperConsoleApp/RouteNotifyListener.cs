using Meou.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZookeeperConsoleApp
{
    public class RouteNotifyListener : INotifyListener
    {
        public Task Notify(List<RegisterMeta> registerMeta)
        {
            registerMeta.ForEach((item) =>
            {
                Console.WriteLine(item.ToString());
            });
            return Task.CompletedTask;
        }
    }
}
