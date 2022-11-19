using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            modelBuilder.Entity<User>().HasMany(u => u.Subscriptions).WithOne(s => s.Subscriber);
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
        public DbSet<Subscription> Subscriptions =>Set<Subscription>();
    }
}
