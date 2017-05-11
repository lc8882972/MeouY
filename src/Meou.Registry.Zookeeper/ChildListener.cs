using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Zookeeper
{
    public interface ChildListener
    {
        void childChanged(String path, List<String> children);
    }
}
