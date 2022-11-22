using Api.Services;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using static System.Net.WebRequestMethods;
using Common.Extentions;
using Api.Models.Posts;
using Api.Models.Comments;
using Api.Models.Attaches;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly PostService _postService;
        private readonly AttachService _attachService;

        public PostController(UserService userService, PostService postService, AttachService attachService, LinkGeneratorService links)
        {
            _userService = userService;
            _postService = postService;
            _attachService = attachService;
            _postService.SetLinkGenerator(_contentLinkGenerator);
            links.LinkAvatarGenerator = x => Url.ControllerAction<UserController>(nameof(UserController.GetUserAvatar), new
            {
                userId = x.Id,
            });
            links.LinkContentGenerator = x => Url.ControllerAction<PostController>(nameof(PostController.GetPostContent), new
            {
                postAttachId = x.Id,
            });
        }

        private string? _contentLinkGenerator(Guid postAttachId)
        {
            return Url.ControllerAction<PostController>(nameof(PostController.GetPostContent), new
            {
                postAttachId
            });
        }

        [Authorize]
        [HttpPost]
        public async Task AddPost(List<MetadataModel>meta, string text)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                var user = await _userService.GetUser(userId);
                if (user != null)
                {
                    foreach (MetadataModel model in meta)
                    {
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
                            }
                        }
                    }
                    await _postService.CreatePost(meta, userId, text);
                }
            }
            else
                throw new Exception("You are not authorized");
        }

        [Authorize]
        [HttpPost]
        public async Task AddPostWithNewFiles([FromForm] List<IFormFile> files, string text)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            var userId = GetCurrentUserId();
            var user = await _userService.GetUser(userId);
            if (user != null)
            {
                var meta = new List<MetadataModel>();
                foreach (IFormFile file in files)
                {
                    var model = await _attachService.LoadFile(file);
                    meta.Add(model);
                }

                await AddPost(meta, text);
            }

        }

        [HttpGet]
        [Route("")]
        public async Task<PostModel> GetPost(Guid id) => await _postService.GetPostById(id);

        [HttpGet]
        public async Task<FileResult> GetPostContent(Guid postAttachId)
        {
            var attach = await _postService.GetPostAttachById(postAttachId);

            if(attach == null)
                throw new Exception("The attach is null");

            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype);
        }

        [Authorize]
        [HttpPost]
        public async Task AddCommentToPost(Guid postId, CommentInputModel model)
        {
            var userId = GetCurrentUserId();
                await _postService.AddComment(model, userId, postId);
        }

        [HttpGet]
        public async Task<List<CommentOutputModel>> GetAllComments(Guid postId) => await _postService.GetComments(postId);


        [Authorize]
        [HttpGet]
        private Guid GetCurrentUserId()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
                return userId;
            else
                throw new Exception("You are not authorized");
        }

        //[Authorize]
        [HttpGet]
        public async Task<List<PostModel>> GetTopPosts(int take, int skip)=> await _postService.GetTopPosts(take, skip);

        [Authorize]
        [HttpGet]
        public async Task<List<PostModel>> GetSubscribedPosts(int take, int skip) 
            => await _postService.GetPostsOfThoseYoureSubscribedTo(take, skip, GetCurrentUserId());
    }

}
