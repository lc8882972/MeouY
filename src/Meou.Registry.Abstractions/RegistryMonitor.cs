using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Abstractions
{
    public interface RegistryMonitor
    {

        /**
         * Returns the address list of publisher.
         */
        List<String> listPublisherHosts();

        /**
         * Returns the address list of subscriber.
         */
        List<String> listSubscriberAddresses();

        /**
         * Returns to the service of all the specified service provider's address.
         */
        List<String> listAddressesByService(String group, String serviceProviderName, String version);

        /**
         * Finds the address(host, port) of the corresponding node and returns all
         * the service names it provides.
         */
        List<String> listServicesByAddress(String host, int port);
    }
}
