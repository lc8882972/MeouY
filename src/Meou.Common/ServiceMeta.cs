using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Common
{
    public class ServiceMeta
    {
        // 组别
        private string group;
        // 服务名
        private string name;
        // 版本信息
        private string version;

        public ServiceMeta() { }

        public ServiceMeta(string group, string name, string version)
        {
            this.group = group;
            this.name = name;
            this.version = version;
        }

        public string getGroup()
        {
            return group;
        }

        public void setGroup(string group)
        {
            this.group = group;
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getVersion()
        {
            return version;
        }

        public void setVersion(string version)
        {
            this.version = version;
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            ServiceMeta that = (ServiceMeta)o;

            return !(group != null ? !group.Equals(that.group) : that.group != null)
                    && !(name != null ? !name.Equals(that.name) : that.name != null)
                    && !(version != null ? !version.Equals(that.version) : that.version != null);
        }


        public override int GetHashCode()
        {
            int result = group != null ? group.GetHashCode() : 0;
            result = 31 * result + (name != null ? name.GetHashCode() : 0);
            result = 31 * result + (version != null ? version.GetHashCode() : 0);
            return result;
        }


        public override string ToString()
        {
            return "ServiceMeta{" +
                    "group='" + group + '\'' +
                    ", name='" + name + '\'' +
                    ", version='" + version + '\'' +
                    '}';
        }
    }
   
}
