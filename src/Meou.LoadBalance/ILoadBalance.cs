using System;
using System.Collections.Generic;
using System.Text;
using Meou.Common;

namespace Meou.LoadBalance
{
    public interface ILoadBalance
    {
        RegisterMeta Select(List<RegisterMeta> addr);
    }
}
