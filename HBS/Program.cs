using Meou.Transport.DotNetty;
using System;

namespace HBS
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8808;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            new HeartBeatServer("0.0.0.0",port).StartAsync().Wait();

            Console.ReadLine();
        }
    }
}