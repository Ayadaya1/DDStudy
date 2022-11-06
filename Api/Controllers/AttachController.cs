using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
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
        public async Task<MetadataModel> UploadFile([FromForm] IFormFile file)
        {
            var tmpPath = Path.GetTempPath();

            var meta = new MetadataModel
            {
                TempId = Guid.NewGuid(),
                Name = file.Name,
                MimeType = file.ContentType
            };

            var newPath = Path.Combine(tmpPath, meta.TempId.ToString());

            var fileInfo = new FileInfo(newPath);

            if(fileInfo.Exists)
            {
                throw new Exception("File already exists");
            }
            else
            {
                if(fileInfo.Directory==null)
                {
                    throw new Exception("Temp directory is null");
                }
                if(!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory?.Create();
                }
            }
            using (var stream = System.IO.File.Create(newPath))
            {
                await file.CopyToAsync(stream);
            }

            return meta;
        }
    }
}
