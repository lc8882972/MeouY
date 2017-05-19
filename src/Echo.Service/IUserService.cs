using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;

namespace Echo.Service
{
    public class UserModel
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    [RpcServiceBundle]
    public interface IUserService
    {
        Task<string> GetUserName(int id);

        Task<bool> Exists(int id);

        Task<int> GetUserId(string userName);

        Task<DateTime> GetUserLastSignInTime(int id);

        Task<UserModel> GetUser(int id);

        Task<bool> Update(int id, UserModel model);

        Task<IDictionary<string, string>> GetDictionary();

        [RpcService(IsWaitExecution = false)]
        Task Try();

        Task TryThrowException();
    }
}
