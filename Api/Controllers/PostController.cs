using Api.Models;
using Api.Services;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly PostService _postService;
        private readonly AttachService _attachService;

        public PostController(UserService userService, PostService postService, AttachService attachService)
        {
            _userService = userService;
            _postService = postService;
            _attachService = attachService;
        }

        [Authorize]
        [HttpPost]
        public async Task AddPost([FromForm]List<IFormFile> files, string text)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                var user =  await _userService.GetUser(userId);
                if (user!=null)
                {
                    var meta = new List<MetadataModel>();
                    foreach (IFormFile file in files)
                    {
                        var model = await _attachService.LoadFile(file);
                        var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                        if (!tempFi.Exists)
                        {
                            throw new Exception("File not found!");
                        }
                        else
                        {
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", model.TempId.ToString());
                            var destFi = new FileInfo(path);
                            if (destFi.Directory != null && !destFi.Directory.Exists)
                                destFi.Directory.Create();

                            if (path != null)
                            {
                                System.IO.File.Copy(tempFi.FullName, path, true);
                                meta.Add(model);
                            }
                        }
                    }
                    await _postService.CreatePost(meta,userId, text);
                }
            }
            else
                throw new Exception("You are not authorized");

        }

        [HttpGet]
        [Route("")]
        public async Task<PostModel> GetPost(Guid id)
        {
            var post =  await _postService.GetPostById(id);
            var model = new PostModel
            {
                Created = post.Created,
                Text = post.Text,
                UserName = post.User.Name,
                Comments = post.Comments.Count
            };
            foreach (var attach in post.Attaches)
            {
                var path = "api/Post/GetPostContent?postAttachId=";
                path+=attach.Id.ToString();

                model.Attaches.Add(path);
            }
            return model;
        }

        [HttpGet]
        public async Task<FileResult> GetPostContent(Guid postAttachId)
        {
            var attach = await _postService.GetPostAttachById(postAttachId);

            if(attach == null)
            {
                throw new Exception("The attach is null");
            }

            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype);
        }
    }
}
