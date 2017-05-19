using System;
using System.Collections.Generic;
using System.Text;
using Meou.Registry.Abstractions;
using System.Reflection;

namespace Meou.Registry.Zookeeper
{
    public class ZookeeperServer
    {
        NotifyListener _listener;
        public ZookeeperServer(NotifyListener listener)
        {
            _listener = listener;
        }
        RegistryService _registry = new ZookeeperRegistryServiceBuilder("localhost:2181").Builder();
        public void Start()
        {
            //AssemblyPart tempPart = null;
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
                    if (attr.AttributeType.Name != "ServiceProviderAttribute")
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

                    tempMeta.setVersion("0.1.0");

                }

                //foreach (var impl in typeInfo.ImplementedInterfaces)
                //{
                //    var ti = impl.GetTypeInfo();

                //    foreach (var attr in typeInfo.CustomAttributes)
                //    {
                //        if (attr.AttributeType.Name != "ServiceProviderImplAttribute")
                //            continue;

                //        tempMeta = new RegisterMeta();
                //        foreach (var arg in attr.NamedArguments)
                //        {
                //            switch (arg.MemberName)
                //            {
                //                case "version":
                //                    tempMeta.getServiceMeta().setVersion(arg.TypedValue.Value.ToString());
                //                    break;
                //            }
                //        }

                //    }
                //}

                meta.Add(tempMeta);
            }

           
            _registry.connectToRegistryServer("localhost: 2181");
            foreach (var item in meta)
            {
                _registry.register(item);
                _registry.subscribe(item.getServiceMeta(), _listener);
            }
        
        }

        public void Stop()
        {
            _registry.shutdownGracefully();
        }
    }
}
