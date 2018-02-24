using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Model;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using ShikashiAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/account/uploads")]
    public class UploadsController : UserAuthenticatedController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUploadService _uploadService;

        public UploadsController(IKeyService keyService, IAuthorizationService authService, IUploadService uploadService)
            : base(keyService)
        {
            this._authorizationService = authService;
            this._uploadService = uploadService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUploads()
        {
            var key = await GetCurrentKey();
            if (!(await _authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            var uploads = await _uploadService.GetAllUploads(key.User);

            List<UploadViewModel> models = new List<UploadViewModel>();

            foreach (UploadedContent upload in uploads)
            {
                models.Add(new UploadViewModel
                {
                    FileName = upload.FileName,
                    FileSize = upload.FileSize,
                    MimeType = upload.MimeType,
                    Uploaded = upload.Uploaded,
                    Key = _uploadService.GetIdHash(upload.Id)
                });
            }

            return Ok(models);
        }
    }
}
