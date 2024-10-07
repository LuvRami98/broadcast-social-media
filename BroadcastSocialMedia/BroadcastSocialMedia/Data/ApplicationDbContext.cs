using BroadcastSocialMedia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Broadcast> Broadcasts { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ListeningTo)
                .WithMany(u => u.Listeners)
                .UsingEntity(j => j.ToTable("UserListenings"));

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.BroadcastId })
                .IsUnique();
        }
    }
}
