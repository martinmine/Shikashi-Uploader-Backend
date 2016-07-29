using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShikashiAPI.Hashids.net;
using ShikashiAPI.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ShikashiAPI.Services
{
    public class UploadService : IUploadService
    {
        private int padding;

        private PersistenceContext dbContext;
        private IHashids hashids;

        public UploadService(PersistenceContext dbContext, IHashids hashids, IConfiguration config)
        {
            this.dbContext = dbContext;
            this.hashids = hashids;
            this.padding = int.Parse(config["IdPadding"]);
        }

        public async Task<List<UploadedContent>> GetAllUploads(User owner)
        {
            return await (from p in dbContext.UploadedContent
                          where p.Owner.Id == owner.Id
                          select p).ToListAsync();
        }

        public int GetId(string hash)
        {
            int[] fs = hashids.Decode(hash);
            if (fs.Length == 0)
            {
                return -1;
            }

            return fs[0] - padding;
        }

        public string GetIdHash(int id)
        {
            return hashids.Encode(id + padding);
        }

        public Task<UploadedContent> GetUpload(string uploadId)
        {
            return (from p in dbContext.UploadedContent.Include(p => p.Owner)
                    where p.Id == GetId(uploadId)
                    select p).SingleOrDefaultAsync();
        }

        public async Task RemoveUpload(UploadedContent upload)
        {
            dbContext.UploadedContent.Remove(upload);
            await dbContext.SaveChangesAsync();
        }

        public async Task<UploadedContent> CreateUpload(string contentType, string ipAddress, string fileName, User owner, long fileSize)
        {
            UploadedContent upload = new UploadedContent
            {
                FileName = fileName,
                FileSize = fileSize,
                MimeType = contentType,
                Owner = owner,
                UploaderIP = ipAddress,
                Uploaded = DateTime.Now
            };

            dbContext.UploadedContent.Add(upload);
            await dbContext.SaveChangesAsync();

            return upload;
        }
    }
}
