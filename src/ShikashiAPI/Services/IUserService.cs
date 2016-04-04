using ShikashiAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(string email, string password, string inviteKey);
        Task<User> LoginUser(string email, string password);
        Task<User> GetUser(string email);
        Task<User> SetUserPassword(string email, string currentPassword, string newPassword);

    }
}
