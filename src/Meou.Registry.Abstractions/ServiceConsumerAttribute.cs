using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public class ServiceConsumerAttribute : ServiceProviderAttribute
    {
        public string route = "*";
        public string loadbalance = "";
    }


}
