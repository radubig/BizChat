using BizChat.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;

namespace BizChat.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServerRole> ServerRoles { get; set; }
        public DbSet<ServerUser> ServerUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cheie primara compusa pentru ServerUser
            builder.Entity<ServerUser>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ServerId });

            builder.Entity<ServerUser>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.UserServers)
                .HasForeignKey(ab => ab.UserId);

            builder.Entity<ServerUser>()
                .HasOne(ab => ab.Server)
                .WithMany(ab => ab.Users)
                .HasForeignKey(ab => ab.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Alte relatii definite explicit pentru cascade delete
            builder.Entity<Server>()
                .HasMany(s => s.ServerCategories)
                .WithOne(s => s.Server)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Category>()
                .HasMany(c => c.Channels)
                .WithOne(c => c.Category)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Channel>()
                .HasMany(c => c.ServerMessages)
                .WithOne(c => c.Channel)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}