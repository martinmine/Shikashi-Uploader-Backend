using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using ShikashiAPI.Util;
using ShikashiAPI.ViewModels;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace ShikashiAPI.Controllers
{
    [Route("/upload")]
    public class UploadController : UserAuthenticatedController
    {
        private readonly IUploadService _uploadService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IS3Service _s3Service;
        private readonly int _maxUploadSize;
        private readonly ILogger<UploadController> _logger;

        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        public UploadController(IConfiguration config, IUploadService uploadService, IKeyService keyService,
            IAuthorizationService authService, IS3Service s3Service, ILogger<UploadController> logger)
            : base(keyService)
        {
            _uploadService = uploadService;
            _authorizationService = authService;
            _maxUploadSize = int.Parse(config["MaxFileSize"]);
            _s3Service = s3Service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            var fileSize = int.Parse(Request.Headers["UploadFileSize"]);
            var key = await GetCurrentKey();

            if (!(await _authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            
            var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(Request.ContentType),
                    DefaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);


            var section = await reader.ReadNextSectionAsync();

            if (section == null)
                return BadRequest();

            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (!hasContentDispositionHeader || !MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                return BadRequest();

            var ip = GetIpAddress();
            var file = section.AsFileSection();

            _logger.LogInformation("Attempting to create upload with filename {@FileName} which is {@ContentType} with {@Size} length for user {@UserId} at {@Ip}",
                file.FileName, section.ContentType, fileSize, key.User.Id, ip);

            var upload = await _uploadService.CreateUpload(section.ContentType, ip, file.FileName, key.User, fileSize);

            await _s3Service.StoreUpload(new MultipartStreamWrapper(file.FileStream, fileSize), _uploadService.GetIdHash(upload.Id), section.ContentType, file.FileName, fileSize);
            await _s3Service.CreateUploadAlias(_uploadService.GetIdHash(upload.Id), section.ContentType, file.FileName);

            var viewModel = new UploadViewModel
            {
                FileName = upload.FileName,
                FileSize = upload.FileSize,
                Key = _uploadService.GetIdHash(upload.Id),
                MimeType = upload.MimeType,
                Uploaded = upload.Uploaded
            };

            _logger.LogInformation("Uploaded compelted {@Upload}", viewModel);
            return Ok(viewModel);
        }

        private string GetIpAddress()
        {
            const string proxyForwardingHeader = "X-Forwarded-For";

            if (HttpContext.Request.Headers.ContainsKey(proxyForwardingHeader))
                return HttpContext.Request.Headers[proxyForwardingHeader];
            else
                return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
