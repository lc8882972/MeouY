using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public class ServiceProviderAttribute :Attribute
    {
        public string name = null;
        public string group = "default";
        public string version = "0.0.0";
    }
}
