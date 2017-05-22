using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.RPC.Abstractions.Method
{
    /// <summary>
    /// 服务路由。
    /// </summary>
    public class ServiceMethod
    {
        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }

        #region Equality members

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var model = obj as ServiceMethod;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ServiceDescriptor != ServiceDescriptor;
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ServiceMethod model1, ServiceMethod model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(ServiceMethod model1, ServiceMethod model2)
        {
            return !Equals(model1, model2);
        }

        #endregion Equality members
    }
}
