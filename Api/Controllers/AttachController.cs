using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public AttachController(AttachService attachService)
        {
            _attachService = attachService;
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
