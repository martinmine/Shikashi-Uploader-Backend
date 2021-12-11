using ShikashiAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public interface IUploadService
    {
        Task<List<UploadedContent>> GetAllUploads(User user);

        string GetIdHash(int id);

        int GetId(string hash);

        Task<UploadedContent> GetUpload(string uploadId);

        Task RemoveUpload(UploadedContent upload);

        Task<UploadedContent> CreateUpload(string contentType, string ipAddress, string fileName, User owner, long fileSize);
    }
}
