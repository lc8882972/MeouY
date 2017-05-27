using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meou.Common;

namespace Meou.Registry.Abstractions
{
    public interface RegistryService
    {
        /**
        * Register service to registry server.
        */
        void register(RegisterMeta meta);

        /**
         * Unregister service to registry server.
         */
        void unregister(RegisterMeta meta);

        /**
         * Subscribe a service from registry server.
         */
        void subscribe(ServiceMeta serviceMeta, INotifyListener listener);

        /**
         * Find a service in the local scope.
         */
        Collection<RegisterMeta> lookup(ServiceMeta serviceMeta);

        /**
         * Returns {@code true} if {@link RegistryService} is shutdown.
         */
        bool isShutdown();

        /**
         * Shutdown.
         */
        void Shutdown();
    }
}
