using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();

            modelBuilder
                .Entity<Avatar>()
                .ToTable(nameof(Avatars));

            modelBuilder.
                Entity<PostAttach>().
                ToTable(nameof(PostAttaches));

            modelBuilder.
                Entity<PostLike>().
                ToTable(nameof(PostLikes));

            modelBuilder.
                Entity<AvatarLike>().
                ToTable(nameof(AvatarLikes));

            modelBuilder.
                Entity<CommentLike>().
                ToTable(nameof(CommentLikes));

            modelBuilder.Entity<User>().HasMany(u => u.Subscriptions).WithOne(s => s.Subscriber);

            modelBuilder.Entity<PrivacySettings>().HasKey(p => p.UserId);

            modelBuilder.Entity<PostLike>().HasKey(l => new { l.UserId, l.PostId });

            modelBuilder.Entity<CommentLike>().HasKey(l => new { l.UserId, l.CommentId });

            modelBuilder.Entity<AvatarLike>().HasKey(l => new { l.UserId, l.AvatarId });


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<PostAttach> PostAttaches => Set<PostAttach>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<PrivacySettings> PrivacySettings => Set<PrivacySettings>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<AvatarLike> AvatarLikes => Set<AvatarLike>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
        //public DbSet<Like> Likes => Set<Like>();
    }
}
