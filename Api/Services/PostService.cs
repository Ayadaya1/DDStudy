using AutoMapper;
using DAL;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using Api.Models.Posts;
using Api.Models.Attaches;
using Api.Models.Comments;

namespace Api.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        private Func<Guid, string?>? _contentLinkGenerator;

        public PostService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public void SetLinkGenerator(Func<Guid, string?> contentLinkGenerator)
        {
            _contentLinkGenerator = contentLinkGenerator;
        }

        public async Task CreatePost(List<MetadataModel>attaches, Guid userId, string text)
        {
            var user = await _context.Users.Include(x => x.Posts).ThenInclude(x=>x.Attaches).FirstOrDefaultAsync(x => x.Id == userId);
            if(user == null)
            {
                throw new Exception("User is null");
            }
            var post = new Post
            {
                Id = Guid.NewGuid(),
                User = user,
                Created = DateTime.UtcNow,
                Text = text,
                Attaches = _mapper.Map<List<PostAttach>>(attaches)
            };
            foreach(MetadataModel meta in attaches)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", meta.TempId.ToString());
                var postAttach = new PostAttach
                {
                    Id= Guid.NewGuid(),
                    FilePath = path,
                    Author = user,
                    Mimetype = meta.MimeType,
                    Size = meta.Size,
                    Name = meta.Name
                };
                post.Attaches.Add(postAttach);
            }
            user.Posts.Add(post);
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task<PostModel> GetPostById(Guid id)
        {
            var post = await _context.Posts.Include(x=>x.User).ThenInclude(x=>x.Avatar).Include(x=>x.Attaches).Include(x=>x.Comments).Include(x=>x.Likes).FirstOrDefaultAsync(x => x.Id == id);
            if(post== null)
            {
                throw new Exception("Post not found");
            }    
            return _mapper.Map<PostModel>(post);
        }

        public async Task<AttachModel> GetPostAttachById(Guid id)
        {
            var attach = await _context.PostAttaches.FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<AttachModel>(attach);
        }

        public async Task AddComment(CommentInputModel model, Guid userId, Guid postId)
        {
            var post = await _context.Posts.Include(x => x.Comments).ThenInclude(x => x.User).FirstOrDefaultAsync(x => x.Id == postId);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if(user==null)
            {
                throw new Exception("User is null");
            }
            if(post==null)
            {
                throw new Exception("Post is null");
            }
            var comment = new Comment
            {
                Text = model.Text,
                Created = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                User = user,
                
            };
            if(post!=null)
                post.Comments.Add(comment);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

        }

        public string? FixContent(PostAttach s)
        {
            return _contentLinkGenerator?.Invoke(s.Id);
        }

        public async Task<List<CommentOutputModel>>GetComments(Guid postId)
        {
            var post = await _context.Posts
                .Include(x=>x.Comments).ThenInclude(x=>x.User).ThenInclude(x => x.Avatar)
                .Include(x => x.Comments).ThenInclude(x => x.User).ThenInclude(x => x.Subscribers)
                .Include(x => x.Comments).ThenInclude(x => x.User).ThenInclude(x => x.Subscriptions)
                .Include(x => x.Comments).ThenInclude(x => x.Likes)
                .FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new Exception("Post not found");
            return _mapper.Map<List<CommentOutputModel>>(post.Comments.OrderBy(x=>x.Created));
        }

        public async Task<List<PostModel>> GetTopPosts(int take, int skip)
        {
            var posts = await _context.Posts.Include(x=>x.Likes)
                .Include(x=>x.Attaches)
                .Include(x=>x.User).ThenInclude(x=>x.Subscribers)
                .Include(x => x.User).ThenInclude(x => x.Subscriptions)
                .Include(x => x.User).ThenInclude(x => x.Avatar)
                .OrderByDescending(x => x.Likes.Count).Skip(skip).Take(take).ToListAsync();
            return _mapper.Map<List<PostModel>>(posts);
        }

        public async Task<List<PostModel>> GetPostsOfThoseYoureSubscribedTo(int take, int skip, Guid userId)
        {
            var posts = await _context.Posts.Include(x => x.Likes)
                .Include(x => x.Attaches)
                .Include(x => x.User).ThenInclude(x => x.Subscribers).ThenInclude(x => x.Subscriber)
                .Include(x => x.User).ThenInclude(x => x.Subscriptions)
                .Include(x => x.User).ThenInclude(x => x.Avatar)
                .OrderByDescending(x => x.Created).Skip(skip).Take(take).Where(x=>x.User.Subscribers.FirstOrDefault(x=>x.Subscriber.Id==userId)!=null).ToListAsync();
            return _mapper.Map<List<PostModel>>(posts);
        }
    }
}
