using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Zookeeper
{
    public interface StateListener
    {
        //int DISCONNECTED = 0;

        //int CONNECTED = 1;

        //int RECONNECTED = 2;
        void stateChanged(int connected);
    }
}
