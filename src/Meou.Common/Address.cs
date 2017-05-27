using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Common
{
    /**
     * 不要轻易修改成员变量, 否则将影响hashCode和equals, Address需要经常放入List, Map等容器中.
     */
    public class Address
    {
        // 地址
        private string host;
        // 端口
        private int port;

        public Address() { }

        public Address(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public static Address Parse(string host)
        {
            if (string.IsNullOrEmpty(host) && host.IndexOf(':') == -1)
                throw new System.ArgumentException("host:port 格式错误");

            string[] array = host.Split(':');
            Address temp = new Address(array[0], Convert.ToInt32(array[1]));
            return temp;
        }
        public string getHost()
        {
            return host;
        }

        public void setHost(string host)
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
}
