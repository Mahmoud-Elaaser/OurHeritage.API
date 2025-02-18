using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class LikeConfiguration : IEntityTypeConfiguration<Like>
    {
        public void Configure(EntityTypeBuilder<Like> builder)
        {
            builder
                .HasOne(l => l.CulturalArticle)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.CulturalArticleId);


            builder
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId);


            builder.HasIndex(l => l.CulturalArticleId);
            builder.HasIndex(l => l.UserId);

        }
    }
}
