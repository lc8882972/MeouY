using Meou.Registry.Zookeeper;
using System;

namespace ZookeeperConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ZookeeperServer zs = new ZookeeperServer();
            zs.Start();

            Console.ReadLine();
        }
    }
}