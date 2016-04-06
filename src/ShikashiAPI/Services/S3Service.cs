using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ShikashiAPI.Services
{
    public class S3Service : IS3Service
    {
        private BasicAWSCredentials credentials;
        private RegionEndpoint region;
        private string bucketName;

        public S3Service(IConfiguration configuration)
        {
            this.credentials = new BasicAWSCredentials(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"]);
            this.bucketName = configuration["AWS:AwsUploadBucket"];
            this.region = RegionEndpoint.GetBySystemName(configuration["AWS:S3Region"]);
        }
        
        public async Task CreateUploadAlias(string uploadId, string contentType, string uploadName)
        {
            if (!uploadName.Contains("."))
            {
                return;
            }

            string extension = uploadName.Substring(uploadName.LastIndexOf('.') + 1);

            using (var client = new AmazonS3Client(credentials, region))
            {
                PutObjectRequest putRequest = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = $"{uploadId}.{extension}",
                    InputStream = new MemoryStream(new byte[] { })
                };

                putRequest.Headers["Content-Type"] = contentType;
                putRequest.Headers["Content-Length"] = "0";
                putRequest.Headers["x-amz-website-redirect-location"] = $"/{uploadId}";

                var response = await client.PutObjectAsync(putRequest);
            }
        }

        public async Task DeleteUpload(string uploadId)
        {
            using (var client = new AmazonS3Client(credentials, region))
            {
                await client.DeleteObjectAsync(bucketName, uploadId);
            }
        }

        public async Task<long> StoreUpload(Stream stream, string uploadId, string contentType, long length, string name)
        {
            using (var client = new AmazonS3Client(credentials, region))
            {
                PutObjectRequest putRequest = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = uploadId,
                    InputStream = stream
                };


                //putRequest.ContentType = contentType;

                putRequest.Headers["Content-Type"] = contentType;
                putRequest.Headers["Content-Disposition"] = $"inline; filename=\"{name}\"";
                //putRequest.Headers["Content-Length"] = length.ToString();

                /*
                putRequest.Metadata.Add("Content-Type", contentType);
                putRequest.Metadata.Add("Content-Length", length.ToString());
                putRequest.Metadata.Add("Content-Disposition", $"inline; filename=\"{name}\"");
                */

                var response = await client.PutObjectAsync(putRequest);

                return response.ContentLength;
            }
        }
    }
}
