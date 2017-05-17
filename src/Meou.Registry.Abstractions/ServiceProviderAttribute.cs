using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public class ServiceProviderAttribute :Attribute
    {
        public string name = string.Empty;
        public string group = string.Empty;
    }
}
