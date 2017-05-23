using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public class ServiceConsumerAttribute : ServiceProviderAttribute
    {
        public string version = "0.0.0";
        public string route = "*";
        public string loadbalance = "";
    }


}
