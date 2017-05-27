using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meou.Common;

namespace Meou.LoadBalance
{
    public abstract class AbstractLoadBalance : ILoadBalance
    {
        //private Dictionary<string, IpAddress> 

        public RegisterMeta Select(List<RegisterMeta> addr)
        {
            var result = doSelect(addr);
            return result;
        }

        public abstract RegisterMeta doSelect(List<RegisterMeta> addr);
    }
}
