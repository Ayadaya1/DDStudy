using Api.Models.Attaches;
using Api.Models.Comments;
using Api.Models.Likes;
using Api.Models.Posts;
using Api.Models.User;
using Api.Services;
using Common.Extentions;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly LikeService _likeService;
        public LikeController(LikeService likeService, LinkGeneratorService links)
        {
            _likeService = likeService;
            links.LinkAvatarGenerator = x => Url.ControllerAction<UserController>(nameof(UserController.GetUserAvatar), new
            {
                userId = x.Id,
            });
            links.LinkLikeableAvatarGenerator = x => Url.ControllerAction<UserController>(nameof(UserController.GetUserAvatar), new
            {
                userId = x.User.Id,
            });
            links.LinkContentGenerator = x => Url.ControllerAction<PostController>(nameof(PostController.GetPostContent), new
            {
                postAttachId = x.Id,
            });
        }

        [Authorize]
        [HttpPost]
        public async Task AddLike(LikeModel model) => await _likeService.AddLike(GetCurrentUserId(), model.ContentType, model.ContentId);

        [Authorize]
        [HttpGet]
        private Guid GetCurrentUserId()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            else
                throw new Exception("You are not authorized");
        }

        [Authorize]
        [HttpGet]
        public async Task<List<PostModel>> GetLikedPosts() => await _likeService.GetLikedPosts(GetCurrentUserId());

        [Authorize]
        [HttpGet]
        public async Task<List<CommentOutputModel>> GetLikedComments() => await _likeService.GetLikedComments(GetCurrentUserId());

        [Authorize]
        [HttpGet]
        public async Task<List<LikeableAvatarModel>> GetLikedAvatars() => await _likeService.GetLikedAvatars(GetCurrentUserId());

        [Authorize]
        [HttpPost]
        public async Task RemoveLike(LikeModel model) => await _likeService.Unlike(model.ContentId, GetCurrentUserId(), model.ContentType);

        [HttpGet]
        public async Task<List<UserModel>> GetThoseWhoLikedContent(string contentType, Guid contentId) => await _likeService.GetThoseWhoLiked(contentType, contentId);

        [Authorize]
        [HttpPost]
        public async Task<bool> CheckLike(LikeModel model) => await _likeService.CheckLike(model, GetCurrentUserId());
    }
}
