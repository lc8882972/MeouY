using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meou.Registry.Abstractions;
using Meou.Common;

namespace Echo.Client
{
    [ServiceConsumer(group = "TestGroup", name = "IUserService", version = "0.0.0")]
    public interface IUserService
    {
        Task<ActionResult> GetUserName(int id);
    }
}
