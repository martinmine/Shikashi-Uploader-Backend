using ShikashiAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public interface IKeyService
    {
        Task<APIKey> GetKey(string authToken);
        Task<APIKey> CreateKey(User user, bool permanent);
    }
}
