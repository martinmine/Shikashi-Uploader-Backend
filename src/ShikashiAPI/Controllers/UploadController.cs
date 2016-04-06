using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using ShikashiAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public UploadController(IConfiguration config, IUploadService uploadService, IKeyService keyService, IAuthorizationService authService, IS3Service s3Service, ILoggerFactory factory)
            : base(keyService)
        {
            this.uploadService = uploadService;
            this.authorizationService = authService;
            this.maxUploadSize = int.Parse(config["MaxFileSize"]);
            this.s3Service = s3Service;
            this.logger = factory.CreateLogger<UploadController>();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(ICollection<IFormFile> files)
        {
            var key = await GetCurrentKey();

            if (!await authorizationService.AuthorizeAsync(User, key, Operations.Create))
            {
                return new ChallengeResult();
            }

            foreach (IFormFile file in files)
            {
                if (file == null || string.IsNullOrEmpty(HttpContext.Request.Headers["UploadFileSize"]) || file.Length > maxUploadSize)
                {
                    return HttpBadRequest();
                }

                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                var upload = await uploadService.CreateUpload(file.ContentType, ip, fileName, null, file.Length);

                await s3Service.StoreUpload(file.OpenReadStream(), uploadService.GetIdHash(upload.Id), file.ContentType, file.Length, fileName);
                await s3Service.CreateUploadAlias(uploadService.GetIdHash(upload.Id), file.ContentType, fileName);

                UploadViewModel viewModel = new UploadViewModel
                {
                    FileName = upload.FileName,
                    FileSize = upload.FileSize,
                    Key = uploadService.GetIdHash(upload.Id),
                    MimeType = upload.MimeType,
                    Uploaded = upload.Uploaded
                };

                return Ok(viewModel);
            }

            return HttpBadRequest();
        }
    }
}
