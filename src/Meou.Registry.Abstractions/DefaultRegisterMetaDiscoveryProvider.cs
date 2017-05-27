using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Meou.Common;

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
                all.AddRange(ProviderRegisterMetaFactory(item as AssemblyPart));
            }
            return all;
        }

        public IList<RegisterMeta> Consumer()
        {
            var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
            List<RegisterMeta> all = new List<RegisterMeta>();
            foreach (var item in parts)
            {
                all.AddRange(ConsumerRegisterMetaFactory(item as AssemblyPart));
            }
            return all;
        }

        public IList<RegisterMeta> Provider()
        {
            var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
            List<RegisterMeta> all = new List<RegisterMeta>();
            foreach (var item in parts)
            {
                all.AddRange(ProviderRegisterMetaFactory(item as AssemblyPart));
            }
            return all;
        }

        private IList<RegisterMeta> ProviderRegisterMetaFactory(AssemblyPart part)
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
                                        tempMeta.setName(iarg.TypedValue.Value.ToString());
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

        private IList<RegisterMeta> ConsumerRegisterMetaFactory(AssemblyPart part)
        {
            var types = part.Types;

            List<RegisterMeta> meta = new List<RegisterMeta>();
            RegisterMeta tempMeta = null;
            foreach (var typeInfo in types)
            {
                if (!typeInfo.IsInterface || typeInfo.IsPrimitive)
                    continue;

                foreach (var attr in typeInfo.CustomAttributes)
                {

                    if (attr.AttributeType.Name != "ServiceConsumerAttribute")
                        continue;

                    tempMeta = new RegisterMeta();

                    foreach (var arg in attr.NamedArguments)
                    {
                        switch (arg.MemberName)
                        {
                            case "name":
                                tempMeta.setName(arg.TypedValue.Value.ToString());
                                break;
                            case "group":
                                tempMeta.setGroup(arg.TypedValue.Value.ToString());
                                break;
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
