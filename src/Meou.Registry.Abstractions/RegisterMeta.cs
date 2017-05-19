using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meou.Registry.Abstractions
{
    public class RegisterMeta
    {
        // 地址
        private Address address = new Address();
        // metadata
        private ServiceMeta serviceMeta = new ServiceMeta();
        // 权重 hashCode() 与 equals() 不把weight计算在内
        private volatile int weight;
        // 建议连接数, jupiter客户端会根据connCount的值去建立对应数量的连接, hashCode() 与 equals() 不把connCount计算在内
        private volatile int connCount;

        public String getHost()
        {
            return address.getHost();
        }

        public void setHost(String host)
        {
            address.setHost(host);
        }

        public int getPort()
        {
            return address.getPort();
        }

        public void setPort(int port)
        {
            address.setPort(port);
        }

        public String getGroup()
        {
            return serviceMeta.getGroup();
        }

        public void setGroup(String group)
        {
            serviceMeta.setGroup(group);
        }

        public String getServiceProviderName()
        {
            return serviceMeta.getServiceProviderName();
        }

        public void setServiceProviderName(String serviceProviderName)
        {
            serviceMeta.setServiceProviderName(serviceProviderName);
        }

        public String getVersion()
        {
            return serviceMeta.getVersion();
        }

        public void setVersion(String version)
        {
            serviceMeta.setVersion(version);
        }

        public Address setAddress(Address addr)
        {
            return address = addr;
        }

        public Address getAddress()
        {
            return address;
        }

        public ServiceMeta getServiceMeta()
        {
            return serviceMeta;
        }

        public int getWeight()
        {
            return weight;
        }

        public void setWeight(int weight)
        {
            this.weight = weight;
        }

        public int getConnCount()
        {
            return connCount;
        }

        public void setConnCount(int connCount)
        {
            this.connCount = connCount;
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            RegisterMeta that = (RegisterMeta)o;

            return !(address != null ? !address.Equals(that.address) : that.address != null)
                    && !(serviceMeta != null ? !serviceMeta.Equals(that.serviceMeta) : that.serviceMeta != null);
        }

        public override int GetHashCode()
        {
            int result = address != null ? address.GetHashCode() : 0;
            result = 31 * result + (serviceMeta != null ? serviceMeta.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            return "RegisterMeta{" +
                    "address=" + address +
                    ", serviceMeta=" + serviceMeta +
                    ", weight=" + weight +
                    ", connCount=" + connCount +
                    '}';
        }

        /**
         * 不要轻易修改成员变量, 否则将影响hashCode和equals, Address需要经常放入List, Map等容器中.
         */
        public class Address
        {
            // 地址
            private String host;
            // 端口
            private int port;

            public Address() { }

            public Address(String host, int port)
            {
                this.host = host;
                this.port = port;
            }

            public static Address Parse(string host)
            {
                if (string.IsNullOrEmpty(host) && host.IndexOf(':') == -1)
                    throw new System.ArgumentException("host:port 格式错误");

                string[] array = host.Split(':');
                Address temp = new Address(array[0],Convert.ToInt32(array[1]));
                return temp;
            }
            public String getHost()
            {
                return host;
            }

            public void setHost(String host)
            {
                this.host = host;
            }

            public int getPort()
            {
                return port;
            }

            public void setPort(int port)
            {
                this.port = port;
            }

            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Address address = (Address)o;

                return port == address.port && !(host != null ? !host.Equals(address.host) : address.host != null);
            }

            public override int GetHashCode()
            {
                int result = host != null ? host.GetHashCode() : 0;
                result = 31 * result + port;
                return result;
            }

            public override string ToString()
            {
                return "Address{" +
                        "host='" + host + '\'' +
                        ", port=" + port +
                        '}';
            }
        }

        /// <summary>
        /// 服务信息
        /// </summary>
        public class ServiceMeta
        {
            // 组别
            private String group;
            // 服务名
            private String serviceProviderName;
            // 版本信息
            private String version;

            public ServiceMeta() { }

            public ServiceMeta(String group, String serviceProviderName, String version)
            {
                this.group = group;
                this.serviceProviderName = serviceProviderName;
                this.version = version;
            }

            public String getGroup()
            {
                return group;
            }

            public void setGroup(String group)
            {
                this.group = group;
            }

            public String getServiceProviderName()
            {
                return serviceProviderName;
            }

            public void setServiceProviderName(String serviceProviderName)
            {
                this.serviceProviderName = serviceProviderName;
            }

            public String getVersion()
            {
                return version;
            }

            public void setVersion(String version)
            {
                this.version = version;
            }

            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                ServiceMeta that = (ServiceMeta)o;

                return !(group != null ? !group.Equals(that.group) : that.group != null)
                        && !(serviceProviderName != null ? !serviceProviderName.Equals(that.serviceProviderName) : that.serviceProviderName != null)
                        && !(version != null ? !version.Equals(that.version) : that.version != null);
            }


            public override int GetHashCode()
            {
                int result = group != null ? group.GetHashCode() : 0;
                result = 31 * result + (serviceProviderName != null ? serviceProviderName.GetHashCode() : 0);
                result = 31 * result + (version != null ? version.GetHashCode() : 0);
                return result;
            }


            public override string ToString()
            {
                return "ServiceMeta{" +
                        "group='" + group + '\'' +
                        ", serviceProviderName='" + serviceProviderName + '\'' +
                        ", version='" + version + '\'' +
                        '}';
            }
        }
    }
}
