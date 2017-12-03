using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using System.Threading.Tasks;

namespace ShikashiAPI.Controllers
{
    [Route("/{uploadKey}/delete")]
    public class DeleteUploadController : UserAuthenticatedController
    {
        private IUserService userService;
        private IAuthorizationService authorizationService;
        private IUploadService uploadService;
        private IS3Service s3Service;

        public DeleteUploadController(IUserService userService, IKeyService keyService, IAuthorizationService authService, IUploadService uploadService, IS3Service s3Service)
            : base(keyService)
        {
            this.userService = userService;
            this.authorizationService = authService;
            this.uploadService = uploadService;
            this.s3Service = s3Service;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUpload(string uploadKey)
        {
            var key = await GetCurrentKey();
            if (!(await authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            var upload = await uploadService.GetUpload(uploadKey);

            if (upload == null)
            {
                NotFound();
            }

            if (upload.Owner.Id != key.User.Id)
            {
                return new ChallengeResult();
            }

            string extension = upload.FileName.Substring(upload.FileName.LastIndexOf('.') + 1);

            await uploadService.RemoveUpload(upload);
            await s3Service.DeleteUpload(uploadService.GetIdHash(upload.Id));
            await s3Service.DeleteUpload($"{uploadService.GetIdHash(upload.Id)}.{extension}");

            return new StatusCodeResult(204);
        }
    }
}
