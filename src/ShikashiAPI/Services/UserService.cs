using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using ShikashiAPI.Model;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public class UserService : IUserService
    {
        private PersistenceContext dbContext;
        private string passwordPepper;

        public UserService(PersistenceContext dbContext, IConfiguration config)
        {
            this.dbContext = dbContext;
            this.passwordPepper = config["PasswordHash"];
        }

        public async Task<User> GetUser(string email)
        {
            return await (from p in dbContext.User
                          where p.Email == email
                          select p).SingleOrDefaultAsync();
        }

        public async Task<User> LoginUser(string email, string password)
        {
            User user = await GetUser(email);
            if (user != null && BCrypt.Net.BCrypt.CheckPassword(password + passwordPepper, user.Password))
            {
                return user;
            }

            return null;
        }

        public async Task<User> RegisterUser(string email, string password, string inviteKey)
        {
            var key = await (from p in dbContext.InviteKey
                             where p.Key == inviteKey
                             select p).SingleOrDefaultAsync();

            if (key == null)
            {
                return null;
            }
            
            User user = new User
            {
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password + passwordPepper, BCrypt.Net.BCrypt.GenerateSalt(4))
            };

            dbContext.InviteKey.Remove(key);
            dbContext.User.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User> SetUserPassword(string email, string currentPassword, string newPassword)
        {
            User user = await LoginUser(email, currentPassword);

            if (user == null)
            {
                return null;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword + passwordPepper, BCrypt.Net.BCrypt.GenerateSalt(4));

            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}
