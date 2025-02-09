using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShikashiAPI.Controllers
{
    [Route("/{uploadKey}/delete")]
    public class DeleteUploadController : UserAuthenticatedController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUploadService _uploadService;
        private readonly IS3Service _s3Service;
        private readonly ILogger<DeleteUploadController> _logger;

        public DeleteUploadController(IUserService userService, IKeyService keyService,
            IAuthorizationService authService, IUploadService uploadService, IS3Service s3Service, ILogger<DeleteUploadController> logger)
            : base(keyService)
        {
            _authorizationService = authService;
            _uploadService = uploadService;
            _s3Service = s3Service;
            _logger = logger;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUpload(string uploadKey)
        {
            var key = await GetCurrentKey();
            if (!(await _authorizationService.AuthorizeAsync(User, key, Operations.Create)).Succeeded)
            {
                return new ChallengeResult();
            }

            var upload = await _uploadService.GetUpload(uploadKey);

            if (upload == null)
            {
                NotFound();
            }

            if (upload.Owner.Id != key.User.Id)
            {
                _logger.LogInformation("User {@User} attempted to delete upload {@UploadKey} which is owned by {@OP}",
                    key.User.Id, uploadKey, upload.Owner.Id);
                return new ChallengeResult();
            }

            string extension = upload.FileName.Substring(upload.FileName.LastIndexOf('.') + 1);

            await _uploadService.RemoveUpload(upload);
            await _s3Service.DeleteUpload(_uploadService.GetIdHash(upload.Id));
            await _s3Service.DeleteUpload($"{_uploadService.GetIdHash(upload.Id)}.{extension}");

            _logger.LogInformation("User {@User} deleted upload {@UploadKey} with owner {@OwnerId}",
                key.User.Id, uploadKey, upload.Owner.Id);

            return new StatusCodeResult(204);
        }
    }
}
