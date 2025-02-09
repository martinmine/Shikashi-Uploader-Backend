using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using ShikashiAPI;
using ShikashiAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [TestFixture]
    public class EndPointTests
    {
        private HttpClient _client;

        private readonly HttpClient _externalClient = new HttpClient();

        private const string BasePath = "https://labs.shikashi.me/";

        private const string TestUserEmail = "test@example.com";
        private const string TestUserPassword = "password";

        [OneTimeSetUp]
        public async Task Initialize()
        {
            var application = new WebApplicationFactory<Startup>()
             .WithWebHostBuilder(builder =>
             {
                 builder.UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings2.json")
                    .Build());
             });

            _client = application.CreateClient();

            var token = await GetToken(TestUserEmail, TestUserPassword);

            _client.DefaultRequestHeaders.Add("Authorization", token.Key);
        }

        private async Task<APIKeyViewModel> GetToken(string email, string password)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("email", email),
                new("password", password),
                new("client", "integration-test")
            };

            var content = new FormUrlEncodedContent(formData);

            using var response = await _client.PostAsync("/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Expected HTTP OK response but got {response.StatusCode}: {responseContent}");

            return JsonConvert.DeserializeObject<APIKeyViewModel>(responseContent);
        }

        [Test]
        public async Task Login_ShouldReturnValidToken()
        {
            var myToken = await GetToken("martin_mine@hotmail.com", "qwerty123");

            Assert.That(myToken, Is.Not.Null);
            Assert.That(myToken, Is.Not.Empty);
            Assert.That(myToken.ExpirationTime, Is.GreaterThan(0));
        }

        [Test]
        public async Task Uploads_ShouldReturnLastUploads()
        {
            var upload = await UploadFile("TestUploadFile.txt");

            var request = await _client.GetAsync("/account/uploads");
            var responseBody = await request.Content.ReadAsStringAsync();
            var uploads = JsonConvert.DeserializeObject<List<UploadViewModel>>(responseBody);

            Assert.That(uploads, Has.Some.Matches<UploadViewModel>(t => t.Key == upload.Key));
        }

        [TestCase("TestUploadFile.txt")]
        public async Task UploadFile_UploadsToS3(string fileName)
        {
            var upload = await UploadFile(fileName);
            var uploadRequest = await _externalClient.GetAsync(BasePath + upload.Key);
            var contentType = MimeMapping.Instance.GetMimeType(Path.GetExtension(fileName));

            Assert.That(contentType, Is.EqualTo(uploadRequest.Content.Headers.ContentType.MediaType));
            Assert.That($"\"{fileName}\"", Is.EqualTo(uploadRequest.Content.Headers.ContentDisposition.FileName));
            Assert.That(uploadRequest.Content.Headers.ContentLength, Is.GreaterThan(0));

            var uploadContent = await uploadRequest.Content.ReadAsStringAsync();

            Assert.That(File.ReadAllText(fileName), Is.EqualTo(uploadContent));
        }

        private async Task<UploadViewModel> UploadFile(string filePath)
        {
            var testFile = File.OpenRead(filePath);
            var fileInfo = new FileInfo(filePath);
            var extension = Path.GetExtension(filePath);
            var contentType = MimeMapping.Instance.GetMimeType(extension);

            _client.DefaultRequestHeaders.Remove("UploadFileSize");
            _client.DefaultRequestHeaders.Add("UploadFileSize", fileInfo.Length.ToString());

            using (var content = new MultipartFormDataContent())
            {
                content.Add(CreateFileContent(testFile, filePath, contentType));

                using (var response = await _client.PostAsync("/upload", content))
                {
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

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
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var s3Response = await _externalClient.GetAsync(BasePath + upload.Key);
            Assert.That(s3Response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
