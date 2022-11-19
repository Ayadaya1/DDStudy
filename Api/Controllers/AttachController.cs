using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.Services;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
        private readonly AttachService _attachService;
        private readonly UserService _userService;

        public AttachController(AttachService attachService, UserService userService)
        {
            _attachService = attachService;
            _userService = userService;
        }
        [HttpPost]
        [Authorize]
        public async Task <List<MetadataModel>> UploadFiles([FromForm]List<IFormFile> files)
        {
            var meta = new List<MetadataModel>();
            foreach (var file in files)
            {
                meta.Add(await UploadFile(file));
            }
            return meta;
        }

        [HttpPost]
        [Authorize]
        private async Task<MetadataModel> UploadFile([FromForm] IFormFile file)
        {
            return await _attachService.LoadFile(file);
        }
    }
}
