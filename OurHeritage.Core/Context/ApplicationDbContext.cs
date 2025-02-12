using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int,
        IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<BlockUser> BlockUsers { get; set; }
        public DbSet<CulturalArticle> CulturalArticles { get; set; }
        public DbSet<HandiCraft> HandiCrafts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<favorite> Favorites { get; set; }
        public DbSet<Follow> Followers { get; set; }
        public DbSet<Follow> Followings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<IdentityUserLogin<int>>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<int>>()
                .HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<int>>()
                .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<HandiCraft>()
                .HasOne(h => h.User)
                .WithMany(u => u.HandiCrafts)
                .HasForeignKey(h => h.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.CulturalArticle)
                .WithMany(ca => ca.Comments)
                .HasForeignKey(c => c.CulturalArticleId);

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<CulturalArticle>()
                .HasMany(ca => ca.Likes)
                .WithOne(l => l.CulturalArticle)
                .HasForeignKey(l => l.CulturalArticleId);

            modelBuilder.Entity<HandiCraft>()
                .HasMany(h => h.Favorite)
                .WithOne(f => f.HandiCraft)
                .HasForeignKey(f => f.HandiCraftId);

            modelBuilder.Entity<HandiCraft>()
                .HasMany(h => h.Orders)
                .WithMany(o => o.HandiCrafts)
                .UsingEntity<Dictionary<string, object>>(
                    "OrderHandiCraft",
                    j => j.HasOne<Order>().WithMany().HasForeignKey("OrderId"),
                    j => j.HasOne<HandiCraft>().WithMany().HasForeignKey("HandiCraftId"),
                    j => j.HasKey("OrderId", "HandiCraftId")
                );
        }

    }
}
