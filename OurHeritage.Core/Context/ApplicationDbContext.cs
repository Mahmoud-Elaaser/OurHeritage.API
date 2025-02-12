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

        public DbSet<User> Users { get; set; }
        public DbSet<CulturalArticle> CulturalArticles { get; set; }
        public DbSet<HandiCraft> HandiCrafts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<favorite> Favorites { get; set; }
        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ ضبط مفاتيح ASP.NET Identity
            modelBuilder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<int>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<int>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // ✅ ضبط اسم جدول `Users`
            modelBuilder.Entity<User>().ToTable("Users");

            // ✅ منع الحذف التتابعي (Cascade Delete) في العلاقات المعقدة
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.CulturalArticle)
                .WithMany(ca => ca.Comments)
                .HasForeignKey(c => c.CulturalArticleId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<Follow>().HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<CulturalArticle>()
                .HasMany(ca => ca.Likes)
                .WithOne(l => l.CulturalArticle)
                .HasForeignKey(l => l.CulturalArticleId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

            modelBuilder.Entity<HandiCraft>()
                .HasMany(h => h.Favorite)
                .WithOne(f => f.HandiCraft)
                .HasForeignKey(f => f.HandiCraftId)
                .OnDelete(DeleteBehavior.Restrict); // ⬅️ الحل هنا

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
