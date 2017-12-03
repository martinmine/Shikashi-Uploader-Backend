using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Model;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using ShikashiAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/account/uploads")]
    public class UploadsController : UserAuthenticatedController
    {
        private IUserService userService;
        private IAuthorizationService authorizationService;
        private IUploadService uploadService;

        public UploadsController(IUserService userService, IKeyService keyService, IAuthorizationService authService, IUploadService uploadService)
            : base(keyService)
        {
            this.userService = userService;
            this.authorizationService = authService;
            this.uploadService = uploadService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUploads()
        {
            var key = await GetCurrentKey();
            if (!(await authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            var uploads = await uploadService.GetAllUploads(key.User);

            List<UploadViewModel> models = new List<UploadViewModel>();

            foreach (UploadedContent upload in uploads)
            {
                models.Add(new UploadViewModel
                {
                    FileName = upload.FileName,
                    FileSize = upload.FileSize,
                    MimeType = upload.MimeType,
                    Uploaded = upload.Uploaded,
                    Key = uploadService.GetIdHash(upload.Id)
                });
            }

            return Ok(models);
        }
    }
}
