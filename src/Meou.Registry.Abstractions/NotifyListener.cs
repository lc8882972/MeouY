using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Abstractions
{
    public interface NotifyListener
    {
        Task Notify(RegisterMeta registerMeta);
    }
}

