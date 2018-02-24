using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using ShikashiAPI.Util;
using ShikashiAPI.ViewModels;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;

namespace ShikashiAPI.Controllers
{
    [Route("/upload")]
    public class UploadController : UserAuthenticatedController
    {
        private IUploadService uploadService;
        private IAuthorizationService authorizationService;
        private IS3Service s3Service;
        private int maxUploadSize;
        private ILogger logger;

        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public UploadController(IConfiguration config, IUploadService uploadService, IKeyService keyService,
            IAuthorizationService authService, IS3Service s3Service, ILoggerFactory factory)
            : base(keyService)
        {
            this.uploadService = uploadService;
            this.authorizationService = authService;
            this.maxUploadSize = int.Parse(config["MaxFileSize"]);
            this.s3Service = s3Service;
            this.logger = factory.CreateLogger<UploadController>();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            // [FromHeader(Name = "UploadFileSize")] long fileSize
            var fileSize = int.Parse(Request.Headers["UploadFileSize"]);
            var key = await GetCurrentKey();

            if (!(await authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(Request.ContentType),
                    _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);


            var section = await reader.ReadNextSectionAsync();

            if (section == null)
                return BadRequest();

            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (!hasContentDispositionHeader || !MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                return BadRequest();

            var ip = GetIPAddress();
            var file = section.AsFileSection();

            var upload = await uploadService.CreateUpload(section.ContentType, ip, file.FileName, key.User, fileSize);

            await s3Service.StoreUpload(new MultipartStreamWrapper(file.FileStream, fileSize), uploadService.GetIdHash(upload.Id), section.ContentType, file.FileName, fileSize);
            await s3Service.CreateUploadAlias(uploadService.GetIdHash(upload.Id), section.ContentType, file.FileName);

            // TODO: Avoid using the filesize header as this is a naive approach which can easily be bypassed 
            return Ok(new UploadViewModel
            {
                FileName = upload.FileName,
                FileSize = upload.FileSize,
                Key = uploadService.GetIdHash(upload.Id),
                MimeType = upload.MimeType,
                Uploaded = upload.Uploaded
            });
        }

        private string GetIPAddress()
        {
            const string proxyForwardingHeader = "X-Forwarded-For";

            if (HttpContext.Request.Headers.ContainsKey(proxyForwardingHeader))
                return HttpContext.Request.Headers[proxyForwardingHeader];
            else
                return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
