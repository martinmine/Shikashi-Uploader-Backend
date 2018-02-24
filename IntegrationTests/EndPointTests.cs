using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using ShikashiAPI;
using ShikashiAPI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [TestFixture]
    public class UnitTest1
    {
        private TestServer _server;
        private HttpClient _client;

        private readonly HttpClient _externalClient = new HttpClient();

        private const string BasePath = "https://labs.shikashi.me/";

        private const string TestUserEmail = "martin_mine@hotmail.com";
        private const string TestUserPassword = "qwerty123";

        [OneTimeSetUp]
        public async Task Initialize()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();

            var token = await GetToken(TestUserEmail, TestUserPassword);

            _client.DefaultRequestHeaders.Add("Authorization", token.Key);
        }

        private async Task<APIKeyViewModel> GetToken(string email, string password)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("client", "integration-test")
            };

            var content = new FormUrlEncodedContent(formData);

            using (var response = await _client.PostAsync("/login", content))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<APIKeyViewModel>(responseContent);
            }
        }

        [Test]
        public async Task Login_ShouldReturnValidToken()
        {
            var myToken = await GetToken("martin_mine@hotmail.com", "qwerty123");

            Assert.NotNull(myToken);
            Assert.IsNotEmpty(myToken.Key);
            Assert.Greater(myToken.ExpirationTime, 0);
        }

        [Test]
        public async Task Uploads_ShouldReturnLastUploads()
        {
            var upload = await UploadFile("TestUploadFile.txt");

            var request = await _client.GetAsync("/account/uploads");
            var responseBody = await request.Content.ReadAsStringAsync();
            var uploads = JsonConvert.DeserializeObject<List<UploadViewModel>>(responseBody);

            Assert.True(uploads.Any(t => t.Key == upload.Key));
        }

        [TestCase("TestUploadFile.txt")]
        public async Task UploadFile_UploadsToS3(string fileName)
        {
            var upload = await UploadFile(fileName);
            var uploadRequest = await _externalClient.GetAsync(BasePath + upload.Key);
            var contentType = MimeMapping.Instance.GetMimeType(Path.GetExtension(fileName));

            Assert.AreEqual(contentType, uploadRequest.Content.Headers.ContentType.MediaType);
            Assert.AreEqual($"\"{fileName}\"", uploadRequest.Content.Headers.ContentDisposition.FileName);
            Assert.Greater(uploadRequest.Content.Headers.ContentLength, 0);

            var uploadContent = await uploadRequest.Content.ReadAsStringAsync();

            Assert.AreEqual(File.ReadAllText(fileName), uploadContent);
        }

        private async Task<UploadViewModel> UploadFile(string filePath)
        {
            var testFile = File.OpenRead(filePath);
            var extension = Path.GetExtension(filePath);
            var contentType = MimeMapping.Instance.GetMimeType(extension);

            _client.DefaultRequestHeaders.Add("UploadFileSize", testFile.Length.ToString());

            using (var content = new MultipartFormDataContent())
            {
                content.Add(CreateFileContent(testFile, filePath, contentType));

                using (var response = await _client.PostAsync("/upload", content))
                {
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UploadViewModel>(responseBody);
                }
            }
        }

        [Test]
        public async Task DeleteFile_RemovesFromS3()
        {
            var upload = await UploadFile("TestUploadFile.txt");

            var response = await _client.DeleteAsync($"/{upload.Key}/delete");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var s3Response = await _externalClient.GetAsync(BasePath + upload.Key);
            Assert.AreEqual(HttpStatusCode.NotFound, s3Response.StatusCode);
        }

        private static StreamContent CreateFileContent(Stream fileStream, string fileName, string contentType)
        {
            StreamContent fileContentStream = new StreamContent(fileStream);
            fileContentStream.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"files\"",
                FileName = "\"" + fileName + "\""
            };

            fileContentStream.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return fileContentStream;
        }
    }
}
