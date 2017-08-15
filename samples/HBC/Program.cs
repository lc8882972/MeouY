using Meou.Transport.DotNetty;
using System;
using System.Text;

namespace HBC
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            int port = 8808;
            var channel = new HeartBeatsClient().Connect("127.0.0.1", port,3);

            Console.ReadLine();

            channel.CloseAsync().Wait();
        }
    }
}