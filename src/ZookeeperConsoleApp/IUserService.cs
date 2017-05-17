using System;
using System.Collections.Generic;
using System.Text;
using Meou.Registry.Abstractions;

namespace ZookeeperConsoleApp
{
   [ServiceProvider(group ="TestGroup",name = "IUserService")]
    public interface IUserService
    {
        string GetUserName();
    }
}
