using Meou.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZookeeperConsoleApp
{
    public class RouteNotifyListener : NotifyListener
    {
        public Task Notify(RegisterMeta registerMeta)
        {
            Console.WriteLine(registerMeta.ToString());

            return Task.CompletedTask;
        }
    }
}
