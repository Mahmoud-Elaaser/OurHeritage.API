using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class CulturalArticleConfiguration : IEntityTypeConfiguration<CulturalArticle>
    {
        public void Configure(EntityTypeBuilder<CulturalArticle> builder)
        {
            builder
                .HasKey(t => t.Id);


            builder
                .Property(c => c.Content)
                .HasMaxLength(280);


            builder
                .HasOne(t => t.User)
                .WithMany(u => u.culturalArticles)
                .HasForeignKey(t => t.UserId);

            builder
                .HasMany(t => t.Likes)
                .WithOne(l => l.CulturalArticle)
                .HasForeignKey(l => l.CulturalArticleId);

            builder
                .HasMany(t => t.Comments)
                .WithOne(c => c.CulturalArticle)
                .HasForeignKey(c => c.CulturalArticleId);


            builder.HasIndex(x => x.UserId);
        }
    }
}
