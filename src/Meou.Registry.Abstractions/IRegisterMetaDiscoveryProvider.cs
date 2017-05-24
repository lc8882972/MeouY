using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public interface IRegisterMetaDiscoveryProvider
    {
        IList<RegisterMeta> Builder();
        IList<RegisterMeta> Consumer();
        IList<RegisterMeta> Provider();
    }
}
