using Meou.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZookeeperConsoleApp
{
    [ServiceProviderImpl(version ="0.1.0")]
    public class UserService : IUserService
    {
        public string GetUserName()
        {
            return "Robbin";
        }
    }
}
