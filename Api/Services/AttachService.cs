using Api.Models;
using AutoMapper;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class AttachService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public AttachService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MetadataModel> LoadFile(IFormFile file)
        {
            var tmpPath = Path.GetTempPath();

            var meta = new MetadataModel
            {
                TempId = Guid.NewGuid(),
                Name = file.FileName,
                MimeType = file.ContentType,
                Size = file.Length,
            };

            var newPath = Path.Combine(tmpPath, meta.TempId.ToString());

            var fileInfo = new FileInfo(newPath);

            if (fileInfo.Exists)
            {
                throw new Exception("File already exists");
            }
            else
            {
                if (fileInfo.Directory == null)
                {
                    throw new Exception("Temp directory is null");
                }
                if (!fileInfo.Directory.Exists)
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

        public async Task<AttachModel> GetAttachById(Guid id)
        {
            var attach = await _context.Attaches.FirstOrDefaultAsync(x => x.Id == id);
            if(attach==null)
            {
                throw new Exception("Attach is null");
            }
            var mappedAttach = _mapper.Map<AttachModel>(attach);
            return mappedAttach;

        }
    }
}
