using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Abstractions
{
    public interface NotifyListener
    {
        void Notify(RegisterMeta registerMeta, NotifyEvent @event);
    }
}

