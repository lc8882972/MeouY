﻿using Meou.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Server
{
    /// <summary>
    /// 服务条目。
    /// </summary>
    public class ServiceEntry
    {
        /// <summary>
        /// 执行委托。
        /// </summary>
        public Func<IDictionary<string, object>, Task<ActionResult>> Func { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor Descriptor { get; set; }
    }
}