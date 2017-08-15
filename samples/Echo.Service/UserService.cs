using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Meou.Registry.Abstractions;
using Meou.Common;

namespace Echo.Service
{
    [ServiceProviderImpl(version ="0.0.0")]
    public class UserService : IUserService
    {
        //private readonly ILogger logger;
        //public UserService(LoggerFactory loggerFactory)
        //{
        //    this.logger = loggerFactory.CreateLogger<UserService>();
        //}
        #region Implementation of IUserService

        public Task<ActionResult> GetUserName(int id)
        {
            //this.logger.LogDebug($"id:{id} is name rabbit.");
            return Task.FromResult<ActionResult>( new ActionResult() { data = $"id:{id} is name rabbit." });
        }

        public Task<bool> Exists(int id)
        {
            return Task.FromResult(true);
        }

        public Task<int> GetUserId(string userName)
        {
            return Task.FromResult(1);
        }

        public Task<DateTime> GetUserLastSignInTime(int id)
        {
            return Task.FromResult(DateTime.Now);
        }

        public Task<UserModel> GetUser(int id)
        {
            return Task.FromResult(new UserModel
            {
                Name = "rabbit",
                Age = 18
            });
        }

        public Task<bool> Update(int id, UserModel model)
        {
            return Task.FromResult(true);
        }

        public Task<IDictionary<string, string>> GetDictionary()
        {
            return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string> { { "key", "value" } });
        }

        public async Task Try()
        {
            Console.WriteLine("start");
            await Task.Delay(5000);
            Console.WriteLine("end");
        }

        public Task TryThrowException()
        {
            throw new Exception("�û�Id�Ƿ���");
        }

        #endregion Implementation of IUserService
    }
}
