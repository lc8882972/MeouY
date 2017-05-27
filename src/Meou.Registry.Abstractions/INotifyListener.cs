﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meou.Common;

namespace Meou.Registry.Abstractions
{
    public interface INotifyListener
    {
        Task Notify(List<RegisterMeta> registerMeta);
    }
}

