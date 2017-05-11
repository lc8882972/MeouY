using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Abstractions
{
    public interface RegistryServer : RegistryMonitor
    {

        void startRegistryServer();
    }
}
