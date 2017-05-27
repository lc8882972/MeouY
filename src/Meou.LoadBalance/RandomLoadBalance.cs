using System;
using System.Collections.Generic;
using Meou.Common;

namespace Meou.LoadBalance
{
    public class RandomLoadBalance : ILoadBalance
    {

        public RegisterMeta Select(List<RegisterMeta> addr)
        {
            if (addr.Count == 0)
            {
                return null;
            }

            if (addr.Count == 1)
            {
                return addr[0];
            }

            int r = new Random().Next(0, addr.Count - 1);

            return addr[r];
        }
    }
}
