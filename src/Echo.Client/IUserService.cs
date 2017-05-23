using Meou.Registry.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Client
{
    [ServiceConsumer(group = "TestGroup", name = "IUserService", version = "0.1.0")]
    public interface IUserService
    {
        Task<string> GetUserName(int id);
    }
}
