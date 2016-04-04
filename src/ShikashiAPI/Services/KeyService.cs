﻿using Microsoft.Data.Entity;
using ShikashiAPI.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public class KeyService : IKeyService
    {
        private PersistenceContext dbContext;

        public KeyService(PersistenceContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<APIKey> CreateKey(User user, bool permanent)
        {
            APIKey key = new APIKey()
            {
                ExpirationTime = UnixTimestamp.Timestamp() + 60 * 60 * 24,
                Identifier = GetUniqueKey(25),
                User = user
            };

            if (permanent)
            {
                key.ExpirationTime = long.MaxValue;
            }

            dbContext.Add(key);
            await dbContext.SaveChangesAsync();

            return key;
        }

        public async Task<APIKey> GetKey(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return null;
            }

            string[] authorizationKey = authToken.Split('-');

            if (authorizationKey.Length != 2)
            {
                return null;
            }

            return await (from p in dbContext.APIKey.Include(p => p.User)
                          where p.Id == int.Parse(authorizationKey[0]) && p.Identifier == authorizationKey[1] && p.ExpirationTime > UnixTimestamp.Timestamp()
                          select p).SingleOrDefaultAsync();
        }
        
        private string GetUniqueKey(int maxSize)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, maxSize)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
