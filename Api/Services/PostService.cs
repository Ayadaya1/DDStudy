using Api.Models;
using AutoMapper;
using DAL;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

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
                Text = text
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

        public async Task<Post> GetPostById(Guid id)
        {
            var post = await _context.Posts.Include(x=>x.User).Include(x=>x.Attaches).Include(x=>x.Comments).FirstOrDefaultAsync(x => x.Id == id);
            if(post== null)
            {
                throw new Exception("Post not found");
            }    
            return post;
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
                User = user
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
    }
}
