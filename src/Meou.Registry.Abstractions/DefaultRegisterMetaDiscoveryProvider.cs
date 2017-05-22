using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Meou.Registry.Abstractions
{
    public class DefaultRegisterMetaDiscoveryProvider : IRegisterMetaDiscoveryProvider
    {
        public IList<RegisterMeta> Builder()
        {
            var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
            List<RegisterMeta> all = new List<RegisterMeta>();
            foreach (var item in parts)
            {
                all.AddRange(RegisterMetaFactory(item as AssemblyPart));
            }
            return all;
        }

        private IList<RegisterMeta> RegisterMetaFactory(AssemblyPart part)
        {
            var types = part.Types;

            List<RegisterMeta> meta = new List<RegisterMeta>();
            RegisterMeta tempMeta = null;
            foreach (var typeInfo in types)
            {
                if (!typeInfo.IsClass || typeInfo.IsPrimitive)
                    continue;

                foreach (var attr in typeInfo.CustomAttributes)
                {

                    if (attr.AttributeType.Name != "ServiceProviderImplAttribute")
                        continue;

                    tempMeta = new RegisterMeta();

                    foreach (var @interface in typeInfo.ImplementedInterfaces)
                    {
                        foreach (var iattr in @interface.GetTypeInfo().CustomAttributes)
                        {
                            if (iattr.AttributeType.Name != "ServiceProviderAttribute")
                                continue;

                            foreach (var iarg in iattr.NamedArguments)
                            {
                                switch (iarg.MemberName)
                                {
                                    case "name":
                                        tempMeta.setServiceProviderName(iarg.TypedValue.Value.ToString());
                                        break;
                                    case "group":
                                        tempMeta.setGroup(iarg.TypedValue.Value.ToString());
                                        break;
                                }
                            }
                        }
                    }

                    foreach (var arg in attr.NamedArguments)
                    {
                        switch (arg.MemberName)
                        {
                            case "version":
                                tempMeta.setVersion(arg.TypedValue.Value.ToString());
                                break;
                        }
                    }

                    meta.Add(tempMeta);
                }
            }

            return meta;
        }
    }
}
