using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Rpc.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目管理者。
    /// </summary>
    public class DefaultServiceEntryManager : IServiceEntryManager
    {
        #region Field

        private readonly Dictionary<string, ServiceEntry> _serviceEntries;

        #endregion Field

        #region Constructor

        public DefaultServiceEntryManager(IEnumerable<IServiceEntryProvider> providers)
        {
            _serviceEntries = new Dictionary<string, ServiceEntry>();

            foreach (var provider in providers)
            {
                var entries = provider.GetEntries().ToArray();
                foreach (var entry in entries)
                {
                    if (_serviceEntries.ContainsKey(entry.Descriptor.Id)) 
                        throw new InvalidOperationException($"本地包含多个Id为：{entry.Descriptor.Id} 的服务条目。");

                    _serviceEntries.Add(entry.Descriptor.Id, entry);
                }
                
            }
        }

        #endregion Constructor

        #region Implementation of IServiceEntryManager

        /// <summary>
        /// 获取服务条目集合。
        /// </summary>
        /// <returns>服务条目集合。</returns>
        public Dictionary<string, ServiceEntry> GetEntries()
        {
            return _serviceEntries;
        }

        #endregion Implementation of IServiceEntryManager
    }
}