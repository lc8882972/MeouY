using System;
using System.Collections.Generic;
using System.Text;
using Meou.Registry.Abstractions;
using System.Reflection;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperServer
    {
        public void Start()
        {
            AssemblyPart tempPart = null;
            //var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(Assembly.GetEntryAssembly().FullName);
            //foreach (var item in parts)
            //{
            //    tempPart = item as AssemblyPart;

            //    if (tempPart != null)
            //    {
            //        foreach (var typeInfo in tempPart.Types)
            //        {
            //            foreach (var attr in typeInfo.CustomAttributes)
            //            {
            //                foreach (var arg in attr.ConstructorArguments)
            //                {

            //                }
            //            }
            //        }
            //    }
            //}

            var types = Assembly.GetEntryAssembly().DefinedTypes;

            List<RegisterMeta> meta = new List<RegisterMeta>();
            RegisterMeta tempMeta = null;
            foreach (var typeInfo in types)
            {
                if (!typeInfo.IsInterface)
                    continue;

                foreach (var attr in typeInfo.CustomAttributes)
                {
                    if (attr.AttributeType.Name != "ServiceProvider")
                        continue;

                    tempMeta = new RegisterMeta();
                    foreach (var arg in attr.NamedArguments)
                    {
                        switch (arg.MemberName)
                        {
                            case "name":
                                tempMeta.setServiceProviderName(arg.TypedValue.Value.ToString());
                                break;
                            case "group":
                                tempMeta.setGroup(arg.TypedValue.Value.ToString());
                                break;
                        }
                    }

                }

                foreach (var impl in typeInfo.ImplementedInterfaces)
                {
                    var ti = impl.GetTypeInfo();

                    foreach (var attr in typeInfo.CustomAttributes)
                    {
                        if (attr.AttributeType.Name != "ServiceProviderImpl")
                            continue;

                        tempMeta = new RegisterMeta();
                        foreach (var arg in attr.NamedArguments)
                        {
                            switch (arg.MemberName)
                            {
                                case "version":
                                    tempMeta.getServiceMeta().setVersion(arg.TypedValue.Value.ToString());
                                    break;
                            }
                        }

                    }
                }

                meta.Add(tempMeta);
            }
        }
    }
}
