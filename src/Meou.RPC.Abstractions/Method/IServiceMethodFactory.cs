﻿using Rabbit.Rpc.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Meou.RPC.Abstractions.Method
{
    /// <summary>
    /// 一个抽象的服务路由工厂。
    /// </summary>
    public interface IServiceRouteFactory
    {
        /// <summary>
        /// 根据服务路由描述符创建服务路由。
        /// </summary>
        /// <param name="descriptors">服务路由描述符。</param>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> CreateServiceRoutesAsync(IEnumerable<ServiceRouteDescriptor> descriptors);
    }
}
