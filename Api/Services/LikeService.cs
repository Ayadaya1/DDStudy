using Api.Exceptions;
using Api.Models.Attaches;
using Api.Models.Comments;
using Api.Models.Likes;
using Api.Models.Posts;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Api.Services
{
    public class LikeService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public LikeService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task AddLike(Guid userId, string contentType, Guid contentId)
        {
            ILikeable likeable;
            switch (contentType)
            {
                case "Post":

                    if (await _context.PostLikes.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == contentId) != null)
                        throw new Exception("Already liked this post");

                    var temp = await _context.Posts.Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == contentId);
                    if (temp == null)
                        throw new PostNotFoundException();

                    likeable = temp;
                    PostLike like = new PostLike
                    {
                        Post = likeable as Post,
                        UserId = userId
                    };

                    (likeable as Post).Likes.Add(like);
                    break;

                case "Avatar":

                    if (await _context.AvatarLikes.FirstOrDefaultAsync(x => x.UserId == userId && x.AvatarId == contentId) != null)
                        throw new Exception("Already liked this avatar");

                    var tempAvatarLike = await _context.Avatars.Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == contentId);
                    if (tempAvatarLike == null)
                        throw new AttachNotFoundException();

                    likeable = tempAvatarLike;
                    AvatarLike avatarLike = new AvatarLike
                    {
                        Avatar = likeable as Avatar,
                        UserId = userId
                    };

                    (likeable as Avatar).Likes.Add(avatarLike);
                    break;

                case "Comment":

                    if (await _context.CommentLikes.FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == contentId) != null)
                        throw new Exception("Already liked this comment");

                    var tempCommentLike = await _context.Comments.Include(x => x.Likes).FirstOrDefaultAsync(x => x.Id == contentId);
                    if (tempCommentLike == null)
                        throw new CommentNotFoundException();

                    likeable = tempCommentLike;
                    CommentLike commentLike = new CommentLike
                    {
                        Comment = likeable as Comment,
                        UserId = userId
                    };

                    (likeable as Comment).Likes.Add(commentLike);
                    break;

                default:
                    throw new InvalidContentTypeException();


            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetLikedPosts(Guid userId)
        {
            var posts = await _context.PostLikes.Include(x => x.Post).ThenInclude(x=>x.User).ThenInclude(u=>u.Avatar)
                .Include(x => x.Post).ThenInclude(x => x.User).ThenInclude(u => u.Subscribers)
                .Include(x => x.Post).ThenInclude(x => x.User).ThenInclude(u => u.Subscriptions)
                .Include(x => x.Post).ThenInclude(x => x.Comments)
                .Include(x => x.Post).ThenInclude(x => x.Attaches)
                .Include(x=>x.Post).ThenInclude(x=>x.Likes)
                .Where(x => x.UserId == userId).Select(x=>x.Post).ToListAsync();

            return _mapper.Map<List<PostModel>>(posts);
        }

        public async Task<List<CommentOutputModel>> GetLikedComments(Guid userId)
        {
            var comments = await _context.CommentLikes.Include(x => x.Comment).ThenInclude(x => x.User).ThenInclude(u => u.Avatar)
                .Include(x => x.Comment).ThenInclude(x => x.User).ThenInclude(u => u.Subscribers)
                .Include(x => x.Comment).ThenInclude(x => x.User).ThenInclude(u => u.Subscriptions)
                .Include(x=>x.Comment).ThenInclude(x=>x.Likes)
                .Where(x => x.UserId == userId).Select(x=>x.Comment).ToListAsync();

            return _mapper.Map<List<CommentOutputModel>>(comments);
        }

        public async Task<List<LikeableAvatarModel>> GetLikedAvatars(Guid userId)
        {
            var avatars = await _context.AvatarLikes.Include(x => x.Avatar).ThenInclude(x => x.User)
                .Include(x => x.Avatar).ThenInclude(x => x.User).ThenInclude(u => u.Subscribers)
                .Include(x => x.Avatar).ThenInclude(x => x.User).ThenInclude(u => u.Subscriptions)
                .Include(x => x.Avatar).ThenInclude(x => x.Likes)
                .Where(x => x.UserId == userId).Select(x => x.Avatar).ToListAsync();

            return _mapper.Map<List<LikeableAvatarModel>>(avatars);
        }

        public async Task Unlike(Guid contentId, Guid userId, string contentType)
        {
            switch (contentType)
            {
                case "Post":
                    var postLike = await _context.PostLikes.FirstOrDefaultAsync(x => x.PostId == contentId && x.UserId == userId);
                    if (postLike == null)
                        throw new PostNotFoundException();
                    _context.PostLikes.Remove(postLike);
                    break;
                case "Avatar":
                    var avatarLike = await _context.AvatarLikes.FirstOrDefaultAsync(x => x.AvatarId == contentId && x.UserId == userId);
                    if (avatarLike == null)
                        throw new AttachNotFoundException();
                    _context.AvatarLikes.Remove(avatarLike);
                    break;
                case "Comment":
                    var commentLike = await _context.CommentLikes.FirstOrDefaultAsync(x => x.CommentId == contentId && x.UserId == userId);
                    if (commentLike == null)
                        throw new CommentNotFoundException();
                    _context.CommentLikes.Remove(commentLike);
                    break;
                default:
                    throw new InvalidContentTypeException();

                    
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserModel>> GetThoseWhoLiked(string contentType, Guid contentId)
        {
            var users = new List<User?>();
            
            switch(contentType)
            {
                case "Post":
                    if (await _context.Posts.FirstOrDefaultAsync(x => x.Id == contentId) == null)
                        throw new PostNotFoundException();
                    var postLikers =  await _context.PostLikes
                        .Include(x=>x.User).ThenInclude(x=>x.Avatar)
                        .Include(x => x.User).ThenInclude(x => x.Subscribers)
                        .Include(x => x.User).ThenInclude(x => x.Subscriptions)
                        .Where(x => x.PostId == contentId).Select(x => x.User).ToListAsync();
                    users = postLikers;
                    break;

                case "Avatar":
                    if (await _context.Avatars.FirstOrDefaultAsync(x => x.Id == contentId) == null)
                        throw new AttachNotFoundException();
                    var avatarLikers = await _context.AvatarLikes
                        .Include(x => x.User).ThenInclude(x => x.Avatar)
                        .Include(x => x.User).ThenInclude(x => x.Subscribers)
                        .Include(x => x.User).ThenInclude(x => x.Subscriptions)
                        .Where(x => x.AvatarId == contentId).Select(x => x.User).ToListAsync();
                    users = avatarLikers;
                    break;

                case "Comment":
                    if (await _context.Comments.FirstOrDefaultAsync(x => x.Id == contentId) == null)
                        throw new CommentNotFoundException();
                    var commentLikers = await _context.CommentLikes
                        .Include(x => x.User).ThenInclude(x => x.Avatar)
                        .Include(x => x.User).ThenInclude(x => x.Subscribers)
                        .Include(x => x.User).ThenInclude(x => x.Subscriptions)
                        .Where(x => x.CommentId == contentId).Select(x => x.User).ToListAsync();
                    users = commentLikers;
                    break;

                default:
                    throw new InvalidContentTypeException();
            }

            return _mapper.Map<List<UserModel>>(users);
        }

        public async Task<bool> CheckLike(LikeModel model, Guid userId)
        {
            switch (model.ContentType)
            {
                case "Post":
                    if (await _context.PostLikes.FirstOrDefaultAsync(x => x.PostId == model.ContentId && x.UserId == userId) != null)
                        return true;
                    break;

                case "Avatar":
                    if (await _context.AvatarLikes.FirstOrDefaultAsync(x => x.AvatarId == model.ContentId && x.UserId == userId) != null)
                        return true;
                    break;

                case "Comment":
                    if (await _context.CommentLikes.FirstOrDefaultAsync(x => x.CommentId == model.ContentId && x.UserId == userId) != null)
                        return true;
                    break;

                default:
                    throw new InvalidContentTypeException();
            }
            return false;
        }
    }
}
