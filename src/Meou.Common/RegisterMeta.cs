using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Common
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

        public void setHost(string host)
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

        public void setGroup(string group)
        {
            serviceMeta.setGroup(group);
        }

        public string getName()
        {
            return serviceMeta.getName();
        }

        public void setName(string serviceProviderName)
        {
            serviceMeta.setName(serviceProviderName);
        }

        public String getVersion()
        {
            return serviceMeta.getVersion();
        }

        public void setVersion(string version)
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
    }
}
