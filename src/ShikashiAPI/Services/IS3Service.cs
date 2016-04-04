using System.IO;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public interface IS3Service
    {
        Task DeleteUpload(string uploadId);

        Task<long> StoreUpload(Stream stream, string uploadId, string contentType, long length, string name);

        Task CreateUploadAlias(string uploadId, string contentType, string uploadName);
    }
}
