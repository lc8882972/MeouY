using Rabbit.Rpc.Messages;
using System.Linq;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目定位器。
    /// </summary>
    public class DefaultServiceEntryLocate : IServiceEntryLocate
    {
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServiceEntryLocate(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }

        #region Implementation of IServiceEntryLocate

        /// <summary>
        /// 定位服务条目。
        /// </summary>
        /// <param name="invokeMessage">远程调用消息。</param>
        /// <returns>服务条目。</returns>
        public ServiceEntry Locate(RemoteInvokeMessage invokeMessage)
        {
            ServiceEntry serviceEntry;
            var serviceEntries = _serviceEntryManager.GetEntries();
            serviceEntries.TryGetValue(invokeMessage.ServiceId,out serviceEntry);
            return serviceEntry;
        }

        #endregion Implementation of IServiceEntryLocate
    }
}